
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Starlink.Api.Dtos;
using Starlink.Api.Models;
using Xunit;

namespace Starlink.Api.Tests.Integration;

public class IsolamentoClientesIntegrationTests
{
    [Fact]
    public async Task ClienteDeOutroDono_NaoPodeSerConsultado()
    {
        // Inicializa a API e o banco de testes.
        using var factory = new StarlinkApiFactory();

        var configuracao = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        };

        // Dois clientes HTTP independentes,
        // mas utilizando a mesma API e o mesmo banco.
        using var clientA = factory.CreateClient(configuracao);
        using var clientB = factory.CreateClient(configuracao);

        factory.CreateDatabase();

        // 1. Registra o proprietario A.
        var cadastroA = await clientA.PostAsJsonAsync(
            "/api/auth/registrar",
            new
            {
                Nome = "Proprietario A",
                Email = "dono.a@teste.com",
                Senha = "SenhaSegura123!"
            });

        Assert.Equal(HttpStatusCode.OK, cadastroA.StatusCode);

        var donoA = await cadastroA.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(donoA);

        // 2. Registra o proprietario B.
        var cadastroB = await clientB.PostAsJsonAsync(
            "/api/auth/registrar",
            new
            {
                Nome = "Proprietario B",
                Email = "dono.b@teste.com",
                Senha = "SenhaSegura456!"
            });

        Assert.Equal(HttpStatusCode.OK, cadastroB.StatusCode);

        var donoB = await cadastroB.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(donoB);

        // Garante que sao usuarios diferentes.
        Assert.NotEqual(donoA.DonoId, donoB.DonoId);

        // 3. Autentica os dois proprietarios.
        clientA.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", donoA.Token);

        clientB.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer", donoB.Token);

        // 4. Proprietario A cadastra um cliente.
        var respostaCadastroCliente =
            await clientA.PostAsJsonAsync(
                "/api/Clientes",
                new
                {
                    Nome = "Cliente Exclusivo do A",
                    Documento = "529.982.247-25"
                });

        Assert.Equal(
            HttpStatusCode.Created,
            respostaCadastroCliente.StatusCode);

        var clienteCriado =
            await respostaCadastroCliente.Content
                .ReadFromJsonAsync<Cliente>();

        Assert.NotNull(clienteCriado);

        Assert.Equal(
            donoA.DonoId,
            clienteCriado.DonoId);

        // 5. O proprietario A pode consultar seu cliente.
        var consultaA = await clientA.GetAsync(
            $"/api/Clientes/{clienteCriado.Id}");

        Assert.Equal(
            HttpStatusCode.OK,
            consultaA.StatusCode);

        // 6. O proprietario B tenta consultar
        // o cliente cadastrado pelo proprietario A.
        var consultaB = await clientB.GetAsync(
            $"/api/Clientes/{clienteCriado.Id}");

        // A API nao deve revelar esse registro.
        Assert.Equal(
            HttpStatusCode.NotFound,
            consultaB.StatusCode);

        // 7. O cliente de A nao deve aparecer
        // na listagem de clientes de B.
        var listagemB = await clientB.GetAsync(
            "/api/Clientes");

        Assert.Equal(
            HttpStatusCode.OK,
            listagemB.StatusCode);

        var clientesB = await listagemB.Content
            .ReadFromJsonAsync<List<Cliente>>();

        Assert.NotNull(clientesB);
        Assert.DoesNotContain(
            clientesB,
            cliente => cliente.Id == clienteCriado.Id);
    }
}
