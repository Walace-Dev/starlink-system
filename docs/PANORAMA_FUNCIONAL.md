# Starlink System — panorama funcional para relatório do Projeto de Extensão

> Documento de apoio à equipe. Descreve o código existente e separa o que está implementado do que ainda depende de validação em execução real. Não substitui os testes com usuários e os registros de evidências.

## 1. Objetivo

O Starlink System é uma aplicação web de apoio ao gerenciamento de operações de venda e aluguel de equipamentos/serviços Starlink por um proprietário. Centraliza cadastro de clientes, registro das operações, consulta de contratos e acompanhamento de indicadores operacionais.

Público atual: **proprietário/gestor da operação**. Não foi identificado portal independente para os clientes finais.

## 2. Arquitetura

| Camada | Tecnologia | Responsabilidade |
| --- | --- | --- |
| Interface | Blazor Web App (.NET 10), Interactive Server | Páginas, formulários, navegação e dashboard |
| Serviços frontend | IStarlinkService, MockStarlinkService, ApiStarlinkService | Alternância entre dados demonstrativos e API real |
| Backend | ASP.NET Core Web API (.NET 10) | Endpoints HTTP, validação e autenticação |
| Segurança | JWT Bearer + PasswordHasher | Login, token e identificação do proprietário |
| Persistência | Entity Framework Core 10 + SQLite | Tabelas Donos, Clientes e Vendas; migrations |
| Qualidade | xUnit, WebApplicationFactory, GitHub Actions | Testes unitários/de integração, compilação e auditoria de banco |

## 3. Funcionalidades existentes

| Módulo | Funcionalidade observada no código | Situação |
| --- | --- | --- |
| Acesso | Cadastro de conta de proprietário | API e formulário implementados |
| Acesso | Login por e-mail e senha | API e formulário implementados |
| Segurança | Hash de senha e emissão de JWT | Implementado no backend |
| Dashboard | Total de clientes | Implementado na interface |
| Dashboard | Aluguéis ativos, valor bruto ativo e contratos encerrando em 7 dias | Implementado na interface |
| Dashboard | Cinco registros recentes | Implementado na interface |
| Clientes | Listagem, consulta, cadastro, edição e exclusão | API + interface implementadas |
| Clientes | Validação de CPF/CNPJ, normalização do documento e prevenção de duplicatas por dono | Implementado no backend |
| Vendas/aluguéis | Listagem, criação, detalhamento, alteração de status e exclusão | API + interface implementadas |
| Vendas/aluguéis | Status Ativa, Finalizada, Cancelada | Implementado |
| Operações | Associação com cliente e proprietário autenticado | Implementado no backend |
| Configurações | Exibição do modo MOCK/API, URL da API e dados da conta | Implementado na interface |
| Sessão | Sair da conta | Implementado na interface |

**Modos de funcionamento:**
- `UseMockData=true`: demonstração com dados em memória, sem persistência.
- `UseMockData=false`: o Blazor usa `ApiStarlinkService` para chamar o backend com JWT; requer API e banco operacional.

**Importante:** a existência do código não equivale a homologação de todos os fluxos com banco real, navegador e hospedagem; as evidências práticas ainda devem ser registradas pela equipe.

## 4. Fluxos principais para demonstrar

### Fluxo A — Acesso e autenticação

1. Gestor abre o formulário de registro e cria conta.
2. API valida dados e normaliza e-mail.
3. Senha é armazenada como hash; resposta contém token JWT.
4. Sessão do frontend utiliza JWT nas requisições autenticadas.
5. Login com senha incorreta recebe 401.

### Fluxo B — Cadastro de cliente

1. Gestor autenticado abre Clientes e informa nome/documento.
2. Backend valida CPF/CNPJ e remove máscara.
3. Cliente é associado ao DonoId do JWT.
4. Documento repetido no mesmo dono recebe conflito.
5. Cliente fica visível na listagem do proprietário.

### Fluxo C — Venda e aluguel

1. Gestor seleciona cliente, datas e valor.
2. API confirma que cliente pertence ao gestor.
3. Operação é gravada em Vendas; a existência de DataFimAluguel diferencia aluguel de venda no frontend.
4. Gestor consulta detalhes e atualiza o status.
5. O dashboard considera os registros para indicadores.

### Fluxo D — Isolamento

1. Cria-se usuário A e B.
2. A registra um cliente.
3. B tenta consultar o identificador desse cliente.
4. A API deve retornar 404 e não incluir o cliente de A na listagem de B.
5. O banco também impede associar uma venda a cliente de outro proprietário.

## 5. Interface e páginas

Páginas existentes no frontend: login, registro, dashboard, clientes, formulário de cliente, aluguéis/vendas (contratos), formulário de contrato, detalhes e configurações. A navegação principal disponibiliza acesso aos respectivos módulos de gestão.

A equipe pode registrar capturas de tela do modo demonstração para apresentar a usabilidade; para comprovar persistência, deve utilizar o modo API e reiniciar a aplicação para validar que os registros permanecem.

## 6. Banco de dados

Relacionamentos:
- Donos (1) → (N) Clientes;
- Donos (1) → (N) Vendas;
- Clientes (1) → (N) Vendas.

O documento técnico, dicionário de dados, migrations e regras de integridade estão em [BANCO_DE_DADOS.md](BANCO_DE_DADOS.md).

## 7. O que NÃO deve ser apresentado como entregue

- Integração real com satélites/API oficial da Starlink ou telemetria de equipamentos.
- Processamento de pagamentos on-line, emissão fiscal ou integrações bancárias.
- Cadastro detalhado de equipamentos, estoque e manutenção automatizada.
- Deploy público, monitoramento em produção, infraestrutura escalável e backups cloud.
- Testes de usabilidade realizados com usuários externos, sem evidências.
- Conformidade completa com LGPD ou auditoria independente de segurança.

Esses itens só podem entrar no relatório como propostas ou trabalhos futuros.

## 8. Sugestão de estrutura para o relatório acadêmico

1. **Introdução:** problema de organizar clientes e operações de aluguel/venda.
2. **Objetivo geral:** desenvolver sistema web de gestão com autenticação e persistência.
3. **Objetivos específicos:** modelar banco, implementar API, desenvolver interface e testar fluxos.
4. **Fundamentação:** Blazor, ASP.NET Core, JWT, EF Core, SQLite e qualidade de software.
5. **Metodologia:** requisitos, MER, modelagem relacional, migrations, implementação e testes.
6. **Desenvolvimento:** divisão das atividades, módulos e integração entre camadas.
7. **Resultados:** screenshots reais, execução dos testes, PRs aprovados e CRUD persistente.
8. **Limitações e melhorias:** estoque, equipamentos, pagamentos e infraestrutura definitiva.
9. **Conclusão:** resultados comprovados e próximos passos, sem afirmar funcionalidades não testadas.

## 9. Divisão sugerida de evidências por integrante

- **Banco de dados:** DER, dicionário, migrations, integridade referencial e teste de persistência.
- **Backend:** endpoints, autenticação JWT, regras de negócio e proteção por proprietário.
- **Frontend:** capturas das páginas, navegação, responsividade e fluxos do usuário.
- **Qualidade/integração:** CI, testes unitários/de integração e evidências da conexão real.
- **Deploy (etapa posterior):** URL pública, arquitetura de hospedagem, variáveis de ambiente e recuperação de dados.
