using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskHub_V1.Data;
using TaskHub_V1.Interfaces;
using TaskHub_V1.Models;
using TaskHub_V1.Repository;
using TaskHub_V1.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "TaskHub API",
        Description = "A task management web application API",
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter the token into the field as 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
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
            new string[] {}
        }
    });
});
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
// For Identity
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    // Password policy
    opt.Password.RequiredLength = 8;  // Increased minimum required length to 8 characters
    opt.Password.RequireDigit = true;  // Now requires at least one digit
    opt.Password.RequireNonAlphanumeric = true;  // Require at least one special character
    opt.Password.RequireUppercase = true;  // Now requires at least one uppercase letter

    // User requirements
    opt.User.RequireUniqueEmail = true;

    // Sign-in requirements
    opt.SignIn.RequireConfirmedEmail = true;
    opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultEmailProvider;
})
.AddEntityFrameworkStores<DataContext>()
.AddDefaultTokenProviders();
// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        // ValidAudience = "your_development_audience",
        // ValidIssuer = "your_development_issuer",
        // IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_development_secret"))
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});
// Add Email Options Configuration
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));

// Add EmailService
builder.Services.AddTransient<EmailService>();
// Add SmtpClient as a scoped service
builder.Services.AddScoped<SmtpClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskHub API V1");
        c.OAuthClientId("swagger-ui");
        c.OAuthClientSecret("swagger-ui-secret");
        c.OAuthUsePkce();
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
