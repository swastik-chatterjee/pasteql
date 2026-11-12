using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PasteQL.Data;
using PasteQL.GraphQL;
using PasteQL.Services;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var databaseUrl = builder.Configuration["DATABASE_URL"]
    ?? throw new InvalidOperationException(
        "No database connection URL was found. Set DATABASE_URL in your environment or .env file.");
var connectionString = ToNpgsqlConnectionString(databaseUrl);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PasteDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(System.IO.Path.Combine(builder.Environment.ContentRootPath, "keys")));

builder.Services.AddScoped<PasteService>();
builder.Services.AddGraphQLServer()
    .AddQueryType<PasteGraphQuery>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PasteDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapGraphQL("/graphql");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string ToNpgsqlConnectionString(string databaseUrl)
{
    var uri = new Uri(databaseUrl, UriKind.Absolute);
    if (uri.Scheme is not ("postgres" or "postgresql"))
    {
        throw new InvalidOperationException("DATABASE_URL must use the postgres:// or postgresql:// scheme.");
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var connectionString = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = Uri.UnescapeDataString(userInfo[0])
    };

    if (userInfo.Length == 2)
    {
        connectionString.Password = Uri.UnescapeDataString(userInfo[1]);
    }

    var queryParameters = System.Web.HttpUtility.ParseQueryString(uri.Query);
    foreach (var key in queryParameters.AllKeys)
    {
        if (key is not null)
        {
            connectionString[key] = queryParameters[key] ?? string.Empty;
        }
    }

    return connectionString.ConnectionString;
}