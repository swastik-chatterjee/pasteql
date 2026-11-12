using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PasteQL.Data;
using PasteQL.GraphQL;
using PasteQL.Services;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["PG_CONNECTION_STRING"]
    ?? builder.Configuration["POSTGRES_CONNECTION_STRING"]
    ?? throw new InvalidOperationException(
        "No database connection string was found. Set ConnectionStrings__DefaultConnection or PG_CONNECTION_STRING in your environment or .env file.");

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