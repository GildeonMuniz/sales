# Ambev Developer Evaluation — Backend

API REST em .NET 8 com Clean Architecture, CQRS e PostgreSQL.

---

## Pré-requisitos

| Ferramenta                                                        | Versão mínima              | Para quê 
|                                                                   |                            |
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0)      | 8.0                        | Compilar e rodar a aplicação 
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | qualquer                   | Subir todos os serviços com um comando 
| [Docker Compose](https://docs.docker.com/compose/) | v2           | Incluído no Docker Desktop |

Para rodar localmente sem Docker, também é necessário um PostgreSQL 13+ acessível em `localhost:5432`.

---

## Opção 1 — Docker Compose (recomendado)

Sobe a WebApi, PostgreSQL, MongoDB e Redis de uma vez. Não requer nenhuma configuração adicional.

### 1. Clone o repositório

```bash
git clone <url-do-repositorio>
cd backend
```

### 2. Suba todos os serviços

```bash
docker compose up -d
```

O Docker irá:
1. Construir a imagem da WebApi a partir do `Dockerfile`
2. Iniciar o PostgreSQL e aguardar o healthcheck passar
3. Iniciar a WebApi (que aplicará as migrations automaticamente)
4. Iniciar MongoDB e Redis (o mongo db e o redis veio na versão inicial do projeto)

### 3. Acesse o Swagger

```
http://localhost:8080/swagger
```

### Comandos úteis

```bash
# Ver logs em tempo real
docker compose logs -f ambev.developerevaluation.webapi

# Parar todos os serviços
docker compose down

# Parar e remover volumes (apaga dados do banco)
docker compose down -v

# Recriar a imagem da WebApi após mudanças no código
docker compose up -d --build ambev.developerevaluation.webapi
```

---

## Opção 2 — Rodar localmente com `dotnet run`

Útil para desenvolvimento com hot-reload e debug no IDE.

### 1. Suba apenas os serviços de infraestrutura

```bash
docker compose up -d ambev.developerevaluation.database ambev.developerevaluation.nosql ambev.developerevaluation.cache
```

### 2. Aplique as migrations

```bash
dotnet ef database update \
  --project src/Ambev.DeveloperEvaluation.ORM \
  --startup-project src/Ambev.DeveloperEvaluation.WebApi
```

> No Windows (PowerShell), substitua `\` por `` ` `` para continuar a linha.

### 3. Inicie a aplicação

```bash
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

### 4. Acesse o Swagger

```
http://localhost:5119/swagger
```

---

## Credenciais dos serviços

| Serviço    | Host (local) | Porta  | Usuário     | Senha        | Database 
|            |              |        |             |              |
| PostgreSQL | localhost    | 5432   | `developer` | `ev@luAt10n` | `developer_evaluation`  
| MongoDB    | localhost    | 27017  | `developer` | `ev@luAt10n` |  
| Redis      | localhost    | 6379   | —           | `ev@luAt10n` |  

---

## URLs por ambiente

| Ambiente                    | Swagger 
|                             |
| Docker Compose              | `http://localhost:8080/swagger` 
| `dotnet run` local          | `http://localhost:5119/swagger` 
| VS "Container (Dockerfile)" | `http://localhost:<porta-aleatória>/swagger` 

---

## Endpoints da API de Vendas

Todos os endpoints requerem autenticação JWT. Obtenha o token em `POST /api/auth` antes de fazer as chamadas.

| Método   | Rota                             | Descrição 
|          |                                  |
| `POST`   | `/api/sales`                     | Criar venda 
| `GET`    | `/api/sales/{id}`                | Buscar venda por ID 
| `GET`    | `/api/sales?page=1&size=10`      | Listar vendas (paginado) 
| `PUT`    | `/api/sales/{id}`                | Atualizar venda 
| `DELETE` | `/api/sales/{id}`                | Cancelar venda 
| `DELETE` | `/api/sales/{id}/items/{itemId}` | Cancelar item da venda 

### Regras de desconto

| Quantidade de itens iguais | Desconto aplicado 
|                            |
| Menos de 4                 | 0% 
| 4 a 9                      | 10% 
| 10 a 20                    | 20% 
| Mais de 20 | Não permitido |

---

## Rodar os testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test tests/Ambev.DeveloperEvaluation.Unit

# Com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## Estrutura do projeto

```
src/
├── Domain          — entidades, regras de negócio, interfaces
├── Application     — casos de uso (Commands, Queries, Handlers via MediatR)
├── ORM             — EF Core, repositórios, migrations (PostgreSQL)
├── WebApi          — controllers, middleware, Program.cs
├── Common          — JWT, BCrypt, Serilog, HealthChecks
└── IoC             — registro de dependências

tests/
├── Unit            — testes de domínio e application
├── Integration     — testes com banco real
└── Functional      — testes end-to-end

 
