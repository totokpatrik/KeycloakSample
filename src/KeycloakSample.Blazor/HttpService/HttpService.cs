using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KeycloakSample.Blazor.HttpService;

public class HttpService : IHttpService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpService(
        HttpClient httpClient,
        NavigationManager navigationManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<T> Delete<T>(string uri, object value)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, uri);
        request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        return await sendRequest<T>(request);
    }

    public async Task<T> Get<T>(string uri)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        return await sendRequest<T>(request);
    }

    public async Task<T> Post<T>(string uri, object value)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        return await sendRequest<T>(request);
    }

    public async Task<T> Put<T>(string uri, object value)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
        return await sendRequest<T>(request);
    }

    // helper methods

    private async Task<T> sendRequest<T>(HttpRequestMessage request)
    {
        // add jwt auth header if user is logged in and request is to the api url
        var accessToken = await _httpContextAccessor.HttpContext!.GetTokenAsync("access_token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request);

        // auto logout on 401 response
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await _httpContextAccessor.HttpContext!.SignOutAsync();
            return default!;
        }

        // throw exception on error response
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            //var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            //throw new Exception(error!["message"]);
        }

        var result = await response.Content.ReadFromJsonAsync<T>();

        return result!;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}
