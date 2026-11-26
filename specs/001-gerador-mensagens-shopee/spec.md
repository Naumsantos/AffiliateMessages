# Feature Specification: Gerador de mensagens de promocao Shopee

**Feature Branch**: `[001-gerador-mensagens-shopee]`

**Created**: 2026-11-26

**Status**: Draft

**Input**: User description: "Gerar mensagens prontas para WhatsApp a partir de um CSV exportado pela Shopee, com frase persuasiva opcional gerada por IA local."

## Clarifications

### Session 2026-11-26

- Q: Como o CSV sem cabeçalho deve ser interpretado? → A: Os campos seguem posições fixas: coluna 1 nome, coluna 2 preço e coluna 8 link de afiliado; a coluna 0 é ID e não é usada.
- Q: Qual modelo Ollama deve ser usado como padrão? → A: `llama3.2:latest`.
- Q: Como o link de afiliado deve aparecer na mensagem final? → A: Livre, sem crases.
- Q: Qual texto deve separar cada mensagem no arquivo final? → A: `====================`.
- Q: Onde as linhas inválidas devem ser registradas? → A: No arquivo `logs.txt`.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gerar mensagens a partir do CSV (Priority: P1)

Como afiliado, quero selecionar o arquivo CSV exportado pela Shopee e gerar as mensagens de todos os produtos em um unico resultado, para publicar ou copiar para o WhatsApp sem montar cada texto manualmente.

**Why this priority**: Esta jornada elimina o trabalho principal repetitivo e entrega o valor central da funcionalidade.

**Independent Test**: Pode ser testada fornecendo um CSV com produtos validos e confirmando que cada produto produz uma mensagem legivel no arquivo de resultado.

**Acceptance Scenarios**:

1. **Given** um CSV UTF-8 com cabecalho e o link na coluna 8, **When** o usuario inicia a geracao, **Then** o sistema cria uma mensagem para cada produto valido.
2. **Given** um CSV com campos que contem virgulas entre aspas, **When** o arquivo e processado, **Then** o nome do produto permanece completo e nao e dividido em campos.
3. **Given** um produto com preco 79,19, **When** a mensagem e gerada, **Then** a linha de preco aparece como `✅ POR : R$ 79,19` e o link usado e o da coluna 8.
4. **Given** um produto com preco 1700, **When** a mensagem e gerada, **Then** o preco aparece como `R$ 1.700`.

---

### User Story 2 - Continuar o processamento quando a IA local falha (Priority: P2)

Como afiliado, quero que o lote continue quando o servico de IA local estiver indisponivel ou falhar, para nao perder as mensagens dos demais produtos.

**Why this priority**: Garante uso util mesmo sem dependencia opcional da IA.

**Independent Test**: Pode ser testada mantendo a geracao de frases indisponivel e confirmando que todas as mensagens validas sao geradas sem a primeira linha e que o lote termina normalmente.

**Acceptance Scenarios**:

1. **Given** um lote com produtos validos e o gerador local de frases indisponivel, **When** o usuario gera as mensagens, **Then** cada mensagem valida e criada sem a frase persuasiva.
2. **Given** uma falha temporaria ao gerar uma frase, **When** o processamento segue para o proximo produto, **Then** o produto atual nao interrompe nem cancela o lote.

---

### User Story 3 - Identificar e ignorar linhas invalidas (Priority: P3)

Como afiliado, quero que linhas com preco ou link invalidos sejam ignoradas e registradas, para que eu saiba quais itens nao entraram no resultado sem que o lote seja interrompido.

**Why this priority**: Torna o processamento confiavel em arquivos reais, que podem conter dados incompletos.

**Independent Test**: Pode ser testada com uma mistura de linhas validas e invalidas e confirmando que somente as linhas validas aparecem no resultado e que todas as invalidas aparecem no log.

**Acceptance Scenarios**:

1. **Given** uma linha com preco invalido e outras linhas validas, **When** o lote e processado, **Then** a linha invalida e ignorada e todas as linhas validas continuam sendo processadas.
2. **Given** uma linha sem preco ou sem link na coluna 8, **When** o lote e processado, **Then** o sistema registra a linha em `logs.txt` e nao adiciona mensagem ao resultado.

---

### Edge Cases

- O CSV pode ter cabecalho ou iniciar diretamente pelos dados.
- Com ou sem cabecalho, as colunas permanecem fixas: nome na coluna 1, preco na coluna 2 e link de afiliado na coluna 8.
- O nome fica vazio e a linha continua valida quando a coluna 1 nao tiver valor.
- Linhas invalidas sao registradas em `logs.txt`, incluindo o numero da linha e o motivo.
- Campos com virgulas devem permanecer unidos quando estiverem entre aspas.
- Arquivos com codificacao diferente de UTF-8 nao sao esperados, mas qualquer falha de leitura deve ser registrada sem criar um resultado parcial enganoso.
- Precos em formato brasileiro podem conter separador decimal, milhares ou apenas parte inteira.
- Produtos validos devem continuar sendo processados quando uma frase nao puder ser criada.
- Linhas vazias, incompletas e com dados nao numericos no preco devem ser ignoradas e registradas.
- O valor gerado para a frase deve ter no maximo sete palavras, usar portugues brasileiro e conter no maximo um emoji.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: O sistema DEVE ler arquivos CSV em UTF-8 com ou sem a primeira linha de cabecalho.
- **FR-002**: O sistema DEVE interpretar campos separados por virgulas e preservar virgulas internas presentes dentro de campos entre aspas.
- **FR-002A**: O sistema DEVE usar a coluna 1 como nome, a coluna 2 como preco e a coluna 8 como link de afiliado, independentemente da presença de cabecalho.
- **FR-002B**: O sistema DEVE permitir nome vazio quando a coluna 1 estiver vazia, sem considerar essa ausencia motivo de invalidacao.
- **FR-003**: O sistema DEVE converter preco em formato brasileiro para um valor decimal antes da exibicao.
- **FR-004**: O sistema DEVE apresentar preco decimal com separador decimal virgula e milhares com ponto, por exemplo `79,19` e `1.700`.
- **FR-005**: O sistema DEVE usar o campo da coluna 8 como link do produto.
- **FR-006**: O sistema DEVE gerar uma frase persuasiva em portugues brasileiro com no maximo sete palavras e no maximo um emoji.
- **FR-007**: O sistema DEVE montar cada mensagem com a frase quando disponivel, o nome do produto, o preco e o link do afiliado.
- **FR-008**: O sistema DEVE gravar todas as mensagens em `messages.txt`, separando mensagens diferentes por uma linha divisoria.
- **FR-008A**: A linha divisoria entre mensagens DEVE ser exatamente `====================`.
- **FR-009**: O sistema DEVE omitir a linha da frase quando a geracao falhar ou o servico local estiver offline, sem interromper o lote.
- **FR-010**: O sistema DEVE ignorar linhas invalidas e registrar o motivo da invalidacao em um log.
- **FR-010A**: O log de linhas invalidas DEVE ser gravado em `logs.txt` e incluir o numero da linha e o motivo.
- **FR-011**: O sistema DEVE processar arquivos de ate 50 produtos em menos de tres minutos com o recurso de IA configurado para `llama3.2:latest`.
- **FR-011A**: O modelo Ollama padrao DEVE ser `llama3.2:latest`.
- **FR-012**: O sistema NAO DEVE enviar mensagens automaticamente ao WhatsApp, consultar o Mercado Livre ou buscar preco ou cupom em sites externos.

### Key Entities *(include if feature involves data)*

- **Produto CSV**: Registro exportado contendo nome do produto, preco e link de afiliado; o nome e o link sao usados na mensagem e o preco e normalizado para exibicao.
- **Mensagem gerada**: Texto pronto para WhatsApp composto pela frase opcional, nome, preco e link.
- **Registro de erro**: Informacao sobre uma linha ignorada, incluindo o numero da linha e o motivo.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: O usuario consegue gerar o resultado completo de um CSV com ate 50 produtos em menos de tres minutos.
- **SC-002**: 100% das linhas validas reconhecidas produzem uma mensagem no arquivo de resultado.
- **SC-003**: 100% das linhas com preco invalido ou link ausente sao ignoradas e aparecem no log.
- **SC-004**: Quando o gerador de frases estiver indisponivel, pelo menos 100% dos produtos validos continuam produzindo mensagens sem a frase.
- **SC-005**: Em testes representativos, os precos exibem corretamente os formatos brasileiros para valores inteiros e decimais.
- **SC-006**: Todas as mensagens geradas deixam o link visivel, incluem o nome quando preenchido e nao realizam envio automatico a outro aplicativo.
- **SC-007**: 100% das linhas de teste usam corretamente nome da coluna 1, preco da coluna 2 e link da coluna 8.
- **SC-008**: Cada mensagem separada no resultado usa exatamente uma linha `====================`.
- **SC-009**: Cada linha invalida registrada em `logs.txt` identifica o numero da linha e um motivo reconhecivel.

## Assumptions

- O arquivo CSV sera fornecido pelo usuario e sera a unica fonte de dados dos produtos.
- A primeira linha sera tratada como cabecalho quando existir, mas nao sera usada como dado; sem cabecalho, a primeira linha sera processada normalmente.
- O modelo padrao do gerador local sera `llama3.2:latest`.
- O link sera apresentado sem crases ou marcacao adicional, deixando-o pronto para copiar.
- A linha divisoria sera exatamente `====================` entre todas as mensagens e nao fara parte do conteudo enviado.
- As linhas invalidas serao registradas em `logs.txt`, no mesmo local do arquivo de resultado.
- Nenhum preco, cupom ou estoque sera obtido de fontes externas na versao atual.
