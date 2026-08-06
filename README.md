# TaskFlow API

API RESTful para gestão de projetos e tarefas, construída em **ASP.NET Core 10** com autenticação **JWT**, **Entity Framework Core** e arquitetura em camadas. Projeto de portfólio focado em boas práticas de API design, segurança e testes automatizados.

## Stack

- **.NET 10 / ASP.NET Core Web API**
- **Entity Framework Core 10** — SQLite em desenvolvimento, SQL Server em produção
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **Swagger / OpenAPI** (Swashbuckle) com suporte a autenticação Bearer
- **xUnit** + `Microsoft.AspNetCore.Mvc.Testing` para testes de integração ponta a ponta
- **Docker** + **Azure Container Apps** (serverless, escala a zero) para produção
- **CI/CD**: GitHub Actions — build + testes em todo push/PR, deploy automático na `main` (build da imagem, push para o GitHub Container Registry, `az containerapp update`), autenticado via OIDC (sem senha/secret de longa duração)

## Arquitetura

Solução organizada em camadas, para separar regras de domínio, acesso a dados e a superfície HTTP:

```
TaskFlow.slnx
├── src/
│   ├── TaskFlow.Api/              # Controllers, DTOs, autenticação JWT, Swagger, composição da aplicação
│   ├── TaskFlow.Domain/           # Entidades e enums de domínio (sem dependências externas)
│   └── TaskFlow.Infrastructure/   # EF Core: DbContext, mapeamentos, migrations
└── tests/
    └── TaskFlow.Tests/            # Testes de integração (WebApplicationFactory) e unitários
```

### Modelo de domínio

- **AppUser** — dono de projetos, autenticado via e-mail/senha (hash com `PasswordHasher<T>` do ASP.NET Core Identity)
- **Project** — pertence a um `AppUser` (`OwnerId`)
- **TaskItem** — pertence a um `Project`, com `Status` (`Pendente`, `EmAndamento`, `Concluida`)

Todo acesso a `Project`/`TaskItem` é escopado ao usuário autenticado (isolamento por `OwnerId`, validado em cada endpoint).

## Endpoints

| Método | Rota                                   | Auth | Descrição                          |
|--------|-----------------------------------------|:----:|--------------------------------------|
| POST   | `/api/auth/register`                    |  **N**  | Cria usuário e retorna token JWT     |
| POST   | `/api/auth/login`                       |  **N**  | Autentica e retorna token JWT        |
| GET    | `/api/projects`                         |  **S**  | Lista projetos do usuário autenticado|
| GET    | `/api/projects/{id}`                    |  **S**  | Detalha um projeto                   |
| POST   | `/api/projects`                         |  **S**  | Cria projeto                         |
| PUT    | `/api/projects/{id}`                    |  **S**  | Atualiza projeto                     |
| DELETE | `/api/projects/{id}`                    |  **S**  | Remove projeto                       |
| GET    | `/api/projects/{projectId}/tasks`       |  **S**  | Lista tarefas de um projeto          |
| GET    | `/api/projects/{projectId}/tasks/{id}`  |  **S**  | Detalha uma tarefa                   |
| POST   | `/api/projects/{projectId}/tasks`       |  **S**  | Cria tarefa                          |
| PUT    | `/api/projects/{projectId}/tasks/{id}`  |  **S**  | Atualiza tarefa                      |
| DELETE | `/api/projects/{projectId}/tasks/{id}`  |  **S**  | Remove tarefa                        |

`Auth`: **S** = requer token Bearer, **N** = público. Documentação interativa completa (Swagger UI) disponível em `/swagger` ao rodar a aplicação.

## Como rodar localmente

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### 1. Configurar a chave JWT (obrigatório)

A chave de assinatura do token **não** fica versionada em `appsettings.json` — é lida via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
dotnet user-secrets set "Jwt:Key" "uma-chave-secreta-longa-e-aleatoria" --project src/TaskFlow.Api
```

### 2. Rodar a API

```bash
dotnet run --project src/TaskFlow.Api
```

A aplicação aplica as migrations do EF Core automaticamente na inicialização (banco SQLite local, `taskflow.db`). Acesse `https://localhost:<porta>/swagger` para testar os endpoints.

### 3. Rodar os testes

```bash
dotnet test
```

Os testes de integração sobem a API inteira em memória (`WebApplicationFactory`), cada classe de teste com seu próprio banco SQLite temporário e chave JWT isolada — sem tocar no ambiente de desenvolvimento.

## Deploy

A imagem Docker (`Dockerfile` na raiz) roda a API sem nenhuma alteração de código. Em produção:

- **Azure Container Apps** (plano Consumption) — serverless, escala a zero quando ocioso
- **Azure SQL Database** (tier gratuito, serverless, auto-pausa) como banco de produção
- Imagem publicada no **GitHub Container Registry** (`ghcr.io`) a cada push na `main`
- Autenticação do GitHub Actions no Azure via **OIDC/federated credentials** — nenhuma senha ou client secret de longa duração é armazenada como secret do repositório

## Roadmap

- [x] Modelagem de domínio (User, Project, TaskItem) + EF Core + migrations
- [x] Autenticação JWT (registro/login)
- [x] CRUD de Projects e Tasks com isolamento por usuário
- [x] Testes de integração (Auth, Projects, Tasks) e unitários (TokenService)
- [x] Pipeline CI/CD (GitHub Actions): build + testes a cada push/PR
- [x] Deploy contínuo para Azure Container Apps
- [x] Banco de produção (Azure SQL Database) via secret no Container App

## Licença

Projeto pessoal de portfólio, sem licença de uso comercial definida.
