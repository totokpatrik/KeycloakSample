using Microsoft.AspNetCore.Antiforgery;
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
    private readonly IAntiforgery _antiforgery;

    public HttpService(
        HttpClient httpClient,
        NavigationManager navigationManager,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IAntiforgery antiforgery
    )
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _antiforgery = antiforgery;
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
        Console.WriteLine("Token: " + accessToken);

        using var response = await _httpClient.SendAsync(request);

        // auto logout on 401 response
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
            _navigationManager.NavigateTo($"/authentication/logout?returnUrl={Uri.EscapeDataString(returnUrl)}", true);
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
}
