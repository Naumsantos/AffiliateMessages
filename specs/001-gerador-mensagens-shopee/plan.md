# Implementation Plan: Gerador de mensagens de promocao Shopee

**Branch**: `001-gerador-mensagens-shopee` | **Date**: 2026-11-26 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-gerador-mensagens-shopee/spec.md`

## Summary

Criar um console app .NET 8 que leia um CSV UTF-8, interprete nome, preco e link pelas
posicoes fixas esclarecidas, gere mensagens em portugues e grave o lote em `messages.txt`.
A frase persuasiva sera opcional e sera solicitada ao Ollama em localhost. Falhas da IA ou
de linhas individuais nao interrompem o lote.

## Technical Context

**Language/Version**: C# com .NET 8

**Primary Dependencies**: CsvHelper, Microsoft.Extensions.Logging

**Storage**: Arquivos locais; CSV de entrada, `messages.txt` e `logs.txt`

**Testing**: xUnit

**Target Platform**: Console local no Windows

**Project Type**: Console application com dois projetos

**Performance Goals**: Processar ate 50 produtos em menos de tres minutos

**Constraints**: Somente Ollama em localhost; sem banco de dados; sem envio ao WhatsApp

**Scale/Scope**: Um lote por execucao, com ate 50 produtos

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Execucao local:** PASSA. Nenhum dado sera enviado a um servico externo.
- **Resiliencia:** PASSA. Processamento por linha com fallback sem frase.
- **Test-first:** PASSA. Regras de CSV, preco, formatacao e Ollama terao testes xUnit.
- **Simplicidade:** PASSA. Dois projetos e dependencias limitadas.
- **Idioma:** PASSA. Mensagens em portugues do Brasil; identificadores em ingles.

### Reavaliacao apos o design

- Os artefatos confirmam processamento local, fallback resiliente, testes isolaveis com
  HttpMessageHandler falso e estrutura minima de dois projetos.
- Nenhuma dependencia de banco de dados, WhatsApp ou API externa foi introduzida.
- Todos os gates continuam passando.

## Project Structure

### Documentation (this feature)

```text
specs/001-gerador-mensagens-shopee/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code (repository root)

```text
src/
└── AffiliateMessages/
    ├── AffiliateMessages.csproj
    ├── appsettings.json
    ├── Program.cs
    ├── Models/
    ├── Services/
    └── Configuration/

tests/
└── AffiliateMessages.Tests/
    └── AffiliateMessages.Tests.csproj
```

**Structure Decision**: Console app em `src/AffiliateMessages` e testes xUnit em
`tests/AffiliateMessages.Tests`, conforme a stack definida.

## Complexity Tracking

Nenhuma violacao constitui gate aberto. Nao ha excecoes justificadas.
