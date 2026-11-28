# Contrato da API HTTP local

A API é privada ao cliente Angular e ao processo local. No modo de interface, servir o build Angular e estes endpoints na mesma origem, vinculada exclusivamente a `127.0.0.1` em uma porta efêmera atribuída pelo sistema operacional. Não habilitar CORS. Validar o `Host` esperado em todas as rotas. Não aceitar requisições mutáveis sem `Origin` exata, `Sec-Fetch-Site: same-origin` e o token anti-CSRF da execução em `X-CSRF-Token`.

O token é aleatório por execução, mantido somente em memória e obtido pelo cliente Angular na rota segura `GET /api/session`. Essa resposta usa `Cache-Control: no-store`, `Cross-Origin-Resource-Policy: same-origin` e `X-Content-Type-Options: nosniff`; a API não usa cookies de autenticação.

## `GET /api/session`

Fornece ao cliente da mesma origem o token necessário para chamadas mutáveis.

### Sucesso: `200 OK`

```json
{
  "csrfToken": "<token aleatório desta execução>"
}
```

Não habilitar CORS para esta rota. Validar `Host` e rejeitar `Sec-Fetch-Site` diferente de `same-origin` quando esse cabeçalho estiver presente; impedir cache e carregamento por contexto de origem cruzada.

## `POST /api/conversions`

Inicia e aguarda uma conversão CSV.

### Requisição

- Tipo de conteúdo: `multipart/form-data`; outro tipo de conteúdo é rejeitado.
- Campo obrigatório: `file` (um arquivo `.csv` selecionado pelo usuário)
- O cliente envia o conteúdo, não um caminho local.
- Cabeçalho obrigatório: `X-CSRF-Token` com o token retornado por `GET /api/session`.
- `Origin` deve corresponder exatamente a `http://127.0.0.1:<porta-atribuída>`; `Host` deve corresponder ao endereço/porta efetivos.
- Extensão `.csv` sem distinção entre maiúsculas/minúsculas. `Content-Type` e nome de caminho do cliente não são confiáveis.
- Limite aprovado para o conteúdo do arquivo: 1 MB decimal (1.000.000 bytes). O servidor aplica o limite durante a leitura, não apenas com base no cabeçalho de tamanho.

### Sucesso: `200 OK`

```json
{
  "messages": [
    {
      "sequence": 1,
      "text": "Nome do produto\n✅ POR : R$ 79,19\nhttps://afiliado.example"
    }
  ],
  "outputPath": "C:\\dados\\messages.txt",
  "canOpenOutputFolder": true,
  "invalidRowsCount": 2,
  "messagesWithoutPhrase": 1
}
```

`messages` pode estar vazio quando um CSV não vazio não contém linhas válidas. A conversão é concluída com arquivo vazio, contagem de linhas ignoradas e aviso de que não foram geradas mensagens; linhas inválidas ficam em `logs.txt`. `messagesWithoutPhrase` informa mensagens produzidas sem frase pela falha do recurso opcional local. Cada texto corresponde à mensagem completa gravada no arquivo de saída, sem o divisor entre mensagens.

### Validação e erros

Retornar `application/problem+json` com `code`, `title` e `detail` apropriados para o usuário; não incluir stack traces, caminhos temporários ou detalhes internos. `code` é obrigatório, estável e usa os valores definidos nas tabelas abaixo. Registrar a exceção detalhada apenas no log local.

| Status | `code` | Significado |
|---|---|---|
| `400 Bad Request` | `invalid_file` | Arquivo ausente, extensão incorreta ou arquivo de zero bytes |
| `403 Forbidden` | `request_forbidden` | `Host`, `Origin`, `Sec-Fetch-Site` ou token anti-CSRF inválido |
| `409 Conflict` | `conversion_in_progress` | Outra conversão já está em andamento |
| `413 Content Too Large` | `file_too_large` | Arquivo excede o limite aprovado |
| `415 Unsupported Media Type` | `unsupported_media_type` | Requisição não usa `multipart/form-data` |
| `422 Unprocessable Entity` | `invalid_csv` | Erro estrutural impede o parser CSV de ler o arquivo |
| `500 Internal Server Error` | `conversion_failed` | Falha de I/O no host, falha global do processador ou falha na gravação da saída |

Valores de linha inválidos não retornam erro HTTP: o processador existente registra cada linha e continua o lote. O host grava o upload em um arquivo temporário local, com nome criado no servidor, e o remove em sucesso, erro ou cancelamento. Os caminhos configurados para resultado e log permanecem como fonte de verdade.

## `POST /api/output/open-folder`

Abre a pasta que contém o arquivo de saída configurado.

### Requisição

- Corpo vazio; nenhum parâmetro de caminho é aceito.
- Cabeçalho obrigatório `X-CSRF-Token`; as mesmas verificações de `Origin`, `Host` e `Sec-Fetch-Site` do endpoint de conversão.

### Respostas

| Status | `code` | Significado |
|---|---|---|
| `204 No Content` | — | O sistema operacional local aceitou a solicitação de abertura |
| `403 Forbidden` | `request_forbidden` | Verificação de origem ou token anti-CSRF falhou |
| `404 Not Found` | `output_folder_not_available` | O arquivo ou a pasta de saída ainda não está disponível |
| `500 Internal Server Error` | `output_folder_open_failed` | O host local não conseguiu abrir a pasta |

Somente a pasta pai do arquivo configurado em `AppOptions.OutputFile` pode ser aberta. O endpoint não aceita caminho, nome ou argumento de processo do cliente. No Windows, a abertura usa o Explorer e passa a pasta como argumento separado, sem executar uma linha de comando composta. Se a ação falhar, o cliente mantém `outputPath` visível.

## `POST /api/host/shutdown`

Solicita o encerramento do host iniciado no modo `--ui`.

### Requisição

- Corpo vazio; nenhum comando, caminho ou processo é aceito do cliente.
- Cabeçalho obrigatório `X-CSRF-Token`; as mesmas verificações de `Origin`, `Host` e `Sec-Fetch-Site` dos outros endpoints mutáveis.
- A UI pede confirmação e desativa a ação enquanto uma conversão estiver ativa. O servidor também rejeita o encerramento se houver conversão em andamento.

### Respostas

| Status | `code` | Significado |
|---|---|---|
| `202 Accepted` | — | Encerramento aceito; o host para depois que a resposta for enviada |
| `403 Forbidden` | `request_forbidden` | Verificação de origem ou token anti-CSRF falhou |
| `409 Conflict` | `conversion_in_progress` | Há uma conversão em andamento; ela precisa terminar antes do encerramento |

Fechar a aba não encerra o host. `Ctrl+C` no console continua disponível como alternativa; se usado durante uma conversão, cancela a requisição e limpa o upload temporário.

## Localidade e tratamento de falhas

- O host escuta somente em `127.0.0.1`; requisições com host/origem divergentes ou sem token anti-CSRF são rejeitadas.
- O cliente Angular mostra imediatamente o estado de processamento enquanto `POST /api/conversions` está pendente e só informa conclusão após uma resposta bem-sucedida.
- Erros de entrada são mostrados junto ao seletor; conflitos, falhas de processamento e falhas de escrita recebem mensagens distintas e sem detalhes internos. Uma requisição interrompida não é apresentada como resultado concluído.
- O encerramento pela UI solicita confirmação, não é oferecido durante a conversão e só mostra aceitação depois de `202`; o host encerra após enviar essa resposta.
- As operações da área de transferência ocorrem no navegador e não fazem parte desta API HTTP.
