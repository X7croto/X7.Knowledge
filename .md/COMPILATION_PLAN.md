# COMPILATION_PLAN.md

**Projeto:** X7.Knowledge
**Versão:** 2.3
**Status:** Normativo (autoridade 2)
**Derivado de:** `PROJECT_CONSTITUTION.md` v2.5
**Substitui:** `COMPILATION_PLAN.md` v1.0 e `ROADMAP.md` v1.0 (ambos revogados — ver ADR-032)

---

## 1. Propósito

A Constituição responde *o que o projeto é*. Este documento responde *como o compilador evolui*.

Uma **capacidade** é um conhecimento permanente que o compilador passa a possuir. Implementações são substituíveis; capacidades não.

Este documento não descreve implementação, arquitetura interna, tecnologia nem backlog.

---

## 2. Regras do plano

- **PL-01 — Permanência.** Capacidade concluída nunca é removida. Pode ser aprimorada.
- **PL-02 — Independência tecnológica.** Nenhuma capacidade menciona Roslyn, Markdown, SQLite, JSON ou Git. São detalhes de implementação.
- **PL-03 — Ordem.** Nenhuma capacidade inicia antes da conclusão verificada de suas dependências.
- **PL-04 — Entrega publicada.** Toda capacidade termina com projeção publicada e utilizável. Não existe capacidade cujo resultado seja apenas código interno.
- **PL-05 — Conclusão verificável.** Todo critério de conclusão é objetivo e automatizável. Julgamento subjetivo não conclui capacidade.
- **PL-06 — Não regressão.** Nenhuma capacidade degrada conhecimento ou projeção anterior.
- **PL-07 — Medição obrigatória.** Toda capacidade mede o Context Ratio (Constituição §7) antes e depois. O "antes" é produzido por corte na capacidade anterior sobre o snapshot atual (ADR-038), nunca recuperado de medição passada. Aumento da mediana do conjunto pareado (ADR-034) bloqueia a conclusão.
- **PL-08 — Nível declarado.** Toda capacidade declara o nível de aquisição exigido (S ou X). Capacidade de nível S não produz saída degradada em nível X; produz ausência declarada.

---

## 3. Estrutura de uma capacidade

Toda capacidade declara exatamente estes cinco campos:

| Campo | Conteúdo |
|---|---|
| **Objetivo** | O comportamento novo do compilador |
| **Conhecimento produzido** | Quais Observations e Inferences passam a existir |
| **Projeções mínimas** | O que a Base publica ao final |
| **Dependências** | Apenas capacidades anteriores |
| **Critério de conclusão** | Condição objetiva e testável |

---

## 4. Cadeia de evolução

```
C01 → C02 → C03 → C04 → C05 → C06 → C07 → C08 → C09 → C10 → C11 → C12
```

Detalhamento das dependências reais (não estritamente lineares):

| Capacidade | Depende de | Nível exigido |
|---|---|---|
| C01 Estrutura Física | — | X |
| C02 Estrutura Arquitetural | C01 | X |
| C03 Estrutura do Código | C02 | X (parcial) / S (completa) |
| C04 Modelo Estrutural | C03 | S |
| C05 Modelo Comportamental | C04 | S |
| C06 Relações | C04, C05 | S |
| C07 Fluxos | C06 | S |
| C08 Convenções | C03, C06 | S |
| C09 Padrões | C08 | S |
| C10 Linguagem de Domínio | C09 | S |
| C11 Regras de Negócio | C10 | S |
| C12 Consolidação e Publicação | C01–C11 | — |

O compilador é utilizável e entrega valor a partir de C01. Cada capacidade concluída amplia a Base sem invalidar a anterior.

---

# C01 — Aquisição da Estrutura Física

**Objetivo.** Compreender a organização física de uma solução. É a fundação: nenhuma outra capacidade existe sem ela.

**Conhecimento produzido.**
- Solução: nome, identidade, raiz.
- Projetos: nome, caminho relativo, diretório, identidade.
- Target frameworks de cada projeto.
- Organização em pastas de solução e hierarquia lógica.
- Tipo de saída de cada projeto (biblioteca, executável, teste), quando declarado.
- Nível de aquisição alcançado.

**Projeções mínimas.**
```
Knowledge/
  README.md          visão geral da Base e manifesto legível
  Structure/
    Solution.md      solução, projetos, frameworks, árvore física
```

**Dependências.** Nenhuma.

**Critério de conclusão.**
1. Qualquer solução suportada compila sem erro.
2. Toda a estrutura física é reproduzida corretamente contra um caso de referência verificado manualmente.
3. Duas compilações da mesma entrada produzem saída byte-idêntica.
4. Nenhum conhecimento desta capacidade depende de análise semântica.
5. CR medido e registrado como linha de base.

---

# C02 — Representação Arquitetural

**Objetivo.** Compreender como os projetos se relacionam e que arquitetura formam.

**Conhecimento produzido.**
- Dependências entre projetos.
- Referências externas declaradas (identidade e versão, sem resolver conteúdo).
- Grafo de dependência entre projetos, com detecção de ciclos.
- Camadas inferidas por posição no grafo, com Evidence e Confidence.
- Projetos-raiz e projetos-folha.

**Projeções mínimas.**
```
Knowledge/
  Architecture/
    Architecture.md        visão arquitetural
    ProjectDependencies.md grafo de dependências
```

**Dependências.** C01.

**Critério de conclusão.**
1. A arquitetura da solução é compreensível exclusivamente pela projeção publicada, sem abrir código.
2. Toda dependência declarada aparece na Base; nenhuma dependência inventada aparece.
3. Toda camada inferida aponta sua Evidence.
4. CR não regride.

---

# C03 — Representação Estrutural do Código

**Objetivo.** Representar a organização lógica do código sem interpretar comportamento.

**Conhecimento produzido.**
- Namespaces e sua hierarquia.
- Tipos existentes e sua localização (projeto, namespace, arquivo).
- Distribuição de tipos por projeto e por namespace.
- Relação entre organização física e organização lógica.

**Projeções mínimas.**
```
Knowledge/
  Structure/
    Namespaces.md
    Types.Index.md
```

**Dependências.** C02.

**Critério de conclusão.**
1. Qualquer tipo da solução é localizável pela Base sem consultar código.
2. Em nível X, tipos são listados e a limitação é declarada em cada Observation.
3. CR não regride.

---

# C04 — Modelo Estrutural

**Objetivo.** Transformar tipos em entidades estruturadas da Base. O compilador deixa de conhecer arquivos e passa a conhecer modelos.

**Conhecimento produzido.**
Para cada tipo: classificação (classe, interface, record, struct, enum, delegate), nome, namespace, projeto, localização, modificadores, parâmetros genéricos, tipo base declarado e interfaces implementadas.

**Projeções mínimas.** (Alteradas por ADR-035 e ADR-036.)
```
Knowledge/
  Structure/
    Types/
      INDEX.md        projeto, contagem de tipos, link
      {projeto}.md    inventário do projeto, seccionado por namespace
  Relations/
    INDEX.md
    {projeto}.md      herança e implementação, por projeto
```

Partição por projeto e separação entre inventário e relação são exigidas por
`KNOWLEDGE_MODEL.md` §9.1. O índice nunca lista nomes de tipo.

Acessibilidade e modificadores integram o conhecimento produzido e **não** são
publicados nesta projeção: são consultados com a superfície pública, no C05
(ADR-036). IV-07 proíbe projeção com informação ausente do modelo, não o
contrário.

**Dependências.** C03. Exige nível S para relações entre tipos; classificação,
modificadores, parâmetros genéricos e aninhamento são obtidos em qualquer
nível.

**Critério de conclusão.**
1. Todo tipo da solução possui representação própria e completa na Base, verificado por IV-14 (`KNOWLEDGE_MODEL.md` §11).
2. Herança e implementação são fatos resolvidos semanticamente, nunca deduzidos por nome.
3. CR não regride.

---

# C05 — Modelo Comportamental

**Objetivo.** Adicionar comportamento aos modelos estruturais. O compilador passa de *o que existe* para *o que cada componente faz*.

**Conhecimento produzido.**
Métodos, construtores, propriedades, campos, eventos, operadores, assinaturas, parâmetros, tipos de retorno, modificadores, restrições genéricas.

A separação entre representação estrutural e comportamental é preservada no modelo.

**Entrega em fatias.** A primeira fatia cobre métodos, construtores e
propriedades com assinatura (ADR-039). Campos, eventos, operadores,
indexadores e restrições genéricas vêm na seguinte, e até lá a ausência é
declarada por `acquisition.limitation` de escopo `type-members-partial`.
Fatiar não altera a capacidade: ela só conclui quando todo o conhecimento
acima existe.

**Projeções mínimas.** (Alteradas por ADR-040.)
```
Knowledge/
  Behavior/
    INDEX.md                  projeto, contagem, convenção de nome
    {projeto}/
      {nomeQualificado}.md    superfície pública de um tipo
```

Um arquivo por tipo, e não por projeto: a unidade de consulta desta projeção é
o tipo, porque quem pergunta o que um tipo expõe já sabe qual é o tipo
(`KNOWLEDGE_MODEL.md` §9.1, segunda regra). O índice nunca lista nomes de
tipo; o caminho é derivado da identidade.

É nesta projeção que acessibilidade e modificadores de tipo aparecem
publicados, quitando o prazo declarado na ADR-036.

**Dependências.** C04. Exige nível S. Em nível X a capacidade não produz saída
degradada: declara `acquisition.limitation` de escopo `type-members` e nada
mais (PL-08).

**Critério de conclusão.**
1. O comportamento público da solução é compreensível sem abrir código.
2. Toda assinatura publicada é semanticamente correta e verificável contra o compilador de referência.
3. CR não regride.

O critério 1 não é verificável por invariante, ao contrário do critério
equivalente do C04: não existe invariante de cobertura para membro, porque
tipo sem membro é legítimo e o modelo não sabe o que ficou de fora
(ADR-039). Quem o torna objetivo, como PL-05 exige, é o critério 2 — a
conferência de assinatura contra o compilador de referência.

---

# C06 — Representação das Relações

**Objetivo.** Compreender o sistema como conjunto conectado, e não como componentes isolados.

**Conhecimento produzido.**
- Herança e implementação, em ambas as direções.
- Composição e agregação.
- Dependências entre tipos.
- Instanciações.
- Registros de injeção de dependência.
- Uso de interfaces por implementação concreta.

**Projeções mínimas.**
```
Knowledge/
  Relations/
    TypeDependencies.md
    Implementations.md
    Injections.md
```

**Dependências.** C04, C05. Exige nível S.

**Critério de conclusão.**
1. Toda relação existente possui representação explícita e bidirecional.
2. É possível navegar entre componentes relacionados sem consultar código.
3. CR não regride.

---

# C07 — Representação dos Fluxos

**Objetivo.** Compreender como o comportamento percorre a solução. O objetivo não é executar código, é compreender sua organização lógica.

**Conhecimento produzido.**
- Pontos de entrada.
- Cadeias de chamada estaticamente resolvíveis.
- Chamadas através de interface, marcadas como não resolvidas e com implementações candidatas.
- Dependências comportamentais entre componentes.

**Limite declarado.** Chamadas por reflexão, delegates dinâmicos e despacho em tempo de execução não são resolvidos e são registrados como fronteira de conhecimento, nunca omitidos.

**Projeções mínimas.**
```
Knowledge/
  Flows/
    EntryPoints.md
    CallChains.md
    Boundaries.md    limites conhecidos do conhecimento estático
```

**Dependências.** C06. Exige nível S.

**Critério de conclusão.**
A Base responde, sem consultar código:
- Quem utiliza este componente?
- O que ocorre após esta operação?
- Quais componentes participam deste fluxo?
- Onde o conhecimento estático termina?

CR não regride.

---

# C08 — Inferência de Convenções

**Objetivo.** Transformar regularidades observadas em conhecimento explícito sobre como a equipe constrói software.

**Conhecimento produzido.**
Convenções de nomenclatura, organização de diretórios, namespaces, registro de DI, testes, validação, herança e composição.

Toda convenção declara obrigatoriamente:
- a regra observada;
- a frequência (ocorrências conformes / total);
- as exceções encontradas, nominalmente;
- a Evidence que a sustenta;
- a Confidence (`Asserted` sem exceções, `Observed` com frequência).

**Projeções mínimas.**
```
Knowledge/
  Conventions/
    Naming.md
    Organization.md
    Registration.md
    Testing.md
```

**Dependências.** C03, C06. Exige nível S.

**Critério de conclusão.**
1. A Base explica como um novo componente deve ser construído para seguir o padrão da solução.
2. Nenhuma convenção é publicada sem frequência, exceções e Evidence.
3. Um limiar de frequência declarado separa convenção de coincidência, e é configurável e registrado.
4. CR não regride.

---

# C09 — Inferência de Padrões

**Objetivo.** Identificar abstrações recorrentes específicas desta solução. O objetivo não é reconhecer padrões de literatura, é compreender os padrões que este projeto de fato usa.

**Conhecimento produzido.**
Padrões arquiteturais, de criação, de composição, de persistência, de comunicação e específicos do projeto.

Cada padrão declara: descrição, participantes, ocorrências encontradas, frequência, contraexemplos e Evidence.

Distinção obrigatória entre padrão documentado oficialmente e padrão inferido por recorrência.

**Projeções mínimas.**
```
Knowledge/
  Patterns/
    Index.md
    Architectural.md
    Implementation.md
```

**Dependências.** C08. Exige nível S.

**Critério de conclusão.**
A Base responde: quais padrões existem, onde aparecem, quais são obrigatórios, quais são opcionais e como um novo componente semelhante deve ser implementado.

CR não regride.

---

# C10 — Linguagem de Domínio

**Objetivo.** Descobrir os conceitos que a solução usa para falar do seu domínio.

**Escopo fechado (ADR-033).** Extraído exclusivamente de construções identificáveis: nomes de tipos e membros do núcleo de domínio, agrupamentos por namespace de domínio, tipos marcados por convenção já inferida em C08, e documentação estruturada já existente no repositório.

**Conhecimento produzido.**
- Glossário de termos recorrentes, com ocorrências e localização.
- Candidatos a conceito de domínio: entidade, objeto de valor, agregado, serviço, caso de uso — sempre como candidato com Evidence e Confidence.
- Divergências terminológicas: o mesmo conceito nomeado de formas diferentes.

**Limite declarado.** O compilador não interpreta significado. Ele registra terminologia e sua distribuição.

**Projeções mínimas.**
```
Knowledge/
  Domain/
    Glossary.md
    Concepts.md
    Divergences.md
```

**Dependências.** C09. Exige nível S.

**Critério de conclusão.**
1. Todo termo recorrente da solução aparece no glossário com ocorrências e localização.
2. Todo candidato a conceito declara Evidence e Confidence.
3. Nenhuma afirmação de significado é publicada sem base sintática.
4. CR não regride.

---

# C11 — Regras de Negócio

**Objetivo.** Tornar explícitas as regras que governam o domínio, dentro do que é deterministicamente observável.

**Escopo fechado (ADR-033).** Catálogo declarado de construções extraíveis:
- guard clauses e lançamentos condicionais de exceção;
- atributos de validação;
- bibliotecas de validação declarativa;
- restrições declaradas em tipos e propriedades;
- invariantes verificadas em construtores;
- condições de autorização declarativas.

O catálogo é versionado e pode crescer por ADR. Regras fora do catálogo são registradas como **lacuna conhecida**, não silenciadas.

**Conhecimento produzido.**
Regras extraídas, sua localização, sua condição, sua consequência, o conceito de domínio afetado e a Evidence.

**Projeções mínimas.**
```
Knowledge/
  Domain/
    BusinessRules.md
    Gaps.md    o que o compilador sabe que não consegue extrair
```

**Dependências.** C10. Exige nível S.

**Critério de conclusão.**
1. Toda construção do catálogo declarado é extraída com cobertura verificada sobre a solução de referência.
2. Toda regra publicada aponta localização e Evidence.
3. Lacunas são publicadas explicitamente.
4. CR não regride.

---

# C12 — Consolidação e Publicação

**Objetivo.** Transformar o conhecimento acumulado em uma Base única, integrada e navegável, e materializá-la em formatos de consumo.

Esta capacidade **não produz conhecimento novo**. Consolida e publica.

**Conhecimento produzido.**
Nenhum conhecimento novo. Produz estrutura de acesso: índices globais, referências cruzadas, mapa de conhecimento, sumário, navegação contextual.

**Projeções mínimas.**
```
Knowledge/
  README.md
  SUMMARY.md
  INDEX.md
  KnowledgeMap.md
  model/knowledge.model.json
```

Publishers adicionais (SQLite, grafo, outros formatos) são equivalentes e opcionais. Nenhum altera o modelo conceitual.

**Dependências.** C01 a C11.

**Critério de conclusão.**
1. Toda informação produzida anteriormente é alcançável pela navegação da própria Base.
2. Nenhum item de conhecimento permanece isolado, sem contexto, origem ou relacionamento.
3. O usuário não depende da estrutura física de arquivos para localizar informação.
4. A mesma Base é publicada em ao menos dois formatos preservando integralmente o modelo conceitual.
5. Nenhum Publisher produz informação ausente do KnowledgeModel — verificado por teste.
6. CR final medido e registrado.

---

## 5. Fora do escopo do compilador

Retiradas do plano por ADR-030. Permanecem aqui apenas como orientação para projetos consumidores separados.

| Retirada | Antes | Agora |
|---|---|---|
| Consulta da Base | C13 | Projeto consumidor independente |
| Consumo por agentes | C14 | Projeto consumidor independente |
| Seleção de contexto | Context Builder | Projeto consumidor independente |
| Enriquecimento semântico por IA | camada posterior | Projeto consumidor independente, descartável e regenerável |

Consumidores dependem da Base publicada. O compilador nunca depende de consumidores.

---

## 6. Evolução deste plano

Este é um documento vivo, sujeito às restrições:

- Nenhuma capacidade existente é removida.
- Nenhuma capacidade altera a finalidade de outra anterior.
- Capacidade concluída integra permanentemente o compilador.

Toda nova capacidade justifica, por ADR:
1. qual lacuna de conhecimento resolve;
2. por que não cabe em capacidade existente;
3. qual pergunta ela ajuda a responder;
4. qual seu efeito esperado sobre o Context Ratio;
5. sua posição exata na cadeia de dependências.

---

## 7. Estado final esperado

Ao término deste plano, o X7.Knowledge transforma uma solução C# suportada em uma Base de Conhecimento **navegável, verificável, incremental, determinística, rastreável, extensível e independente de tecnologia**, na qual toda afirmação declara sua origem, sua evidência e sua confiança — e cujo valor é demonstrado por redução medida do contexto necessário para compreender e evoluir a solução.

Implementações evoluem continuamente. As capacidades acima constituem o contrato permanente do compilador.

---
*Fim de `COMPILATION_PLAN.md` v2.3.*
*Alterado por ADR-034 (PL-07), ADR-035 e ADR-036 (projeções do C04),*
*ADR-039 e ADR-040 (fatias, projeções e critério do C05).*
