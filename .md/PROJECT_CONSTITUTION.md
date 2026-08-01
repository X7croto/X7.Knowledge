# PROJECT_CONSTITUTION.md

**Projeto:** X7.Knowledge
**Versão:** 2.1
**Status:** Normativo — fonte única de verdade
**Substitui:** `PROJECT_CONSTITUTION.md` v1.0 e `ObjetivoX7.docx` (ambos revogados)

---

## 0. Autoridade e hierarquia documental

Existem exatamente três documentos normativos. Nenhum outro documento cria obrigação.

| Documento | Responde | Autoridade |
|---|---|---|
| `PROJECT_CONSTITUTION.md` | O que o projeto é | 1 — prevalece sempre |
| `COMPILATION_PLAN.md` | Quais capacidades o compilador adquire e em que ordem | 2 |
| `KNOWLEDGE_MODEL.md` | Qual é a forma exata do artefato produzido | 3 |

Regras:

- Em caso de divergência, o documento de menor número prevalece.
- Nenhum documento fora desta tabela é normativo. Conversas, anotações, código e comentários não alteram a arquitetura.
- Alteração normativa só ocorre por ADR aprovada e registrada na Seção 8.
- Documentos revogados permanecem no repositório apenas como histórico, marcados como `REVOGADO`.

---

## 1. Missão

### M-01

O X7.Knowledge é um **compilador de conhecimento** para soluções C#.

Ele transforma código-fonte em conhecimento estruturado, de forma determinística, sem uso de IA.

### M-02

O problema que resolve: uma LLM possui contexto limitado; uma solução grande não cabe nesse contexto.

O X7.Knowledge produz uma representação que permite compreender e evoluir a solução **lendo drasticamente menos código**.

### M-03

O critério de sucesso do projeto é objetivo e mensurável: **redução de contexto**, medida conforme a Seção 7.

Conhecimento que não reduz contexto não justifica sua existência.

### M-04

O produto é conhecimento **sobre** o sistema, nunca o sistema em si.

---

## 2. Não objetivos

O X7.Knowledge **não é** e **não se tornará**:

| ID | Não objetivo | Consequência |
|---|---|---|
| N-01 | Um indexador de código | Catalogar símbolos não é o produto |
| N-02 | Um mecanismo de busca textual | Localizar arquivos não é o objetivo |
| N-03 | Um analisador estático / gerador de diagnósticos | O produto não é relatório de problemas |
| N-04 | Um servidor | Servir consultas não pertence ao núcleo |
| N-05 | Um agente | Executar tarefas pertence ao consumidor |
| N-06 | Dependente de qualquer LLM | Não conhece Claude, GPT, Ollama, Cursor, MCP ou equivalentes |
| N-07 | Um respondedor de perguntas | Produz conhecimento; responder é do consumidor |
| N-08 | Um gerador de código | Geração pertence ao consumidor |
| N-09 | Específico de uma solução | É genérico para qualquer solução C# suportada |

---

## 3. Ontologia oficial

Esta é a **única** terminologia aceita. Os termos abaixo têm exatamente este significado em documentação, código, ADRs e nomes de tipos.

### 3.1 Cadeia de conhecimento

A cadeia é linear e cada nível deriva exclusivamente do anterior.

```
Code → Syntax → Semantics → Observation → Evidence → Inference → KnowledgeModel
```

| Termo | Definição |
|---|---|
| **Code** | Código-fonte da solução. Matéria-prima. Nunca é o produto. |
| **Syntax** | Representação sintática do código. Não produz conhecimento. |
| **Semantics** | Representação semântica resolvida (tipos, símbolos, referências). Não produz conhecimento. |
| **Observation** | Fato atômico observado deterministicamente, com proveniência obrigatória. Ex.: *"o tipo A implementa a interface B"*. Uma Observation nunca interpreta. |
| **Evidence** | Agrupamento nomeado e consistente de Observations que sustenta uma conclusão. |
| **Inference** | Conhecimento derivado exclusivamente de Evidence, por regra determinística e declarada. Toda Inference aponta para sua Evidence. |
| **Knowledge** | Representação conceitual do projeto: arquitetura, convenções, padrões, responsabilidades, domínio. Composto de Observations e Inferences. |
| **KnowledgeModel** | O modelo canônico único que contém todo o Knowledge produzido. **É o artefato central e o produto oficial do compilador.** |

`Fact` (v1) é sinônimo retirado. Usar `Observation`.
`Knowledge IR` (v1) é sinônimo retirado. Usar `KnowledgeModel`.

### 3.2 Componentes

| Termo | Definição |
|---|---|
| **Pass** | Etapa determinística do pipeline de compilação. |
| **Producer** | Componente que **adiciona** conhecimento ao KnowledgeModel durante um Pass. Nunca modifica conhecimento existente. |
| **Publisher** | Componente que **materializa** o KnowledgeModel em um formato. Nunca produz, infere ou altera conhecimento. |
| **Projection** | Saída produzida por um Publisher: Markdown, JSON, SQLite, grafo, etc. Uma Projection nunca é o produto. |
| **Provenance** | Metadado obrigatório de todo item de conhecimento: origem, produtor, capacidade e nível de aquisição. |
| **Consumer** | Qualquer sistema externo que utilize o KnowledgeModel. Fora do escopo do compilador. |
| **Context** | Subconjunto do KnowledgeModel selecionado por um Consumer para executar uma tarefa. Fora do escopo do compilador. |

### 3.3 Conhecimento derivado

| Termo | Definição |
|---|---|
| **Convention** | Regularidade recorrente observada na solução (nomenclatura, organização, registro de DI, testes). É Inference, sempre com frequência e Evidence. |
| **Pattern** | Generalização de múltiplas ocorrências semelhantes. Ex.: *"Services implementam interfaces (117/120 casos)"*. É Inference. |
| **Confidence** | Atributo obrigatório de toda Inference: `Asserted` (regra exata, sem exceções) ou `Observed` (regularidade estatística, com frequência declarada). |

---

## 4. Princípios

Princípios são invioláveis. Qualquer decisão que os contrarie é inválida, independentemente do benefício aparente.

### PR-01 — Modelo canônico único
Existe exatamente um modelo de conhecimento: o `KnowledgeModel`. Toda outra estrutura deriva dele.

### PR-02 — Compilação determinística
A mesma entrada produz exatamente a mesma saída, em qualquer máquina e execução. Ver Seção 6.

### PR-03 — IA não participa da compilação
Nenhuma etapa entre Code e KnowledgeModel utiliza modelo de linguagem, heurística não declarada ou fonte não reproduzível.

### PR-04 — Proveniência obrigatória
Todo item de conhecimento declara de onde veio, quem produziu e por qual capacidade. Conhecimento sem proveniência é inválido e deve falhar a compilação.

### PR-05 — Monotonicidade dentro da compilação
Durante uma compilação, conhecimento apenas é adicionado. Nenhum Producer remove ou modifica conhecimento produzido por outro Producer.

**Esclarecimento normativo (resolve conflito v1):** a monotonicidade é uma propriedade **intra-compilação**. Entre compilações não há acumulação: cada compilação é uma função total da entrada e substitui integralmente a saída anterior. Um tipo removido do código desaparece do KnowledgeModel seguinte. Base de Conhecimento nunca contradiz a solução.

### PR-06 — Publishers não inferem
Materializar não é produzir. Se um Publisher precisa calcular algo, esse cálculo pertence a um Producer.

### PR-07 — Armazenamento não define arquitetura
Graph, JSON, SQLite, Markdown, Neo4j, DuckDB são projeções equivalentes. Nenhum define o modelo.

### PR-08 — Conhecimento antes de estrutura
A arquitetura é orientada pelas perguntas que precisam ser respondidas, não pela forma do código. O compilador explica o projeto, não explica arquivos.

### PR-09 — Toda evolução reduz contexto
Nenhuma capacidade entra no compilador sem demonstrar ganho na métrica da Seção 7.

### PR-10 — Fluxo unidirecional
Consumidores nunca alteram o resultado da compilação. Enriquecimento semântico por IA, se existir, é camada externa, descartável e regenerável.

### PR-11 — Explicitação da incerteza
Toda Inference declara sua `Confidence` e sua Evidence. O compilador nunca apresenta regularidade estatística como verdade absoluta.

### PR-12 — Simplicidade primeiro
Infraestrutura só é introduzida quando um problema concreto já existe. Complexidade sem benefício mensurável é rejeitada.

---

## 5. Arquitetura congelada

### 5.1 Pipeline

```
                  ── COMPILAÇÃO (determinística, sem IA) ──
Code → Syntax → Semantics → Observations → Evidence → Inferences → KnowledgeModel
                                                                        │
                                                                        ▼
                                                                   Publishers
                                                                        │
                                                                        ▼
                                                                  Projections
─────────────────────────────────────────────────────────────────────────────
                  ── CONSUMO (fora do escopo do projeto) ──
                                Context Builder → LLM / Agentes / Ferramentas
```

### 5.2 Fronteira oficial

**Dentro do escopo:** aquisição, observação, inferência, consolidação, publicação.

**Fora do escopo:** seleção de contexto, consulta, interpretação, geração, execução, agentes.

A missão do X7.Knowledge termina quando o KnowledgeModel é publicado.

### 5.3 Níveis de aquisição

Nem toda solução oferece as mesmas garantias. O compilador declara explicitamente o nível em que operou.

| Nível | Condição | Conhecimento disponível |
|---|---|---|
| **S — Semântico** | A solução restaura e compila; o modelo semântico está disponível | Completo. Herança, implementação, referências e chamadas são fatos resolvidos |
| **X — Sintático** | A solução não compila ou o SDK não está disponível | Reduzido. Apenas o que a árvore sintática garante. Relações entre tipos ficam não resolvidas |

Regras:

- O nível é registrado no manifesto e **em cada Observation**.
- Uma capacidade que exige nível S declara isso e não produz saída degradada silenciosamente.
- O compilador nunca infere relação semântica a partir de nome quando está em nível X.

---

## 6. Determinismo — regras operacionais

Determinismo é obrigação verificável, não intenção. Toda compilação obedece a:

- **D-01** Toda coleção de saída é ordenada por identidade canônica (ordinal, invariante de cultura). Nunca por ordem de descoberta no sistema de arquivos.
- **D-02** Todo caminho na saída é relativo à raiz da solução, com separador `/`. Caminhos absolutos nunca aparecem na saída.
- **D-03** Nenhum timestamp, hostname, usuário, variável de ambiente ou número de execução aparece no KnowledgeModel.
- **D-04** Paralelismo é permitido. Influência do paralelismo na saída não é.
- **D-05** Toda identidade é derivada de conteúdo ou de posição lógica estável, nunca de endereço de memória, GUID gerado ou contador de execução.
- **D-06** A serialização é canônica: UTF-8 sem BOM, chaves ordenadas, `LF`, formatação numérica invariante.
- **D-07** O manifesto registra versão do compilador, versão do modelo, nível de aquisição e hash das entradas.
- **D-08** Teste obrigatório de todo Producer: duas compilações da mesma entrada produzem saídas byte-idênticas.

---

## 7. Métrica oficial de compressão de contexto

O princípio PR-09 é inútil sem medida. Esta seção a define.

### 7.1 Benchmark

Existe um conjunto versionado de **perguntas-tarefa** sobre uma **solução de referência** fixa. Exemplos de perguntas-tarefa:

- Onde implemento uma nova feature do tipo X?
- Quais componentes são impactados ao alterar o comportamento Y?
- Qual convenção devo seguir ao criar um novo componente da família Z?
- Onde está a regra de negócio que governa W?

### 7.2 Medida

Para cada pergunta-tarefa:

- `T_code` = tokens dos arquivos-fonte que seriam necessários para responder sem a Base.
- `T_kb` = tokens do subconjunto da Base suficiente para responder.
- **CR** (Context Ratio) = `T_kb / T_code`.

A métrica do projeto é a **mediana de CR** sobre o conjunto de perguntas.

### 7.3 Regras

- **MT-01** Toda capacidade nova mede CR antes e depois. O resultado entra no critério de conclusão.
- **MT-02** Nenhuma capacidade pode aumentar a mediana de CR do **conjunto pareado**, definido em ADR-034. Aumento é regressão e bloqueia a conclusão. Aumento em pergunta individual exige causa registrada; sem ela, bloqueia igualmente.
- **MT-03** Uma resposta que a Base não consegue sustentar conta como falha, não como CR baixo.
- **MT-04** O conjunto de perguntas cresce; nunca encolhe.

---

## 8. ADRs

ADRs de v1 permanecem válidas salvo revogação explícita. As ADRs 027–033 abaixo resolvem os conflitos identificados na consolidação. As ADRs 034 e 035 são posteriores e decorrem de medição.

### ADR-001 a ADR-026 — Mantidas

Consolidadas e incorporadas às Seções 1–5 deste documento. Exceções:

- **ADR-002** — alterada: o artefato central mantém-se único, mas passa a chamar-se `KnowledgeModel`. Ver ADR-028.
- **ADR-013** (Context Builder) — mantida como decisão histórica, porém o Context Builder deixa de ser componente do projeto. Ver ADR-030.

---

### ADR-027 — Nome oficial do projeto

**Status:** APROVADA

**Contexto:** os documentos usavam `X7.ProjectIndexer`, `X7.Knowledge` e `ProjectIndexer` de forma intercambiável. `ProjectIndexer` contradiz diretamente DR-001 e N-01, que rejeitam a definição de indexador.

**Decisão:** o nome oficial é **X7.Knowledge**. `X7.ProjectIndexer` é retirado de toda documentação, namespace e nome de assembly.

**Consequências:** renomeação de raiz de namespace. Documentos históricos permanecem com o nome antigo marcados como revogados.

---

### ADR-028 — Nome oficial do artefato central

**Status:** APROVADA

**Contexto:** `Knowledge IR` (Constituição v1) e `KnowledgeModel` (ObjetivoX7) nomeavam a mesma coisa.

**Decisão:** o artefato central é o **`KnowledgeModel`**. O termo `Knowledge IR` é retirado.

**Motivo:** "Intermediate Representation" descreve algo entre duas etapas. Pela ADR-026 e M-04, este artefato é o produto **final** do compilador, não um intermediário.

**Consequências:** todo tipo, arquivo e documento passa a usar `KnowledgeModel`.

---

### ADR-029 — Ontologia única

**Status:** APROVADA

**Contexto:** duas ontologias concorrentes: `Fact → Evidence → Knowledge` e `Observation → Evidence → Inference → Knowledge`.

**Decisão:** vale a cadeia da Seção 3.1. `Fact` é substituído por `Observation`. `Inference` passa a ser conceito de primeira classe, com `Confidence` e `Evidence` obrigatórios.

**Consequências:** a distinção entre observar e concluir torna-se explícita no modelo, não apenas na documentação.

---

### ADR-030 — Consulta e consumo saem do escopo do compilador

**Status:** APROVADA

**Contexto:** o `COMPILATION_PLAN.md` v1 listava as capacidades C13 (Consulta) e C14 (Consumo por Agentes) como capacidades do compilador. Isso contradiz ADR-012, ADR-026, DR-010 e N-04/N-05, que colocam consulta e consumo fora do núcleo. Ambas declaravam "Conhecimento Produzido: Nenhum".

**Decisão:** C13 e C14 são **retiradas** do plano de capacidades. O contrato do compilador termina em C12 (Publicação). Mecanismos de consulta e consumo passam a um anexo não normativo.

**Consequências:** o escopo do projeto fica fechado e verificável. Consumidores tornam-se projetos separados que dependem da Base, não do compilador.

---

### ADR-031 — Monotonicidade é intra-compilação

**Status:** APROVADA

**Contexto:** PF-006 (v1) determinava que conhecimento nunca é removido. Aplicado entre compilações, isso faria a Base contradizer a solução após qualquer remoção de código.

**Decisão:** vale PR-05. Monotonicidade dentro de uma compilação; substituição integral entre compilações.

**Consequências:** a Base é sempre um espelho fiel do estado atual do código. Histórico, se desejado, é responsabilidade de versionamento externo.

---

### ADR-032 — Documento único de evolução

**Status:** APROVADA

**Contexto:** existiam três planos concorrentes: `ROADMAP.md` (11 macroetapas), `COMPILATION_PLAN.md` (14 capacidades) e a Seção 11 da Constituição v1 (R-001..R-009). Descreviam a mesma evolução em vocabulários diferentes, sem mapeamento.

**Decisão:** `ROADMAP.md` é revogado e absorvido. Existe um único documento de evolução: `COMPILATION_PLAN.md` v2, onde cada capacidade declara objetivo, conhecimento produzido, projeções mínimas, dependências e critério de conclusão.

**Consequências:** uma capacidade, um critério, um lugar.

---

### ADR-033 — Escopo verificável do conhecimento de domínio

**Status:** APROVADA

**Contexto:** as capacidades de linguagem ubíqua e regras de negócio prometiam extrair invariantes, políticas e conceitos de domínio deterministicamente, sem IA. No caso geral isso não é alcançável, o que tornaria essas capacidades incapazes de satisfazer o critério de verificabilidade e portanto infinitas.

**Decisão:** essas capacidades produzem **candidatos com Evidence e Confidence**, restritos a construções sintaticamente identificáveis — guard clauses, exceções condicionais, atributos de validação, bibliotecas de validação declarativa, agregados nomeados, documentação estruturada existente. Nunca interpretação semântica livre.

**Consequências:** o critério de conclusão passa a ser cobertura sobre um catálogo declarado de construções, e não "compreender o domínio". O escopo torna-se fechado e testável.

---

### ADR-034 — Verificação de MT-02 por comparação pareada

**Status:** APROVADA

**Contexto:** MT-02, aplicada literalmente, é inválida quando a cobertura muda. Duas medições com conjuntos de perguntas sustentadas diferentes produzem medianas calculadas sobre populações diferentes: a mediana sobe porque o conjunto cresceu, não porque a Base piorou. Uma capacidade que passa a sustentar perguntas caras seria bloqueada por ter melhorado. Há um segundo modo de invalidação: quando a solução de referência muda entre as medições, `T_code` muda, e as frações deixam de ser comparáveis.

**Decisão:** MT-02 é verificada sobre o **conjunto pareado** — perguntas sustentadas em ambas as medições, excluídas nominalmente aquelas cujo `T_code` mudou. Regressão é aumento da mediana desse conjunto. Cobertura é métrica independente e nunca diminui. Aumento em pergunta individual exige causa registrada: causa externa à capacidade conclui, causa na capacidade bloqueia, ausência de causa bloqueia.

**Validade:** a comparação exige snapshot fixo da solução de referência. Conjunto pareado menor que metade das sustentadas não conclui capacidade; a linha de base é regravada antes.

**Consequências:** ganho de cobertura deixa de aparecer como falha. Degradação de resposta existente continua bloqueando. Exclusões são sempre relatadas — exclusão silenciosa permitiria esconder regressão mexendo na entrada. Medir o compilador contra o próprio código passa a ter custo explícito, reforçando a migração da solução de referência prevista antes do C08.

---

### ADR-035 — Projeções do C04 particionadas por projeto

**Status:** APROVADA

**Contexto:** o `COMPILATION_PLAN.md` exigia, no C04, seis arquivos monolíticos por classificação (`Classes.md`, `Interfaces.md`, …). O `KNOWLEDGE_MODEL.md` §9.1 exige partição por projeto e separação entre inventário e relação. A §9.1 nasceu de medição: publicar herança como coluna da tabela de tipos elevou o CR de "onde está o tipo X" de 5410‰ para 6731‰. Pela hierarquia da §0 o plano prevaleceria, desfazendo um ganho medido e contrariando PR-09 e AC-11.

**Decisão:** as projeções mínimas do C04 passam a ser `Structure/Types/{projeto}.md` mais `INDEX.md`, e `Relations/{projeto}.md` mais `INDEX.md`. Classificação vira seção dentro do arquivo do projeto, não arquivo próprio. O índice nunca lista nomes de tipo.

**Motivo da alteração de documento de maior autoridade:** o plano foi redigido antes de a medição existir. O conflito não se resolve escolhendo um lado da hierarquia, e sim corrigindo o documento que ficou desatualizado em relação ao fato medido.

**Consequências:** o conhecimento exigido pelo C04 permanece integralmente publicado; muda o eixo de particionamento. O critério de conclusão do C04 não muda. Precedente registrado para C05 e C06, cujas projeções serão redigidas sob a §9.1 por ADR própria.

---

## 9. Decisões rejeitadas

Não reabrir sem nova ADR.

| ID | Decisão | Status | Motivo |
|---|---|---|---|
| DR-001 | Projeto como indexador de código | REJEITADA | Indexação não representa conhecimento |
| DR-002 | Graph como modelo principal | REJEITADA | Graph é projeção, não modelo |
| DR-003 | Acoplamento a Claude Code ou qualquer LLM | REJEITADA | A Base deve ser reutilizável |
| DR-004 | IA produzindo conhecimento durante a compilação | REJEITADA | Quebra reprodutibilidade |
| DR-005 | Misturar compilação e interpretação | REJEITADA | Responsabilidades distintas |
| DR-006 | Conhecimento acoplado ao armazenamento | REJEITADA | Dependência tecnológica |
| DR-007 | Consulta dependente da persistência | REJEITADA | Conceitos independentes |
| DR-008 | Context Builder inferindo | REJEITADA | Inferência é do compilador |
| DR-009 | Conhecimento organizado só por estrutura técnica | REJEITADA | Organiza-se por perguntas |
| DR-010 | Compilador respondendo perguntas | REJEITADA | Separação de responsabilidades |
| DR-011 | Agente próprio na arquitetura | REVOGADA | Ferramentas existentes já cumprem esse papel |
| DR-012 | Infraestrutura antes do modelo estável | REJEITADA | O modelo é o núcleo |
| DR-013 | Parser produzindo conhecimento implícito | REJEITADA | Parser produz apenas estrutura |
| DR-014 | Ferramenta específica de uma solução | REJEITADA | Deve ser genérica |
| DR-015 | Conhecimento variando conforme o consumidor | REJEITADA | Existe uma única verdade sobre o projeto |
| **DR-016** | **Base acumulativa entre compilações** | **REJEITADA** | Faria a Base divergir do código (ver ADR-031) |
| **DR-017** | **Extração semântica livre de domínio sem IA** | **REJEITADA** | Não verificável (ver ADR-033) |

---

## 10. Regras de desenvolvimento

- **RD-01** Toda alteração arquitetural relevante resulta em ADR. Discussão isolada não altera arquitetura.
- **RD-02** ADR aprovada só é alterada por outra ADR que a substitua explicitamente.
- **RD-03** Todo componente responde por escrito: *"qual pergunta este componente ajuda a responder?"*. Sem resposta objetiva, não entra.
- **RD-04** Toda capacidade entrega, ao final, projeção publicada e utilizável. Não existe etapa cujo resultado seja apenas código interno.
- **RD-05** Nenhuma capacidade é iniciada antes da conclusão verificada de suas dependências.
- **RD-06** Toda abstração introduzida representa um conceito do domínio arquitetural. Abstrações por conveniência de implementação são rejeitadas.
- **RD-07** Compilar, publicar, consultar e consumir são responsabilidades separadas por construção, não por convenção.

---

## 11. Armadilhas conhecidas

Erros de processo já cometidos. Servem de checagem antes de cada decisão.

| ID | Armadilha | Regra |
|---|---|---|
| AC-01 | Reabrir decisão congelada porque o contexto cresceu | ADR só cai por ADR |
| AC-02 | Confundir hipótese explorada com decisão aprovada | Só o registrado é normativo |
| AC-03 | Tratar representação como se fosse o conhecimento | Graph, JSON e Markdown são projeções |
| AC-04 | Componente acumulando responsabilidades | Uma responsabilidade por componente |
| AC-05 | Infraestrutura antes do modelo | O modelo vem primeiro |
| AC-06 | Projetar a tecnologia em vez do problema | Tecnologia é detalhe |
| AC-07 | Otimizar antes de compreender | Compreender, organizar, então otimizar |
| AC-08 | Complexidade sem ganho mensurável | Medir com a Seção 7 |
| AC-09 | Assumir que está certo porque compila | Avaliar contra o objetivo, não contra o build |
| AC-10 | Esquecer o problema original | Reduzir o contexto de uma LLM |
| AC-11 | Produzir informação que não reduz contexto | Questionar sua existência |
| AC-12 | Confundir conhecimento estrutural com semântico | Estrutural é do compilador |
| AC-13 | Criar componente sem pergunta clara | Ver RD-03 |
| AC-14 | Reinventar o que ferramentas de IA já fazem | Complementar, não competir |
| **AC-15** | **Definir o modelo canônico em abstrato, antes de qualquer Producer real** | **O modelo estabiliza contra uso concreto. Ver `KNOWLEDGE_MODEL.md`, regra de congelamento** |

---

## 12. Cláusula final

Este documento é a fonte oficial de verdade do X7.Knowledge.

Havendo divergência entre este documento e qualquer conversa, código, comentário, documentação derivada ou interpretação posterior, prevalece este documento.

Nenhuma decisão congelada é reinterpretada informalmente.

A missão permanece:

> Compilar, de forma determinística e rastreável, o conhecimento existente em uma solução de software, reduzindo drasticamente a quantidade de código que uma LLM precisa ler para compreendê-la e evoluí-la.

---
*Fim de `PROJECT_CONSTITUTION.md` v2.1.*
