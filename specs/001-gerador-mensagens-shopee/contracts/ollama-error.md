# Contrato de Fallback do Ollama

O cliente considera falha qualquer timeout, erro HTTP, resposta nao JSON, resposta vazia ou
frase que viole as regras de quantidade de palavras ou emoji.

Em caso de falha, o resultado de `GeneratedMessage.Phrase` sera vazio. O nome, o preco e o
link continuarao sendo gravados. Nenhuma excecao do Ollama pode interromper o lote.
