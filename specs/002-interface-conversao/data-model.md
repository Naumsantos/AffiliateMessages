# Modelo de dados: Interface local para conversão de CSV

## Entidades

### CSV selecionado

Representa a entrada escolhida no navegador antes e durante uma conversão.

| Campo | Tipo | Regras |
|---|---|---|
| `file` | `File` do navegador | Necessário para iniciar; vem do seletor de arquivos |
| `fileName` | String | Somente nome para exibição; nunca é tratado como caminho do sistema de arquivos do servidor |
| `sizeBytes` | Inteiro | Conferido no cliente para feedback rápido e novamente no servidor durante o recebimento |

O navegador envia o conteúdo do arquivo à API local. O servidor valida a extensão `.csv` sem diferenciar maiúsculas/minúsculas, rejeita arquivo vazio e aplica o limite aprovado de 1 MB decimal (1.000.000 bytes) durante a leitura. O servidor atribui um nome temporário aleatório e remove o arquivo temporário após o processamento, tanto em caso de sucesso quanto de falha.

Um CSV não vazio cujo parser reconheça linhas, mas não encontre nenhuma linha válida, continua sendo processado: o resultado é concluído sem mensagens, as linhas inválidas são registradas e a interface mostra o aviso correspondente. Erros estruturais do parser são erros de entrada.

### Mensagem gerada

Representa uma mensagem completa produzida pelo formatador existente e exibida como um resultado individual.

| Campo | Tipo | Regras |
|---|---|---|
| `sequence` | Inteiro positivo | Ordenação estável nesta resposta; único na resposta |
| `text` | String | Texto exato da mensagem gravada em `messages.txt`, sem o divisor entre mensagens |

A resposta contém somente mensagens geradas. Linhas CSV inválidas continuam registradas e ignoradas pelo processador existente. Se o gerador local de frases estiver indisponível, a mensagem pode ser válida sem frase.

### Resultado da conversão

Representa um lote concluído, retornado à tela Angular.

| Campo | Tipo | Regras |
|---|---|---|
| `messages` | Lista de `GeneratedMessage` | Pode estar vazia se nenhuma mensagem válida tiver sido produzida |
| `outputPath` | Caminho local absoluto | Arquivo de saída configurado e realmente gravado pelo processador |
| `canOpenOutputFolder` | Booleano | Verdadeiro somente quando o host local pode oferecer a ação para abrir a pasta de saída configurada |
| `invalidRowsCount` | Inteiro não negativo | Número de linhas ignoradas e registradas pelo processador |
| `messagesWithoutPhrase` | Inteiro não negativo | Número de mensagens válidas geradas sem frase pelo fallback local |

Não há histórico persistente de resultados nem banco de dados. O resultado permanece no estado da tela atual e no arquivo de saída existente.

### Sessão HTTP local

| Campo | Tipo | Regras |
|---|---|---|
| `csrfToken` | String aleatória | Gerada por execução, mantida em memória e enviada pelo cliente em cada operação mutável |

O token não é persistido nem representa autenticação de usuário. Ele é usado junto à validação estrita de `Host`, `Origin` e `Sec-Fetch-Site`.

### Estado de encerramento da aplicação

Estado somente no cliente para a ação explícita de encerramento: `idle`, `confirming` ou `shuttingDown`. A ação exige confirmação e fica indisponível durante uma conversão. Após a API aceitar o pedido, a interface não permite novas operações; o host encerra depois de enviar a resposta `202 Accepted`.

### Estado da tela de conversão

Estado somente no cliente para o fluxo principal.

| Estado | Significado | Próximos estados permitidos |
|---|---|---|
| `idle` | Nenhum arquivo selecionado | `selected` |
| `selected` | Um CSV está pronto para envio | `processing`, `selected`, `idle` |
| `processing` | O upload e a conversão estão pendentes | `completed`, `failed` |
| `completed` | Um `ConversionResult` está disponível | `selected`, `processing`, `idle` |
| `failed` | A requisição falhou e o erro está visível | `selected`, `processing`, `idle` |

Cancelar o seletor preserva a seleção anterior. A interface permite somente uma conversão ativa por vez; a ação de envio fica desativada em `processing`.

Ao submeter uma nova conversão, a tela limpa o resultado anterior e mostra carregamento imediatamente. Erros de extensão, tamanho ou arquivo vazio ficam junto ao seletor; `409` indica lote já em andamento; falhas gerais mostram mensagem segura e não indicam conclusão nem resultado salvo.

### Estado da cópia

Estado somente no cliente, associado ao botão de cópia de cada mensagem.

| Estado | Significado |
|---|---|
| `idle` | Nenhuma cópia pendente |
| `copying` | `navigator.clipboard.writeText` está pendente para esta mensagem |
| `copied` | A promise da área de transferência foi concluída e o feedback de sucesso está visível |
| `failed` | A operação da área de transferência foi rejeitada e um erro está visível |

O estado é independente por mensagem; uma falha não pode ser apresentada como cópia bem-sucedida.

### Estado da ação de pasta

Estado somente no cliente para a ação da pasta configurada: `idle`, `opening` ou `failed`. A API não recebe um caminho fornecido pelo cliente; ela só pode abrir a pasta pai do `AppOptions.OutputFile`.

## Relações e ciclo de vida

- Um `SelectedCsv` é enviado em uma requisição de conversão.
- Um `ConversionResult` contém zero ou mais itens `GeneratedMessage` ordenados.
- Cada `GeneratedMessage` exibida tem seu próprio `CopyState`.
- O processador de lote existente continua responsável pela formatação, divisores, registro de linhas inválidas e fallback quando o Ollama local falha; seu resultado é ampliado para devolver mensagens e contagens sem reler nem reimplementar o CSV.
- Ao submeter uma nova conversão, a tela limpa o resultado anterior; somente uma resposta bem-sucedida preenche o novo resultado. Uma requisição com falha não pode ser apresentada como resultado salvo.
