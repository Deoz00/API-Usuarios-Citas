using api_barber.Data;
using api_barber.Mappers;
using api_barber.Model;
using api_barber.Repository;
using api_barber.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// config sql connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSql"));
});

// mapper
builder.Services.AddAutoMapper(typeof(Mapper));

// .net identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false; // Requiere un número
    options.Password.RequiredLength = 8; // Longitud mínima
    options.Password.RequireNonAlphanumeric = false; // No requiere caracteres especiales
    options.Password.RequireUppercase = false; // Requiere mayúsculas
    options.Password.RequireLowercase = false; // Requiere minúsculas

   
})
.AddEntityFrameworkStores<ApplicationDbContext>();


// repositories

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICitasRepository, CitasRepository>();


// cors

builder.Services.AddCors(p => p.AddPolicy("PolicyCors", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
        "Autenticación JWT usando el esquema Bearer. \r\n\r" +
        "Ingresa la palabra 'Bearer' seguida de un [Espacio] y despues su token en el campo de abajo \r\n\r" +
        "Ejemplo: \"Bearer SDADASDAS\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
         {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }

    });


});


// autenticacion
var key = builder.Configuration.GetValue<string>("ApiSettings:Secreta");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


// Add services to the container.

// Agregar el filtro globalmente
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true; // Disable automatic model validation
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();


var app = builder.Build();

// Configure the HTTP request pipeline.


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

// soporte cors
app.UseCors("PolicyCors");

app.UseAuthentication();

app.UseAuthorization();


app.MapControllers();

// Inicializar roles y administrador
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    var roleInitializer = new RoleInitializer(roleManager, userManager, builder.Configuration);
    await roleInitializer.InitializeRolesAsync();
}
app.Run();
