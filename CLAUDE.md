# Starlink Aluguel — contexto do projeto

Este arquivo orienta o Claude Code sobre o projeto. Leia antes de mexer no código.

## O que é

**Starlink** (namespace interno **Starlink**) é um app de **agendamento de aluguel utilizando o sistema da starlink**,
com apenas um portal, no caso do dono para registrar os eventos, tanto de ordem de serviço quanto financeiros.

## Estrutura

- `frontend/`

- Telas do dono: `dashboard`, `ver-clientes`, `cadastrar-cliente`, `agendamentos`,
    `financeiro`, `servicos`, `meu-lava-jato`, `eventos`, `assinaturas`, `integracoes`.

- `backend/` — Web API em **C# / ASP.NET Core (.NET 10)** com **Entity Framework Core**.