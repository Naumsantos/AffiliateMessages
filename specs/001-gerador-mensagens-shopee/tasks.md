# Tasks: Gerador de mensagens de promocao Shopee

**Input**: Design documents from `/specs/001-gerador-mensagens-shopee/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Criar a solution e os projetos base.

- [X] T001 Criar a solution e os projetos console/teste em AffiliateMessages.sln, src/AffiliateMessages/AffiliateMessages.csproj e tests/AffiliateMessages.Tests/AffiliateMessages.Tests.csproj.
- [X] T002 [P] Adicionar referencias CsvHelper, Microsoft.Extensions.Logging e Microsoft.Extensions.Configuration em src/AffiliateMessages/AffiliateMessages.csproj.
- [X] T003 [P] Adicionar referencias xUnit e pacotes de teste em tests/AffiliateMessages.Tests/AffiliateMessages.Tests.csproj.
- [X] T004 Criar src/AffiliateMessages/appsettings.json com URL Ollama, modelo llama3.2:latest, arquivo de entrada e caminhos messages.txt/logs.txt.
- [X] T005 [P] Criar a estrutura de pastas src/AffiliateMessages/Models, src/AffiliateMessages/Services, src/AffiliateMessages/Configuration e tests/AffiliateMessages.Tests.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Implementar modelos e configuracoes compartilhadas necessarios por todas as historias.

- [X] T006 Escrever testes dos modelos em tests/AffiliateMessages.Tests/Models/ProductTests.cs e tests/AffiliateMessages.Tests/Models/GeneratedMessageTests.cs.
- [X] T007 Implementar os modelos Product e GeneratedMessage em src/AffiliateMessages/Models/Product.cs e src/AffiliateMessages/Models/GeneratedMessage.cs, com Name nullable, Price decimal obrigatorio e AffiliateLink obrigatoria.
- [X] T008 Escrever testes de configuracao em tests/AffiliateMessages.Tests/Configuration/AppOptionsTests.cs.
- [X] T009 Implementar AppOptions em src/AffiliateMessages/Configuration/AppOptions.cs para modelo, URL Ollama, arquivo de entrada e caminhos de saida.
- [X] T010 Escrever testes de formatacao compartilhada em tests/AffiliateMessages.Tests/Services/PriceFormatterTests.cs.
- [X] T011 Implementar formatacao pt-BR em src/AffiliateMessages/Services/PriceFormatter.cs usando CultureInfo("pt-BR") (RF3, RF2).
- [X] T012 Configurar Microsoft.Extensions.Logging no console em src/AffiliateMessages/Program.cs e src/AffiliateMessages/Services/ConsoleLogger.cs.

**Checkpoint**: Foundation pronta; as historias podem ser implementadas independentemente.

---

## Phase 3: User Story 1 - Gerar mensagens a partir do CSV (Priority: P1) 🎯 MVP

**Goal**: Ler CSV real ou sem cabecalho e produzir messages.txt com nome, preco e link da coluna 8.

**Independent Test**: Executar com um CSV contendo campos entre aspas, 79,19 e 1700 e confirmar a mensagem, o link da coluna 8 e a linha divisoria.

### Tests for User Story 1

- [X] T013 [P] [US1] Escrever testes de CSV com cabecalho, sem cabecalho e virgulas entre aspas em tests/AffiliateMessages.Tests/Services/CsvReaderTests.cs (RF1, RF2).
- [X] T014 [P] [US1] Escrever testes de montagem da mensagem em tests/AffiliateMessages.Tests/Services/MessageFormatterTests.cs (RF4, RF5).
- [X] T015 [P] [US1] Escrever testes de escrita de lote em tests/AffiliateMessages.Tests/Services/MessageWriterTests.cs (RF5).

### Implementation for User Story 1

- [X] T016 [US1] Implementar leitura CSV com CsvHelper em src/AffiliateMessages/Services/CsvReader.cs, usando colunas fixas 1, 2 e 8 (RF1, RF2A).
- [X] T017 [US1] Implementar conversao de preco pt-BR em src/AffiliateMessages/Services/PriceParser.cs, com 79,19 -> decimal e 1700 -> decimal (RF2, RF3).
- [X] T018 [US1] Implementar montagem da mensagem em src/AffiliateMessages/Services/MessageFormatter.cs, omitindo o nome da primeira linha quando vazio e usando link da coluna 8 (RF4, RF5).
- [X] T019 [US1] Implementar gravacao em src/AffiliateMessages/Services/MessageWriter.cs, separando mensagens por ==================== em messages.txt (RF5, RF8A).
- [X] T020 [US1] Integrar leitura, formatacao e escrita no lote em src/AffiliateMessages/Services/MessageBatchProcessor.cs (RF1-RF5).

**Checkpoint**: User Story 1 gera mensagens validas independentemente.

---

## Phase 4: User Story 2 - Continuar o processamento quando a IA local falha (Priority: P2)

**Goal**: Chamar Ollama localmente e manter o lote mesmo quando a IA falhar.

**Independent Test**: Usar HttpMessageHandler falso com falha/offline e confirmar que todas as mensagens validas sao gravadas sem a primeira linha.

### Tests for User Story 2

- [X] T021 [P] [US2] Escrever testes unitarios do OllamaClient com HttpMessageHandler falso em tests/AffiliateMessages.Tests/Services/OllamaClientTests.cs, cobrindo sucesso, timeout, HTTP falho e JSON invalido (RF6).
- [X] T022 [P] [US2] Escrever testes da frase em tests/AffiliateMessages.Tests/Services/PhraseValidatorTests.cs, cobrindo ate 7 palavras e ate 1 emoji (RF3).
- [X] T023 [P] [US2] Escrever teste de fallback do lote em tests/AffiliateMessages.Tests/Services/MessageBatchProcessorTests.cs (RF6).

### Implementation for User Story 2

- [X] T024 [US2] Implementar cliente Ollama com HttpClient puro em src/AffiliateMessages/Services/OllamaClient.cs, POST localhost:11434/api/generate, stream=false e modelo configuravel (RF3, RF6, RF11A).
- [X] T025 [US2] Implementar validacao da frase em src/AffiliateMessages/Services/PhraseValidator.cs, em pt-BR, ate 7 palavras e ate 1 emoji (RF3).
- [X] T026 [US2] Integrar fallback sem frase no lote em src/AffiliateMessages/Services/MessageBatchProcessor.cs, continuando a proxima linha apos falha da IA (RF6).

**Checkpoint**: User Story 2 mantem o lote disponivel com Ollama offline.

---

## Phase 5: User Story 3 - Identificar e ignorar linhas invalidas (Priority: P3)

**Goal**: Pular preco/link invalidos e registrar diagnosticos em logs.txt sem derrubar o lote.

**Independent Test**: Misturar linha invalida e linhas validas e confirmar a ausencia da invalida em messages.txt e o registro completo em logs.txt.

### Tests for User Story 3

- [X] T027 [P] [US3] Escrever testes de linhas invalidas em tests/AffiliateMessages.Tests/Services/CsvReaderTests.cs e tests/AffiliateMessages.Tests/Services/InvalidLineLoggerTests.cs (RF7, RF10A).
- [X] T028 [P] [US3] Escrever teste de lote misto em tests/AffiliateMessages.Tests/Services/MessageBatchProcessorTests.cs, confirmando que linhas invalidas nao cancelam demais produtos (RF7).

### Implementation for User Story 3

- [X] T029 [US3] Implementar logger de linhas invalidas em src/AffiliateMessages/Services/InvalidLineLogger.cs, gravando numero da linha e motivo em logs.txt (RF7, RF10A).
- [X] T030 [US3] Integrar validacao e registro no lote em src/AffiliateMessages/Services/MessageBatchProcessor.cs, ignorando preco ou link invalidos e continuando (RF7).

**Checkpoint**: Linhas invalidas sao isoladas e registradas sem interromper o lote.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Fechar configuracao CLI, robustez, documentacao e validacao final.

- [X] T031 [P] Implementar argumentos de linha de comando em src/AffiliateMessages/Program.cs para modelo, URL Ollama, entrada, saida e log.
- [X] T032 [P] Atualizar src/AffiliateMessages/appsettings.json e specs/001-gerador-mensagens-shopee/quickstart.md com os valores padrao.
- [X] T033 Adicionar tratamento de UTF-8 e erro de leitura em src/AffiliateMessages/Services/CsvReader.cs e src/AffiliateMessages/Services/MessageBatchProcessor.cs (RF1).
- [X] T034 [P] Executar a suite completa em tests/AffiliateMessages.Tests e corrigir falhas relacionadas aos requisitos RF1-RF12.
- [X] T035 Medir processamento de 50 produtos em src/AffiliateMessages e confirmar conclusao em menos de tres minutos (RF11).
- [ ] T036 Validacao final bloqueada por entrada externa: aguarda um CSV real exportado da Shopee; executar dotnet run --project src/AffiliateMessages com o arquivo fornecido, revisar messages.txt e logs.txt e confirmar pelo menos um preco 79,19, o link da coluna 8 e divisorias (RF1-RF12).

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1): inicia imediatamente.
- Foundational (Phase 2): depende da Phase 1 e bloqueia todas as historias.
- User Story 1 (Phase 3): depende da Phase 2.
- User Story 2 (Phase 4): depende da Phase 2 e se integra ao processador da US1.
- User Story 3 (Phase 5): depende da Phase 2 e se integra ao processador da US1.
- Polish (Phase 6): depende de todas as historias desejadas.

### User Story Dependencies

- US1: independente apos a fundacao.
- US2: independente na implementacao; usa o OllamaClient e o processador da US1.
- US3: independente na implementacao; usa a validacao e o processador da US1.

### Within Each User Story

- Testes primeiro e confirmados como falhos antes da implementacao.
- Modelos e parser antes dos services.
- Cliente Ollama/testes antes do fallback integrado.
- Logica completa antes da integracao final.

## Parallel Execution Examples

### User Story 1

- T013, T014 e T015 podem ser executadas em paralelo.
- T016 e T017 podem iniciar em paralelo apos os testes.

### User Story 2

- T021, T022 e T023 podem ser executadas em paralelo.
- T024 e T025 podem ser implementadas em arquivos diferentes.

### User Story 3

- T027 e T028 podem ser executadas em paralelo.
- T029 pode ser implementada enquanto T030 integra o processador.

## Implementation Strategy

### MVP First

1. Completar Setup e Foundational.
2. Completar User Story 1.
3. Validar CSV e arquivos de saida independentemente.
4. Prosseguir para fallback Ollama e linhas invalidas.

### Incremental Delivery

1. Setup + Foundation.
2. US1 gera o MVP.
3. US2 adiciona resiliencia com IA.
4. US3 adiciona isolamento de linhas invalidas.
5. Polish executa validacao completa.

## Notes

- Tasks com [P] usam arquivos diferentes e nao dependem de tarefas incompletas.
- Task labels [US1], [US2] e [US3] aparecem apenas nas fases de historia.
- Toda regra de implementacao deve ser coberta por teste anterior.
- Requisitos citados usam RF1-RF12 da especificacao.
