---
description: "Task list for the local Angular CSV conversion interface"
---

# Tasks: Interface Angular local para conversão de CSV

**Input**: Design documents from `specs/002-interface-conversao/`

**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/http-api.md`, `quickstart.md`

**Tests**: Add focused xUnit, API integration, and Angular tests before implementing the corresponding batch-processing, HTTP API, and asynchronous UI behavior, as required by the plan and project constitution.

**Organization**: Tasks are grouped by user story so each increment can be implemented and reviewed separately.

## Phase 1: Setup

**Purpose**: Align the approved requirements and initialize the Angular/.NET build structure.

- [X] T001 [P] Verify that `specs/002-interface-conversao/spec.md`, `specs/002-interface-conversao/plan.md`, and `specs/002-interface-conversao/contracts/http-api.md` agree on the 1,000,000-byte upload ceiling, confirmed UI shutdown, the 50-product performance criterion, and stable `ProblemDetails.code` values; resolve any remaining discrepancy before implementation.
- [X] T002 [P] Scaffold the Angular workspace and configure its unit-test runner in `src/AffiliateMessages/ClientApp/angular.json`, `src/AffiliateMessages/ClientApp/package.json`, `src/AffiliateMessages/ClientApp/package-lock.json`, `src/AffiliateMessages/ClientApp/tsconfig.json`, `src/AffiliateMessages/ClientApp/tsconfig.app.json`, `src/AffiliateMessages/ClientApp/tsconfig.spec.json`, `src/AffiliateMessages/ClientApp/src/main.ts`, `src/AffiliateMessages/ClientApp/src/index.html`, and `src/AffiliateMessages/ClientApp/src/styles.css`; configure production output for `src/AffiliateMessages/wwwroot/`.
- [X] T003 [P] Add the ASP.NET Core shared framework reference and configure Angular static assets for build/publish in `src/AffiliateMessages/AffiliateMessages.csproj`.

---

## Phase 2: Foundational

**Purpose**: Provide the secure local host and shared API behavior required by all user stories.

**⚠️ CRITICAL**: Complete this phase before implementing user stories.

- [X] T004 [P] Add API integration tests for session-token issuance, exact `Host` validation, mutating-request `Origin`/`Sec-Fetch-Site` checks, safe `ProblemDetails` codes/details, and confirmation that Kestrel binds only to `127.0.0.1` on a nonzero OS-assigned port in `tests/AffiliateMessages.Tests/Api/LocalRequestSecurityTests.cs`, `tests/AffiliateMessages.Tests/Api/ApiProblemDetailsTests.cs`, and `tests/AffiliateMessages.Tests/Api/LocalUiHostTests.cs`.
- [X] T005 [P] Implement per-process CSRF token generation, exact `Host` validation, mutating-request `Origin`/`Sec-Fetch-Site` checks, and the `GET /api/session` response in `src/AffiliateMessages/Api/LocalRequestSecurity.cs`.
- [X] T006 [P] Implement safe `ProblemDetails` responses, stable error-code handling, and server-side exception logging without exposing stack traces or temporary paths in `src/AffiliateMessages/Api/ApiProblemDetails.cs`.
- [X] T007 Integrate `--ui` dispatch while preserving CLI behavior, start Kestrel only on `127.0.0.1:0`, serve the Angular build from the same origin without enabling CORS, register the security/error components, open the browser after the host is ready, and print the URL if browser launch fails in `src/AffiliateMessages/Program.cs` and `src/AffiliateMessages/Api/LocalUiHost.cs`.

**Checkpoint**: The console mode remains available and the local UI host is ready for secured endpoints.

---

## Phase 3: User Story 1 - Selecionar um CSV e iniciar a conversão (Priority: P1) 🎯 MVP

**Goal**: Let the user select a CSV, start one conversion, and see that processing is underway.

**Independent Test**: Select a valid CSV, start conversion, observe the loading state within two seconds, and ensure a second conversion cannot start concurrently; the existing CLI remains usable.

### Implementation for User Story 1

- [X] T008 [P] [US1] Add xUnit tests for valid batch results, invalid-row continuation, Ollama failure continuation, generated messages, and the saved output in `tests/AffiliateMessages.Tests/Services/MessageBatchProcessorTests.cs`; add API integration tests for missing, empty, wrong-extension, and over-1,000,000-byte uploads and their documented error responses in `tests/AffiliateMessages.Tests/Api/ConversionEndpointsTests.cs`.
- [X] T009 [P] [US1] Add a batch result with generated message texts, invalid-row count, and messages without a phrase in `src/AffiliateMessages/Services/BatchProcessResult.cs`; update `src/AffiliateMessages/Services/MessageBatchProcessor.cs` to return it after writing the output, without rereading the CSV or changing the CLI rules.
- [X] T010 [P] [US1] Add the conversion response types (`messages` with `sequence`/`text`, `outputPath`, `canOpenOutputFolder`, `invalidRowsCount`, and `messagesWithoutPhrase`) and an Angular API service that obtains the session token and posts the `file` multipart field with `X-CSRF-Token` in `src/AffiliateMessages/ClientApp/src/app/models/conversion-result.model.ts` and `src/AffiliateMessages/ClientApp/src/app/services/conversion-api.service.ts`.
- [X] T011 [US1] Implement `POST /api/conversions` in `src/AffiliateMessages/Api/ConversionEndpoints.cs`: accept exactly one `.csv` file, reject missing/zero-byte/wrong-extension uploads, enforce at most 1,000,000 file bytes while receiving, use a server-generated temporary path with cleanup in `finally`, allow only one active conversion, call `MessageBatchProcessor`, and return the documented response or matching `ProblemDetails`.
- [X] T012 [P] [US1] Add Angular component tests for file-picker cancellation, loading feedback, preventing concurrent submissions, and failed requests in `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.spec.ts`.
- [X] T013 [US1] Implement CSV selection and conversion submission in `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.ts`, `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.html`, and `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.css`, including extension/size feedback, preserving the previous selection when the picker is cancelled, and showing loading while the request is pending.
- [X] T014 [US1] Integrate the conversion form into the Angular root view and configure `HttpClient` in `src/AffiliateMessages/ClientApp/src/app/app.ts`, `src/AffiliateMessages/ClientApp/src/app/app.html`, `src/AffiliateMessages/ClientApp/src/app/app.css`, and `src/AffiliateMessages/ClientApp/src/app/app.config.ts`.

**Checkpoint**: The user can select and submit one CSV, see processing state, and the existing CLI path is preserved.

---

## Phase 4: User Story 2 - Consultar e copiar resultados e localizar o arquivo (Priority: P2)

**Goal**: Show each generated message and the saved path, support per-message copying, and offer the configured output folder.

**Independent Test**: After a successful conversion, inspect the ordered messages and output path, copy one message without neighboring text, and request opening the configured output folder.

### Implementation for User Story 2

- [X] T015 [P] [US2] Add Angular tests for ordered result display, per-message clipboard success/failure feedback, and folder-opening loading/error states in `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.spec.ts` and `src/AffiliateMessages/ClientApp/src/app/components/message-card.component.spec.ts`.
- [X] T016 [P] [US2] Create the result-list and message-card components in `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.ts`, `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.html`, `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.css`, `src/AffiliateMessages/ClientApp/src/app/components/message-card.component.ts`, `src/AffiliateMessages/ClientApp/src/app/components/message-card.component.html`, and `src/AffiliateMessages/ClientApp/src/app/components/message-card.component.css`; use `navigator.clipboard.writeText` only after the corresponding button action and represent pending, success, and failure separately.
- [X] T017 [P] [US2] Implement `POST /api/output/open-folder` in `src/AffiliateMessages/Api/ConversionEndpoints.cs`, resolving only the configured `AppOptions.OutputFile` parent directory, accepting no client path, and opening it on Windows with a separately supplied Explorer argument.
- [X] T018 [US2] Add the folder-opening call to `src/AffiliateMessages/ClientApp/src/app/services/conversion-api.service.ts` and integrate result display, absolute output path, folder-loading feedback, and folder errors in `src/AffiliateMessages/ClientApp/src/app/app.ts` and `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.ts`.

**Checkpoint**: A completed result is visible, each message can be copied independently, and folder-opening failures do not hide the saved path.

---

## Phase 5: User Story 3 - Entender uma falha de processamento (Priority: P3)

**Goal**: Distinguish invalid input and processing failures from successful or partial results.

**Independent Test**: Submit invalid or unreadable input and cause an API/processing failure; the UI retains the selected file, explains the failure, and never claims an unconfirmed result. Valid batches with skipped rows or unavailable Ollama still complete.

### Implementation for User Story 3

- [X] T019 [P] [US3] Add Angular tests for documented API error-code mapping, distinct conversion failure states, and partial-success notices in `src/AffiliateMessages/ClientApp/src/app/services/api-error-message.service.spec.ts`, `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.spec.ts`, and `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.spec.ts`.
- [X] T020 [P] [US3] Add a Portuguese user-message mapping for the documented `ProblemDetails.code` and status values in `src/AffiliateMessages/ClientApp/src/app/services/api-error-message.service.ts`.
- [X] T021 [US3] Integrate the error mapping into `src/AffiliateMessages/ClientApp/src/app/components/conversion-form.component.ts` so file, request, conflict, parser, and processing failures remain distinct, preserve the selected file, and never display a failed request as a saved result.
- [X] T022 [P] [US3] Present partial-success notices from `invalidRowsCount` and `messagesWithoutPhrase` in `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.ts` and `src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.html`, while keeping valid messages and the confirmed output path available.

**Checkpoint**: Input errors, global failures, and recoverable partial results have distinct and accurate UI states.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Complete the approved host lifecycle and keep operator documentation aligned.

- [X] T023 [P] Add API integration tests for shutdown CSRF protection, `409` during an active conversion, and `202` before host termination in `tests/AffiliateMessages.Tests/Api/HostLifecycleEndpointsTests.cs`.
- [X] T024 Implement the protected `POST /api/host/shutdown` endpoint in `src/AffiliateMessages/Api/HostLifecycleEndpoints.cs` and register it in `src/AffiliateMessages/Api/LocalUiHost.cs`; require the session CSRF token, return `409` while a conversion is active, and stop the host only after sending `202`.
- [X] T025 Add the shutdown API call and a confirmation-based **Encerrar aplicação** control in `src/AffiliateMessages/ClientApp/src/app/services/conversion-api.service.ts`, `src/AffiliateMessages/ClientApp/src/app/components/shutdown-control.component.ts`, `src/AffiliateMessages/ClientApp/src/app/components/shutdown-control.component.html`, `src/AffiliateMessages/ClientApp/src/app/components/shutdown-control.component.css`, `src/AffiliateMessages/ClientApp/src/app/app.ts`, and `src/AffiliateMessages/ClientApp/src/app/app.html`; disable the control during conversion and prevent further actions after shutdown is accepted.
- [X] T026 [P] Add an opt-in xUnit benchmark marked with `Trait("Category", "Performance")` for complete conversion of an embedded representative CSV fixture with 50 valid products, including CSV reading, processing, and output writing with local Ollama; require completion in under three minutes and record the reference Windows machine, .NET runtime, Ollama version, and model in `tests/AffiliateMessages.Tests/Performance/BatchProcessingPerformanceTests.cs`.
- [X] T027 Align `specs/002-interface-conversao/quickstart.md` with the final `--ui` startup, browser-launch fallback, 1,000,000-byte limit, conversion/result flows, UI shutdown behavior, and the hardware/fixture/runtime details required to reproduce the performance benchmark.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T001, T002, and T003 can run in parallel.
- **Foundational (Phase 2)**: Depends on Setup. T004 adds security/error tests; T005 and T006 implement the tested behavior and can run in parallel; T007 integrates them and blocks all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational. T008 adds batch/API tests before implementation; T009 and T010 implement the batch result and client contract in parallel; T011 depends on both; T012 adds Angular state tests before T013 implements the form; T014 integrates the form after T011 and T013.
- **User Story 2 (Phase 4)**: Depends on User Story 1. T015 adds UI behavior tests before T016 and T017 implement the result components and folder endpoint; T018 integrates both.
- **User Story 3 (Phase 5)**: Depends on User Stories 1 and 2. T019 adds error/partial-result UI tests before T020-T022 implement the corresponding behavior.
- **Polish (Phase 6)**: Depends on all desired stories. T023 adds shutdown API tests before T024 implements the endpoint; T025 adds the UI control; T026 benchmarks the 50-product performance criterion; T027 documents the benchmark environment and final quickstart flow after T026.

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational; no dependency on another user story.
- **User Story 2 (P2)**: Depends on User Story 1 for the conversion response and API client.
- **User Story 3 (P3)**: Depends on User Story 1 error contracts and User Story 2 result UI for partial-result notices.

### Parallel Opportunities

- Setup work T001/T002/T003 can proceed in parallel because the requirements documents, Angular workspace, and .NET project file are separate deliverables.
- In the foundational phase, after T004 defines the expected behavior, T005 and T006 touch separate API support files.
- In User Story 1, after T008 adds the focused tests, T009 (C# batch result) and T010 (Angular API types/client) can be developed in parallel against the documented contract.
- In User Story 2, after T015 adds the UI tests, T016 (Angular result/copy UI) and T017 (folder endpoint) touch separate files.
- In User Story 3, after T019 adds the UI tests, T020 (error-message mapping) and T022 (partial-result notices) touch separate files.
- After the user stories, T023 (shutdown API tests) and T026 (opt-in performance benchmark) touch separate test files and can proceed in parallel.
- User stories themselves are not independent in this design: P2 requires P1's result contract, and P3 builds on both conversion errors and the result view.

---

## Parallel Examples

### User Story 1

```text
Task: "Add BatchProcessResult and update MessageBatchProcessor in src/AffiliateMessages/Services/"
Task: "Add conversion models and API service in src/AffiliateMessages/ClientApp/src/app/"
```

### User Story 2

```text
Task: "Create conversion-results and message-card components in src/AffiliateMessages/ClientApp/src/app/components/"
Task: "Implement the configured-folder endpoint in src/AffiliateMessages/Api/ConversionEndpoints.cs"
```

### User Story 3

```text
Task: "Map API error codes to Portuguese messages in src/AffiliateMessages/ClientApp/src/app/services/api-error-message.service.ts"
Task: "Show partial-result counts in src/AffiliateMessages/ClientApp/src/app/components/conversion-results.component.html"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational phases, including security and error-response tests before their implementations.
2. Complete User Story 1: add batch/API tests, implement CSV selection and conversion, then verify the loading state and independent acceptance criteria.
3. Evaluate the User Story 1 independent acceptance criteria before adding result presentation.

### Incremental Delivery

1. Setup + Foundational → secure host and Angular shell.
2. User Story 1 → selection and conversion start (MVP).
3. User Story 2 → messages, copying, output path, and folder action.
4. User Story 3 → accurate error and partial-result feedback.
5. Polish → explicit UI shutdown action, 50-product performance benchmark, and final quickstart alignment.

## Notes

- Every task uses the required checkbox, sequential ID, optional `[P]`, story label only in user-story phases, and explicit repository paths.
- Focused xUnit, API integration, and Angular tests are explicit tasks before their corresponding batch, endpoint, and asynchronous UI implementations.
- The project constitution's test-first rule remains applicable to any parsing or formatting rules changed during implementation; preserve and run the existing parsing/formatting tests.
- `[P]` tasks use separate files and do not depend on unfinished tasks; story dependencies are recorded above.
