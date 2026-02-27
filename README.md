# Scheduly - Sistema de Agendamento SaaS

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/Angular-21-DD0031?style=for-the-badge&logo=angular&logoColor=white" />
  <img src="https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img src="https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white" />
  <img src="https://img.shields.io/badge/Material-21-757575?style=for-the-badge&logo=materialdesign&logoColor=white" />
</p>

Sistema completo de agendamento multi-tenant para clinicas, saloes, consultorios e prestadores de servico. Construido com foco em **Clean Architecture**, **padroes de contabilidade** e **experiencia do usuario**.

---

## Destaques Tecnicos

- **Clean Architecture** com separacao em 4 camadas (Domain, Application, Infrastructure, API)
- **CQRS + MediatR** para segregacao de comandos e consultas
- **Multi-tenancy** por header (`X-Tenant-Id`) com query filters globais no EF Core
- **Autenticacao JWT** com access + refresh tokens e rotacao segura
- **Valores financeiros em centavos (int)** seguindo padroes de contabilidade — sem float/decimal para dinheiro
- **Fluxo de cobranca integrado** — agendamento gera transacao + email automatico via SMTP
- **Stepper de agendamento** — fluxo guiado em 4 passos com cadastro inline de cliente
- **Sistema de vagas** — profissionais abrem horarios, clientes escolhem vagas disponiveis
- **Background jobs** com Hangfire (lembretes, limpeza de tokens)
- **Angular 21** com standalone components, signals e lazy loading
- **Validacao server-side** com FluentValidation
- **Emails transacionais** com templates HTML via SMTP (MailHog para dev)

---

## Arquitetura

```
+-----------------------------------------------------------+
|                     Frontend (Angular 21)                  |
|  Dashboard | Agendamentos | Servicos | Vagas | Financeiro |
+-----------------------------------------------------------+
|                      API (.NET 10)                        |
|          Controllers -> MediatR -> Handlers               |
+-----------------------------------------------------------+
|  Application Layer          |  Infrastructure Layer       |
|  Commands / Queries / DTOs  |  EF Core / SMTP / Hangfire  |
+-----------------------------------------------------------+
|              Domain Layer (Entities / Enums)               |
+-----------------------------------------------------------+
|          PostgreSQL 16  |  MailHog  |  Redis (opcional)    |
+-----------------------------------------------------------+
```

## Estrutura do Projeto

```
schedule-asp-saas/
|-- src/
|   |-- Scheduly.Domain/          # Entidades, Enums, Excecoes
|   |-- Scheduly.Application/     # Commands, Queries, DTOs, Interfaces
|   |-- Scheduly.Infrastructure/  # EF Core, Servicos, Jobs, Identity
|   +-- Scheduly.Api/             # Controllers, Middleware, Config
|-- frontend/                     # Angular 21 + Material
|   +-- src/app/
|       |-- core/                 # Models, Services, Guards, Interceptors
|       |-- shared/               # Pipes, Components reutilizaveis
|       +-- features/             # Modulos de funcionalidade
|           |-- appointments/     # Lista, Calendario, Stepper, Detalhes
|           |-- customers/        # CRUD de clientes
|           |-- services/         # Catalogo de servicos
|           |-- vacancies/        # Gestao de vagas/horarios
|           |-- finance/          # Dashboard financeiro
|           |-- users/            # Gestao de equipe
|           +-- dashboard/        # Visao geral
|-- tests/
|   +-- Scheduly.UnitTests/       # Testes unitarios
+-- docker/
    +-- docker-compose.yml        # Stack completa
```

---

## Entidades e Fluxo de Negocio

| Entidade | Descricao |
|----------|-----------|
| **Tenant** | Empresa/clinica (multi-tenancy) |
| **User** | Profissional da equipe (Admin/Staff) |
| **Customer** | Cliente que agenda servicos |
| **Service** | Catalogo de servicos (nome, duracao, preco) |
| **Vacancy** | Vaga de horario aberta por um profissional |
| **Appointment** | Agendamento vinculando cliente + servico + profissional |
| **Transaction** | Registro financeiro (cobranca, pagamento, cancelamento) |

### Fluxo do Agendamento

```
1. Selecionar/cadastrar cliente
2. Escolher servico e profissional
3. Selecionar vaga disponivel ou informar horario
4. Confirmar -> Cria Appointment (status: PendingPayment)
                Cria Transaction (status: Pending)
                Marca Vacancy como booked
                Envia email de cobranca ao cliente
5. Registrar pagamento -> Appointment: Confirmed, Transaction: Paid
6. Concluir atendimento -> Appointment: Completed
```

### Status dos Agendamentos

| Status | Descricao |
|--------|-----------|
| `PendingPayment` | Aguardando pagamento |
| `Confirmed` | Pagamento confirmado |
| `Completed` | Atendimento concluido |
| `Cancelled` | Cancelado (libera vaga e cancela cobranca) |
| `NoShow` | Cliente nao compareceu |

---

## Stack Tecnologica

### Backend
| Tecnologia | Versao | Uso |
|-----------|--------|-----|
| .NET | 10.0 | Runtime e SDK |
| ASP.NET Core | 10.0 | Web API RESTful |
| Entity Framework Core | 10.0 | ORM + Migrations |
| PostgreSQL | 16 | Banco de dados |
| MediatR | 12.x | CQRS / Mediator pattern |
| FluentValidation | 11.x | Validacao de comandos |
| Hangfire | 1.8.x | Background jobs |
| Serilog | 4.x | Logging estruturado |
| System.Net.Mail | - | Envio de emails SMTP |

### Frontend
| Tecnologia | Versao | Uso |
|-----------|--------|-----|
| Angular | 21 | Framework SPA |
| Angular Material | 21 | UI Components |
| TypeScript | 5.9 | Linguagem |
| RxJS | 7.8 | Programacao reativa |

### Infraestrutura
| Tecnologia | Uso |
|-----------|-----|
| Docker Compose | Orquestracao local |
| MailHog | Servidor SMTP para desenvolvimento |
| Nginx | Proxy reverso (frontend Docker) |
| Vercel | Hospedagem frontend (producao) |
| Railway | Hospedagem backend (producao) |
| Neon | PostgreSQL serverless (producao) |

---

## Como Rodar o Projeto

### Pre-requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (recomendado)
- Ou, para desenvolvimento local:
  - [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
  - [Node.js 20+](https://nodejs.org/)
  - [PostgreSQL 16](https://www.postgresql.org/download/)

---

### Opcao 1: Docker Compose (Recomendado)

Sobe toda a stack com um comando:

```bash
cd docker
docker compose up --build
```

| Servico | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| API (Swagger) | http://localhost:5000/swagger |
| MailHog (Emails) | http://localhost:8025 |
| PostgreSQL | localhost:5432 |

---

### Opcao 2: Desenvolvimento Local

#### 1. Banco de Dados

Inicie apenas o PostgreSQL e MailHog via Docker:

```bash
cd docker
docker compose up postgres mailhog -d
```

#### 2. Backend

```bash
# Aplicar migrations
dotnet ef database update --project src/Scheduly.Infrastructure --startup-project src/Scheduly.Api

# Rodar a API
dotnet run --project src/Scheduly.Api
```

A API estara disponivel em `http://localhost:5000` com Swagger em `/swagger`.

#### 3. Frontend

```bash
cd frontend
npm install
npm start
```

O frontend estara disponivel em `http://localhost:4200` com proxy configurado para a API.

---

### Primeiro Acesso

1. Acesse `http://localhost:4200/register`
2. Crie uma conta (sera o Admin do tenant)
3. Cadastre servicos em **Servicos**
4. Abra vagas de horario em **Vagas**
5. Crie agendamentos com o stepper em **Agendamentos**
6. Acompanhe cobrancas em **Financeiro**
7. Visualize emails enviados em `http://localhost:8025` (MailHog)

---

## API Endpoints

### Autenticacao
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| POST | `/api/auth/register` | Criar conta + tenant |
| POST | `/api/auth/login` | Login (retorna JWT) |
| POST | `/api/auth/refresh-token` | Renovar access token |
| POST | `/api/auth/revoke-token` | Revogar refresh token |

### Agendamentos
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/appointments` | Listar (com filtros e paginacao) |
| GET | `/api/appointments/:id` | Detalhes (inclui transacao) |
| POST | `/api/appointments` | Criar agendamento |
| PUT | `/api/appointments/:id` | Editar horario |
| PATCH | `/api/appointments/:id/confirm` | Confirmar |
| PATCH | `/api/appointments/:id/cancel` | Cancelar |
| PATCH | `/api/appointments/:id/done` | Marcar concluido |

### Servicos
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/services` | Listar todos |
| GET | `/api/services/:id` | Detalhes |
| POST | `/api/services` | Criar servico |
| PUT | `/api/services/:id` | Atualizar servico |

### Vagas
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/vacancies` | Listar (com filtros) |
| POST | `/api/vacancies` | Criar vaga individual |
| POST | `/api/vacancies/bulk` | Criar vagas em lote |
| DELETE | `/api/vacancies/:id` | Remover vaga |

### Transacoes
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET | `/api/transactions` | Listar (com filtros e paginacao) |
| GET | `/api/transactions/:id` | Detalhes |
| GET | `/api/transactions/summary` | Resumo financeiro |
| PATCH | `/api/transactions/:id/pay` | Registrar pagamento |
| PATCH | `/api/transactions/:id/cancel` | Cancelar transacao |

### Clientes e Equipe
| Metodo | Endpoint | Descricao |
|--------|----------|-----------|
| GET/POST/PUT/DELETE | `/api/customers` | CRUD de clientes |
| GET/POST/PUT/DELETE | `/api/users` | CRUD de usuarios (Admin) |

---

## Deploy em Producao (Gratuito)

A stack recomendada para producao gratuita:

| Servico | Plataforma | Tier |
|---------|-----------|------|
| Frontend | [Vercel](https://vercel.com) | Free |
| Backend | [Railway](https://railway.app) | Free trial ($5) |
| PostgreSQL | [Neon](https://neon.tech) | Free (0.5GB) |
| Email | [Brevo](https://brevo.com) | Free (300/dia) |

### Passo a Passo

#### 1. Neon (PostgreSQL)
1. Criar conta em [neon.tech](https://neon.tech)
2. Criar projeto "scheduly"
3. Copiar connection string (incluir `SslMode=Require`)

#### 2. Railway (Backend)
1. Criar conta em [railway.app](https://railway.app)
2. New Project → Deploy from GitHub
3. Railway detecta o `railway.toml` automaticamente
4. Configurar variaveis de ambiente (ver tabela abaixo)
5. Copiar a URL publica gerada

#### 3. Vercel (Frontend)
1. Criar conta em [vercel.com](https://vercel.com)
2. Import GitHub repo
3. Configurar:
   - Root Directory: `frontend`
   - Build Command: `npm run build`
   - Output Directory: `dist/scheduly-frontend/browser`
4. Atualizar `ALLOWED_ORIGINS` no Railway com a URL do Vercel

#### 4. Configurar API_URL no Vercel
Em Settings → Environment Variables do projeto Vercel, adicionar:
- `API_URL` = `https://sua-url.up.railway.app`

O script `prebuild` gera automaticamente o `environment.prod.ts` com essa URL.

---

## Variaveis de Ambiente

### Backend (.NET)

| Variavel | Padrao | Obrigatoria | Descricao |
|----------|--------|-------------|-----------|
| `ASPNETCORE_ENVIRONMENT` | Development | Sim | Ambiente: Development ou Production |
| `ConnectionStrings__DefaultConnection` | (ver appsettings) | Sim | Connection string PostgreSQL |
| `Jwt__Secret` | (dev key) | Sim | Chave secreta JWT (min 32 chars) |
| `Jwt__Issuer` | Scheduly | Nao | Emissor do token |
| `Jwt__Audience` | Scheduly | Nao | Audiencia do token |
| `Jwt__AccessTokenExpirationMinutes` | 15 | Nao | Expiracao do access token |
| `Jwt__RefreshTokenExpirationDays` | 7 | Nao | Expiracao do refresh token |
| `ALLOWED_ORIGINS` | (vazio = AllowAny) | Prod: Sim | Origens CORS permitidas (comma-separated) |
| `Email__Host` | localhost | Sim | Host SMTP |
| `Email__Port` | 1025 | Sim | Porta SMTP |
| `Email__From` | noreply@scheduly.com | Nao | Email remetente |
| `Email__FromName` | Scheduly | Nao | Nome do remetente |
| `Email__EnableSsl` | false | Nao | Habilitar SSL no SMTP |
| `Email__Username` | (vazio) | Prod: Sim | Usuario SMTP (autenticacao) |
| `Email__Password` | (vazio) | Prod: Sim | Senha SMTP (autenticacao) |

### Frontend (Vercel)

| Variavel | Descricao |
|----------|-----------|
| `API_URL` | URL completa da API backend (ex: `https://sua-url.up.railway.app`) |

O script `prebuild` (`scripts/set-env.js`) gera `environment.prod.ts` automaticamente a partir da env var `API_URL` durante o build do Vercel.

Veja `.env.example` na raiz do projeto para referencia completa.

---

## Testes

```bash
dotnet test
```

Os testes unitarios cobrem:
- Transicoes de status de agendamentos
- Criacao de agendamentos com validacao de overlap
- Registro e autenticacao de usuarios
- Isolamento multi-tenant

---

## Decisoes Tecnicas

| Decisao | Motivo |
|---------|--------|
| **Valores em centavos (int)** | Evita problemas de precisao com float/decimal. Padrao contabil. |
| **Multi-tenancy por header** | Simples, escalavel, sem necessidade de bancos separados. |
| **CQRS com MediatR** | Desacoplamento entre controllers e logica, facilita testes. |
| **Standalone components** | Padrao moderno Angular, elimina NgModules desnecessarios. |
| **Stepper no agendamento** | UX guiada reduz erros e permite cadastro inline. |
| **Reference numbers sequenciais** | `TXN-20260227-001` — padrao profissional para identificar cobrancas. |
| **MailHog em dev** | Captura emails sem configurar SMTP real. UI web para visualizar. |

---

## Roadmap

- [ ] Notificacoes push / WhatsApp
- [ ] Relatorios e dashboards avancados
- [ ] Integracao com gateways de pagamento (Stripe, Mercado Pago)
- [ ] App mobile (Ionic / Flutter)
- [ ] Agenda publica para clientes agendarem online
- [ ] Importacao/exportacao de dados (CSV)

---

## Licenca

Este projeto e de uso privado. Todos os direitos reservados.
