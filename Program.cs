using API.Data;
using API.DTOs;
using API.DTOs.Mapping;
using API.Filters;
using API.Models;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Services;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Asp.Versioning;
using System.Reflection;

//using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "Companies", Version = "v1" });

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "CompaniesManagement",
        Description = "An API for companies management",
        TermsOfService = new Uri("https://localhost/termsOfServices"),
        License = new OpenApiLicense
        {
            Name = "License",
            Url = new Uri("https://localhost/license")
        },
        Contact = new OpenApiContact
        {
            Name = "Rafael",
            Email = "rafael@gmail.com",
            Url = new Uri("https://localhost/contact")
        }
    });

    string file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, file));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \\r\\n\\r\\n Enter 'Bearer'[space] and then your token in the text input below.\r\n\\r\\n\\r\\nExample: \\\"Bearer 12345abcdef\\\"\""
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

 // Controllers
builder.Services.AddControllers(options => options.Filters.Add(typeof(ExceptionFilter))).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Repositories

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// AutoMapper

builder.Services.AddAutoMapper(typeof(MappingProfile));

// Identity

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AplicationContext>()
    .AddDefaultTokenProviders();

// Connection

string? mySqlStringConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AplicationContext>(options => options.UseMySql(mySqlStringConnection, ServerVersion.AutoDetect(mySqlStringConnection)));

// Authentication

var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("The secret key can't be null");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddScoped<ITokenService, TokenService>();

// Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminMasterOnly", policy => policy.RequireRole("admin").RequireClaim("id", "rafael"));
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
    options.AddPolicy("Employee", policy => policy.RequireRole("employee"));

});

// CORS

builder.Services.AddCors(options => 
                          options.AddPolicy("AllowedOrigins",
                                             policy => policy.WithOrigins("https://wwww.apirequest.io")));

// Rate Limiting

builder.Services.AddRateLimiter(rateOptions =>
{
    rateOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    rateOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(), factory => 
    new FixedWindowRateLimiterOptions
    {
        AutoReplenishment = true,
        PermitLimit = 2,
        QueueLimit = 0,
        Window = TimeSpan.FromSeconds(10)
    }
    ));

    /*
    rateOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 1;
        options.Window = TimeSpan.FromSeconds(5);
        options.QueueLimit = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
     
     */
});


// Versionamento

builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.ReportApiVersions = true;
    setupAction.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader()
        );
}).AddApiExplorer(setupAction =>
{
    setupAction.GroupNameFormat = "'v'VVV";
    setupAction.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.UseRateLimiter();

app.Run();
