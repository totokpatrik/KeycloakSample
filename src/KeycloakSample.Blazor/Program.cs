using KeycloakSample.Blazor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("API",
    client => client.BaseAddress = new Uri("http://localhost:50051/"))
  .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"];
    options.ProviderOptions.ClientId = builder.Configuration["Keycloak:resource"];
    options.ProviderOptions.MetadataUrl = builder.Configuration["Keycloak:auth-server-url"] + "/realms/" + builder.Configuration["Keycloak:realm"] + "/.well-known/openid-configuration";

    options.ProviderOptions.ResponseType = "id_token token";
    options.UserOptions.RoleClaim = "roles";
});

await builder.Build().RunAsync();