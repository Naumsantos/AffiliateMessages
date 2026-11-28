# System Requirements Quality Checklist: Interface local para conversão de CSV

**Purpose**: Avaliar a qualidade dos requisitos nas fronteiras de segurança, contrato CSV/API, experiência acessível, desempenho e ciclo de vida do host local.
**Created**: 2026-11-27
**Feature**: [spec.md](../spec.md)

**Note**: Esta checklist customizada foi gerada por `/speckit-checklist` com base nos requisitos e decisões da feature.
**Review Ownership**: Esta checklist pertence ao revisor. Marque um item `[x]` somente quando o critério de qualidade do requisito tiver sido revisado e considerado satisfeito.
**Marker Semantics**: `[x]` significa que o requisito está adequado quanto à qualidade; não significa que a implementação esteja concluída.

## Requisitos de segurança e fronteira local

- [ ] CHK001 A fronteira de confiança entre a interface no navegador, o host loopback e outras origens locais está declarada, incluindo quais ameaças locais o recurso pretende impedir? [Completeness, Gap, Spec §FR-008; Plan §Host local]
- [ ] CHK002 O propósito e os limites do token anti-CSRF estão claros, inclusive que ele não autentica um usuário e que sua validade se restringe à execução atual? [Clarity, Data Model §Sessão HTTP local; Contract §GET /api/session]
- [ ] CHK003 Os requisitos definem se as proteções de carregamento cruzado e cache da resposta de sessão (`no-store`, `Cross-Origin-Resource-Policy` e `nosniff`) são normativas para o cliente e para o host? [Consistency, Contract §GET /api/session; Gap, Spec §FR-008]

## Contrato de entrada CSV e falhas

- [ ] CHK004 Está explícito que o limite de 1.000.000 bytes se aplica ao conteúdo do arquivo, enquanto a sobrecarga multipart é contabilizada separadamente no limite da requisição? [Clarity, Plan §Validação do arquivo CSV; Contract §POST /api/conversions]
- [ ] CHK005 As regras de encoding, cabeçalho, colunas e formato aceito são referenciadas de modo que a nova interface não introduza uma interpretação diferente das regras CSV existentes? [Consistency, Spec §Assumptions; Plan §Validação do arquivo CSV]
- [ ] CHK006 O comportamento esperado diante de upload interrompido ou cancelado define se a conversão é abortada, como o usuário é informado e como se evita tratar um resultado parcial como sucesso? [Coverage, Gap, Plan §Validação do arquivo CSV; Contract §POST /api/conversions]

## Requisitos de interface e acessibilidade

- [ ] CHK007 Os requisitos especificam como estados assíncronos de conversão, cópia, abertura de pasta e encerramento são comunicados a usuários de tecnologia assistiva? [Completeness, Gap, Spec §FR-004, §FR-006, §FR-011]
- [ ] CHK008 Os requisitos definem a navegação por teclado, o foco após erros e a resposta quando o usuário cancela a confirmação de encerramento? [Coverage, Gap, Spec §FR-013]
- [ ] CHK009 O comportamento da cópia distingue API de área de transferência indisponível, permissão negada e falha da operação, e define a mensagem correspondente sem sugerir sucesso falso? [Clarity, Edge Case, Spec §FR-011; Data Model §Estado da cópia]

## Requisitos de performance

- [ ] CHK010 A fixture de 50 produtos tem composição documentada o suficiente para que “representativa” não dependa de interpretação do executor? [Clarity, SC-009; Plan §Performance Goals]
- [ ] CHK011 O critério deixa claro se carregamento inicial do modelo Ollama está excluído ou incluído no limite de três minutos, e se a medição cobre a conversão completa? [Clarity, Measurability, SC-009; Quickstart §Benchmark de performance]
- [ ] CHK012 O procedimento define quantidade de execuções ou estatística usada para avaliar o limite, além de registrar hardware, versões e modelo? [Measurability, Gap, SC-009; Quickstart §Benchmark de performance]

## Requisitos de ciclo de vida do host

- [ ] CHK013 Os requisitos definem uma precedência atômica quando solicitações de conversão e encerramento chegam simultaneamente? [Coverage, Gap, FR-003, FR-013; Contract §POST /api/host/shutdown]
- [ ] CHK014 O comportamento da interface está definido caso o servidor aceite o encerramento, mas a resposta `202` não chegue ao navegador? [Recovery, Gap, FR-013; Contract §POST /api/host/shutdown]
- [ ] CHK015 Se `Ctrl+C` interromper uma conversão ativa, os requisitos definem o feedback ao usuário e deixam claro que nenhum resultado não confirmado deve ser apresentado? [Consistency, Gap, Plan §Host local; Contract §Localidade e tratamento de falhas]

## Notes

- Todos os itens começam desmarcados; a marcação é responsabilidade do revisor e avalia qualidade dos requisitos, não conclusão da implementação.
- Os itens são perguntas sobre completude, clareza, consistência, mensurabilidade e cobertura dos documentos, não casos de teste da implementação.
- `/speckit-implement` lê o estado das checklists como gate e não deve modificar seus marcadores.
- Esta checklist complementa `feature-quality.md` e `requirements.md`; não substitui nem altera os itens existentes.
