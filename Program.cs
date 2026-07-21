using StudentManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Repositories;
using StudentManagement.API.Services;
using StudentManagement.API.Middlewares;
using Serilog;
using FluentValidation;
using StudentManagement.API.Validators.Students;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection.Metadata;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JwtSettings:TokenSecret"] ?? throw new InvalidOperationException("Secret Not Found");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Add services to the container.
builder.Host.UseSerilog();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var securitySchema = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description ="Enter Valid JWT Token"
        };
        document.Components ??=new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", securitySchema);

        var secuirtyRequirements = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }   
        };
        document.SecurityRequirements = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference {Type = ReferenceType.SecurityScheme, Id ="Bearer"}}] = new List<string>()
            }
        };
        return Task.CompletedTask;
    });  
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<StudentCreateRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddDbContext<AppDBContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(Program));

//DI Registration
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

//JWT Authentication Cofiguration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(Options =>
{
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateIssuer = true,
        ValidIssuer = "StudentManagement.API",

        ValidateAudience = true,
        ValidAudience = "StudentManagement.Client",

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add Memory Cache
builder.Services.AddMemoryCache();

var app = builder.Build();

//HTTP Request Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Student Management API V1");
        options.RoutePrefix= "swagger";
        options.EnablePersistAuthorization();
    }
    );
}

//Middlewares
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
