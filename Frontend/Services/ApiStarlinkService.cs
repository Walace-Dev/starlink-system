using System.Net.Http.Headers;
using System.Net.Http.Json;
using StarlinkSystem.Blazor.Models;

namespace StarlinkSystem.Blazor.Services;

/// <summary>Cliente HTTP para os endpoints reais do backend Starlink.Api.</summary>
public sealed class ApiStarlinkService(IHttpClientFactory factory, AuthSession session) : IStarlinkService
{
    private HttpClient Client => factory.CreateClient("StarlinkApi");

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var result = await SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/login", request, false);
        session.SignIn(result.Nome, result.Email, result.Token, result.DonoId);
        return result;
    }

    public async Task<LoginResponse> RegisterAsync(RegistroRequest request)
    {
        var result = await SendAsync<LoginResponse>(HttpMethod.Post, "api/auth/registrar", request, false);
        session.SignIn(result.Nome, result.Email, result.Token, result.DonoId);
        return result;
    }

    public async Task<List<ClienteDto>> GetClientesAsync() => await SendAsync<List<ClienteDto>>(HttpMethod.Get, "api/Clientes") ?? [];
    public Task<ClienteDto?> GetClienteAsync(int id) => SendAsync<ClienteDto?>(HttpMethod.Get, $"api/Clientes/{id}");

    public async Task<ClienteDto> SaveClienteAsync(ClienteDto cliente)
    {
        if (cliente.Id == 0)
            return (await SendAsync<ClienteDto>(HttpMethod.Post, "api/Clientes", cliente))!;
        await SendAsync<object?>(HttpMethod.Put, $"api/Clientes/{cliente.Id}", cliente);
        return (await GetClienteAsync(cliente.Id)) ?? cliente;
    }

    public async Task DeleteClienteAsync(int id) => await SendAsync<object?>(HttpMethod.Delete, $"api/Clientes/{id}");
    public async Task<List<VendaDto>> GetVendasAsync() => await SendAsync<List<VendaDto>>(HttpMethod.Get, "api/Vendas") ?? [];
    public async Task<VendaDto> CreateVendaAsync(VendaDto venda) => (await SendAsync<VendaDto>(HttpMethod.Post, "api/Vendas", venda))!;
    public async Task UpdateStatusAsync(int id, StatusVenda status) => await SendAsync<object?>(HttpMethod.Put, $"api/Vendas/{id}/status?status={status}");
    public async Task DeleteVendaAsync(int id) => await SendAsync<object?>(HttpMethod.Delete, $"api/Vendas/{id}");

    private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body = null, bool authenticated = true)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null) request.Content = JsonContent.Create(body);
        if (authenticated && !string.IsNullOrWhiteSpace(session.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        HttpResponseMessage response;
        try { response = await Client.SendAsync(request); }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException("Não foi possível conectar à API. Confira se o backend está em execução e se ApiBaseUrl está correto. Detalhe: " + ex.Message);
        }
        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new InvalidOperationException("Acesso não autorizado. Faça login novamente. " + detail);
                throw new InvalidOperationException($"A API retornou {(int)response.StatusCode} ({response.ReasonPhrase}). {detail}");
            }
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent || response.Content.Headers.ContentLength == 0)
                return default;
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}
