# Make it Short!

API de encurtamento de URLs feita em .NET 9, pronta para rodar com Docker.

## Pré-requisitos

Docker Desktop instalado.

## Como executar

No diretório raiz do projeto, executar os comandos:

### Windows (PowerShell ou CMD)

```
docker compose up -d
```

### Linux / macOS (Terminal)
```
docker compose up -d
```

Após isso, a API estará disponível em: http://localhost:5000

> **Opcional:** Postman ou Insomnia para testar a API. Importe as rotas do arquivo "v1.json", localizado no diretório: docs.

## Ferramentas necessárias para desenvolvimento

Para manter ou desenvolver o projeto, você precisará de:

- .NET 9 SDK
- Visual Studio 2022
- Docker Desktop
- Git
