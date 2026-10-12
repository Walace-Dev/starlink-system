using StarlinkSystem.Blazor.Models;

namespace StarlinkSystem.Blazor.Services;

/// <summary>Dados de demonstração em memória. Nada é gravado em banco de dados.</summary>
public sealed class MockStore
{
    public sealed record UsuarioDemo(string Nome, string Email, string Senha, int Id);

    public List<UsuarioDemo> Usuarios { get; } =
    [ new("Administrador Demo", "admin@starlink.local", "123456", 1) ];
    public List<ClienteDto> Clientes { get; } =
    [
        new() { Id = 1, Nome = "João da Silva", Documento = "12345678909", Email = "joao@email.com", Telefone = "(69) 99999-1111", Endereco = "Porto Velho - RO", DataCadastro = DateTime.Today.AddDays(-30) },
        new() { Id = 2, Nome = "Maria Santos", Documento = "52998224725", Email = "maria@email.com", Telefone = "(69) 99999-2222", Endereco = "Ariquemes - RO", DataCadastro = DateTime.Today.AddDays(-18) },
        new() { Id = 3, Nome = "Empresa Norte Ltda.", Documento = "11222333000181", Email = "contato@empresanorte.com", Telefone = "(69) 3222-3333", Endereco = "Porto Velho - RO", DataCadastro = DateTime.Today.AddDays(-8) }
    ];
    public List<VendaDto> Vendas { get; } =
    [
        new() { Id = 124, ClienteId = 1, DataInicioAluguel = DateTime.Today.AddDays(-4), DataFimAluguel = DateTime.Today.AddDays(27), Valor = 250m, Status = StatusVenda.Ativa, DataVenda = DateTime.Today.AddDays(-4), Observacao = "Kit Starlink padrão", Cliente = null },
        new() { Id = 123, ClienteId = 2, DataInicioAluguel = DateTime.Today.AddDays(-14), DataFimAluguel = DateTime.Today.AddDays(2), Valor = 300m, Status = StatusVenda.Ativa, DataVenda = DateTime.Today.AddDays(-14), Observacao = "Aluguel mensal" },
        new() { Id = 122, ClienteId = 1, DataInicioAluguel = DateTime.Today.AddDays(-45), DataFimAluguel = DateTime.Today.AddDays(-15), Valor = 200m, Status = StatusVenda.Finalizada, DataVenda = DateTime.Today.AddDays(-45), Observacao = "Contrato finalizado" },
        new() { Id = 121, ClienteId = 3, DataInicioAluguel = DateTime.Today.AddDays(-2), DataFimAluguel = null, Valor = 2400m, Status = StatusVenda.Ativa, DataVenda = DateTime.Today.AddDays(-2), Observacao = "Venda de equipamento" }
    ];
    public int ProximoClienteId => Clientes.Count == 0 ? 1 : Clientes.Max(c => c.Id) + 1;
    public int ProximaVendaId => Vendas.Count == 0 ? 1 : Vendas.Max(v => v.Id) + 1;
}

public sealed class MockStarlinkService(MockStore store, AuthSession session) : IStarlinkService
{
    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = store.Usuarios.FirstOrDefault(u => u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase) && u.Senha == request.Senha);
        if (user is null)
            throw new InvalidOperationException("E-mail ou senha inválidos. Para o modo demonstração, use admin@starlink.local / 123456.");
        session.SignIn(user.Nome, user.Email, "DEMO-TOKEN", user.Id, true);
        return Task.FromResult(new LoginResponse { Nome = user.Nome, Email = user.Email, DonoId = user.Id, Token = "DEMO-TOKEN", ExpiraEm = DateTime.UtcNow.AddHours(2) });
    }

    public Task<LoginResponse> RegisterAsync(RegistroRequest request)
    {
        if (store.Usuarios.Any(u => u.Email.Equals(request.Email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Já existe um usuário de demonstração com esse e-mail.");
        var id = store.Usuarios.Count == 0 ? 1 : store.Usuarios.Max(u => u.Id) + 1;
        store.Usuarios.Add(new MockStore.UsuarioDemo(request.Nome.Trim(), request.Email.Trim(), request.Senha, id));
        session.SignIn(request.Nome.Trim(), request.Email.Trim(), "DEMO-TOKEN", id, true);
        return Task.FromResult(new LoginResponse { Nome = request.Nome.Trim(), Email = request.Email.Trim(), DonoId = id, Token = "DEMO-TOKEN", ExpiraEm = DateTime.UtcNow.AddHours(2) });
    }

    public Task<List<ClienteDto>> GetClientesAsync() => Task.FromResult(store.Clientes.OrderBy(c => c.Nome).Select(Clone).ToList());
    public Task<ClienteDto?> GetClienteAsync(int id) => Task.FromResult(store.Clientes.FirstOrDefault(c => c.Id == id) is { } c ? Clone(c) : null);

    public Task<ClienteDto> SaveClienteAsync(ClienteDto cliente)
    {
        var documento = OnlyDigits(cliente.Documento);
        if (!DocumentoValidator.IsValid(documento)) throw new InvalidOperationException("Informe um CPF ou CNPJ válido.");
        if (store.Clientes.Any(c => c.Id != cliente.Id && OnlyDigits(c.Documento) == documento))
            throw new InvalidOperationException("Já existe um cliente cadastrado com esse CPF/CNPJ.");
        if (cliente.Id == 0)
        {
            cliente.Id = store.ProximoClienteId;
            cliente.DataCadastro = DateTime.UtcNow;
            store.Clientes.Add(Clone(cliente));
        }
        else
        {
            var existente = store.Clientes.FirstOrDefault(c => c.Id == cliente.Id) ?? throw new InvalidOperationException("Cliente não encontrado.");
            existente.Nome = cliente.Nome.Trim(); existente.Documento = documento; existente.Email = cliente.Email;
            existente.Telefone = cliente.Telefone; existente.Endereco = cliente.Endereco;
        }
        return Task.FromResult(Clone(store.Clientes.First(c => c.Id == cliente.Id)));
    }

    public Task DeleteClienteAsync(int id)
    {
        if (store.Vendas.Any(v => v.ClienteId == id)) throw new InvalidOperationException("Não é possível excluir um cliente que possui contratos registrados.");
        var cliente = store.Clientes.FirstOrDefault(c => c.Id == id) ?? throw new InvalidOperationException("Cliente não encontrado.");
        store.Clientes.Remove(cliente);
        return Task.CompletedTask;
    }

    public Task<List<VendaDto>> GetVendasAsync()
    {
        var result = store.Vendas.OrderByDescending(v => v.DataVenda).Select(CloneVenda).ToList();
        foreach (var venda in result) venda.Cliente = store.Clientes.FirstOrDefault(c => c.Id == venda.ClienteId) is { } c ? Clone(c) : null;
        return Task.FromResult(result);
    }

    public Task<VendaDto> CreateVendaAsync(VendaDto venda)
    {
        if (!store.Clientes.Any(c => c.Id == venda.ClienteId)) throw new InvalidOperationException("Selecione um cliente existente.");
        if (venda.DataFimAluguel.HasValue && venda.DataFimAluguel.Value.Date < venda.DataInicioAluguel.Date)
            throw new InvalidOperationException("A data de fim não pode ser anterior à data de início.");
        venda.Id = store.ProximaVendaId;
        venda.DataVenda = DateTime.UtcNow;
        venda.Cliente = null;
        store.Vendas.Add(CloneVenda(venda));
        return Task.FromResult(CloneVenda(venda));
    }

    public Task UpdateStatusAsync(int id, StatusVenda status)
    {
        var venda = store.Vendas.FirstOrDefault(v => v.Id == id) ?? throw new InvalidOperationException("Registro não encontrado.");
        venda.Status = status;
        return Task.CompletedTask;
    }

    public Task DeleteVendaAsync(int id)
    {
        var venda = store.Vendas.FirstOrDefault(v => v.Id == id) ?? throw new InvalidOperationException("Registro não encontrado.");
        store.Vendas.Remove(venda);
        return Task.CompletedTask;
    }

    private static ClienteDto Clone(ClienteDto c) => new() { Id = c.Id, Nome = c.Nome, Documento = c.Documento, Email = c.Email, Telefone = c.Telefone, Endereco = c.Endereco, DataCadastro = c.DataCadastro };
    private static VendaDto CloneVenda(VendaDto v) => new() { Id = v.Id, ClienteId = v.ClienteId, DataInicioAluguel = v.DataInicioAluguel, DataFimAluguel = v.DataFimAluguel, Valor = v.Valor, Status = v.Status, DataVenda = v.DataVenda, Observacao = v.Observacao };
    private static string OnlyDigits(string? value) => new((value ?? "").Where(char.IsDigit).ToArray());
}
