using System.Text;
using SpeckitDemo.Application.Abstractions;
using SpeckitDemo.Application.Validation;
using SpeckitDemo.Application.Workflows;
using SpeckitDemo.Data;
using SpeckitDemo.DataAccess.Abstractions;
using SpeckitDemo.DataAccess.Stores;
using SpeckitDemo.Security.Abstractions;
using SpeckitDemo.Security.Passwords;
using SpeckitDemo.Security.Tokens;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace SpeckitDemo.Api.Hosting;

public static class ServiceRegistration
{
    public static WebApplicationBuilder AddSpeckitDemoServices(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "o";
        });

        builder.Services.AddDbContext<SpeckitDemoContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        var signingKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key configuration is required.");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc();

        builder.Services.AddControllers();
        builder.Services.AddValidatorsFromAssemblyContaining<CredentialRules>();

        builder.Services.AddSingleton<ISecretHasher, SecretHasher>();
        builder.Services.AddSingleton<ITokenIssuer, TokenIssuer>();
        builder.Services.AddScoped<IAccountReader, AccountReader>();
        builder.Services.AddScoped<ISignInWorkflow, SignInWorkflow>();
        builder.Services.AddScoped<IAccountLookup, AccountLookup>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "SpeckitDemo Access",
                Version = "v1",
                Description = "Sign-in and profile API for the SpeckitDemo access service."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste the bearer token returned by the sign-in endpoint."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            var xmlName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlName);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });

        return builder;
    }
}
