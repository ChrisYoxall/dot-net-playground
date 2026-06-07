/*
 
 This was largely taken from https://codeopinion.com/testing-needs-a-seam-not-an-interface/
 
 Our application has a user class that needs to make an API call to GitHub. We create a service that encapsulates
 this functionality and to make it testable, add an interface. However, we are creating that interface only for testing
 purposes; there is no real need for it. Tests need a 
 
 Testing needs a 'seam' - a point in the code where we can inject a mock or stub for testing purposes. That seam
 does not have to be an interface.
 
 Can remove the IGitHubService below and update the UserService to use the concrete GitHubService, and the tests
 except the one using Moq will pass. It's more code, but you avoid needing to create an interface around
 all of your classes, and avoid third party tools like Moq.
 
 Also note, if you have a class/ service with one method, then all you really need is a function / delegate
  
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Moq;

namespace DesignPrinciples;

public record GitHubUser(string Name, string Login, int Id, int PublicRepos);

/* Using source generation rather than reflection in this example. Overkill for this example but a reminder that I want to create
another example. Refer https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/reflection-vs-source-generation */
[JsonSourceGenerationOptions(
    // Automatically maps "public_repos" to "PublicRepos" and "login" to "Login"
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,

    // Metadata mode generates the necessary type info for fast deserialization (reading)
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(GitHubUser))]
internal partial class SourceGenerationContext : JsonSerializerContext { }

public interface IGitHubService
{
    Task<GitHubUser> GetUser(string login);
}

public class GitHubService(HttpClient httpClient) : IGitHubService
{
    public async Task<GitHubUser> GetUser(string login)
    {
        using var response = await httpClient.GetAsync(new Uri($"https://api.github.com/users/{login}"));

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync(
            SourceGenerationContext.Default.GitHubUser
        );

        return result ?? throw new InvalidOperationException("Failed to deserialize GitHub user.");
    }
}

public record User(string Name, bool HasPublicGitHubRepos);

public class UserService(IGitHubService gitHubService)
{
    public async Task<User> CreateUser(string name, string gitHubLogin)
    {
        var gitHubUser = await gitHubService.GetUser(gitHubLogin);
        return new User(name, gitHubUser.PublicRepos > 0);
    }
}



public class Seams
{
    // Wouldn't normally have this unit test, but want to call the real service used in this example
    // to confirm it's realistic. Normally the HttpClient would be injected in when the service is used.
    [Fact]
    public async Task Call_real_user_service()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "GitHubServiceTestClient");
        var gitHubService = new GitHubService(client);

        var userService = new UserService(gitHubService);
        var user = await userService.CreateUser("Chris", "ChrisYoxall");

        Assert.NotNull(user);
        Assert.Equal("Chris", user.Name);
        Assert.True(user.HasPublicGitHubRepos);
    }

    // How you would typically test. Mock relies on the IGitHubService interface.
    [Fact]
    public async Task Test_user_service()
    {
        var gitHubService = new Mock<IGitHubService>();
        gitHubService.Setup(service => service.GetUser("ChrisYoxall"))
            .ReturnsAsync(new GitHubUser("Chris Yoxall", "ChrisYoxall", 11111, 0));

        var userService = new UserService(gitHubService.Object);
        var user = await userService.CreateUser("Chris", "ChrisYoxall");

        Assert.NotNull(user);
        Assert.Equal("Chris", user.Name);
        Assert.False(user.HasPublicGitHubRepos);
    }

    /// <summary>
    /// A stub HttpMessageHandler that returns a pre-defined response.
    /// </summary>
    /// <remarks>
    /// Can make the function that you pass in as complex as you need in the calling test. May want to examine
    /// the request to determine what response to return, or could count the number of requests and return a failure
    /// on the first two calls but succeed on the third. This class does not need to change.
    /// </remarks>
    /// <param name="responseFactory"></param>
    private class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory =
            responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = _responseFactory(request);
            return Task.FromResult(response);
        }
    }

    // The GitHubService uses HTTP, that can be used as the seam to enable testing. No need to rely on an interface and additional
    // packages such as Moq
    [Fact]
    public async Task Test_user_service_using_http_seam()
    {
        const string gitHubResponse = """
                                      {
                                          "login": "ChrisYoxall",
                                          "name": "Chris Yoxall",
                                          "id": 11111,
                                          "public_repos": 5
                                      }
                                      """;

        HttpMessageHandler handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(gitHubResponse)
            }
        );
        var gitHubService = new GitHubService(new HttpClient(handler));
        var userService = new UserService(gitHubService);
        var user = await userService.CreateUser("Chris", "ChrisYoxall");

        Assert.NotNull(user);
        Assert.Equal("Chris", user.Name);
        Assert.True(user.HasPublicGitHubRepos);
    }

    // Could introduce a helper class to simplify testing by returning different GitHubServices as needed.
    private static class GitHubServiceFactory
    {
        public static GitHubService ServiceReturnUserWithNoPublicRepos()
        {
            const string gitHubResponse = """
                                          {
                                              "login": "ChrisYoxall",
                                              "name": "Chris Yoxall",
                                              "id": 11111,
                                              "public_repos": 0
                                          }
                                          """;

            HttpMessageHandler handler = new StubHttpMessageHandler(_ =>
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(gitHubResponse)
                }
            );

            return new GitHubService(new HttpClient(handler));
        }

    }

    [Fact]
    public async Task Test_user_service_using_fixture()
    {
        var userService = new UserService(GitHubServiceFactory.ServiceReturnUserWithNoPublicRepos());
        var user = await userService.CreateUser("Chris", "ChrisYoxall");

        Assert.NotNull(user);
        Assert.Equal("Chris", user.Name);
        Assert.False(user.HasPublicGitHubRepos);
    }


    // Instead of GitHubService, which is a service with a single method, could instead use a delegate. Typically,
    // the code would reside in an extension method on IServiceCollection and injected into UserServiceWithDelegate
    // This is stepping out of traditional Object-Oriented Programming (OOP) and moving into Functional Programming (FP) territory.

    private delegate Task<GitHubUser> FetchGitHubUser(string login);

    private class UserServiceWithDelegate(FetchGitHubUser fetchGitHubUser)
    {
        public async Task<User> CreateUser(string name, string gitHubLogin)
        {
            var gitHubUser = await fetchGitHubUser(gitHubLogin);
            return new User(name, gitHubUser.PublicRepos > 0);
        }
    }

    // To test UserServiceWithDelegate, you don't need any test doubles, stubs, or mocking libraries. You just pass a lambda
    // expression right into the constructor. This is where the delegate pattern shines
    [Fact]
    public async Task Test_user_service_with_delegate()
    {
        FetchGitHubUser fetchUser = _ => Task.FromResult(new GitHubUser("Chris Yoxall", "ChrisYoxall", 111, 10));
        var sut = new UserServiceWithDelegate(fetchUser);
        
        var user = await sut.CreateUser("Chris", "ChrisYoxall");
        Assert.Equal("Chris", user.Name);
        Assert.True(user.HasPublicGitHubRepos);
    }

}


/*

TESTING TERMINOLOGY REFERENCE

   Test Double
   The umbrella term for any pretend object used in place of a real one during a test (like a stunt double in a movie).

   1. Dummy
   Objects that are passed around to satisfy parameter requirements and keep the compiler happy, but are never actually used or executed in the test.
   Example: Passing an empty string or null into a constructor for a parameter your test doesn't care about.

   2. Fake
   An object that has a working implementation, but takes a shortcut that makes it completely unsuitable for production.
   Example: Using a lightweight in-memory database instead of a real SQL database.

   3. Stub
   An object configured to return a hardcoded, canned response. It provides predictable data to the system under test.
   Example: A weather API stand-in that always returns "72 degrees" regardless of the city requested.

   4. Spy
   A stub that also records information about how it was called so you can assert against it later.
   Example: An email service that returns success but keeps a tally of how many times the Send() method was called.

   5. Mock
   An object pre-programmed with strict expectations used to verify behavior. If a mock expects to be called exactly once and isn't, the mock itself fails the test.
   Example: Setting up a payment gateway with the instruction, "Expect ChargeCard() to be called exactly once for $50."

   QUICK CHEAT SHEET: STUB vs. MOCK
   - Use a STUB to test STATE: Provide canned input, let the system run, and check if the final output is correct.
   - Use a MOCK to test BEHAVIOR: Verify that your system communicated with its external dependencies correctly.

   Note that a spy is similar to a mock, but it records calls and provides a way to assert against them, while a mock is stricter and
   fails the test if expectations are not met. A spy follows the arrangement, act, assert pattern, while a mock is setup, act, verify.

   Fixture (Test Fixture)
   This is not a test double. It is the baseline state or environment that must exist before your tests run to ensure they are consistent and repeatable.
   Example: Loading a specific set of sample users into a database before a test runs, or the Setup/Teardown methods in your test class.

*/