using Microsoft.EntityFrameworkCore;
using Web.Store.Backend.Data;

namespace Web.Store.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DB")));

        var app = builder.Build();

        //app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}