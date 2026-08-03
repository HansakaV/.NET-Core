using StudentManagement.API.Data;
using Microsoft.EntityFrameworkCore;
using StudentManagement.API.Interfaces;
using StudentManagement.API.Repositories;
using StudentManagement.API.Services;
using Serilog;
using FluentValidation;
using StudentManagement.API.Validators.Students;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using StudentManagement.API.ExceptionHandlers;
using StudentManagement.API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Tree;
using StudentManagement.API.Validators.auth;
using Microsoft.Extensions.Options;
using Serilog.Events;
using Serilog.Extensions.Hosting;
using System.Security.Claims;

try
{
Log.Logger = new LoggerConfiguration()  
        .WriteTo.Console()
        .CreateBootstrapLogger();
    
Log.Information("Starting Student Management API");

var builder = WebApplication.CreateBuilder(args);

var jwtSecret = builder.Configuration["JwtSettings:TokenSecret"] ?? throw new InvalidOperationException("Secret Not Found");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

// Add services to the container.
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

//logger configuration
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();

});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

//validators Registration
builder.Services.AddValidatorsFromAssemblyContaining<StudentCreateRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<StudentUpdateRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ForgetPasswordRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

//Databsae Context
builder.Services.AddDbContext<AppDBContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
    }
});
        
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpContextAccessor();

//DI Registration
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMessagePublisher, MessagePublisher>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

//Background Worker
builder.Services.AddHostedService<OutBoxBackgroundService>();

//JWT Authentication Cofiguration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(Options =>
{
    Options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = key,

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

//Api Behavior options Validation
builder.Services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = 
            context =>
            {
                var errors = context.ModelState
                    .Where(entry =>
                        entry.Value != null && entry.Value?.Errors.Count > 0 )
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors    
                            .Select(error =>
                                string.IsNullOrWhiteSpace(error.ErrorMessage)
                                    ? "The Enterd value Is Invalid"
                                    : error.ErrorMessage)
                            .ToArray());
                
                var problemDetails = new ValidationProblemDetails(errors)
                {
                        Type =
                            "https://api.studentmanagement.com/errors/validation-failed",

                        Title =
                            "Validation failed",

                        Status =
                            StatusCodes.Status400BadRequest,

                        Detail =
                            "One or more validation errors occurred.",

                        Instance =
                            context.HttpContext.Request.Path
                };
                problemDetails.Extensions["errorCode"] = "VALIDATION_FAILED";
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                if(context.HttpContext.Items.TryGetValue("X-Correlation-ID", out var correlctionId) && correlctionId is string id)
                {
                    problemDetails.Extensions["correlationId"] = id;
                }

                return new BadRequestObjectResult(problemDetails); 
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
app.UseMiddleware<CorellectionIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (context, elapsed, exception) =>
    {
        if(exception is not null || context.Response.StatusCode >= 500)
            return LogEventLevel.Warning;
        
        if(context.Response.StatusCode >= 400)
            return LogEventLevel.Warning;
        
        return LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, context) =>
    {
        diagnosticContext.Set("TraceId", context.TraceIdentifier);
        if(context.Items.TryGetValue("X-Correlation-ID", out var correlationId) && correlationId is string id)
        {
            diagnosticContext.Set("CorreletionId", id);
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Student Management API Terminated Unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
