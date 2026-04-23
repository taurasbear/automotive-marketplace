using Automotive.Marketplace.Application;
using Automotive.Marketplace.Infrastructure;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Automotive.Marketplace.Server.Filters;
using Automotive.Marketplace.Server.Hubs;
using Automotive.Marketplace.Server.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var AllowClientOrigins = "allowClientOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowClientOrigins, policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                context.Token = accessToken;
            return Task.CompletedTask;
        }
    };
});

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationExceptionFilter>();
        options.Filters.Add<NotFoundExceptionFilter>();
        options.Filters.Add<UnauthorizedExceptionFilter>();
    })
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

string? connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("Development")
    : builder.Configuration.GetConnectionString("Production");

builder.Services.ConfigureInfrastructure(builder.Configuration, connectionString);
builder.Services.ConfigureApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddHostedService<Automotive.Marketplace.Server.Services.OfferExpiryService>();

var app = builder.Build();

app.UseExceptionHandler();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(AllowClientOrigins);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var automotiveContext = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
    await automotiveContext.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        foreach (var seeder in scope.ServiceProvider.GetServices<IDevelopmentSeeder>())
        {
            await seeder.SeedAsync(CancellationToken.None);
        }
    }
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatHub>("/hubs/chat");

app.MapFallbackToFile("/index.html");

app.Run();
