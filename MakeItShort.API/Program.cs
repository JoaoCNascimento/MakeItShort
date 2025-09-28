using DotNetEnv;
using MakeItShort.API.Repository;
using MakeItShort.API.Repository.Interfaces;
using MakeItShort.API.Services;
using MakeItShort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers();

if (builder.Environment.IsDevelopment())
{
    Env.Load("../.env");
    builder.Configuration.AddEnvironmentVariables();
}

builder.Services.AddDbContext<MakeItShortDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Services
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

// Repositories
builder.Services.AddScoped<IUrlShortenerRepository, UrlShortenerRepository>();

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MakeItShortDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
