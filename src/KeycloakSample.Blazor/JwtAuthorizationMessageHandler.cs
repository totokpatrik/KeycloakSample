using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace KeycloakSample.Blazor
{
    public class JwtAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public JwtAuthorizationMessageHandler(IAccessTokenProvider provider,
          NavigationManager navigation)
          : base(provider, navigation)
        {
            ConfigureHandler(authorizedUrls: new[] { "http://localhost:50051/" });
        }
    }
}