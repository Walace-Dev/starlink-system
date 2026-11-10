# Starlink System — fluxo Git e publicação

## 1. Estratégia de branches

- `feat/*`, `fix/*`, `chore/*`: desenvolvimento isolado.
- `develop`: integração e homologação. Pode evoluir sem alterar o site de produção, desde que produção não faça deploy desta branch.
- `main`: versão estável aprovada para produção.

Ciclo recomendado:

```text
feat/* → Pull Request → develop → testes/homologação → Pull Request → main → deploy de produção
```

Nenhum merge é feito automaticamente por esta documentação. Exigir build, testes e revisão conforme o ruleset do GitHub.

## 2. Podemos alterar develop enquanto o site estiver no ar?

**Sim.** As mudanças na `develop` não precisam afetar a produção. Depois de testar, abrir PR de `develop` para `main`. O serviço de produção deve observar apenas `main` ou um release/tag aprovado.

Se a plataforma de deploy estiver conectada à `develop` para produção, todo push/merge poderá colocar mudanças no ar sem a homologação esperada. Conferir a configuração do projeto de hospedagem.

## 3. Cuidado especial com o banco

Migrations em produção exigem:
1. Backup consistente verificado.
2. Revisão do impacto no esquema e nos dados existentes.
3. Janela de atualização e plano de recuperação.
4. Execução controlada da migration.
5. Publicação da nova versão da API e validação dos fluxos.

Nunca copiar o `starlink.db` local de testes por cima de uma base produtiva.

## 4. Infraestrutura futura

A versão atual utiliza Blazor Interactive Server e ASP.NET Core, portanto precisa de um ambiente compatível com serviços .NET de execução contínua. A Vercel não possui um runtime .NET oficial para suas Functions; seu armazenamento local de funções também não é persistente.

Caminhos a avaliar no deploy:
- Hospedar frontend Blazor Server e API ASP.NET em serviço de aplicação/container que suporte .NET 10.
- Para um protótipo pequeno, usar SQLite **apenas com volume persistente e backup consistente**.
- Para crescimento e concorrência, estudar PostgreSQL gerenciado (por exemplo, Supabase), com configuração de provedor e migrations específicas; não basta trocar uma connection string do SQLite.

A escolha do provedor e a publicação pública acontecerão após a revisão visual do frontend, os testes de navegação e a aprovação de infraestrutura.
