


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;


using System.Text;

using System;
using Project.Business.Infrastructure.Repository;
using Project.Business.Infrastructure;
using Project.Data.Entity;
using Project.Data.Data;
using Project.Business.Service.Email;
using Project.Business.Service;
using Project.Business.Service.Events;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Project.Business.Service.Auth;
using Project.Business.Service.Report;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Demo API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[] { }
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("https://localhost:7104")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add DbContext
builder.Services.AddDbContext<ProjectDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")) );
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<RoleRepository>();
builder.Services.AddTransient<UserRoleRepository>();
builder.Services.AddTransient<UserRepository>();
builder.Services.AddTransient<AuditTrailRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<CandidateRepository>();
builder.Services.AddTransient<EventRepository>();
builder.Services.AddTransient<RemarkHistoryRepository>();
builder.Services.AddTransient<SourcingHistoryRepository>();
builder.Services.AddTransient<UpdateHistoryRepository>();

builder.Services.AddScoped<ICandidateService, CandidateServices>();

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IEventService, EventServices>();
builder.Services.AddScoped<IReportService , ReportService>();

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBaseRepository<User>, UserRepository>();

builder.Services.AddIdentity<User, Role>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ProjectDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JWT");
    var secretKey = jwtSettings.GetValue<string>("Secret");
    var validIssuer = jwtSettings.GetValue<string>("ValidIssuer");
    var validAudience = jwtSettings.GetValue<string>("ValidAudience");

    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = validIssuer,
        ValidAudience = validAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireLoggedIn", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuizKuber Web API v1");
        options.RoutePrefix = string.Empty;
    });
   
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
