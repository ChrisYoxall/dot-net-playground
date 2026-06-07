# xUnit v3 & Testcontainers Playground

This project demonstrates setting up a .NET 10 test project using **xUnit v3** and **Microsoft.Testing.Platform (MTP)**. 

---

## What is xUnit v3 Solving?

xUnit.net v3 is a major redesign of the xUnit testing framework aimed at solving several long-standing issues of version 2:

1. **Standalone Test Executables:** In v2, tests required external runners or test hosts (like VSTest). In v3, test projects compile directly into executable binaries (`<OutputType>Exe</OutputType>`). You can run your tests by executing the compiled binary directly.
2. **Microsoft.Testing.Platform (MTP) Native Integration:** It natively runs on Microsoft's new, lightweight, and high-performance runner platform instead of relying on the heavy, legacy VSTest pipeline.
3. **Out-of-Process Execution:** Test cases can be run in separate processes easily, avoiding shared-state pollution.
4. **No Test SDK Required:** Because it targets MTP natively and builds as an executable, traditional dependencies like `Microsoft.NET.Test.Sdk` and `xunit.runner.visualstudio` are technically no longer required.

---

## Important Discoveries & Workarounds

During setup with the .NET 10 SDK and JetBrains Rider, we identified two main caveats:

### 1. JetBrains Rider Test Discovery Workaround
*   **The Issue:** Although `Microsoft.NET.Test.Sdk` is technically optional for xUnit v3, JetBrains Rider's Unit Test Explorer currently relies on the package reference to recognize the project as a test suite and discover tests in the GUI.
*   **The Solution:** We explicitly include `Microsoft.NET.Test.Sdk` in `testcontainers.csproj` to satisfy Rider's discovery engine:
    ```xml
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    ```

### 2. Required `global.json`
*   **The Issue:** The .NET 10 SDK requires a `global.json` file to specify the use of `Microsoft.Testing.Platform`


### 3. Mismatched Test Runners at Solution Root
*   **The Issue:** The solution contains a mix of **xUnit v2 (VSTest)** and **xUnit v3 (MTP)** test projects.
*   **Why `dotnet test` fails at the Solution Root:**
    *   If you run `dotnet test` from the root, the .NET 10 CLI defaults to VSTest. Microsoft.Testing.Platform projects (like this one) explicitly block running under the VSTest target on .NET 10+, throwing the error: 
        > *Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and later.*
    *   If you force MTP globally via a root `global.json`, `dotnet test` fails because it expects *all* projects (including the legacy `design-principles` project) to use MTP.
*   **The Solution:** Keep the projects using their respective runners and execute them independently.

---

## How to Run the Tests

The v3 test projects tests will show up in the Rider test explorer.

Can run `dotnet test` from the project directory. Running it at the solution level will result in some failures.

---

## Running Postgres in Docker

Simply run:
```bash
docker run --name postgres-db -e POSTGRES_PASSWORD=mysecretpassword -p 5432:5432 -d postgres:18-alpine
```

The default username is `postgres`.
