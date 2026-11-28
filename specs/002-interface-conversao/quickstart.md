# Início rápido: Interface Angular local

## Pré-requisitos

- Windows
- Runtime .NET 8 e um SDK que consiga compilar `net8.0` (SDK 8 ou SDK mais novo com os targeting packs necessários)
- Node.js 24.18.0 ou versão compatível com Angular 22, e npm
- Ollama local em `http://localhost:11434`, com `llama3.2:latest` disponível
- CSV UTF-8 conforme as regras de colunas existentes

> **Nota sobre suporte:** O suporte ao .NET 8 terminou em 2026-11-10. A constituição atual ainda exige .NET 8; o target framework não deve ser alterado nesta feature sem uma decisão explícita de projeto/constituição. O ambiente de planejamento tem SDKs .NET 9 e 10 e runtime .NET 8, mas não o SDK 8; confirme que o targeting pack `net8.0` pode ser restaurado antes de compilar.

## Ciclo de vida do host

- Inicie em modo de interface com `--ui`. O mesmo executável continua oferecendo o fluxo CLI; a opção `--ui` inicia Kestrel somente em `127.0.0.1`, usando uma porta efêmera atribuída pelo sistema operacional, e serve a UI e API na mesma origem.
- A aplicação abre `http://127.0.0.1:<porta>/` no navegador padrão somente depois que o host está pronto. CORS não é habilitado; as rotas validam `Host`, e operações mutáveis exigem `Origin`, `Sec-Fetch-Site: same-origin` e o token da sessão.
- Se a abertura automática falhar, copie a URL mostrada no console e abra-a manualmente; o host continua ativo.
- Fechar a aba não encerra o host. Use a ação **Encerrar aplicação** na UI; ela pede confirmação e fica desativada durante uma conversão. `Ctrl+C` na janela do processo continua disponível como alternativa e, durante um lote, cancela a requisição e remove o upload temporário sem indicar conclusão.

## Compilar e executar a interface local

Na raiz do repositório:

```powershell
dotnet build src\AffiliateMessages\AffiliateMessages.csproj
dotnet run --project src\AffiliateMessages\AffiliateMessages.csproj -- --ui
```

O target de build .NET executa `npm ci` quando `ClientApp\node_modules` ainda não existe, compila o Angular e inclui os assets em `wwwroot` no build/publish. A invocação CLI existente continua disponível e não depende da interface.

## Validar o fluxo do usuário

1. Selecione um CSV válido e inicie a conversão. Confirme que o indicador aparece antes da resposta e que o início fica desativado enquanto o lote está em andamento.
2. Após o sucesso, confirme que cada mensagem aparece separadamente e que o caminho absoluto de `messages.txt` está visível.
3. Use o botão de cópia de uma mensagem, cole em um campo local e verifique que somente aquela mensagem completa foi copiada. O feedback de sucesso deve aparecer após a resolução da promise da área de transferência.
4. No teste Angular, simule uma escrita rejeitada na área de transferência e confirme que aparece o feedback de falha, não de sucesso.
5. Use a ação da pasta e confirme que a pasta configurada abre. Simule uma falha da API e verifique que o caminho continua visível com a mensagem de erro.
6. Teste um CSV com linhas inválidas e repita com o Ollama parado. Confirme que as mensagens válidas ainda aparecem, as linhas inválidas continuam registradas e a ausência de frase não marca o lote como falho.
7. Teste uma entrada ilegível ou ausente e confirme que a tela informa a falha sem apresentar um novo resultado como salvo.
8. Teste extensão não CSV, CSV vazio e arquivo acima de 1 MB (1.000.000 bytes). Confirme a rejeição, a mensagem específica e a limpeza do upload temporário após a resposta.
9. Envie requisições sem token CSRF, com token incorreto, `Host` diferente, `Origin` divergente ou `Sec-Fetch-Site` cross-site. Confirme que a API as rejeita e não inicia conversão nem abre pasta.
10. Tente abrir a pasta com arquivo de saída inexistente e confirme erro seguro; envie um caminho no corpo e confirme que ele não substitui a pasta configurada.
11. Solicite o encerramento pela UI, confirme a ação e verifique que o endpoint responde `202` antes de o host parar. Confirme que uma solicitação de encerramento durante uma conversão é rejeitada/desativada.

## Benchmark de performance (SC-009)

Execute o benchmark opt-in em uma máquina Windows de referência com Ollama local disponível e `llama3.2:latest` instalado. A fixture incorporada contém 50 produtos válidos; o teste mede a conversão completa — leitura do CSV, processamento e gravação do resultado — sem incluir compilação ou inicialização do runner:

```powershell
dotnet test tests\AffiliateMessages.Tests\AffiliateMessages.Tests.csproj --filter "Category=Performance" --logger "console;verbosity=detailed"
```

Execução de referência registrada:

| Item | Ambiente de referência |
|---|---|
| Windows | Microsoft Windows NT 10.0.26200.0 |
| CPU | Intel(R) Core(TM) 5 210H, 12 processadores lógicos |
| Memória disponível durante o teste | 25.376.317.440 bytes (aprox. 23,6 GiB) |
| Arquitetura | x64 |
| Runtime .NET | .NET 8.0.30 |
| Ollama | 0.40.18 |
| Modelo | `llama3.2:latest` |
| Fixture e escopo | CSV incorporado com 50 produtos; leitura, processamento e gravação |
| Tempo medido | 28,624 segundos |

O benchmark registra as informações do ambiente e a duração no output do teste. O critério passa somente se a conversão terminar em menos de 180 segundos.

## Verificações automatizadas

```powershell
dotnet test tests\AffiliateMessages.Tests\AffiliateMessages.Tests.csproj
Set-Location src\AffiliateMessages\ClientApp
npm test -- --watch=false
```

Os formatos de resposta e erro HTTP estão em [contracts/http-api.md](./contracts/http-api.md); o resultado e o ciclo de estado da interface estão em [data-model.md](./data-model.md).
