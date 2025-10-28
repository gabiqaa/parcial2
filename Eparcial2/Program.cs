using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Eparcial2.Data;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// CONFIGURAR BASE DE DATOS
// -------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------------
// CONFIGURAR AUTENTICACIÓN JWT
// -------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"] ?? "Secret_Password";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

// -------------------------
// CONFIGURAR CORS
// -------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// -------------------------
// SWAGGER Y CONTROLADORES
// -------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------------
// MIDDLEWARES
// -------------------------

// Habilitar Swagger SIEMPRE (en Azure también)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Parcial2 v1");
    c.RoutePrefix = "swagger"; // URL final: /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// -------------------------
// ENDPOINTS
// -------------------------
app.MapControllers();
app.MapGet("/", () => "🚀 API Parcial2 funcionando correctamente en Azure 🚀");

// -------------------------
// EJECUCIÓN
// -------------------------
app.Run();  
