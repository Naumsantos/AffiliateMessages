# Research

## Modelo e integracao Ollama

- **Decisao:** Usar `llama3.2:latest` por padrao e chamar
  `POST http://localhost:11434/api/generate` com JSON e `stream=false`.
- **Rationale:** Satisfaz o modelo ja instalado na maquina do usuario e mantem o
  processamento local.
- **Alternatives considered:** Outros modelos locais e SDK Ollama. Outros modelos foram
  deixados configuraveis; SDK foi rejeitado para evitar dependencia desnecessaria.

## Posicao das colunas

- **Decisao:** Ignorar cabecalho quando identificado e ler nome na coluna 1, preco na coluna 2
  e link na coluna 8; indices sao baseados em posicao.
- **Rationale:** O formato real foi explicitamente esclarecido e permanece identico com ou sem
  cabecalho.
- **Alternatives considered:** Detecao pelo texto ou pelo nome do cabecalho. Ambas introduzem
  ambiguidade e nao sao necessarias para o formato conhecido.

## Conversao e formatacao de preco

- **Decisao:** Validar e converter com `CultureInfo("pt-BR")`, armazenar como decimal e
  formatar inteiros e decimais em padrao brasileiro.
- **Rationale:** Garante `79,19`, `1.700` e `1.700,50` a partir do mesmo valor.
- **Alternatives considered:** Conversao textual manual foi rejeitada por maior risco.

## CSV

- **Decisao:** Usar CsvHelper com leitura UTF-8 e modo por indice de coluna.
- **Rationale:** O helper respeita campos entre aspas com virgulas internas.
- **Alternatives considered:** Parser manual foi rejeitado por complexidade e risco.

## Resiliencia e logging

- **Decisao:** Processar cada linha isoladamente; erro de preco/link sera registrado em
  `logs.txt`; erro Ollama fara fallback sem frase.
- **Rationale:** A falha de uma linha ou da IA nao pode cancelar o lote.
- **Alternatives considered:** Cancelar o lote foi rejeitado por contrariar requisito funcional.

## Configuracao

- **Decisao:** Usar `appsettings.json` como padrao e argumentos de linha de comando com
  precedencia maior para modelo, URL, CSV, diretorio de saida e divisoria.
- **Rationale:** Permite configuracao simples sem dependencias desnecessarias.
