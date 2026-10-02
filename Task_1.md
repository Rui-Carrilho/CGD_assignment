# Tarefa 1 — Análise Funcional

## 1. Interpretação das regras de negócio

As regras automatizam a pré-análise de pedidos de crédito pessoal, verificando se os dados fornecidos são válidos e se o pedido cumpre os critérios de elegibilidade e de capacidade financeira definidos no enunciado.

Os pedidos com dados inválidos recebem a decisão **PEDIDO INVÁLIDO**. Os pedidos válidos que cumprem uma condição de recusa recebem a decisão **RECUSADO**. As situações que exigem apreciação por um analista recebem a decisão **ANÁLISE MANUAL**. Quando não se verifica nenhuma condição de invalidade, recusa ou análise manual, o resultado é **APROVADO**.

Quando várias condições se aplicam simultaneamente, prevalece a decisão mais restritiva. A aplicação deve explicar o resultado através dos motivos aplicáveis e dos indicadores calculados.

Estas regras implementam os critérios apresentados no exercício. Uma decisão de aprovação não constitui uma garantia de que o cliente conseguirá reembolsar o crédito. Os pedidos inválidos são classificados como tal, e não simplesmente ignorados.

## 2. Ordem de aplicação das regras

É necessário distinguir a ordem de execução das regras da prioridade das decisões.

1. **Validar os dados de entrada:** aplicar a Regra 1 e recolher todos os erros de validação. Caso a validação falhe, devolver **PEDIDO INVÁLIDO**, indicar os motivos e não efetuar os cálculos financeiros.
2. **Calcular os indicadores:** para os pedidos válidos, calcular a prestação estimada, a taxa de esforço e a idade estimada no final do contrato.
3. **Aplicar as Regras 2 a 7:** verificar todas as condições e registar cada motivo aplicável.
4. **Aplicar a prioridade da Regra 8:** selecionar a decisão mais restritiva.
5. **Apresentar o resultado:** devolver a decisão final, os motivos e os indicadores calculados.

A prioridade é:

**PEDIDO INVÁLIDO > RECUSADO > ANÁLISE MANUAL > APROVADO**

A avaliação de um pedido válido não deve terminar assim que for encontrada uma condição de análise manual, pois outra regra pode determinar a recusa. A verificação de todas as regras permite também apresentar uma explicação completa. Se nenhuma regra determinar recusa ou análise manual, o pedido é aprovado.

## 3. Situações não previstas ou ambíguas

### Situações não previstas

- **Campos em falta:** não é definido o tratamento de campos obrigatórios não preenchidos, como a situação profissional ou a existência de incidentes de crédito.
- **Valores não reconhecidos:** não é indicado o resultado para uma situação profissional diferente das três opções previstas.
- **Prestações atuais negativas:** o enunciado não as proíbe expressamente, embora reduzam artificialmente a taxa de esforço.
- **Idade ou prazo com casas decimais:** não é explicitado se estes valores devem ser números inteiros.
- **Valores muito elevados:** faltam limites técnicos e um tratamento definido para valores que excedam a capacidade de representação dos tipos numéricos.

### Situações ambíguas

| Tema | Ambiguidade |
|---|---|
| Validação do NIF | Basta ter exatamente nove dígitos ou é pretendida alguma validação adicional? |
| Idade no final do contrato | A idade em anos inteiros não permite determinar a idade exata no vencimento sem a data de nascimento e a data de início do contrato. |
| Arredondamentos | Não é indicado quando nem como arredondar. Arredondar antes da comparação pode alterar a decisão junto dos limites de 35% e 50%. |
| Relação entre as Regras 7 e 8 | A expressão «independentemente das restantes regras» pode sugerir que a análise manual prevalece sobre uma recusa, contrariando a prioridade da Regra 8. |
| Motivos apresentados | Não é claro se devem ser apresentados todos os motivos detetados ou apenas os que sustentam a decisão final. |
| Significado de aprovação | Não é explicitado se corresponde apenas à aprovação da pré-análise ou à autorização final de concessão do crédito. |
| Análise manual | Não estão definidos o responsável, as decisões possíveis nem a forma de registar a evolução do pedido. |
| «Último mês» | Pode significar o mês civil anterior ou um período móvel contado a partir da data da consulta. |
| Pedidos «terminados» | Não é claro se se pretende a decisão automática inicial, o estado atual ou o encerramento definitivo do processo. |
| Motivo de recusa mais frequente | Um pedido pode ter vários motivos de recusa. É necessário definir quais entram na contagem e como tratar empates. |
| Evolução de análise manual para aprovado | Não é claro se basta ter existido uma aprovação posterior ou se o pedido tem de continuar aprovado à data da consulta. |

A composição do agregado familiar ou alterações futuras da situação profissional poderão ser discutidas numa evolução das regras. Porém, não são dados adicionais necessários para implementar o enunciado atual. Condições de saúde, saúde mental e perfis psicológicos não fazem parte dos requisitos fornecidos e não devem ser introduzidos como critérios por iniciativa da equipa de desenvolvimento.

## 4. Questões a colocar ao analista de negócio

As seguintes questões permitem esclarecer comportamentos que afetam diretamente a implementação. Os pressupostos apresentados são propostas a confirmar, e não requisitos expressos no enunciado.

| Questão | Pressuposto proposto até confirmação |
|---|---|
| Se um pedido exceder 50.000 € e também cumprir uma condição de recusa, qual é a decisão final? | Prevalece **RECUSADO**, de acordo com a Regra 8. |
| Campos em falta, prestações atuais negativas ou uma situação profissional não reconhecida tornam o pedido inválido? A idade e o prazo têm de ser inteiros? | Sim. Estes casos são tratados como erros de validação. |
| A validação do NIF limita-se a verificar exatamente nove dígitos? | Sim, sem validação adicional do dígito de controlo. |
| Como deve ser calculada a idade no final do contrato com os dados disponíveis? | Comparar `Idade × 12 + PrazoMeses` com `75 × 12`, documentando a aproximação. |
| A taxa de esforço deve ser arredondada antes da comparação com os limites? | Não. Comparar com a precisão do cálculo e arredondar apenas para apresentação. |
| Devem ser apresentados todos os motivos detetados? | Sim, identificando o nível de decisão associado a cada motivo. |
| A aprovação é apenas da pré-análise ou representa autorização final para conceder o crédito? | É a aprovação no âmbito da pré-análise efetuada por este componente. |
| Quem pode concluir uma análise manual e que alterações de estado são permitidas? | Um analista autorizado pode aprovar ou recusar o pedido, registando uma justificação. |
| «Último mês» corresponde ao mês civil anterior ou a um período móvel? | Mês civil anterior, com o fuso horário de reporting acordado. |
| Devem contar todos os motivos de recusa quando existe mais do que um no mesmo pedido? | Contar cada motivo distinto de recusa uma vez por pedido recusado e apresentar todos os motivos empatados no máximo. |
| O relatório de evolução para aprovado inclui pedidos que voltaram a mudar de estado? | Confirmar se se pretende o estado atual aprovado ou qualquer aprovação posterior à análise manual. |

## 5. Proposta de solução em formato de user story

### História principal — Avaliar automaticamente um pedido de crédito pessoal

**Como** analista de crédito,  
**quero** que a aplicação avalie um pedido de crédito pessoal de acordo com as regras de negócio definidas,  
**para** obter uma decisão de pré-análise consistente, compreender os seus motivos e identificar os pedidos que exigem análise manual.

### Critérios de aceitação

1. **Dados de entrada:** a aplicação recebe NIF, idade, rendimento mensal líquido do agregado, total das prestações mensais atuais, valor pretendido, prazo em meses, situação profissional e indicação de incidentes de crédito.
2. **Pedido inválido:** perante dados que falhem a validação, a aplicação devolve **PEDIDO INVÁLIDO**, identifica todas as validações falhadas e não calcula os indicadores financeiros.
3. **Indicadores:** para dados válidos, a aplicação calcula:
   - `PrestacaoEstimada = ValorPretendido / PrazoMeses`
   - `TaxaEsforco = (PrestacoesAtuais + PrestacaoEstimada) / RendimentoMensalLiquido × 100`
4. **Aplicação das regras:** a aplicação verifica todas as regras de negócio aplicáveis e regista os respetivos motivos.
5. **Prioridade:** quando várias regras se aplicam, prevalece a decisão mais restritiva, conforme a Regra 8.
6. **Aprovação:** se os dados forem válidos e nenhuma regra determinar recusa ou análise manual, a decisão é **APROVADO**.
7. **Valores de fronteira:** uma taxa de esforço de exatamente 35% não agrava a decisão; uma taxa de exatamente 50% determina análise manual. Um montante de exatamente 50.000 € não ativa a Regra 7. Uma idade estimada de exatamente 75 anos no final do contrato não ativa a Regra 2. Um montante exatamente igual a 20 vezes o rendimento mensal não ativa a Regra 5. As restantes regras continuam a aplicar-se.
8. **Explicação:** o resultado apresenta a decisão final, os motivos aplicáveis, a prestação estimada e a taxa de esforço. Em caso de aprovação, indica que não foram identificadas condições de recusa ou de análise manual. Em pedidos inválidos, os indicadores são apresentados como não calculados.
9. **Utilização repetida:** o utilizador pode avaliar outro pedido de forma independente dos anteriores.

Os critérios dependentes de pressupostos, nomeadamente validações adicionais, cálculo da idade e arredondamentos, devem ser ajustados após confirmação pelo analista de negócio.

### História de apoio — Registar o resultado da análise manual

**Como** analista de crédito,  
**quero** registar o resultado de uma análise manual, com a data, o autor e a justificação,  
**para** permitir a auditoria e o reporting da evolução do pedido, preservando a decisão automática original.

Critérios de aceitação propostos:

- Um analista autorizado pode aprovar ou recusar um pedido em análise manual.
- A alteração regista o estado anterior, o novo estado, a data, o autor e a justificação.
- A decisão automática e os motivos originais permanecem disponíveis.
- A alteração do estado atual e o respetivo registo no histórico são guardados na mesma transação.

Esta história suporta as extrações pedidas na Tarefa 4. O enunciado não especifica integralmente a interface nem as permissões deste processo; trata-se de uma proposta a confirmar.

## 6. Modelo de dados de suporte à aplicação

Um diagrama de classes pode representar a estrutura da aplicação. No entanto, como o exercício exige consultas à base de dados, propõe-se um **modelo entidade–relação**, acompanhado da descrição dos campos, relações e restrições.

O diagrama exportado encontra-se nos ficheiros `Modelo-Dados.svg` e `Modelo-Dados.png`. O ficheiro `Modelo-Dados.mmd` contém a versão Mermaid editável.

### Entidades

| Entidade | Finalidade e campos principais |
|---|---|
| **CLIENTE** | Identifica um cliente. Campos: `Id` (chave primária), `NIF` (texto, único). |
| **PEDIDO** | Preserva os dados submetidos. Campos: `Id`, `ClienteId` (opcional), `DataSubmissao`, `NIFSubmetido`, `Idade`, `RendimentoMensalLiquido`, `PrestacoesAtuais`, `ValorPretendido`, `PrazoMeses`, `SituacaoProfissional`, `IncidentesCredito` e `EstadoAtual`. |
| **AVALIACAO** | Guarda o resultado automático. Campos: `Id`, `PedidoId` (único), `DataAvaliacao`, `DecisaoAutomatica`, `PrestacaoEstimada`, `TaxaEsforco`, `IdadeFinalMeses` e `VersaoRegras`. Os indicadores são opcionais para pedidos inválidos. |
| **MOTIVO_AVALIACAO** | Guarda cada motivo identificado. Campos: `Id`, `AvaliacaoId`, `CodigoRegra`, `CodigoMotivo`, `NivelDecisao` e `Descricao`. |
| **HISTORICO_ESTADO** | Regista a evolução do pedido. Campos: `Id`, `PedidoId`, `EstadoAnterior` (opcional no registo inicial), `EstadoNovo`, `DataAlteracao`, `Autor` e `Justificacao`. |

### Relações

- Um cliente pode ter zero ou vários pedidos. Um pedido pode estar associado a zero ou um cliente, para permitir o registo de submissões com NIF inválido.
- Um pedido pode ter zero ou uma avaliação automática; após o processamento, deve ter uma avaliação. Para este exercício, não se prevê reavaliação automática do mesmo pedido.
- Uma avaliação pode ter zero ou vários motivos. Uma aprovação pode não ter motivos adversos.
- Um pedido tem um ou vários registos de histórico, incluindo o registo inicial.
- Cada avaliação, motivo e registo de histórico pertence a um único registo da entidade principal correspondente.

### Decisões de modelação e restrições

1. **NIF como texto:** evita tratá-lo como um valor usado em cálculos e preserva eventuais zeros iniciais. Num registo de cliente válido, deve conter nove dígitos e ser único.
2. **Dados preservados por pedido:** rendimento, prestações, idade e situação profissional são guardados tal como usados na avaliação. Uma alteração futura dos dados do cliente não modifica pedidos anteriores.
3. **Submissões inválidas:** a associação ao cliente é opcional. Se forem guardados campos em falta ou valores que não possam ser convertidos para os tipos previstos, deve preservar-se a representação submetida numa estrutura de entrada própria, permitindo campos tipados nulos. O diagrama é lógico; esta representação deve ser detalhada no modelo físico.
4. **Precisão numérica:** os montantes e as taxas usam tipos decimais. A precisão, a escala e os limites técnicos devem ser definidos na implementação. A comparação com os limites antecede o arredondamento para apresentação.
5. **Indicadores opcionais:** num pedido inválido, os indicadores não calculados são nulos, e não zero.
6. **Motivos estruturados:** cada motivo possui um código estável, uma descrição e o nível de decisão associado. Por exemplo, `CREDIT_INCIDENTS` identifica incidentes de crédito e contribui para **RECUSADO**.
7. **Decisão automática preservada:** uma aprovação após análise manual altera o estado atual e acrescenta um registo de histórico, sem substituir a avaliação original.
8. **Consistência do estado:** o estado atual deve corresponder ao último registo do histórico. A atualização de ambos deve ser transacional.
9. **Estados controlados:** os estados de decisão são **PEDIDO INVÁLIDO**, **RECUSADO**, **ANÁLISE MANUAL** e **APROVADO**. Eventuais estados técnicos, como «em processamento», exigem uma definição separada.
10. **Datas e autoria:** guardar instantes de forma consistente, preferencialmente em UTC, e aplicar o fuso horário acordado na apresentação e nos relatórios. O autor pode identificar o sistema ou o analista responsável.
11. **Extensão futura:** se for necessário reavaliar um pedido, a relação entre pedido e avaliação passa a permitir várias avaliações, com uma regra explícita para identificar a avaliação relevante para cada relatório.

