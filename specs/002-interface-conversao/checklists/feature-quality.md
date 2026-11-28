# Feature Requirements Quality Checklist: Interface local para conversão de CSV

**Purpose**: Avaliar a completude, clareza, consistência e testabilidade dos requisitos da feature e dos contratos arquiteturais relacionados.
**Created**: 2026-11-27
**Feature**: [spec.md](../spec.md)

**Note**: Esta checklist customizada foi gerada por `/speckit-checklist` com base no contexto da feature.
**Review Ownership**: Esta checklist é um artefato de revisão da qualidade dos requisitos, pertencente ao revisor. Marque um item `[x]` somente quando o revisor considerar satisfeito o critério de qualidade do requisito.
**Marker Semantics**: `[x]` significa que o critério foi revisado e satisfeito quanto à qualidade dos requisitos. Não significa que a implementação esteja concluída.

## Requirement Completeness

- [ ] CHK001 Os requisitos da feature registram o limite aprovado de 1.000.000 bytes e a ação de encerramento na UI, ou identificam claramente esses pontos como decisões normativas do plano? [Gap, Spec §Requirements; Plan §Decisões confirmadas]
- [ ] CHK002 Os cenários cobrem seleção e cancelamento, conversão em andamento, conclusão com e sem mensagens, cópia, localização do resultado, falhas e encerramento? [Coverage, Spec §User Scenarios]
- [ ] CHK003 Os requisitos de segurança do host local estão explícitos como requisitos da feature, incluindo bind, validação de origem, CSRF, ausência de CORS e restrição de caminhos? [Gap, Spec §FR-008; Plan §Host local]
- [ ] CHK004 Há requisitos de carregamento e feedback separados para conversão, cópia, abertura da pasta e encerramento? [Gap, Plan §Contratos, erros e reutilização; Data Model §Estados]
- [ ] CHK005 Há requisitos de acessibilidade para teclado, foco e anúncio de estados/erros por tecnologia assistiva em todos os controles interativos? [Gap, Spec §FR-001, §FR-004, §FR-006, §FR-011]
- [ ] CHK006 O contrato especifica requisição, resposta e falhas para os quatro endpoints, incluindo a aquisição do token e o encerramento do host? [Completeness, Contract §API HTTP local]

## Requirement Clarity

- [ ] CHK007 O limite de upload é inequívoco como 1.000.000 bytes do arquivo, sem confusão com a sobrecarga do `multipart/form-data`? [Clarity, Plan §Validação do arquivo CSV; Contract §POST /api/conversions]
- [ ] CHK008 Os requisitos distinguem arquivo ausente, extensão inválida, arquivo vazio, mídia não suportada, erro estrutural do parser e linhas inválidas? [Clarity, Spec §Edge Cases; Contract §Validação e erros]
- [ ] CHK009 Os valores estáveis de `code` e sua correspondência com cada status HTTP estão enumerados, além da presença genérica do campo em `ProblemDetails`? [Gap, Contract §Validação e erros]
- [ ] CHK010 O que constitui erro estrutural do parser e quando isso difere de uma linha inválida recuperável está definido sem interpretações concorrentes? [Clarity, Plan §Validação do arquivo CSV; Contract §Validação e erros]
- [ ] CHK011 O resultado de CSV somente com cabeçalho, linhas em branco ou sem linhas válidas está diferenciado do arquivo de zero bytes? [Gap, Spec §Edge Cases; Plan §Validação do arquivo CSV]
- [ ] CHK012 O significado e a contagem de `messagesWithoutPhrase` estão definidos para que o aviso de resultado parcial não atribua causas ambíguas? [Clarity, Contract §POST /api/conversions]
- [ ] CHK013 As verificações de `Host`, `Origin`, `Sec-Fetch-Site` e token CSRF definem claramente os valores aceitos e o tratamento de cabeçalhos ausentes? [Clarity, Contract §GET /api/session; Contract §Endpoints mutáveis]

## Requirement Consistency

- [ ] CHK014 A retenção do arquivo selecionado após falha é consistente com a limpeza do resultado anterior ao iniciar uma nova conversão? [Consistency, Spec §FR-007; Data Model §Estado da tela]
- [ ] CHK015 O sucesso com resultado vazio, o arquivo de saída vazio, a contagem de linhas inválidas e o aviso correspondente são coerentes entre spec, plano, modelo e contrato? [Consistency, Spec §Edge Cases; Contract §POST /api/conversions]
- [ ] CHK016 O escopo de abertura da pasta corresponde ao caminho configurado em todas as descrições e exclui caminhos arbitrários fornecidos pelo navegador? [Consistency, Spec §FR-006; Plan §Abertura da pasta; Contract §POST /api/output/open-folder]
- [ ] CHK017 O ciclo de encerramento é consistente entre os requisitos, o estado da UI e o contrato, inclusive confirmação, conversão ativa, resposta `202` e aba fechada? [Consistency, Plan §Host local; Data Model §Estado de encerramento; Contract §POST /api/host/shutdown]

## Acceptance Criteria Quality

- [ ] CHK018 Os critérios de aceite distinguem objetivamente um arquivo de exatamente 1.000.000 bytes de outro com 1.000.001 bytes? [Measurability, Plan §Validação do arquivo CSV; Gap, Spec §Success Criteria]
- [ ] CHK019 Há critérios mensuráveis para rejeição de origem/token, limite de upload e recusa de caminho arbitrário, não apenas para os fluxos visuais? [Gap, Spec §Success Criteria; Contract §Localidade e tratamento de falhas]
- [ ] CHK020 Os critérios definem quando a interface pode afirmar sucesso, mostrar caminho salvo ou apresentar conclusão parcial diante de erros e linhas/frases omitidas? [Acceptance Criteria, Spec §FR-005, §FR-007, §FR-009]
- [ ] CHK021 A meta de até 50 produtos em menos de três minutos está definida como compromisso verificável da feature ou identificada apenas como objetivo de planejamento? [Ambiguity, Plan §Technical Context; Gap, Spec §Success Criteria]

## Scenario Coverage

- [ ] CHK022 Há requisitos de contingência para falha ao vincular a porta e para falha ao abrir automaticamente o navegador, incluindo como o usuário obtém acesso ou é informado? [Gap, Plan §Host local; Quickstart §Ciclo de vida do host]
- [ ] CHK023 O comportamento após atualização da página, perda de conexão ou resposta interrompida durante a conversão define se o lote continua, como o resultado é recuperado e como evitar uma repetição confusa? [Gap, Spec §User Scenarios; Contract §POST /api/conversions]
- [ ] CHK024 Os cenários alternativos e de recuperação cobrem arquivo removido/ilegível, falha de clipboard, saída indisponível, origem inválida, Ollama offline e encerramento durante conversão? [Coverage, Spec §Edge Cases; Contract §Validação e erros]
- [ ] CHK025 As regras para concorrência entre conversão, abertura da pasta e encerramento estão definidas, inclusive quais ações ficam indisponíveis e quais retornam conflito? [Gap, Contract §POST /api/conversions; Contract §POST /api/output/open-folder; Contract §POST /api/host/shutdown]

## Edge Case Coverage

- [ ] CHK026 Os limites de arquivo de zero bytes, um byte, exatamente 1.000.000 bytes e acima do limite têm resultados distintos e definidos? [Coverage, Plan §Validação do arquivo CSV; Contract §POST /api/conversions]
- [ ] CHK027 Os resultados de CSV com cabeçalho somente, linhas em branco, combinação de linhas válidas/inválidas e todas as linhas inválidas estão especificados? [Coverage, Spec §Edge Cases; Data Model §CSV selecionado]
- [ ] CHK028 O comportamento está definido quando o arquivo/pasta de saída desaparece ou o sistema operacional não consegue abrir a pasta após a conversão? [Coverage, Spec §Edge Cases; Contract §POST /api/output/open-folder]

## Non-Functional Requirements

- [ ] CHK029 A preservação local dos dados e a proibição de envio a IA na nuvem são consistentes com os requisitos de host, API e dependências externas? [Consistency, Spec §FR-008; Constitution §I; Plan §Technical Context]
- [ ] CHK030 Os navegadores suportados e as condições necessárias para a Clipboard API em contexto loopback estão definidos como requisito ou dependência validada? [Gap, Spec §FR-011; Plan §Technical Context]

## Dependencies & Assumptions

- [ ] CHK031 A disponibilidade do Ollama local, seu fallback e o aviso de resultado parcial estão documentados sem transformar uma falha tolerável em falha total? [Dependency, Spec §FR-009; Constitution §II; Contract §POST /api/conversions]
- [ ] CHK032 O risco de suporte do .NET 8 está ligado a uma decisão explícita de release, sem conflitar com a restrição de plataforma vigente? [Assumption, Plan §Constitution Check; Research §Risco do ciclo de suporte]

## Ambiguities & Conflicts

- [ ] CHK033 Está explícito qual documento é normativo quando a spec, o plano e o contrato divergem sobre limite, segurança, estados ou ciclo de vida? [Ambiguity, Spec §Requirements; Plan §Decisões confirmadas; Contract §API HTTP local]

## Notes

- Todos os itens começam desmarcados; a marcação pertence ao revisor e avalia qualidade dos requisitos, não conclusão da implementação.
- Esta checklist customizada complementa `checklists/requirements.md`, mantido por `/speckit-specify` e `/speckit-clarify`.
- `/speckit-implement` lê o estado dos itens como gate, mas não deve alterar os marcadores.
- Os itens apontam gaps ou decisões a revisar nos documentos da feature; não são casos de teste da implementação.
