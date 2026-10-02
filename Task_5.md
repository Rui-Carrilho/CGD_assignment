# Tarefa 5 - Melhoria da Solução

## Enquadramento

A solução desenvolvida é um protótipo funcional de pré-análise de crédito. Recebe os dados do pedido, valida-os, calcula os indicadores, aplica regras de negócio versionadas, guarda o resultado em SQL Server e permite o tratamento de pedidos enviados para análise manual.

Para evoluir para produção, seria necessário reforçar:
- segurança;
- disponibilidade;
- capacidade de auditoria;
- operação da aplicação. 

As melhorias seguintes seriam priorizadas de acordo com o risco e o volume esperado.

## 1. Melhorias técnicas

### Autenticação e autorização

A área de análise manual teria autenticação integrada com o diretório da organização. Os utilizadores teriam perfis e permissões distintos, por exemplo:

- requerente - acesso apenas à submissão e consulta dos seus pedidos;
- analista de crédito - acesso à fila de análise e capacidade para aprovar ou recusar;
- supervisor - permissões de consulta, redistribuição e eventual reabertura;
- auditor - acesso de leitura ao histórico e aos relatórios.

O nome do analista deixaria de ser introduzido manualmente. A identidade usada no histórico seria obtida da sessão autenticada, evitando a possibilidade de alguém registar uma decisão em nome de outra pessoa.

### Gestão de segredos e acesso à base de dados

A palavra-passe guardada num ficheiro `.env` é adequada apenas a desenvolvimento local. Em produção, as credenciais seriam obtidas de um gestor de segredos aprovado pela organização. A aplicação utilizaria uma conta de serviço com os privilégios mínimos necessários, sem recorrer ao utilizador administrador `sa`.

As ligações ao SQL Server seriam cifradas e os ambientes de desenvolvimento, testes, qualidade e produção teriam bases de dados e credenciais separadas.

### Configuração e injeção de dependências

O endereço e as opções de ligação à base de dados seriam configurados externamente, em vez de estarem definidos na classe de ligação. `CreditEvaluator`, `CreditRequestRepository` e `ReportRepository` seriam registados no contentor de injeção de dependências do ASP.NET Core. Isto facilitaria testes, configuração por ambiente e substituição controlada de implementações.

### Evolução controlada da base de dados

Os scripts SQL passariam a ser migrations versionadas e executadas por um processo de entrega controlado. Cada alteração teria uma estratégia de compatibilidade e de reversão. Os backups seriam automáticos e seriam realizados testes regulares de restauro, pois um backup não verificado não garante recuperação.

### Concorrência e consistência

A solução já altera o estado do pedido e o respetivo histórico na mesma transação. Em produção, acrescentaria controlo de concorrência explícito, por exemplo através de `rowversion`, para detetar alterações simultâneas.

Assim, se dois analistas abrissem o mesmo pedido, apenas a primeira decisão válida seria aceite. O segundo receberia uma mensagem clara de que o pedido já tinha sido tratado. As operações de submissão também teriam uma chave de idempotência para impedir pedidos duplicados causados por duplo clique, repetição automática ou falha de rede.

### Resiliência e tratamento de erros

A aplicação teria tempos limite definidos, cancelamento de operações e repetição limitada apenas para falhas transitórias seguras. Uma indisponibilidade da base de dados originaria uma mensagem compreensível para o utilizador e um identificador de correlação para suporte, sem apresentar detalhes técnicos ou dados sensíveis.

Seriam adicionadas páginas de erro adequadas, verificações de saúde da aplicação e da base de dados, e mecanismos de degradação controlada. Uma operação de escrita nunca seria repetida sem garantir idempotência.

### Observabilidade

Os registos seriam estruturados e conteriam, quando aplicável:

- identificador de correlação;
- identificador técnico do pedido;
- operação executada;
- duração;
- resultado técnico;
- versão da aplicação e das regras;
- serviço ou dependência que falhou.

Não seriam registados rendimentos, NIF completos, palavras-passe ou outros dados pessoais desnecessários. Métricas e alertas acompanhariam taxas de erro, latência, indisponibilidade da base de dados, tamanho da fila manual e tempo médio de tratamento.

### Privacidade e proteção de dados

Os dados seriam classificados segundo a sua sensibilidade. Aplicar-se-iam cifragem em trânsito e em repouso, controlo de acesso, mascaramento de NIF nas listagens, política de retenção e eliminação, e registo de acessos administrativos.

Os dados recolhidos seriam limitados ao necessário para a finalidade definida. A utilização para reporting ou ambientes não produtivos recorreria a dados anonimizados ou pseudonimizados.

### Desempenho e escalabilidade

As listagens teriam paginação no servidor, filtros e limites configuráveis. Os índices seriam revistos com base nas consultas reais e nos planos de execução. Os relatórios mais pesados poderiam ser executados numa réplica de leitura ou num repositório analítico, evitando impacto no processamento de pedidos.

### Processo de entrega

O código passaria por integração contínua com compilação, testes, análise estática, verificação de dependências e validação dos scripts de base de dados. A entrega seria automatizada por ambiente, com aprovação para produção, possibilidade de rollback e monitorização após a publicação.

## 2. Testes adicionais

### Testes unitários das regras

Além dos cenários fornecidos, seriam mantidos testes para todos os limites relevantes:

- rendimento, montante e prazo iguais a zero;
- idade exatamente igual a 18 anos;
- idade final exatamente igual e imediatamente superior a 75 anos;
- taxa de esforço exatamente igual e imediatamente superior a 35% e 50%;
- montante exatamente igual e imediatamente superior a 20 vezes o rendimento;
- montante exatamente igual e imediatamente superior a 50.000 euros;
- aplicação simultânea de várias regras e respetiva prioridade;
- campos em falta, formatos inválidos, valores negativos e valores excessivamente grandes;
- comportamento de arredondamento e aceitação dos formatos decimais definidos.

### Testes de integração com SQL Server

Os repositórios seriam testados contra uma instância isolada de SQL Server. Estes testes verificariam:

- gravação e leitura integral de um pedido;
- persistência dos motivos e indicadores;
- rollback quando uma operação falha a meio;
- histórico criado em conjunto com cada transição;
- transições permitidas e proibidas;
- concorrência entre dois analistas;
- execução das migrations numa base de dados vazia e numa versão anterior;
- resultados das cinco consultas de reporting com dados conhecidos.

Cada teste seria independente e limparia os seus próprios dados.

### Testes da aplicação web

Seriam criados testes de integração e end-to-end para os fluxos principais:

1. submeter um pedido válido e consultar o resultado;
2. submeter dados inválidos e visualizar todos os motivos;
3. enviar um pedido para análise manual;
4. aprovar ou recusar como analista autenticado;
5. confirmar a atualização da fila, do histórico e dos relatórios;
6. impedir que um utilizador sem permissão aceda às funções do analista;
7. impedir submissões duplicadas após atualização da página ou repetição do pedido HTTP.

### Testes não funcionais

Seriam igualmente realizados:

- testes de carga e capacidade;
- testes de indisponibilidade e recuperação da base de dados;
- testes de segurança, incluindo controlo de acesso, CSRF, injeção e exposição de dados;
- análise de dependências e vulnerabilidades;
- testes de acessibilidade por teclado e com leitor de ecrã;
- testes nos navegadores e tamanhos de ecrã suportados;
- testes de backup e restauro;
- testes de aceitação com analistas de crédito.

## 3. Informação para auditoria e reporting

### Auditoria do pedido e da decisão

Guardaria de forma imutável:

- identificador único do pedido;
- dados originais submetidos;
- data e hora de submissão em UTC;
- decisão automática original;
- motivos estruturados, com código, severidade e mensagem;
- indicadores calculados;
- versão das regras e versão da aplicação;
- todas as alterações de estado;
- decisão anterior e decisão nova;
- data e hora de cada alteração;
- identidade autenticada do interveniente;
- fundamentação da decisão manual;
- identificador de correlação da operação.

A decisão automática original não seria substituída por uma decisão humana. O estado atual e o histórico permitiriam reconstruir todo o percurso do pedido.

### Informação operacional

Para operação e suporte guardaria eventos técnicos relacionados com erros, duração das operações e dependências utilizadas. Estes eventos usariam identificadores técnicos e evitariam dados pessoais sempre que possível.

### Informação para reporting

O modelo analítico deveria permitir medir, entre outros:

- pedidos por estado e por período;
- motivos automáticos de recusa mais frequentes;
- pedidos repetidos por cliente;
- volume e idade da fila manual;
- tempo médio até à decisão;
- percentagem de análises manuais aprovadas e recusadas;
- distribuição da taxa de esforço e do montante solicitado;
- utilização de cada versão das regras;
- taxas de erro e abandono do processo.

Relatórios agregados não deveriam expor NIF ou outros dados pessoais sem necessidade funcional. As definições de cada indicador, período e fuso horário seriam documentadas para que os resultados fossem reproduzíveis.


