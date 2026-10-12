# StarLink Rental — Blazor + API C#

Projeto convertido do HTML/CSS enviado para uma aplicação Blazor Web App (.NET 10). O frontend tem dois modos:

- **Demonstração (padrão):** funciona sem banco e sem iniciar a API. Login, dashboard, cadastro/edição/exclusão de clientes, criação de aluguéis e vendas, pesquisa, filtros, detalhes e alteração de status usam dados fictícios em memória.
- **API real (preparado):** `ApiStarlinkService` chama os endpoints existentes do `Starlink.Api` com JWT. Só ative quando o backend e seu banco/migrations estiverem prontos.

> Não foi incluído nenhum arquivo de banco de dados. No modo demonstração, alterações são mantidas apenas durante a sessão do navegador/circuito e são reiniciadas quando a sessão é recriada. Este é um ambiente de teste, não de produção.

## Pré-requisitos

- Visual Studio 2022 atualizado com suporte ao SDK .NET 10 (ou Visual Studio compatível com .NET 10).
- Workload **ASP.NET and web development** instalado.
- Alternativa via terminal: SDK .NET 10.

## Iniciar o teste sem banco (recomendado agora)

1. Extraia o ZIP para uma pasta local.
2. Abra `StarlinkSystem.sln` no Visual Studio.
3. No Gerenciador de Soluções, clique com o botão direito em **StarlinkSystem.Blazor** e escolha **Set as Startup Project / Definir como projeto de inicialização**.
4. Confirme em `Frontend/appsettings.json` que existe `"UseMockData": true`.
5. Pressione **F5** ou **Ctrl+F5**. O navegador abrirá o frontend. O perfil local usa `https://localhost:7284` (ou `http://localhost:5284`).
6. Entre com o usuário de demonstração:
   - E-mail: `admin@starlink.local`
   - Senha: `123456`

Também pode executar no terminal, na pasta `Frontend`:

```bash
dotnet restore
dotnet run
```

Abra a URL informada no terminal. Se houver aviso de certificado HTTPS local, execute `dotnet dev-certs https --trust` em um terminal com o SDK instalado ou use o endereço HTTP mostrado pelo perfil.

## O que testar no modo demonstração

- **Dashboard:** indicadores calculados a partir dos registros simulados e acesso aos detalhes.
- **Clientes:** pesquisar, filtrar por CPF/CNPJ, cadastrar, editar e excluir (a exclusão é bloqueada se houver contrato relacionado).
- **Aluguéis:** criar contrato com cliente, valor, datas, observações e status; ver detalhes, alterar status e excluir.
- **Vendas:** registrar uma venda (sem data final de aluguel), ver detalhes, alterar status e excluir.
- **Configurações:** verificar se está em modo demonstração ou API.
- **Sair / criar conta:** os formulários de login e registro funcionam no modo demonstração. O usuário cadastrado no modo mock não é persistido em banco.

## Integrar com o backend real depois

O cliente HTTP foi escrito com base no código C# enviado e usa estas rotas:

- `POST /api/auth/login`
- `POST /api/auth/registrar`
- `GET /api/Clientes`, `GET /api/Clientes/{id}`, `POST /api/Clientes`, `PUT /api/Clientes/{id}`, `DELETE /api/Clientes/{id}`
- `GET /api/Vendas`, `GET /api/Vendas/{id}`, `POST /api/Vendas`, `PUT /api/Vendas/{id}/status?status=Ativa`, `DELETE /api/Vendas/{id}`

Quando o responsável finalizar e configurar o banco:

1. Garanta que as migrations do backend foram aplicadas ao banco.
2. Inicie `Backend/Starlink.Api.csproj` e confirme a URL em que a API está escutando (o perfil enviado usa `https://localhost:7147` e `http://localhost:5153`).
3. Edite `Frontend/appsettings.json`:

```json
{
  "UseMockData": false,
  "ApiBaseUrl": "https://localhost:7147/"
}
```

4. Se usar outra porta, ajuste `ApiBaseUrl` para a URL correta.
5. Inicie a API e o Blazor. Como o Blazor faz as chamadas HTTP pelo servidor, não é necessário configurar CORS para a chamada server-to-server local.
6. Se o certificado HTTPS local impedir a conexão, confie no certificado de desenvolvimento ou use a URL HTTP local durante o teste.

**Importante:** sem banco/tabelas e migrations aplicadas, o modo API não conseguirá autenticar ou salvar dados. Mantenha `UseMockData: true` para testar agora sem banco.

## Notas de implementação

- A API enviada tem o modelo `Venda` com `DataInicioAluguel` e `DataFimAluguel`; por isso o frontend representa aluguel quando há data final e venda quando ela é nula.
- O enum real do backend é `Ativa`, `Finalizada` e `Cancelada`. A interface usa esses valores, em vez de status que não existem no backend.
- O frontend usa Blazor Interactive Server e não precisa de JavaScript próprio para as funções entregues.
- A chave JWT não é versionada no `Backend/appsettings.json`. Configure `Jwt:Key` localmente com User Secrets ou `Jwt__Key` como variável de ambiente em hospedagem.
- O ambiente de geração não tinha o SDK `dotnet`, então não foi possível executar `dotnet build` aqui. Abra a solução com o SDK .NET 10 instalado e confira o build inicial; caso o Visual Studio indique algum erro de compilação específico, envie a mensagem para corrigirmos.


## Correção para carregamento no Visual Studio

Esta versão inclui `@using static Microsoft.AspNetCore.Components.Web.RenderMode` em `_Imports.razor`, para reconhecer `InteractiveServer`, e `@inherits LayoutComponentBase` no `MainLayout.razor`, para reconhecer `@Body`. Se o Visual Studio mantiver erros antigos, use **Compilar > Limpar Solução** e depois **Compilar > Recompilar Solução**.

## Preparação para desenvolvimento em equipe

O repositório não versiona banco SQLite local, saídas de compilação nem segredos.

### Configurar a chave JWT no Windows

No terminal, dentro da pasta `Backend`:

```powershell
dotnet user-secrets set "Jwt:Key" "COLOQUE_AQUI_UMA_CHAVE_LOCAL_FORTE_E_UNICA"
```

O valor fica fora do repositório. Em ambientes de hospedagem, configure a variável de ambiente `Jwt__Key`.

### Restaurar e executar

Backend:

```powershell
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

Frontend, em outro terminal:

```powershell
cd Frontend
dotnet restore
dotnet run
```

Para usar a API real, altere `Frontend/appsettings.json` para `"UseMockData": false` e confira `ApiBaseUrl`.

## Fluxo Git da equipe

Branches permanentes:

- `main`: versão estável.
- `develop`: integração do trabalho da equipe.

Branches de trabalho:

- `feat/nome-da-feature`
- `fix/nome-do-ajuste`
- `chore/nome-da-tarefa`

Depois da configuração inicial do repositório, mudanças devem entrar em `develop` por Pull Request. A promoção para `main` também deve ocorrer por Pull Request.

## Integração contínua

O workflow `.github/workflows/dotnet-ci.yml` roda em Pull Requests para `develop` e `main` e em pushes nessas branches. Ele restaura as dependências e compila toda a solução .NET 10 no modo Release.

**Limitação atual:** a solução ainda não contém projeto de testes automatizados. A aprovação na CI comprova a compilação, não o funcionamento das regras de negócio. O próximo avanço recomendado é criar testes de API e adicioná-los ao workflow.
