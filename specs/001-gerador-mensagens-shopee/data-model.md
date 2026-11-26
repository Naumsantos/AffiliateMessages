# Data Model

## Product

Representa um registro extraido do CSV.

| Campo | Tipo | Origem | Validacao |
|---|---|---|---|
| Id | string | Coluna 0 | Opcional |
| Name | string | Coluna 1 | Pode ficar vazio |
| Price | decimal | Coluna 2 | Obrigatorio; formato pt-BR valido |
| AffiliateLink | string | Coluna 8 | Obrigatorio; nao pode ficar vazio |

**Invariantes:**

- O link vem da coluna fixa 8, nao da ultima coluna.
- Preco deve ser convertido pela cultura `pt-BR`.
- O nome pode ser vazio sem invalidar o produto.
- Produto com preco ou link invalido nao gera `GeneratedMessage`.

## GeneratedMessage

Representa o texto pronto para WhatsApp de um produto.

| Campo | Tipo | Regra |
|---|---|---|
| Phrase | string opcional | Ate 7 palavras, pt-BR, ate 1 emoji |
| Name | string opcional | Pode repetir o nome do CSV |
| PriceText | string | Exemplo: `R$ 79,19` ou `R$ 1.700` |
| AffiliateLink | string | Copiado da coluna 8 |

**Representacao final:**

```text
[frase opcional]
[nome, se preenchido]
✅ POR : [preco]
[link]
```

Se a frase falhar, apenas a primeira linha sera omitida. `messages.txt` separa mensagens pela
linha divisoria exata `====================`.
