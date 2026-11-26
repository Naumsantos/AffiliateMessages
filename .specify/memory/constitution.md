<!--
SYNC IMPACT REPORT
Version change: placeholder -> 1.0.0
Modified principles: none (initial project-specific constitution)
Added sections:
- Execucao 100% Local
- Resiliencia
- Test-First
- Simplicidade Tecnica
- Idioma do Codigo e das Mensagens
- Restricoes Tecnicas
- Fluxo de Desenvolvimento
- Governance
Removed sections: none
Follow-up TODOs: none
-->

# Conversor CSV Constitution

## Core Principles

### I. Execucao 100% Local

Todos os dados de produtos, incluindo nomes, precos e links de afiliado, DEVEM ser processados
integralmente em ambiente local. Nenhum dado do produto ou link PODERA ser enviado a uma
IA em nuvem. A unica integracao de IA autorizada para esta versao DEVE usar o Ollama em
`localhost`.

### II. Resiliencia

Falhas na IA local, falhas de leitura de uma linha do CSV ou dados invalidos em uma linha
NAO PODEM interromper o lote. O processamento restante DEVE continuar, e a linha ou
produto afetado DEVE ser registrado em log sem impedir a criacao das mensagens validas.

### III. Test-First

Toda regra de parsing e toda regra de formatacao DEVEM possuir primeiro um teste unitario
xUnit que demonstre o comportamento esperado. Os testes DEVEM cobrir casos validos,
invalidos, limites e falhas externas relevantes antes da implementacao da regra.

### IV. Simplicidade Tecnica

A aplicacao DEVE ser um console app .NET 8, com o menor numero necessario de projetos,
dependencias e abstracoes. Frameworks, design patterns e camadas adicionais NAO DEVEM ser
introduzidos sem uma necessidade concreta que nao possa ser resolvida diretamente.

### V. Idioma do Codigo e das Mensagens

Codigo-fonte, nomes de arquivos, metodos, classes, variaveis e mensagens voltadas ao
usuario DEVEM usar portugues do Brasil. Os identificadores tecnicos de codigo DEVEM usar
ingles, conforme as convencoes da linguagem e da plataforma.

## Restricoes Tecnicas

- A plataforma principal DEVE ser .NET 8.
- A camada de IA DEVE acessar somente o Ollama em localhost.
- O resultado principal DEVE permanecer disponivel localmente no arquivo `messages.txt`.
- Nao DEVE haver envio automatico de mensagens ao WhatsApp ou integracao com o Mercado Livre.

## Fluxo de Desenvolvimento

- Testes unitarios xUnit precedem a implementacao das regras de parsing e formatacao.
- Mudancas devem ser pequenas, diretamente relacionadas ao requisito e verificaveis por testes.
- Tratamento de falha deve preservar o processamento do lote e registrar a causa.
- A complexidade adicional exige justificacao por um requisito ou criterio de aceite mensuravel.

## Governance

Esta constitucao prevalece sobre as praticas locais do projeto. Qualquer emenda deve:

- ser apresentada como uma alteracao explicita em `.specify/memory/constitution.md`;
- registrar a razao da alteracao;
- atualizar a versao conforme semantica de versao; e
- ser revisada quanto a conformidade antes da implementacao relacionada.

O versionamento segue SemVer:

- MAJOR para remocao ou redefinicao incompativel de principios existentes;
- MINOR para adicao de principio ou ampliacao material de uma regra; e
- PATCH para esclarecimentos ou ajustes nao semanticos.

Toda especificacao, plano e tarefa DEVE declarar ou demonstrar conformidade com os principios
antes da conclusao. As revisoes devem verificar privacidade local, resiliencia do lote,
test-first, simplicidade e uso consistente do idioma.

**Version**: 1.0.0 | **Ratified**: 2026-11-26 | **Last Amended**: 2026-11-26
