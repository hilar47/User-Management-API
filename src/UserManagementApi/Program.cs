using Microsoft.OpenApi.Models;
using UserManagementApi.Middleware;
using UserManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IUserService, UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "User Management API", Version = "v1" });
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "API key sent in the X-API-KEY header",
        Name = ApiKeyAuthMiddleware.HeaderName,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Order matters: exceptions -> logging -> authentication -> endpoints
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapControllers();

app.Run();

// Exposed so the integration tests can use WebApplicationFactory<Program>
public partial class Program { }
