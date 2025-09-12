using System.Text;
using MetalfluxApi.Server;
using MetalfluxApi.Server.Authentication.Service;
using MetalfluxApi.Server.Core.Middleware;
using MetalfluxApi.Server.Modules.Media;
using MetalfluxApi.Server.Modules.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

const string corsPolicyName = "AllowOrigins";

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration["Jwt:Secret"] == null)
    throw new ArgumentNullException(
        nameof(builder.Configuration),
        "Jwt:Secret configuration is null"
    );

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IMediaRepository, MediaRepository>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddSingleton<TokenProvider>();

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var allowedOrigins = builder.Configuration["AllowedOrigins"];
if (allowedOrigins != null)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            name: corsPolicyName,
            policy =>
            {
                policy
                    .WithOrigins(allowedOrigins.Split(";"))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        );
    });
}

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.RequireHttpsMetadata = false;
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (ctx.Request.Cookies.ContainsKey(builder.Configuration["Jwt:TokenCookieName"]!))
                    ctx.Token = ctx.Request.Cookies[builder.Configuration["Jwt:TokenCookieName"]!];

                return Task.CompletedTask;
            },
        };
        o.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            ),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UsePathBase("/api/v1");
app.UseStaticFiles();

app.UseMiddleware<ExceptionHandlingMiddleware>();

/*
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
*/

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors(corsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
