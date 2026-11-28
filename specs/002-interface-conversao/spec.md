# Feature Specification: Interface local para conversão de CSV

**Feature Branch**: `[002-interface-conversao]`

**Created**: 2026-11-27

**Status**: Draft

**Input**: User description: "Crie um frontend para essa aplicação aonde o usuário vai poder buscar o arquivo no computador através de um botão de buscar. Mostrar para o usuário quando o processo iniciar e finalizar e aonde o arquivo foi salvo (podendo ser um botão clicavel que abrirá a pasta)"

## Clarifications

### Session 2026-11-27

- Q: A lista de mensagens com botões para copiar cada uma deve complementar ou substituir a informação sobre onde o arquivo foi salvo? → A: A lista complementa a tela de resultado: mostrar cada mensagem com um botão para copiá-la, manter visível o caminho completo do arquivo salvo e oferecer a abertura da pasta quando possível.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Selecionar um CSV e iniciar a conversão (Priority: P1)

Como usuário, quero localizar um arquivo CSV no meu computador e iniciar sua conversão pela interface, para gerar as mensagens sem precisar informar caminhos manualmente.

**Why this priority**: Selecionar o arquivo e iniciar o trabalho são o fluxo principal que torna a aplicação acessível pela interface.

**Independent Test**: Selecionar um CSV válido e iniciar a conversão; confirmar que o arquivo escolhido é identificado e que a interface informa o início do processamento.

**Acceptance Scenarios**:

1. **Given** que nenhum arquivo foi escolhido, **When** o usuário aciona a busca e seleciona um CSV existente, **Then** a interface mostra qual arquivo foi selecionado e permite iniciar a conversão.
2. **Given** que um CSV válido foi selecionado, **When** o usuário inicia a conversão, **Then** a interface informa que o processamento começou e evita iniciar outra conversão simultaneamente.
3. **Given** que o usuário fecha a busca sem escolher um arquivo, **When** retorna à interface, **Then** a conversão não começa e nenhuma seleção anterior é substituída.
4. **Given** que um CSV com dados de produtos foi selecionado, **When** a conversão é executada, **Then** os dados permanecem no ambiente local e não são enviados a serviços de IA na nuvem.

---

### User Story 2 - Consultar e copiar resultados e localizar o arquivo (Priority: P2)

Como usuário, quero ver as mensagens geradas na mesma tela, copiá-las individualmente e saber onde o resultado foi salvo, para usar as mensagens e localizar o arquivo.

**Why this priority**: O usuário precisa de confirmação de conclusão e de uma forma clara de acessar o resultado para completar o fluxo.

**Independent Test**: Concluir uma conversão e confirmar que a interface lista as mensagens com botões individuais de cópia, indica o término e apresenta o caminho do arquivo de resultado.

**Acceptance Scenarios**:

1. **Given** que uma conversão está em andamento, **When** ela termina com sucesso, **Then** a interface informa a conclusão e mostra o caminho onde o arquivo de resultado foi salvo.
2. **Given** que o caminho do resultado foi exibido, **When** o usuário aciona a opção para abrir a pasta, **Then** a pasta que contém o arquivo de resultado é aberta.
3. **Given** que a pasta do resultado não pode ser aberta, **When** o usuário aciona essa opção, **Then** a interface informa que não foi possível abrir a pasta e mantém o caminho do arquivo visível.
4. **Given** que a conversão foi concluída e gerou mensagens, **When** o usuário consulta a tela de resultado, **Then** cada mensagem aparece individualmente com seu próprio botão para copiá-la.
5. **Given** que uma mensagem aparece na tela de resultado, **When** o usuário aciona o botão daquela mensagem, **Then** o texto completo é copiado e a interface confirma a ação.

---

### User Story 3 - Entender uma falha de processamento (Priority: P3)

Como usuário, quero ser informado quando a conversão não puder começar ou terminar, para corrigir o problema sem confundir uma falha com uma conversão bem-sucedida.

**Why this priority**: Mensagens de erro claras evitam que o usuário procure um resultado que não foi criado.

**Independent Test**: Tentar processar um arquivo inexistente ou ilegível e confirmar que a interface mostra uma falha compreensível sem indicar conclusão bem-sucedida.

**Acceptance Scenarios**:

1. **Given** que o arquivo selecionado deixou de estar disponível ou não pode ser lido, **When** o usuário inicia a conversão, **Then** a interface informa que o processamento falhou e não apresenta um resultado como concluído.
2. **Given** que ocorre uma falha durante a conversão, **When** a falha é comunicada, **Then** a interface apresenta o erro sem apagar a seleção do usuário nem exibir um caminho de resultado não confirmado.
3. **Given** que o CSV contém linhas inválidas ou o gerador local de frases está indisponível, **When** o restante do lote pode ser processado, **Then** a conversão continua e a interface informa a conclusão quando o resultado for criado.

---

### Edge Cases

- O usuário cancela a seleção do arquivo ou tenta iniciar sem selecionar um CSV.
- O arquivo selecionado é removido, movido ou perde permissão de leitura antes do início.
- O processamento demora; o estado de processamento deve permanecer visível até sua conclusão ou falha.
- O processamento conclui sem mensagens válidas, conforme as regras existentes, e a interface deve refletir somente o resultado realmente criado.
- O arquivo de resultado existe, mas sua pasta não pode ser aberta ou não está mais disponível.
- A cópia de uma mensagem não pode ser concluída; a interface não deve indicar que o texto foi copiado.
- O arquivo de entrada contém linhas inválidas ou o serviço local opcional de geração de frases falha; esses casos não devem ser apresentados como falha total se a conversão existente concluir normalmente.
- O arquivo selecionado excede o limite de 1.000.000 bytes; a conversão não deve começar e a interface deve explicar a rejeição.
- O usuário solicita encerrar a aplicação enquanto uma conversão está ativa; o host não deve encerrar nem interromper o lote por essa solicitação.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A interface DEVE permitir que o usuário procure e selecione um arquivo CSV no computador.
- **FR-002**: Após a seleção, a interface DEVE identificar o arquivo escolhido antes que o usuário inicie a conversão.
- **FR-003**: A interface DEVE permitir iniciar a conversão do arquivo selecionado e impedir uma segunda execução enquanto a primeira estiver em andamento.
- **FR-004**: Ao iniciar a conversão, a interface DEVE apresentar um estado visível de processamento em até dois segundos após a ação do usuário.
- **FR-005**: Ao término bem-sucedido, a interface DEVE informar que a conversão foi concluída e exibir o caminho completo do arquivo de resultado realmente salvo.
- **FR-006**: A interface DEVE manter visível o caminho completo do arquivo de resultado e, quando o ambiente permitir, oferecer uma ação clicável para abrir a pasta que o contém.
- **FR-007**: Se a seleção, a leitura do arquivo ou a conversão falhar, a interface DEVE apresentar uma mensagem compreensível e não indicar sucesso nem exibir como confirmado um arquivo de resultado que não foi salvo.
- **FR-008**: A interface DEVE manter o processamento e os arquivos de entrada e resultado no ambiente local do usuário, sem enviar dados do CSV a serviços de IA na nuvem.
- **FR-009**: A interface DEVE preservar as regras existentes de conversão, incluindo a tolerância a linhas inválidas e à indisponibilidade do gerador local de frases, sem interromper o lote por essas falhas isoladas.
- **FR-010**: Após a conversão bem-sucedida, a interface DEVE exibir na mesma tela cada mensagem gerada individualmente, além de manter acessível a localização do arquivo de resultado.
- **FR-011**: Cada mensagem exibida DEVE ter seu próprio botão para copiar seu texto completo; a interface DEVE confirmar quando a cópia for concluída e informar quando não for possível copiá-la.
- **FR-012**: A aplicação DEVE aceitar somente um arquivo `.csv` não vazio de até 1.000.000 bytes; o servidor DEVE aplicar o limite durante a leitura e rejeitar arquivos inválidos antes de iniciar a conversão.
- **FR-013**: A interface DEVE oferecer encerramento explícito e confirmado da aplicação; o host DEVE rejeitar a solicitação enquanto uma conversão estiver ativa e encerrar somente após aceitar a solicitação quando não houver conversão em andamento.

### Key Entities *(include if feature involves data)*

- **Arquivo de entrada**: CSV escolhido pelo usuário, identificado por seu nome e localização no computador.
- **Estado da conversão**: Situação visível da execução, como aguardando, em processamento, concluída ou com falha.
- **Mensagem gerada**: Texto produzido para um produto, exibido individualmente e disponível para cópia.
- **Arquivo de resultado**: Arquivo produzido pela conversão, identificado pelo caminho completo e pela pasta que pode ser aberta pelo usuário.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Em testes de uso, pelo menos 90% dos usuários conseguem selecionar um CSV e iniciar a conversão em até um minuto sem informar caminhos manualmente.
- **SC-002**: Em 100% das execuções de teste iniciadas, a interface apresenta o estado de processamento em até dois segundos.
- **SC-003**: Em 100% das conversões concluídas com sucesso, o caminho completo do resultado fica visível; quando a abertura de pastas estiver disponível, a ação correspondente abre a pasta correta.
- **SC-004**: Em 100% das falhas de teste, a interface distingue a falha de uma conclusão bem-sucedida e não apresenta um resultado inexistente como salvo.
- **SC-005**: Em testes de interface, linhas inválidas e falhas do gerador local de frases não impedem a conclusão do lote quando o processamento existente consegue produzir o resultado.
- **SC-006**: Em 100% das conversões de teste com mensagens geradas, cada mensagem aparece individualmente na tela e seu botão copia o texto correspondente sem incluir mensagens vizinhas.
- **SC-007**: Em 100% dos testes com arquivos ausentes, vazios, de extensão incorreta ou acima de 1.000.000 bytes, a API rejeita a requisição antes da conversão e a interface não apresenta sucesso.
- **SC-008**: Em 100% dos testes, o encerramento com conversão ativa é recusado sem parar o host; sem conversão ativa, o host encerra somente após aceitar a solicitação confirmada.
- **SC-009**: No ambiente de referência documentado no quickstart, a conversão completa de um CSV com uma fixture representativa de 50 produtos válidos, incluindo processamento e gravação e com o Ollama local disponível, conclui em menos de três minutos.

## Assumptions

- A interface será usada no mesmo computador em que a aplicação processa os arquivos, para que a seleção de arquivos locais e a abertura da pasta de resultado sejam possíveis.
- A conversão continuará usando as regras e o comportamento já existentes; esta funcionalidade trata da interação do usuário e da apresentação do estado e do destino do resultado.
- O caminho de saída continuará seguindo a configuração atual da aplicação. Quando não houver outra configuração, o nome padrão do arquivo de resultado será `messages.txt`.
- O processamento de produtos e qualquer geração opcional de frases continuarão locais, conforme as restrições existentes do projeto.
- O formato CSV aceito e os logs de linhas inválidas permanecem conforme a funcionalidade existente.
- O limite de upload aprovado é 1.000.000 bytes decimais; a ação de encerramento exige confirmação e não é aceita durante uma conversão ativa.
