# Research: Interface Angular local para conversão de CSV

## Decisões

### Cliente Angular e ferramentas compatíveis

- **Decisão**: Usar Angular 22.3.0 e Angular CLI 22.3.0. Usar Node.js 24.18.0 e npm 11.16.0 já disponíveis no ambiente.
- **Justificativa**: O registro npm indica a versão 22.3.0 para `@angular/core` e `@angular/cli`; os requisitos de Node incluem 24.15 ou superior, compatível com Node 24.18. Angular é a escolha explícita do usuário.
- **Alternativas consideradas**: Usar uma versão Angular anterior ou outra tecnologia de frontend. Nenhuma melhora a compatibilidade com o ambiente Node atual ou atende melhor ao pedido.

### Comportamento da área de transferência

- **Decisão**: Chamar `navigator.clipboard.writeText(message.text)` somente pelo clique no botão da mensagem correspondente. Aguardar a promise; mostrar sucesso apenas quando ela for concluída e uma mensagem de erro em português se for rejeitada. Não recorrer silenciosamente a APIs de cópia obsoletas.
- **Justificativa**: A documentação MDN descreve `writeText()` como assíncrona, restrita a contexto seguro e sujeita a rejeição com `NotAllowedError`. O clique explícito fornece a ação do usuário. Hospedar em `http://127.0.0.1` permite o contexto de loopback confiável sem implantação HTTPS pública.
- **Alternativas consideradas**: `document.execCommand("copy")` está obsoleto e não atende à escolha solicitada. Copiar automaticamente ao carregar a tela não seria uma ação iniciada pelo usuário.

### Carregamento e feedback por componente

- **Decisão**: Manter signals de estado assíncrono nos componentes Angular responsáveis por cada operação: processamento da conversão, cópia da mensagem selecionada e abertura da pasta. Desabilitar a ação correspondente enquanto estiver pendente; mostrar conclusão e erro sem ocultar a lista de resultados.
- **Justificativa**: Atende ao pedido de estado de carregamento por componente sem adicionar uma biblioteca de estado global nem um subsistema de jobs/polling. O indicador de conversão aparece ao enviar o arquivo; a resposta da API fornece o estado final e o resultado.
- **Alternativas consideradas**: Adicionar uma dependência de estado compartilhado ou polling de jobs em segundo plano. Nenhuma é necessária para um usuário local e uma conversão ativa.

### Fronteira local entre navegador e processador

- **Decisão**: Manter o executável .NET existente e adicionar o modo opcional `--ui`, que hospeda o build estático do Angular e uma pequena interface HTTP ASP.NET Core em `127.0.0.1`. Servir a UI de produção e a API na mesma origem. Preservar o modo CLI atual.
- **Justificativa**: O navegador não pode chamar diretamente o processador C# nem ler um caminho arbitrário do sistema de arquivos. Um host loopback recebe o conteúdo do `File` selecionado localmente, reutiliza o processador e retorna o caminho e as mensagens sem serviço de nuvem ou segundo backend. O Kestrel permite vincular um endereço específico; o host não deve escutar em interface pública.
- **Alternativas consideradas**: Um projeto ASP.NET Core separado adiciona processo e unidade de implantação. Um Angular somente no navegador não executa o fluxo .NET/Ollama existente nem abre a pasta de saída. Uma API remota viola o princípio local do projeto.

### Arquivo, resposta do resultado e ação de pasta

- **Decisão**: Enviar o CSV selecionado como `multipart/form-data` ao host loopback. Copiá-lo para um arquivo temporário local com nome único, executar o processador de lote existente, remover o temporário em um bloco `finally` e retornar a lista de mensagens geradas e o caminho absoluto configurado para a saída. Ajustar `MessageBatchProcessor.ProcessAsync` para retornar os textos que já monta, depois de gravar o arquivo. A ação de pasta só pode abrir a pasta de saída da aplicação; o navegador nunca fornece um caminho.
- **Justificativa**: O seletor do navegador expõe o conteúdo/objeto do arquivo, não um caminho absoluto confiável. Enviar o conteúdo ao host na mesma máquina mantém o processamento local. Retornar a lista permite exibir e copiar cada mensagem independentemente enquanto o gravador existente mantém a criação de `messages.txt`.
- **Alternativas consideradas**: Reimplementar parsing e formatação CSV em TypeScript duplicaria regras de negócio. Aceitar um caminho arbitrário para abrir pasta ampliaria desnecessariamente a API local.

### Proteção das requisições locais

- **Decisão**: Vincular Kestrel exclusivamente a `IPAddress.Loopback` (`127.0.0.1`), sem aliases ou interfaces externas. Usar porta efêmera, compartilhar uma única origem, desabilitar CORS, validar `Host`/`Origin` e `Sec-Fetch-Site`, e exigir um token CSRF aleatório por execução em todas as operações mutáveis. O token vem de `GET /api/session`, sem cache, e fica em memória.
- **Justificativa**: Loopback limita a exposição de rede, mas não impede por si só que uma página de outra origem tente requisições ao localhost. A origem estrita, o cabeçalho não-simples com token e a ausência de CORS impedem que uma página externa leia o token ou invoque operações por formulário. A API não usa cookies de autenticação.
- **Alternativas consideradas**: Vincular a todas as interfaces, permitir CORS wildcard ou depender somente do token sem validar `Origin`/`Host` ampliaria a superfície local.

### Validação do upload

- **Decisão**: Validar extensão `.csv` no servidor, ignorando caixa, rejeitar arquivo vazio, limitar bytes durante a cópia a 1 MB decimal (1.000.000 bytes), conforme aprovação do usuário, e usar um nome temporário gerado no servidor. Não confiar no MIME type ou em `Content-Length`.
- **Justificativa**: O browser filtra arquivos por conveniência, mas o servidor é a fronteira de confiança. A validação por tamanho real impede uploads maiores que o limite configurado; nomes aleatórios evitam traversal e sobrescrita de arquivos.
- **Tratamento de conteúdo**: Reutilizar `CsvReader` para as regras CSV. Linhas inválidas permanecem erros de linha recuperáveis; erro estrutural de parser retorna `422`; CSV de zero bytes retorna `400`; CSV não vazio sem linhas válidas conclui vazio, registra linhas inválidas e exibe aviso, preservando a resiliência.

### Contrato e apresentação de erros

- **Decisão**: Usar quatro endpoints same-origin documentados em [contracts/http-api.md](./contracts/http-api.md): `GET /api/session`, `POST /api/conversions`, `POST /api/output/open-folder` e `POST /api/host/shutdown`. A conversão retorna a lista ordenada de mensagens, caminho absoluto, capacidade de abrir a pasta e contagens de resultado parcial.
- **Justificativa**: Um contrato pequeno evita que o Angular reimplemente regras do domínio. `ProblemDetails` com códigos estáveis separa erro de entrada, CSRF, concorrência, parser e falha do servidor. Mensagens visíveis são em pt-BR; stack trace e detalhes internos ficam apenas nos logs.
- **Tratamento parcial**: Linhas inválidas e frase local indisponível não derrubam um lote que conseguiu gravar a saída. A resposta informa contagens e a UI mostra aviso. Falha de leitura, processamento global ou gravação é erro e não publica resultado como concluído.

### Pasta de saída e ciclo de vida

- **Decisão**: O endpoint de pasta não aceita caminho do cliente; resolve `AppOptions.OutputFile`, verifica que a pasta/arquivo existe e abre somente a pasta pai configurada. No Windows, passar a pasta como argumento separado ao Explorer, sem shell composto. O host inicia com `--ui`, usa porta efêmera em `127.0.0.1` e abre o browser somente depois de escutar.
- **Ciclo de vida aprovado**: Se a abertura automática do navegador falhar, imprimir a URL e manter o host ativo. A UI fornece ação explícita de encerramento, protegida por CSRF, confirmação e desativada durante conversões; o endpoint rejeita encerramento enquanto houver lote ativo e para o host depois de enviar `202`. `Ctrl+C` permanece como alternativa e cancela requisição ativa/limpa temporários. Fechar a aba não encerra o processo, pois o browser não oferece sinal confiável de encerramento da aba.

### Reutilização dos serviços

- **Decisão**: O endpoint valida e armazena temporariamente o upload, mas delega ao `MessageBatchProcessor` existente. O processador passa a devolver a lista de mensagens, quantidade de linhas inválidas e quantidade de frases omitidas após gravar pelo `MessageWriter`.
- **Justificativa**: O `CsvReader` atual já contém parsing e regras de linha; `MessageFormatter`, `OllamaClient`, `InvalidLineLogger` e `MessageWriter` já determinam o restante do comportamento. Retornar o resultado calculado evita reler o CSV e impede duplicação das regras no TypeScript.

### Risco do ciclo de suporte do .NET

- **Decisão**: Manter `net8.0` neste plano por ser uma restrição explícita do projeto; não alterar silenciosamente o target framework.
- **Justificativa**: A política Microsoft informa que o suporte ao .NET 8 terminou em 10 de novembro de 2026, antes da data deste plano. A constituição atual ainda exige .NET 8, portanto migrar para uma versão suportada requer decisão explícita sobre a constituição/projeto. O ambiente tem SDKs .NET 9.0.317 e 10.0.401 e runtimes .NET 8.0.30, mas não o SDK 8; a compilação deve confirmar a disponibilidade do targeting pack para `net8.0`.
- **Alternativas consideradas**: Migrar agora para .NET 10 LTS. É uma opção plausível de manutenção, mas violaria a constituição atual sem a emenda necessária e está fora do escopo desta feature.

## Referências

- [Metadados do pacote Angular](https://www.npmjs.com/package/@angular/core) e do [Angular CLI](https://www.npmjs.com/package/@angular/cli); versões e requisitos de engine verificados com `npm view`.
- Guias oficiais de [Signals](https://angular.dev/guide/signals) e [testes Angular](https://angular.dev/guide/testing).
- [MDN: Clipboard API](https://developer.mozilla.org/en-US/docs/Web/API/Clipboard_API) e [Clipboard.writeText()](https://developer.mozilla.org/en-US/docs/Web/API/Clipboard/writeText).
- [MDN: contextos seguros](https://developer.mozilla.org/en-US/docs/Web/Security/Defenses/Secure_Contexts).
- [Microsoft Learn: endpoints do Kestrel](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0).
- [Microsoft Learn: prevenção de CSRF no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0).
- [Política de suporte do .NET da Microsoft](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core).
