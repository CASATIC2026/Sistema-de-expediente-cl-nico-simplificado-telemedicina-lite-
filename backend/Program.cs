using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TelMedAPI.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using QuestPDF.Infrastructure;
using TelMedAPI.Helpers;
using TelMedAPI.Models;
using TelMedAPI.Services;

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.EnableDebugging = true;

var builder = WebApplication.CreateBuilder(args);

// Controllers y Validadores
builder.Services.AddControllers()
.AddJsonOptions(options=>
    { 
        // Evita ciclos JSON
        options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
//Eso registra todos los validators del proyecto sin importar el nombre
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "TelMedAPI",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingrese el token así: Bearer {su token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// DbContext PostgreSQL
builder.Services.AddDbContext<TelMedAPIContext>(options =>
      options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS
builder.Services.AddCors(options =>
{ // Cambiar configuración por las URLs específicas de frontend en producción
    options.AddPolicy("PublicPolicy", policy =>
        {
            policy.AllowAnyOrigin() // Permite cualquier URL
                  .AllowAnyHeader() // Permite headers como 'Authorization' o 'Content-Type'
                  .AllowAnyMethod(); // Permite GET, POST, PUT, DELETE, etc.
        });
});

// JWT Authentication
var keyString = builder.Configuration["Jwt:Key"]
    ?? throw new ArgumentNullException("Jwt:Key no está configurado en appsettings.json");
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Authorization
builder.Services.AddAuthorization();

// Servicio de Email (verificación de cuenta y restablecimiento de contraseña)
builder.Services.AddScoped<EmailService>();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

var app = builder.Build();
app.UseRouting();

// Middleware
app.UseCors("PublicPolicy");
app.UseAuthentication();
app.UseMiddleware<ForcePasswordChangeMiddleware>();
app.UseAuthorization();

//Swagger siempre activo
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Scope para crear admin maestro si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TelMedAPIContext>();
    
    db.Database.Migrate();//Aplica migraciones pendientes al iniciar la app
    
    if (!db.Usuarios.Any(u => u.Rol == Roles.Admin))
    { //Crea un admin maestro si no existe, con cambio obligatorio de password
        var admin = new Usuario
        {
            Nombre = "Admin",
            Apellido = "Maestro",
            Email = "admin@telmed.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cambiar123!"),
            Rol = Roles.Admin,
            FechaNacimiento = new DateOnly(1970, 1, 1),
            Genero = "N/A",
            Direccion = "Sistema",
            Telefono = "+503 00000000",
            DebeCambiarPassword = true,
            EmailVerified = true
        };

        db.Usuarios.Add(admin);
        db.SaveChanges();

        Console.WriteLine("Admin Maestro creado con cambio obligatorio de password.");
    }
}

//Llama al seeder
using(var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TelMedAPIContext>();
    //DbSeeder.Seed(context);
}

app.Run();