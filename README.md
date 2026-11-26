# AffiliateMessages

Aplicativo de console local para gerar mensagens promocionais prontas para copiar no
WhatsApp a partir de um CSV exportado da Shopee. As frases promocionais sao geradas pelo
Ollama local; os dados dos produtos nao sao enviados a servicos de IA na nuvem.

## Requisitos

- .NET 8 SDK
- Ollama instalado e em execucao em `http://localhost:11434`
- Modelo `llama3.2:latest` disponivel localmente
- CSV UTF-8 exportado da Shopee

Para instalar o modelo:

```powershell
ollama pull llama3.2:latest
```

## Formato do CSV

O programa usa as mesmas posicoes com ou sem cabecalho:

| Coluna (indice) | Conteudo | Uso |
|---|---|---|
| 0 | ID | Opcional |
| 1 | Nome do produto | Pode ficar vazio |
| 2 | Preco | Obrigatorio; formato brasileiro |
| 3 | Vendas | Ignorado |
| 4 | Loja | Ignorado |
| 5 | Taxa | Ignorado |
| 6 | Comissao | Ignorado |
| 7 | Link do produto | Ignorado |
| 8 | Link de afiliado | Obrigatorio |

O CSV deve seguir as regras usuais de quoting; campos que contenham virgulas devem estar
entre aspas, por exemplo `"79,19"` ou `"Sandalia, feminina"`.

## Executar

Na raiz do repositorio:

```powershell
dotnet run --project src/AffiliateMessages -- --input "C:\dados\produtos.csv"
```

O caminho de entrada pode apontar para qualquer pasta. Por padrao, `messages.txt` e
`logs.txt` sao criados na pasta atual do terminal. Para escolher outros caminhos:

```powershell
dotnet run --project src/AffiliateMessages -- `
  --input "C:\dados\produtos.csv" `
  --output "C:\dados\messages.txt" `
  --log "C:\dados\logs.txt"
```

Opcoes suportadas: `--input`, `--output`, `--log`, `--model` e `--ollama-url`. Os mesmos
valores padrao podem ser ajustados em `src/AffiliateMessages/appsettings.json`. A URL deve
continuar apontando para o Ollama local.

As mensagens usam o preco formatado em pt-BR e o link de afiliado da coluna 8, sem crases.
Mensagens diferentes sao separadas por `====================`. Se o Ollama falhar, o lote
continua sem a frase; linhas com preco ou link invalidos sao ignoradas e registradas no log.

## Testes

```powershell
dotnet test tests/AffiliateMessages.Tests/AffiliateMessages.Tests.csproj
```

Os testes cobrem leitura CSV com e sem cabecalho, campos com virgulas entre aspas,
formatacao de preco, montagem das mensagens, validacao de frases e falha do Ollama por meio
de um `HttpMessageHandler` falso.

## Arquivos locais ignorados pelo Git

CSV de entrada, `messages.txt` e `logs.txt` sao arquivos locais e ficam ignorados pelo Git.
Nao os adicione ao repositorio: CSVs e links de afiliado podem conter dados privados.
