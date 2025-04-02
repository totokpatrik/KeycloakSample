using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.MetadataAddress = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = false;
        options.Authority = $"{builder.Configuration["Keycloak:auth-server-url"]}realms/{builder.Configuration["Keycloak:realm"]}/account";
        options.Audience = $"{builder.Configuration["Keycloak:audience"]}";
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors(a => a.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

//app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();