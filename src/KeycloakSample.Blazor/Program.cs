using KeycloakSample.Blazor;
using KeycloakSample.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Namespace;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("API",
    client => client.BaseAddress = new Uri("https://localhost:8081/"))
  .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    options.ProviderOptions.Authority = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"];
    options.ProviderOptions.ClientId = builder.Configuration["Keycloak:resource"];
    options.ProviderOptions.MetadataUrl = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"] + "/.well-known/openid-configuration";

    options.ProviderOptions.ResponseType = "id_token token";
    options.UserOptions.RoleClaim = "roles";

});
builder.Services.AddApiAuthorization().AddAccountClaimsPrincipalFactory<CustomUserFactory>();

await builder.Build().RunAsync();

namespace Namespace
{
    public class JwtAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public JwtAuthorizationMessageHandler(IAccessTokenProvider provider,
          NavigationManager navigation)
          : base(provider, navigation)
        {
            ConfigureHandler(authorizedUrls: new[] { "http://localhost:8091/" });
        }
    }
}