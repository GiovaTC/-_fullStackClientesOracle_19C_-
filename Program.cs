using System.Text;
using Backend.Data;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

if (Environment.GetEnvironmentVariable("GENERAR_HASH") == "1")
{
    string hash =
        Backend.Security.PasswordHasher.Hash(
            "UnaClaveLargaYUnica!2026"
        );  

    Console.WriteLine("HASH GENERADO:");
    Console.WriteLine(hash);

    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<OracleConnectionFactory>();
builder.Services.AddScoped<ClienteRepository>();
builder.Services.AddSingleton<JwtTokenService>();

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Secret"] ?? throw new InvalidOperationException("Configura Jwt:Secret.");
if (Encoding.UTF8.GetByteCount(secret) < 32)
    throw new InvalidOperationException("Jwt:Secret debe tener al menos 32 bytes.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

