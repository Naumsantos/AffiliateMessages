# Quickstart

## Pre-requisitos

- .NET 8 SDK
- Ollama disponivel em `http://localhost:11434`
- Modelo `llama3.2:latest` instalado na maquina local
- Um CSV UTF-8

## Posicoes do CSV

- coluna 0: ID
- coluna 1: nome
- coluna 2: preco
- coluna 8: link de afiliado

## Executar testes

```powershell
dotnet test tests/AffiliateMessages.Tests/AffiliateMessages.Tests.csproj
```

## Executar aplicacao

```powershell
dotnet run --project src/AffiliateMessages -- --input produtos.csv --output messages.txt --log logs.txt
```

## Validar o resultado

1. Um preco `79,19` deve aparecer como `R$ 79,19`.
2. Um preco `1700` deve aparecer como `R$ 1.700`.
3. O link da coluna 8 deve aparecer livre, sem crases.
4. Mensagens diferentes devem ser separadas por `====================`.
5. Com Ollama offline, o lote deve continuar e omitir a frase.
6. Linhas com preco ou link invalidos devem aparecer em `logs.txt`.
