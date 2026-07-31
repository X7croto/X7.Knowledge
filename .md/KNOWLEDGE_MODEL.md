# KNOWLEDGE_MODEL.md

**Projeto:** X7.Knowledge
**Versão do modelo:** v0.6 (provisório)
**Esquema:** `0.6.0`
**Status:** Normativo (autoridade 3)
**Derivado de:** `PROJECT_CONSTITUTION.md` v2.0, `COMPILATION_PLAN.md` v2.0

---

## 0. Por que v0 é provisório

A Constituição v1 exigia definir o modelo canônico completo antes de qualquer implementação. Isso contraria AC-07 e AC-15: um modelo desenhado sem nenhum Producer real é um modelo desenhado contra suposições, e PL-01 impediria corrigi-lo depois.

Portanto:

- **v0.1 cobria exclusivamente C01.** v0.2 adiciona o mecanismo de derivação
  (`Evidence`, `Inference`, `Confidence`) exigido por C02.
- **v0 é explicitamente provisório.** Alterações não exigem ADR enquanto o status for `PROVISÓRIO`.
- **v0 congela após C03 concluída**, quando três Producers reais já exerceram o modelo. A partir do congelamento, toda alteração exige ADR e obedece à regra de extensão da Seção 7.

O congelamento é registrado por ADR e altera o status deste documento para `CONGELADO`.

---

## 1. Princípio estruturante

O KnowledgeModel tem **um substrato e uma indexação**.

- O **substrato** é o conjunto de `Observation`. Toda afirmação sobre a solução é uma Observation com proveniência.
- A **indexação** são as entidades tipadas (`Solution`, `Project`, …). Elas são **projeções tipadas sobre as Observations**, oferecidas por conveniência.

Consequência normativa: **nenhuma entidade tipada contém informação ausente das Observations.** Se um valor existe em `Project.TargetFrameworks`, existe uma Observation que o originou. Isso é verificável por teste e mantém PR-04 estruturalmente, não por disciplina.

---

## 2. Manifesto

Todo KnowledgeModel começa por um manifesto. Ele existe para tornar a compilação auditável.

| Campo | Tipo | Descrição |
|---|---|---|
| `modelVersion` | string | Versão deste esquema. `0.6.0` |
| `compilerVersion` | string | Versão do compilador que produziu |
| `solutionId` | KnowledgeId | Identidade da solução |
| `acquisitionLevel` | `S` \| `X` | Nível alcançado (Constituição §5.3) |
| `capabilities` | string[] | Capacidades executadas. Ex.: `["C01"]` |
| `inputDigest` | string | Hash canônico das entradas consideradas |
| `observationCount` | int | Total de Observations |
| `evidenceCount` | int | Total de Evidence |
| `inferenceCount` | int | Total de Inferences |

O manifesto **não contém** timestamp, máquina, usuário ou caminho absoluto (D-03).

---

## 3. Identidade

Toda entidade e toda Observation possuem `KnowledgeId`: string estável, legível e derivada de posição lógica ou de conteúdo. Nunca de GUID, contador ou endereço.

| Elemento | Formato | Exemplo |
|---|---|---|
| Solução | `sln:{nome}` | `sln:Segundio` |
| Projeto | `proj:{caminhoRelativo}` | `proj:src/Segundio.Domain/Segundio.Domain.csproj` |
| Pasta de solução | `slnfolder:{caminhoLógico}` | `slnfolder:src/Core` |
| Diretório físico | `dir:{caminhoRelativo}` | `dir:src/Segundio.Domain` |
| Namespace | `ns:{nomeCompleto}` | `ns:X7.Knowledge.Model` |
| Tipo | `type:{nomeQualificado}@{nomeDoProjeto}` | `type:X7.Knowledge.Model.Observation@X7.Knowledge` |
| Observation | `obs:{sha256(kind + subjectId + payloadCanônico)[0..16]}` | `obs:9f2c41ab77e0d3b5` |
| Evidence | `ev:{sha256(kind + idsOrdenados)[0..16]}` | `ev:41d0a8c3be92f715` |
| Inference | `inf:{sha256(kind + subjectId + payload + evidenceId)[0..16]}` | `inf:7b3e05c1da84f296` |

Regras:

- Caminhos são relativos à raiz da solução, com separador `/` (D-02).
- Comparação e ordenação são ordinais e invariantes de cultura (D-01).
- Duas Observations idênticas produzem o mesmo id e são deduplicadas naturalmente.

Identidades de tipos e membros serão definidas em C03/C04, com base em símbolo semântico e não em caminho de arquivo.

---

## 4. Observation

Unidade atômica de conhecimento.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | KnowledgeId | sim | Derivado de conteúdo |
| `kind` | string | sim | Tipo da observação, do catálogo da Seção 6 |
| `subject` | KnowledgeId | sim | A quem a observação se refere |
| `payload` | objeto | sim | Dados da observação, formato definido pelo `kind` |
| `provenance` | Provenance | sim | Ver Seção 5 |

Regras:

- **OB-01** Uma Observation nunca interpreta. Se há julgamento, é Inference.
- **OB-02** Um `kind` desconhecido do catálogo é erro de compilação, não item ignorado.
- **OB-03** O `payload` de um mesmo `kind` tem sempre a mesma forma.
- **OB-04** Observations são ordenadas por `subject`, depois `kind`, depois `id`.
  Evidence é ordenada por `id`. Inferences seguem a mesma regra das Observations.

---

## 4.1 Evidence

Agrupamento nomeado e consistente de Observations que sustenta uma conclusão.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | KnowledgeId | sim | Derivado de kind + ids ordenados |
| `kind` | string | sim | Do catálogo da Seção 6.2 |
| `observations` | KnowledgeId[] | sim | Ordenado, sem repetição, **nunca vazio** |
| `producer` | string | sim | Quem montou o agrupamento |
| `capability` | string | sim | Em que capacidade |

Regras:

- **EV-01** Evidence sem Observations é inválida. Conclusão sem sustentação
  não existe.
- **EV-02** As Observations referenciadas já devem existir no modelo no
  momento do registro.
- **EV-03** Evidence com o mesmo conjunto de Observations tem o mesmo `id`,
  independentemente da ordem de entrada, e deduplica.

**Por que Evidence não tem `source` nem `locator`.** Sua origem física é
estrutural: ela aponta para Observations que já declaram, cada uma, sua
proveniência completa. Sintetizar um `source` para a Evidence seria fabricar
dado — o oposto do que PR-04 pede. A rastreabilidade até o arquivo permanece,
por um salto a mais.

---

## 4.2 Inference

Conhecimento derivado exclusivamente de Evidence, por regra determinística e
declarada.

| Campo | Tipo | Obrigatório | Descrição |
|---|---|---|---|
| `id` | KnowledgeId | sim | Derivado de conteúdo |
| `kind` | string | sim | Do catálogo da Seção 6.3 |
| `subject` | KnowledgeId | sim | A quem a conclusão se refere |
| `payload` | objeto | sim | Formato definido pelo `kind` |
| `evidence` | KnowledgeId | sim | A Evidence que sustenta |
| `confidence` | `Asserted` \| `Observed` | sim | Ver 4.3 |
| `frequency` | objeto | condicional | Obrigatório se `Observed`, proibido se `Asserted` |
| `provenance` | InferenceProvenance | sim | Ver 4.4 |

Regras:

- **IN-01** Toda Inference aponta para uma Evidence existente.
- **IN-02** Toda Inference declara `confidence`.
- **IN-03** Uma Inference nunca deriva de outra Inference. A cadeia é
  `Observation → Evidence → Inference`, e não se encadeia sobre si mesma em v0.2.
- **IN-04** Inference não substitui Observation. Se o fato é observável
  diretamente, é Observation (OB-01).

---

## 4.3 Confidence e Frequency

| Valor | Significado | Frequência |
|---|---|---|
| `Asserted` | Regra exata, sem exceções | **Proibida** |
| `Observed` | Regularidade estatística | **Obrigatória** |

`frequency` tem `matching`, `total` e `ratePerMille`.

- `total` maior que zero; `matching` no intervalo `[0, total]`.
- `ratePerMille` é inteiro, derivado. **Nunca ponto flutuante na saída
  canônica:** formatação de `double` é porta de entrada clássica para não
  determinismo (D-06).

`Asserted` com frequência é contradição: se há exceções, a regra não é exata,
e a Confidence correta é `Observed`. A compilação falha nesse caso.

---

## 4.4 InferenceProvenance

| Campo | Tipo | Descrição |
|---|---|---|
| `rule` | string | Identificador estável da regra aplicada. Ex.: `layer-by-graph-depth` |
| `producer` | string | Producer responsável |
| `capability` | string | Capacidade que a produziu |
| `acquisitionLevel` | `S` \| `X` | Nível deste item |

Difere de `Provenance` por um motivo estrutural: **uma Observation vem de um
arquivo, uma Inference vem de uma regra.** A Seção 3.1 da Constituição exige
que a regra seja "determinística e declarada" — `rule` é essa declaração.
Não há `source` porque uma conclusão não tem origem física; a origem física
continua alcançável através da Evidence.

---

## 5. Provenance

Obrigatória em toda Observation e toda Inference. Sem ela, a compilação falha (PR-04).

| Campo | Tipo | Descrição |
|---|---|---|
| `source` | string | Origem física relativa. Ex.: `Segundio.sln`, `src/Domain/Domain.csproj` |
| `locator` | string? | Localização dentro da origem, quando aplicável. Ex.: linha, seção |
| `producer` | string | Nome do Producer responsável |
| `capability` | string | Capacidade que a produziu. Ex.: `C01` |
| `acquisitionLevel` | `S` \| `X` | Nível em que este item específico foi obtido |

`acquisitionLevel` é por item, não apenas global: uma compilação pode ter partes resolvidas semanticamente e partes não.

---

## 6. Catálogos

### 6.1 Observation — C01

Este é o conteúdo completo do conhecimento em v0.

| `kind` | `subject` | `payload` |
|---|---|---|
| `solution.declared` | Solução | `{ name }` |
| `solution.contains-project` | Solução | `{ projectId }` |
| `solution.folder` | Pasta de solução | `{ name, parentId? }` |
| `solution.folder-contains` | Pasta de solução | `{ childId }` |
| `project.declared` | Projeto | `{ name, relativePath, directory }` |
| `project.target-framework` | Projeto | `{ moniker }` |
| `project.output-kind` | Projeto | `{ kind }` |
| `project.language-version` | Projeto | `{ version }` |
| `project.property` | Projeto | `{ name, value }` |
| `project.is-test-project` | Projeto | `{ evidence }` |
| `acquisition.limitation` | Qualquer | `{ reason, affectedScope }` |

`acquisition.limitation` é obrigatória sempre que o compilador não conseguiu obter algo que normalmente obteria. **Ausência silenciosa é proibida**: o que não foi obtido é declarado.

### 6.1.1 Observation — C02

| `kind` | `subject` | `payload` |
|---|---|---|
| `project.references-project` | Projeto | `{ targetId }` |
| `project.package-reference` | Projeto | `{ name, version? }` |

`targetId` sempre referencia projeto declarado na solução. Referência para
fora da solução produz `acquisition.limitation`, nunca aresta inventada.

`version` ausente significa não resolvida, com limitação correspondente.

### 6.1.2 Observation — C03

| `kind` | `subject` | `payload` |
|---|---|---|
| `namespace.declared` | Namespace | `{ name, parentId? }` |
| `namespace.contains` | Namespace | `{ typeId }` |
| `type.declared` | Tipo | `{ name, metadataName, namespace?, projectId }` |
| `type.location` | Tipo | `{ file }` |

Namespace intermediário é declarado mesmo sem tipo direto: `A.B.C` implica
`A` e `A.B`, para que a hierarquia seja completa.

### 6.1.3 Observation — C04

Exigem nível S. Em nível X o Producer declara `acquisition.limitation` com
escopo `type-relations` e não produz nada — deduzir herança por nome é o que
§5.3 proíbe.

| `kind` | `subject` | `payload` |
|---|---|---|
| `type.inherits` | Tipo | `{ baseTypeName, baseTypeId? , external? }` |
| `type.implements` | Tipo | `{ interfaceName, interfaceId?, external? }` |

`baseTypeId` e `interfaceId` só existem quando o alvo pertence à solução, e
então referenciam identidade existente (IV-13). Alvo de fora traz
`external: "true"` e apenas o nome — descartar a relação perderia conhecimento
legítimo, e forjar uma identidade inexistente seria pior.

**Exclusões declaradas.** Bases implícitas pelo próprio tipo do símbolo não são
observadas: `System.Object`, `System.ValueType`, `System.Enum`,
`System.Delegate`, `System.MulticastDelegate`. Toda classe deriva de `Object`;
observar isso produziria uma Observation por tipo da solução sem informar nada.

**Apenas o que é declarado diretamente.** Interface herdada da classe base não
é observada: é derivável do conjunto, e computá-la aqui seria inferência
disfarçada de observação (OB-01).

Cada nova capacidade adiciona `kind`s ao catálogo. Nenhuma capacidade altera
`kind` existente.

### 6.2 Evidence — reservados para C02

Declarados no catálogo; ainda não produzidos por nenhum Producer.

| `kind` | Agrupa |
|---|---|
| `project.graph-position` | O grafo inteiro: `project.declared` e `project.references-project` |
| `project.cycle-path` | Referências que fecham um ciclo |

### 6.3 Inference — reservados para C02

| `kind` | `subject` | `payload` | Confidence | Regra |
|---|---|---|---|---|
| `project.layer` | Projeto | `{ depth }` | `Asserted` | `layer-by-graph-depth` |
| `project.is-root` | Projeto | `{}` | `Asserted` | `root-by-absence-of-dependents` |
| `project.is-leaf` | Projeto | `{}` | `Asserted` | `leaf-by-absence-of-references` |
| `project.participates-in-cycle` | Projeto | `{ cycleId }` | `Asserted` | `cycle-by-strongly-connected-component` |

Todas `Asserted`: posição no grafo é exata dada a estrutura, não regularidade
estatística. `Observed` aparecerá em C08, onde convenção é frequência.

`depth` é a maior distância até um projeto que não referencia nenhum outro da
solução, calculada sobre a condensação em componentes fortemente conexos —
assim a presença de ciclo não torna a profundidade indefinida.

Kind fora de qualquer um dos três catálogos é erro de compilação (IV-04).

---

## 7. Entidades tipadas

Índice sobre as Observations. Nenhum campo aqui existe sem Observation correspondente.

### Solution
```
id                KnowledgeId
name              string
projects          KnowledgeId[]     ordenado
folders           KnowledgeId[]     ordenado
```

### Project
```
id                KnowledgeId
name              string
relativePath      string
directory         string
targetFrameworks  string[]          ordenado
outputKind        string?
languageVersion   string?
isTestProject     bool?
```

### SolutionFolder
```
id                KnowledgeId
name              string
parent            KnowledgeId?
children          KnowledgeId[]     ordenado
```

Campos opcionais ausentes significam **não observado**, e existe uma `acquisition.limitation` correspondente quando a ausência não é natural.

---

## 8. Regra de extensão

Vigora integralmente após o congelamento; recomendada desde já.

- **EX-01** Capacidades novas adicionam `kind`s e entidades. Nunca alteram os existentes.
- **EX-02** Campo novo em entidade existente é sempre opcional.
- **EX-03** Remover ou renomear `kind` ou campo exige incremento de versão maior e ADR.
- **EX-04** Adição compatível incrementa versão menor.
- **EX-05** Todo consumidor ignora `kind` desconhecido sem erro. O compilador, não.

---

## 9. Serialização canônica

O KnowledgeModel é persistido em `Knowledge/model/knowledge.model.json`.

- UTF-8, sem BOM.
- Terminador de linha `LF`.
- Chaves de objeto ordenadas ordinalmente.
- Arrays ordenados pela regra da entidade correspondente.
- `observations`, `evidence`, `inferences` e `entities` são coleções de
  primeiro nível do documento.
- Números com formatação invariante; sem notação científica.
- Campos nulos omitidos, nunca escritos como `null`.
- Sem timestamp, caminho absoluto ou dado de ambiente.

**Teste obrigatório:** duas compilações da mesma entrada produzem arquivos byte-idênticos.

O JSON é a forma de referência, não o produto (PR-07). Markdown, SQLite ou grafo são projeções equivalentes.

---

## 9.1 Granularidade de projeção

**Regra.** Uma projeção cujo conteúdo cresce com o tamanho da solução é
publicada **particionada pela unidade natural de consulta**, com um índice
que aponta onde procurar e não repete conteúdo.

**Motivo.** `T_kb` é contado por arquivo inteiro (BENCHMARK §3). Projeção
monolítica faz qualquer pergunta sobre um subconjunto pagar a solução inteira,
e o CR cresce com o tamanho do projeto sem que a Base tenha piorado.

| Projeção | Partição |
|---|---|
| `Structure/Solution.md` | Nenhuma — tamanho proporcional a projetos, não a código |
| `Architecture/*` | Nenhuma — idem |
| `Structure/Types/*` | **Um arquivo por projeto**, mais `INDEX.md` |
| `Relations/*` | **Um arquivo por projeto**, mais `INDEX.md` |

Relação de tipo é publicada **separada do inventário**, e não como coluna a
mais na tabela de tipos. Motivo medido: com as duas juntas, uma pergunta do
tipo "onde está o tipo X" paga pela informação de herança sem usá-la — o CR
dessa pergunta subiu de 5410‰ para 6731‰ quando as colunas foram acrescentadas.

A regra geral: **o que não é consultado junto não é publicado junto.**

O índice lista projeto, contagem e link. Nunca nomes de tipo: repetir conteúdo
no índice anularia o ganho da partição.

---

## 10. Estrutura publicada

```
Knowledge/
  README.md                     visão geral e manifesto legível
  Structure/
    Solution.md                 solução, projetos, frameworks, árvore
  model/
    knowledge.model.json        forma canônica
```

---

## 11. Invariantes verificáveis

Testáveis por automação; falha bloqueia a conclusão de qualquer capacidade.

- **IV-01** Toda Observation possui Provenance completa.
- **IV-02** Todo campo de entidade tipada é rastreável a pelo menos uma Observation.
- **IV-03** Todo `subject` referencia uma identidade existente no modelo.
- **IV-04** Todo `kind` pertence ao catálogo.
- **IV-05** Não existem duas Observations com o mesmo `id` e payload diferente.
- **IV-06** Compilações repetidas produzem saída byte-idêntica.
- **IV-07** Nenhuma projeção contém informação ausente do KnowledgeModel.
- **IV-08** Nenhuma saída contém caminho absoluto, timestamp ou dado de ambiente.
- **IV-09** Toda Inference referencia uma Evidence existente no modelo.
- **IV-10** Toda Evidence é não vazia e referencia apenas Observations existentes.
- **IV-11** `Observed` tem frequência declarada; `Asserted` não tem frequência.
- **IV-12** Toda Inference declara sua regra.
- **IV-13** Referência a tipo dentro de payload (`baseTypeId`, `interfaceId`)
  aponta para tipo existente no modelo.

---

## 11.1 Histórico de esquema

| Versão | Mudança | Compatibilidade |
|---|---|---|
| `0.1.0` | Substrato de Observations. Catálogo C01 | — |
| `0.2.0` | `Evidence`, `Inference`, `Confidence`, `Frequency`; catálogos 6.2 e 6.3; IV-09..IV-12 | **Aditiva** (EX-01, EX-04). Nenhum `kind` de v0.1 alterado; Observations de v0.1 permanecem válidas |
| `0.3.0` | Kinds de Observation de C02 (6.1.1); `project.graph-position` passa a agrupar nós e arestas | **Aditiva**. Nenhum `kind` anterior alterado |
| `0.4.0` | Kinds de C03 (6.1.2); identidades `ns:` e `type:`; regra de granularidade (9.1) | **Aditiva**. Nenhum `kind` anterior alterado |
| `0.5.0` | `msBuildVersion` no manifesto, presente apenas em nível S | **Aditiva**. Campo opcional (EX-02) |
| `0.6.0` | Kinds de C04 (6.1.3); IV-13 | **Aditiva**. Nenhum `kind` anterior alterado |

---

## 12. Critério de congelamento

Este documento passa de `PROVISÓRIO` a `CONGELADO` quando, simultaneamente:

1. C01, C02 e C03 estiverem concluídas conforme seus critérios;
2. os invariantes IV-01 a IV-08 passarem em execução automatizada;
3. nenhuma das três capacidades tiver exigido alteração incompatível do modelo nas últimas duas iterações;
4. o Context Ratio de linha de base estiver medido e registrado.

O congelamento é formalizado por ADR e, a partir dele, vale a regra de extensão da Seção 8.

---
*Fim de `KNOWLEDGE_MODEL.md` v0.6.*
