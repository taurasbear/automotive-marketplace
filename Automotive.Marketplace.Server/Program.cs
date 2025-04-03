using Automotive.Marketplace.Application;
using Automotive.Marketplace.Infrastructure;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

var AllowClientOrigins = "allowClientOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowClientOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:57263")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

string? connectionString;
if (builder.Environment.IsDevelopment())
{
    var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
    connectionString = isDocker
        ? builder.Configuration.GetConnectionString("Docker")
        : builder.Configuration.GetConnectionString("Development");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("Production");
}
builder.Services.ConfigureInfrastructure(connectionString);
builder.Services.ConfigureApplication();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AllowClientOrigins);

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
