
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Starlink.Api.Dtos;
using Xunit;

namespace Starlink.Api.Tests.Integration;

public class AuthIntegrationTests
{
    // TESTES 1 E 2
    // Endpoints protegidos devem retornar 401
    // quando nenhuma autenticacao for fornecida.

    [Theory]
    [InlineData("/api/Clientes")]
    [InlineData("/api/Vendas")]
    public async Task RecursosProtegidos_SemToken_Retornam401(
        string rota)
    {
        using var factory = new StarlinkApiFactory();

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        // Requisicao HTTP sem token JWT.
        var response = await client.GetAsync(rota);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // TESTE 3
    // Cadastro de usuario deve gerar um JWT.

    [Fact]
    public async Task RegistrarUsuarioValido_DeveRetornarToken()
    {
        using var factory = new StarlinkApiFactory();

        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        // Inicializa o banco SQLite em memoria.
        factory.CreateDatabase();

        // Dados ficticios utilizados somente neste teste.
        var usuario = new
        {
            Nome = "Pessoa Teste",
            Email = "TESTE@EXEMPLO.COM",
            Senha = "SenhaTeste123!"
        };

        // Envia os dados ao endpoint de registro.
        var response = await client.PostAsJsonAsync(
            "/api/auth/registrar",
            usuario);

        // O AuthController retorna HTTP 200.
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        // Desserializa a resposta JSON.
        var resultado = await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(resultado);

        // Verifica se um token JWT foi gerado.
        Assert.False(
            string.IsNullOrWhiteSpace(resultado.Token));

        // Verifica a normalizacao do email.
        Assert.Equal(
            "teste@exemplo.com",
            resultado.Email);

        // Confirma a identificacao do usuario.
        Assert.True(resultado.DonoId > 0);
    }
    // TESTE 4
    // Um usuario registrado deve conseguir fazer login.
    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarToken()
    {
        using var factory = new StarlinkApiFactory();
        using var client = factory.CreateClient();

        factory.CreateDatabase();

        var usuario = new
        {
            Nome = "Usuario Teste",
            Email = "usuario@teste.com",
            Senha = "SenhaSegura123!"
        };

        var cadastro = await client.PostAsJsonAsync(
            "/api/auth/registrar", usuario);

        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = "usuario@teste.com",
                Senha = "SenhaSegura123!"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var resultado = await login.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(resultado);
        Assert.False(string.IsNullOrWhiteSpace(resultado.Token));
        Assert.Equal("usuario@teste.com", resultado.Email);
    }

    // TESTE 5
    // Uma senha incorreta deve impedir o login.

    [Fact]
    public async Task Login_ComSenhaIncorreta_DeveRetornar401()
    {
        using var factory = new StarlinkApiFactory();
        using var client = factory.CreateClient();

        factory.CreateDatabase();

        var usuario = new
        {
            Nome = "Usuario Teste",
            Email = "usuario@teste.com",
            Senha = "SenhaSegura123!"
        };

        var cadastro = await client.PostAsJsonAsync(
            "/api/auth/registrar", usuario);

        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = "usuario@teste.com",
                Senha = "SenhaIncorreta!"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            login.StatusCode);
    }

    // TESTE 6
    // Um usuario autenticado deve conseguir
    // acessar os proprios clientes.

    [Fact]
    public async Task Clientes_ComTokenValido_DeveRetornar200()
    {
        using var factory = new StarlinkApiFactory();
        using var client = factory.CreateClient();

        factory.CreateDatabase();

        var usuario = new
        {
            Nome = "Usuario Teste",
            Email = "usuario@teste.com",
            Senha = "SenhaSegura123!"
        };

        var cadastro = await client.PostAsJsonAsync(
            "/api/auth/registrar", usuario);

        Assert.Equal(HttpStatusCode.OK, cadastro.StatusCode);

        var resultado = await cadastro.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(resultado);

        // Adiciona o JWT no cabecalho Authorization.
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                resultado.Token);

        var response = await client.GetAsync("/api/Clientes");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

}
