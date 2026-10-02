# Tarefa 6 - Resolução de Problemas

## Reporte recebido

**Assunto:** Não consigo pedir o crédito  
**Descrição:** "Estou a tentar fazer um pedido de crédito na aplicação e não consigo avançar. Dá erro."

O reporte deve ser tratado com prioridade inicial suficiente para avaliar o impacto, mas a informação disponível ainda não permite identificar a causa nem escolher uma correção.

## 1. Informação conhecida

É possível concluir apenas que:

- pelo menos um utilizador tentou iniciar ou concluir um pedido de crédito;
- a operação não produziu o resultado esperado;
- o utilizador observou algo que descreve como erro;
- o problema é suficientemente importante para o utilizador pedir uma resolução rápida.

Não se sabe se o pedido falhou realmente, se ficou guardado, se foi classificado como inválido, ou se ocorreu apenas um problema de apresentação.

## 2. Informação em falta

Antes de diagnosticar, seria necessário obter:

- data e hora aproximada da tentativa, incluindo o fuso horário;
- ambiente e endereço da aplicação utilizados;
- etapa exata em que o problema ocorreu;
- ação realizada imediatamente antes do erro;
- texto exato da mensagem apresentada;
- identificador do pedido ou identificador de correlação, caso tenha sido mostrado;
- se o problema ocorre sempre ou apenas com determinados dados;
- se uma nova tentativa produz o mesmo resultado;
- navegador, versão, sistema operativo e tipo de dispositivo;
- existência de alterações recentes no navegador, rede ou sessão;
- existência de outros utilizadores afetados;
- estado visível do serviço e data da última publicação.

Não pediria ao utilizador a palavra-passe, dados bancários completos ou informação pessoal que não fosse necessária. Se fosse indispensável identificar o pedido, privilegiaria o identificador técnico. Qualquer NIF usado no suporte seria tratado pelos canais autorizados.

## 3. Como obter a informação necessária

### Contacto inicial com o utilizador

Responderia rapidamente, confirmando a receção e pedindo os elementos mínimos em falta: hora, etapa, mensagem exata e identificador de correlação ou pedido. Perguntaria ainda se o problema continua a ocorrer e se outros utilizadores parecem afetados.

Não assumiria que se trata de erro de utilização. Uma validação de negócio pouco clara também pode ser um defeito de experiência do utilizador.

### Correlação com informação técnica

Com a hora e o identificador de correlação, consultaria:

- logs estruturados da aplicação;
- métricas de erros e latência;
- estado e métricas do SQL Server;
- histórico do pedido, caso exista;
- falhas de autenticação ou autorização;
- eventos de rede, proxy ou balanceador;
- alterações de configuração e publicações recentes.

Procuraria determinar se a tentativa chegou ao servidor, se iniciou uma transação, se gravou dados e em que componente ocorreu a falha.

### Reprodução controlada

Tentaria reproduzir o problema num ambiente seguro com dados de teste e condições semelhantes. Verificaria o mesmo browser e, depois, um browser suportado alternativo. Não faria experiências diretamente sobre o pedido real do cliente sem avaliar o risco de duplicação ou alteração de estado.

### Avaliação do impacto

Confirmaria:

- quantos utilizadores e pedidos foram afetados;
- quando começou o problema;
- se todas as submissões falham ou apenas algumas;
- se existe perda, duplicação ou inconsistência de dados;
- se há impacto apenas na interface ou também na decisão e persistência;
- se existe uma alternativa temporária segura.

Esta avaliação determina a severidade do incidente e a urgência da intervenção.

## 4. Hipóteses de erro

### Dados e validação

- campo obrigatório vazio;
- NIF com formato inválido;
- rendimento, montante ou prazo igual ou inferior a zero;
- separador decimal ou formato numérico inesperado;
- valor fora do intervalo suportado;
- opção profissional ou resposta sobre incidentes não reconhecida;
- mensagem de validação inexistente ou pouco visível, levando o utilizador a interpretar uma rejeição como erro técnico.

### Interface e navegador

- botão desativado ou evento de submissão não executado;
- erro de JavaScript;
- token antifalsificação inválido ou sessão expirada;
- incompatibilidade com navegador ou dispositivo;
- recursos CSS ou JavaScript não carregados;
- problema de acessibilidade que impede a interação;
- envio repetido causado por duplo clique ou atualização da página.

### Aplicação

- exceção não tratada na avaliação ou no mapeamento dos dados;
- erro ao serializar ou desserializar o pedido;
- configuração ou segredo em falta;
- tempo limite ao contactar uma dependência;
- versão incompatível entre a aplicação e a base de dados;
- falha posterior à gravação que apresenta erro apesar de o pedido já existir.

### Base de dados

- SQL Server indisponível;
- credenciais expiradas ou sem permissões;
- ligação esgotada ou tempo limite;
- migration não aplicada e tabela ou coluna inexistente;
- restrição de integridade violada;
- bloqueio, deadlock ou falta de espaço;
- falha da transação;
- pedido gravado parcialmente por uma implementação sem transação.

### Infraestrutura e segurança

- falha de DNS, proxy, certificado ou conectividade;
- contentor ou serviço parado;
- problema no balanceador de carga;
- regra de firewall alterada;
- recurso insuficiente, como CPU, memória ou disco;
- autenticação indisponível;
- bloqueio legítimo por um controlo de segurança que não foi comunicado claramente.

### Publicação recente

- regressão introduzida numa nova versão;
- variável de ambiente incorreta;
- pacote ou dependência incompatível;
- alteração de regras sem a migration ou configuração correspondente;
- cache com conteúdo de versões diferentes.

## 5. Equipas a envolver

As equipas seriam envolvidas de acordo com os indícios encontrados:

- **suporte ou service desk:** recolha inicial, comunicação com o utilizador e registo do incidente;
- **equipa de desenvolvimento:** reprodução, análise dos logs, diagnóstico do código e correção;
- **base de dados:** disponibilidade, desempenho, bloqueios, permissões, migrations e recuperação;
- **infraestrutura, plataforma ou operações:** contentores, rede, certificados, capacidade e publicação;
- **segurança e identidade:** autenticação, autorização, bloqueios e eventual incidente de segurança;
- **analista de negócio ou risco:** validações ambíguas, comportamento esperado e impacto das regras;
- **qualidade:** reprodução, validação da correção e testes de regressão;
- **proteção de dados ou compliance:** apenas se houver exposição, perda ou tratamento indevido de dados pessoais;
- **gestão do serviço ou incidente:** coordenação quando o impacto for elevado ou abranger vários utilizadores.

Evitaria envolver todas as equipas de imediato. A triagem inicial deve encaminhar o problema com evidência suficiente, mantendo capacidade de escalar rapidamente.

## 6. Plano de atuação

1. Registar o incidente e responder ao utilizador.
2. Obter hora, etapa, mensagem e identificador de correlação.
3. Verificar o estado geral do serviço e procurar outros casos semelhantes.
4. Correlacionar logs, métricas, base de dados e publicações recentes.
5. Determinar se o pedido foi ou não guardado, evitando duplicações.
6. Reproduzir o problema com dados de teste.
7. Classificar impacto, urgência e equipas necessárias.
8. Aplicar uma mitigação segura, se existir.
9. Corrigir a causa e executar testes de regressão.
10. Publicar de forma controlada e monitorizar o resultado.
11. Informar o utilizador da resolução e confirmar o funcionamento.
12. Documentar causa, impacto, correção e ações preventivas.

Se o problema tivesse começado imediatamente após uma publicação e o impacto fosse elevado, consideraria rollback depois de confirmar que seria seguro para o esquema e para os dados. Não faria alterações diretas à base de dados sem evidência, autorização e possibilidade de recuperação.

## 7. Critério de resolução

O incidente só seria considerado resolvido depois de:

- a causa ou condição desencadeadora estar identificada;
- o utilizador conseguir concluir o fluxo esperado;
- se confirmar que não existem pedidos perdidos, duplicados ou inconsistentes;
- os testes relevantes passarem;
- a monitorização não indicar recorrência;
- a solução e as ações preventivas ficarem documentadas.