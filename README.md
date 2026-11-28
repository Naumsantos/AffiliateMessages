# AffiliateMessages

Aplicacao local para gerar mensagens promocionais a partir de um CSV exportado da Shopee. Use a interface Angular no navegador ou o modo de console. As frases sao geradas pelo Ollama em execucao local; os dados dos produtos nao sao enviados a servicos de IA na nuvem.

## Requisitos

- .NET 8 SDK
- Node.js 24.18.0 ou versao compativel com Angular 22, e npm (necessarios para compilar a aplicacao a partir do codigo-fonte)
- Ollama local em `http://localhost:11434`
- Modelo `llama3.2:latest` disponivel no Ollama
- CSV UTF-8 exportado da Shopee

Instale o modelo, se necessario:

```powershell
ollama pull llama3.2:latest
```

> O projeto ainda tem como alvo o .NET 8, cujo suporte oficial terminou em 10 de novembro de 2026.

## Iniciar a interface

Na raiz do repositorio:

```powershell
dotnet build .\src\AffiliateMessages\AffiliateMessages.csproj
dotnet run --project .\src\AffiliateMessages\AffiliateMessages.csproj -- --ui
```

O comando inicia o host local em `127.0.0.1`, escolhe uma porta disponivel e abre a interface no navegador. Se o navegador nao abrir automaticamente, use a URL apresentada no console. O host nao fica acessivel pela rede local.

Na interface, selecione um CSV e inicie a conversao. O arquivo deve ter no maximo 1.000.000 bytes. A tela informa o andamento, apresenta as mensagens geradas e mostra onde `messages.txt` foi salvo. Use os botoes para copiar uma mensagem ou abrir a pasta de saida. Para encerrar, escolha **Encerrar aplicacao** na interface ou pressione `Ctrl+C` no console; fechar a aba nao encerra o host.

Se o Ollama estiver indisponivel ou nao gerar uma frase valida, o processamento pode continuar com a mensagem sem frase promocional. Linhas invalidas do CSV sao ignoradas e registradas no arquivo de log.

## Modo de console

Para processar um arquivo sem abrir o navegador:

```powershell
dotnet run --project .\src\AffiliateMessages\AffiliateMessages.csproj -- --input "C:\dados\produtos.csv"
```

Por padrao, `messages.txt` e `logs.txt` sao criados na pasta atual do terminal. Para definir outros caminhos:

```powershell
dotnet run --project .\src\AffiliateMessages\AffiliateMessages.csproj -- `
  --input "C:\dados\produtos.csv" `
  --output "C:\dados\messages.txt" `
  --log "C:\dados\logs.txt"
```

Opcoes suportadas: `--input`, `--output`, `--log`, `--model` e `--ollama-url`. Os valores padrao tambem podem ser configurados em `src\AffiliateMessages\appsettings.json`.

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

O CSV deve seguir as regras usuais de quoting; campos que contenham virgulas devem estar entre aspas, por exemplo `"79,19"` ou `"Sandalia, feminina"`.

## Testes

Na raiz do repositorio:

```powershell
dotnet test .\tests\AffiliateMessages.Tests\AffiliateMessages.Tests.csproj
Push-Location .\src\AffiliateMessages\ClientApp
npm test -- --watch=false
Pop-Location
```

O benchmark opt-in do processamento com Ollama pode ser executado separadamente:

```powershell
dotnet test .\tests\AffiliateMessages.Tests\AffiliateMessages.Tests.csproj --filter "Category=Performance"
```

## Arquivos locais ignorados pelo Git

CSV de entrada, `messages.txt` e `logs.txt` sao arquivos locais e ficam ignorados pelo Git. Nao os adicione ao repositorio: CSVs e links de afiliado podem conter dados privados.
