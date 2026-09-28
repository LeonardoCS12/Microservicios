using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());

    var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:8080" };
    builder.Services.AddCors(options => options.AddPolicy("PermitirDominiosEspecificos", policy => policy.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader()));
    builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = Rs256Validation.Create(builder.Configuration);
    });
    builder.Services.AddAuthorization();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseCors("PermitirDominiosEspecificos");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/auth/swagger/v1/swagger.json", "Autenticacion");
        options.SwaggerEndpoint("/swagger/users/swagger/v1/swagger.json", "Usuarios");
        options.SwaggerEndpoint("/swagger/products/swagger/v1/swagger.json", "Productos");
        options.RoutePrefix = "swagger";
    });
    app.MapReverseProxy();
    app.MapGet("/", () => Results.Redirect("/swagger"));
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway fallo al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}

public static class Rs256Validation
{
    public static TokenValidationParameters Create(IConfiguration configuration)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(configuration["Jwt:PublicKeyPath"]!));
        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new RsaSecurityKey(rsa),
            ClockSkew = TimeSpan.Zero
        };
    }
}
