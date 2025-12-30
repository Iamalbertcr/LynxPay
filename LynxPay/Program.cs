using LynxPay.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// JWT configuration
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSection["Key"]
    ?? throw new InvalidOperationException("JWT Key not configured");

var jwtIssuer = jwtSection["Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer not configured");

var jwtAudience = jwtSection["Audience"]
    ?? throw new InvalidOperationException("JWT Audience not configured");

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<DatabaseProvisioningService>();
builder.Services.AddScoped<ClientConnectionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://+:8080");
app.Run();
