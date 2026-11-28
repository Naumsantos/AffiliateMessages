# Implementation Plan: Interface Angular local para conversão de CSV

**Branch**: `feat/shopee-message-generator` | **Date**: 2026-11-27 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-interface-conversao/spec.md`; user specified Angular, the browser Clipboard API, component loading states, and approved the single-executable/two-mode architecture conceptually.

## Summary

Adicionar uma interface Angular à aplicação .NET local existente. No modo `--ui`, o mesmo executável hospeda os arquivos Angular compilados e uma API HTTP de mesma origem, vinculada exclusivamente a `127.0.0.1`. O navegador envia o CSV selecionado ao processo local; o processador existente continua responsável por parsing, regras de negócio, geração, logs e gravação. A resposta retorna mensagens, contagens de linhas ignoradas e o caminho efetivamente salvo. O Angular mostra carregamento e erros, e usa `navigator.clipboard.writeText` no botão de cada mensagem. O fluxo CLI permanece disponível.

## Revisão arquitetural e decisões propostas

### Host local, proteção de requisições e inicialização

- O modo `--ui` inicia o Kestrel vinculado somente ao endereço IPv4 de loopback `127.0.0.1`; não usar `localhost`, `0.0.0.0` ou outra interface como endereço de escuta.
- Usar porta efêmera solicitada ao sistema operacional (`127.0.0.1:0`), obter a porta efetiva após o host iniciar e usar a origem `http://127.0.0.1:<porta>` para a UI e as chamadas da API. Isso evita colisões entre execuções sem expor o serviço à rede local.
- A UI estática e a API compartilham a mesma origem; CORS não será habilitado.
- Validar o `Host` esperado em todas as rotas locais. Para operações que alteram estado, exigir origem exata, `Sec-Fetch-Site: same-origin` e token anti-CSRF aleatório, gerado a cada execução e mantido somente em memória. A UI obtém o token por `GET /api/session`, com resposta `no-store`, e o envia em `X-CSRF-Token`. Rejeitar token ausente/incorreto, origem ausente/incorreta e requisições com site de origem cruzada.
- Após o host estar pronto, tentar abrir a URL no navegador padrão. Se isso falhar, manter o host ativo e imprimir a URL e a instrução para abri-la manualmente; falha ao iniciar o navegador não deve ser apresentada como falha de conversão.
- Incluir na UI uma ação explícita e protegida para encerrar a aplicação, com confirmação e desativada enquanto uma conversão estiver ativa. O endpoint correspondente aceita o encerramento somente quando não há lote em andamento e para o host após concluir a resposta. Fechar apenas a aba não encerra o processo; `Ctrl+C` permanece como alternativa e cancela requisições em andamento, limpando uploads temporários.

### Validação do arquivo CSV

- O seletor Angular aceita `.csv` como conveniência; o servidor repete a validação e compara a extensão sem diferenciar maiúsculas/minúsculas. Nunca confiar no `Content-Type` nem usar o nome enviado pelo cliente como caminho no servidor.
- Rejeitar arquivo ausente, arquivo vazio, extensão diferente de `.csv` e conteúdo que provoque erro estrutural do parser. Erros de entrada recebem status apropriado e não iniciam a geração; falhas de I/O no host durante gravação/leitura temporária são erros internos (`500`), registrados localmente sem expor detalhes.
- Limitar o conteúdo do arquivo a **1 MB (1.000.000 bytes)**, conforme aprovado pelo usuário. Validar o tamanho informado e também limitar a leitura real para impedir que um `Content-Length` incorreto ultrapasse o limite. O limite HTTP deve considerar a pequena sobrecarga do `multipart/form-data`.
- Usar um nome temporário aleatório criado pelo servidor e removê-lo em `finally`, em sucesso, erro ou cancelamento. Não definir um limite de linhas nesta feature.
- Erros de validação de uma linha continuam sendo tratados pelo `CsvReader` e `MessageBatchProcessor`: a linha é registrada em `logs.txt` e o lote segue. Um CSV não vazio com zero linhas válidas conclui com lista vazia, arquivo de saída vazio e aviso/contagem na tela; isso preserva a resiliência existente. Arquivo de zero bytes é erro de entrada.

### Contratos, erros e reutilização do processamento

- Especificar `GET /api/session`, `POST /api/conversions`, `POST /api/output/open-folder` e `POST /api/host/shutdown` em [contracts/http-api.md](./contracts/http-api.md), incluindo campos, códigos HTTP e códigos de erro estáveis.
- A resposta de conversão inclui `messages`, caminho absoluto de saída, disponibilidade da ação de pasta e contagens de linhas inválidas/mensagens sem frase. Erros de transporte/processamento usam `ProblemDetails` sem stack trace ou detalhes internos.
- O processador retorna sucesso quando grava o arquivo, inclusive com mensagens vazias após ignorar linhas inválidas; uma falha de leitura, processamento global ou gravação retorna erro e não é mostrada como conclusão. A indisponibilidade da geração opcional de frases permanece fallback resiliente e aparece como aviso de resultado parcial, não como falha total.
- Reutilizar `CsvReader`, `MessageBatchProcessor`, `MessageFormatter`, `OllamaClient`, `InvalidLineLogger` e `MessageWriter`. Não reproduzir parsing CSV, formatação, chamadas ao Ollama ou escrita do arquivo em TypeScript. Alterar o resultado de `MessageBatchProcessor.ProcessAsync` para retornar as mensagens e as contagens que ele já conhece, sem reler o arquivo.

### Abertura da pasta de saída

- A API não recebe parâmetro de caminho. Ela resolve exclusivamente o caminho configurado em `AppOptions.OutputFile`, usa a pasta pai do arquivo e só oferece a ação quando a saída existe e o host consegue abri-la.
- No Windows, iniciar o Explorer com o caminho da pasta como argumento separado, sem `cmd.exe`, concatenação de comandos ou caminho fornecido pelo navegador. Validar o caminho absoluto e a existência da pasta antes de iniciar o processo.
- Em falha, retornar erro seguro; o frontend mantém o caminho do resultado visível e não mostra sucesso na ação de abertura.

## Decisões confirmadas após revisão

1. **Limite de upload**: máximo de 1 MB decimal (1.000.000 bytes) por arquivo CSV.
2. **Ciclo de vida do host**: ação explícita e protegida **Encerrar aplicação** na UI; requer confirmação e fica indisponível durante conversões. Fechar a aba não encerra o processo; `Ctrl+C` permanece como alternativa.

## Technical Context

**Language/Version**: C#/.NET 8 (existing application); Angular 22.3.0 and TypeScript; Node.js 24.18.0

**Primary Dependencies**: CsvHelper e xUnit existentes; framework compartilhado ASP.NET Core para hospedagem HTTP local/arquivos estáticos; Angular CLI; Clipboard API do navegador

**Storage**: Upload temporário local durante a conversão; `messages.txt` e `logs.txt` existentes; sem banco de dados ou histórico persistente de conversões

**Testing**: Suíte xUnit existente e testes de integração focados da API, incluindo bind exclusivo em loopback e porta atribuída pelo sistema operacional; testes Angular CLI de componentes/serviços para estados de carregamento, sucesso e falha; benchmark opt-in da conversão de 50 produtos no ambiente de referência documentado no quickstart

**Target Platform**: Windows desktop, acessado por navegador em endereço loopback

**Project Type**: Executável de console .NET existente, com modo opcional de hospedagem web local e cliente Angular no mesmo projeto

**Performance Goals**: Critério de aceite: concluir a conversão completa, incluindo leitura do CSV, processamento e gravação, de uma fixture representativa de 50 produtos válidos em menos de três minutos, com Ollama local disponível e no ambiente de referência documentado no quickstart; mostrar o estado de processamento em até dois segundos; exibir o feedback de cópia após a conclusão da operação da área de transferência

**Constraints**: Dados dos produtos e requisições ao Ollama permanecem locais; bind exclusivo em `127.0.0.1`; validação de Host/Origin e token anti-CSRF; sem CORS; preservar CLI, regras CSV, formato e caminho de saída; limite de upload aprovado de 1 MB decimal; uso da Clipboard API somente após ação explícita

**Scale/Scope**: Um usuário local e uma conversão por vez; uma tela de resultado com as mensagens geradas; sem contas, banco de dados, implantação remota ou envio automático de mensagens

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **Execução 100% Local — PASSA**: Navegador, host HTTP local, arquivos e Ollama permanecem na mesma máquina. O host escuta somente no loopback e não envia dados de produtos a serviços na nuvem.
- **Resiliência — PASSA**: O processador de lote existente continua ignorando linhas inválidas e seguindo quando a geração de frases local falha. Falhas da requisição são apresentadas explicitamente na interface.
- **Test-First — PASSA**: Criar testes focados para o resultado do lote, comportamento da API e estados assíncronos do Angular antes de implementar esses comportamentos. Manter os testes de parsing e formatação existentes.
- **Simplicidade Técnica — PASSA, com justificativa**: Angular foi solicitado explicitamente, e a ponte HTTP local é necessária para o navegador chamar o processador C# existente. Manter a ponte no executável atual; não adicionar projeto backend separado, banco de dados ou serviço externo.
- **Idioma — PASSA**: Rótulos e feedback voltados ao usuário ficam em português do Brasil; identificadores de código seguem a convenção existente em inglês.
- **Restrições técnicas — PASSA**: Manter o target .NET atual, a integração local com Ollama, a saída `messages.txt` e a proibição de integração automática com WhatsApp ou Mercado Livre.

### Reavaliação após o design

- O Kestrel é vinculado exclusivamente a `127.0.0.1`; host, origem e token anti-CSRF são validados, sem CORS permissivo.
- A API não aceita caminhos de pasta do cliente e serve a UI na mesma origem.
- O limite de arquivo de 1 MB decimal e a ação explícita de encerramento na UI foram aprovados. Fechar a aba, por si só, não encerra o processo.
- Erros de entrada e processamento são apresentados como mensagens acionáveis, sem stack traces; linhas inválidas e fallback local de IA continuam sendo resultados parciais resilientes.
- O fluxo CLI e as regras de geração permanecem. Nenhuma dependência de banco de dados ou nuvem é introduzida.
- O design mantém .NET 8 para cumprir a constituição atual. O suporte dessa versão terminou; isso está registrado como risco de lançamento em [research.md](./research.md), sem mudança silenciosa nesta feature.

## Project Structure

### Documentation (this feature)

```text
specs/002-interface-conversao/
├── plan.md
├── research.md
├── data-model.md
├── contracts/
│   └── http-api.md
└── quickstart.md
```

### Source Code (repository root)

```text
src/
└── AffiliateMessages/
    ├── AffiliateMessages.csproj
    ├── Program.cs
    ├── Api/
    │   ├── ApiProblemDetails.cs
    │   ├── ConversionEndpoints.cs
    │   ├── HostLifecycleEndpoints.cs
    │   ├── LocalRequestSecurity.cs
    │   └── LocalUiHost.cs
    ├── ClientApp/
    │   ├── angular.json
    │   ├── package.json
    │   ├── package-lock.json
    │   ├── tsconfig.json
    │   ├── tsconfig.app.json
    │   └── src/
    │       ├── index.html
    │       ├── main.ts
    │       ├── styles.css
    │       └── app/
    │           ├── components/
    │           │   └── *.spec.ts
    │           ├── models/
    │           └── services/
    │               └── *.spec.ts
    ├── Configuration/
    ├── Models/
    ├── Services/
    └── wwwroot/                 # Angular build output, not hand-maintained source

tests/
└── AffiliateMessages.Tests/
    ├── Api/
    ├── Performance/
    └── Services/
```

Os testes de componentes e serviços Angular ficam junto ao código em `ClientApp/src/app`. Os arquivos `ApiProblemDetails.cs`, `ConversionEndpoints.cs`, `HostLifecycleEndpoints.cs`, `LocalRequestSecurity.cs` e `LocalUiHost.cs` contêm as rotas e o suporte da API local. O benchmark opt-in de performance fica em `tests/AffiliateMessages.Tests/Performance`. `wwwroot` contém os arquivos estáticos gerados. Os serviços e testes existentes permanecem em seus locais atuais.

**Structure Decision**: Manter o executável .NET atual para CLI e para o host local opcional (modo `--ui`). Adicionar um workspace Angular em `ClientApp` e compilar os arquivos estáticos para `wwwroot`. Manter a fronteira HTTP pequena e no mesmo processo para reutilizar serviços existentes sem um segundo serviço .NET ou unidade de implantação. O host seleciona uma porta efêmera em `127.0.0.1`, abre o navegador após iniciar, pode ser encerrado pela ação protegida na UI ou por `Ctrl+C`, e não encerra somente porque a aba foi fechada.

## Complexity Tracking

| Adição | Motivo | Alternativa mais simples rejeitada |
|---|---|---|
| Cliente Angular e hospedagem HTTP local mínima no executável existente | Os requisitos explícitos de Angular/navegador precisam enviar o arquivo selecionado ao processador C# e receber as mensagens geradas. | O fluxo somente por console não oferece a interface nem a Clipboard API; um processo backend separado acrescentaria complexidade sem capacidade adicional. |
