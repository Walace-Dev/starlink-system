using StarlinkSystem.Blazor.Models;

namespace StarlinkSystem.Blazor.Services;

public interface IStarlinkService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegistroRequest request);
    Task<List<ClienteDto>> GetClientesAsync();
    Task<ClienteDto?> GetClienteAsync(int id);
    Task<ClienteDto> SaveClienteAsync(ClienteDto cliente);
    Task DeleteClienteAsync(int id);
    Task<List<VendaDto>> GetVendasAsync();
    Task<VendaDto> CreateVendaAsync(VendaDto venda);
    Task UpdateStatusAsync(int id, StatusVenda status);
    Task DeleteVendaAsync(int id);
}
