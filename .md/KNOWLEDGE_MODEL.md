# KNOWLEDGE_MODEL.md

**Projeto:** X7.Knowledge
**Versão do modelo:** v1.4
**Esquema:** `1.4.0`
**Status:** Normativo (autoridade 3) — **CONGELADO** (ADR-037)
**Derivado de:** `PROJECT_CONSTITUTION.md` v2.8, `COMPILATION_PLAN.md` v2.3

---

## 0. Como este documento chegou aqui

A Constituição v1 exigia definir o modelo canônico completo antes de qualquer
implementação. Isso contraria AC-07 e AC-15: um modelo desenhado sem nenhum
Producer real é um modelo desenhado contra suposições, e PL-01 impediria
corrigi-lo depois.

Por isso o modelo nasceu **provisório**, cobrindo uma capacidade de cada vez,
e cresceu contra código real. Sete versões, todas aditivas, nenhum `kind`
alterado ou removido.

**Está congelado desde a ADR-037.** As quatro condições do §12 foram
satisfeitas: C01 a C04 concluídas, IV-01 a IV-08 passando dentro da
compilação, nenhuma alteração incompatível, e linha de base medida.

Consequências, em vigor:

- A **regra de extensão da Seção 8 vale integralmente**, e não como
  recomendação.
- **Toda alteração deste documento exige ADR**, inclusive as aditivas. A
  dispensa valia apenas enquanto o status era `PROVISÓRIO`.
- Remover ou renomear `kind` ou campo exige incremento de versão maior.

O C05 é o primeiro teste real dessa regra: é a maior adição de `kind`s prevista
no plano, e entra sob o contrato já fechado.

A primeira fatia do C05 entrou pelas ADR-039 e ADR-040 — oito `kind`s novos,
nenhum existente alterado, esquema `1.1.0`. A regra de extensão foi exercida
contra a maior adição do plano e não precisou ser quebrada.

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
| `modelVersion` | string | Versão deste esquema. `1.4.0` |
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
| Membro | `member:{tipoQualificado}.{nome}({parâmetros})@{projeto}` | `member:X7.Knowledge.Model.KnowledgeModelBuilder.Add(X7.Knowledge.Model.Observation)@X7.Knowledge` |
| Observation | `obs:{sha256(kind + subjectId + payloadCanônico)[0..16]}` | `obs:9f2c41ab77e0d3b5` |
| Evidence | `ev:{sha256(kind + idsOrdenados)[0..16]}` | `ev:41d0a8c3be92f715` |
| Inference | `inf:{sha256(kind + subjectId + payload + evidenceId)[0..16]}` | `inf:7b3e05c1da84f296` |

Regras:

- Caminhos são relativos à raiz da solução, com separador `/` (D-02).
- Comparação e ordenação são ordinais e invariantes de cultura (D-01).
- Duas Observations idênticas produzem o mesmo id e são deduplicadas naturalmente.

### 3.1 Identidade de membro (ADR-039)

```
member:{tipoQualificado}.{nome}({tiposDosParâmetros})@{projeto}
```

| Caso | Forma |
|---|---|
| Método | `...KnowledgeModelBuilder.Add(X7.Knowledge.Model.Observation)@X7.Knowledge` |
| Método genérico | aridade de metadados no nome: ``Map`1(System.String)`` |
| Construtor | nome `.ctor`, sempre com parênteses |
| Propriedade, campo, evento | sem parênteses: `...KnowledgeModelBuilder.Observations@X7.Knowledge` |
| Indexador | tipos entre colchetes: `...ObservationPayload.this[System.String]` |
| Operador | nome de metadados: `...Money.op_Addition(...,...)` |
| Construtor estático | `..cctor()` |

Regras:

- Tipos de parâmetro entram totalmente qualificados, separados por vírgula sem
  espaço, na ordem de declaração.
- Tipos de parâmetro entram na **forma construída**: `List<System.String>` e
  `List<System.Int32>` são sobrecargas distintas e precisam de identidades
  distintas.
- Isso **não** vale para `typeId` dentro de payload, que aponta sempre a
  definição original (IV-13). A identidade do tipo é a declaração; a
  assinatura do membro é o uso.
- Parâmetro genérico do próprio membro ou do tipo aparece pelo nome (`T`), que
  é o que está escrito na declaração.

Ausência de parênteses distingue propriedade de método sem parâmetros:
`Value` e `Value()` são declarações diferentes e permanecem identidades
diferentes.

Identidades de tipo e de membro derivam de símbolo semântico, nunca de caminho
de arquivo.

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

**Fronteira do que é observado (ADR-041).** Um arquivo é observado quando está
sob a raiz da solução e fora de `bin/` e de `obj/`. O critério vale nos dois
níveis de aquisição: o nível declara a profundidade da resolução, nunca o
conjunto de arquivos.

Código emitido por gerador e arquivo injetado por pacote ficam fora, e **não**
produzem `acquisition.limitation`. A regra acima obriga a declarar o que o
compilador não conseguiu obter; este não é o caso. É conhecimento que não
pertence à solução — ninguém o escreveu e ninguém o evolui —, e M-02 define a
missão como reduzir o código que se precisa ler para *evoluir* a solução. O
precedente é o das bases implícitas do C04, na §6.1.3.a: exclusão declarada no
catálogo, sem Observation.

O critério é a localização, e não o nome: `*.g.cs` é convenção, `dentro de
obj/` é fato.

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

O C04 produz dois grupos de fatos com exigências de nível diferentes. O grupo
declara o seu nível; o `acquisitionLevel` por item registra o que de fato
ocorreu (Seção 5).

#### 6.1.3.a Relações entre tipos — exigem nível S

Em nível X o Producer declara `acquisition.limitation` com escopo
`type-relations` e não produz nada — deduzir herança por nome é o que
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

#### 6.1.3.b Estrutura do tipo — nível X é suficiente

Classificação, acessibilidade, modificadores, parâmetros genéricos e
aninhamento são fatos da declaração, não do modelo semântico. São observáveis
em ambos os níveis e não produzem `acquisition.limitation` por nível.

| `kind` | `subject` | `payload` |
|---|---|---|
| `type.kind` | Tipo | `{ kind }` |
| `type.accessibility` | Tipo | `{ value }` |
| `type.modifier` | Tipo | `{ name }` |
| `type.generic-parameter` | Tipo | `{ name, ordinal, variance? }` |
| `type.nested-in` | Tipo | `{ containerId }` |

**Vocabulário de `type.kind`.** Fechado, e espelha exatamente as formas
declaráveis em C#:

```
class  interface  record  record-struct  struct  enum  delegate
```

`record` é o `record class`. `record struct` tem valor próprio: é o que está
escrito na declaração, e classificá-lo como `struct` seria interpretar. Valor
fora do vocabulário é erro de compilação (IV-04).

**Vocabulário de `type.accessibility`.** Fechado:

```
public  internal  protected  private  protected-internal  private-protected
```

Acessibilidade é total e de valor único; modificador é conjunto de zero ou
mais. Mantê-los em `kind`s distintos preserva OB-03 — o payload de um `kind`
tem sempre a mesma forma. Uma Observation por modificador segue o padrão já
estabelecido por `project.target-framework`.

**Vocabulário de `type.modifier`.** Fechado, apenas o que o símbolo expõe:

```
abstract  sealed  static  readonly  ref  unsafe
```

`partial` **não** pertence a este vocabulário. Não existe no símbolo; é tratado
como Inference (6.3, `type.is-partial`).

**Por que `ordinal` está no payload.** A ordem dos parâmetros genéricos é
semântica: `<TKey, TValue>` não é `<TValue, TKey>`. D-01 ordena coleções de
saída por identidade canônica, o que embaralharia a ordem de declaração. O
ordinal preserva a informação dentro do próprio fato, e não na posição.

`variance` só aparece em parâmetro de interface ou delegate, com valores
`in` ou `out`; ausente significa invariante.

**Aninhamento — decisão registrada.** Tipo aninhado recebe `type.nested-in`
apontando o tipo contentor. A alternativa avaliada era declarar
`acquisition.limitation` de escopo `type-nesting` e não representar
aninhamento. Rejeitada:

- contenção é fato declarado e diretamente observável (`ContainingType`),
  exatamente a natureza do conhecimento do C04;
- sem ela, o único caminho até a contenção seria interpretar a string do nome
  qualificado — dedução por nome, que §5.3 proíbe em espírito;
- `namespace.contains` já estabeleceu o precedente para contenção lógica em
  C03; `type.nested-in` é o análogo direto;
- o custo é uma Observation por tipo aninhado, uma minoria do total.

Apenas a direção filho → contentor é observada. A direção inversa é derivável
do conjunto, e publicar as duas aqui duplicaria conhecimento; relação
bidirecional é critério do C06, não deste catálogo.

**Consequência sobre `namespace.contains`.** Tipo aninhado tem o mesmo
namespace do contentor. `namespace.contains` referencia apenas tipos de nível
superior; caso contrário a hierarquia passa a ter dois caminhos até o mesmo
tipo e deixa de ser árvore.

Cada nova capacidade adiciona `kind`s ao catálogo. Nenhuma capacidade altera
`kind` existente.

### 6.1.4 Observation — C05 (ADR-039)

Superfície declarada dos tipos. **Exige nível S**: em nível X o Producer
declara `acquisition.limitation` de escopo `type-members` e não produz nada.
Não há caminho sintático paralelo aqui — assinatura resolvida por sintaxe
seria dedução por nome, que a §5.3 da Constituição proíbe.

| `kind` | `subject` | `payload` |
|---|---|---|
| `type.declares-member` | Tipo | `{ memberId }` |
| `member.declared` | Membro | `{ name, kind }` |
| `member.accessibility` | Membro | `{ value }` |
| `member.modifier` | Membro | `{ name }` |
| `member.type` | Membro | `{ typeName, typeId?, external? }` |
| `member.parameter` | Membro | `{ name, ordinal, typeName, typeId?, external?, modifier?, optional?, defaultValue? }` |
| `member.generic-parameter` | Membro | `{ name, ordinal }` |
| `member.accessor` | Membro | `{ kind, accessibility? }` |
| `member.explicit-interface` | Membro | `{ interfaceName, interfaceId?, external? }` |
| `member.constant-value` | Membro | `{ value }` |
| `member.generic-constraint` | Membro | `{ parameter, ordinal, form, value, typeId?, external? }` |
| `type.generic-constraint` | Tipo | `{ parameter, ordinal, form, value, typeId?, external? }` |

`type.declares-member` segue o precedente de `namespace.contains`: contenção é
observada só na direção contentor → contido, e a inversa é derivável do
conjunto.

**`member.type`, e não `member.returns`.** É o tipo escrito na posição de tipo
da declaração: retorno para método, tipo para propriedade e, na fatia
seguinte, tipo do campo e do evento. Um `kind` cobre os quatro casos sem que
nenhum precise ser renomeado depois — e renomear, agora, custaria versão
maior. Construtor não recebe `member.type`: a declaração não escreve tipo
nenhum ali, e escrever `void` seria fabricar.

**Não existe `kind` de assinatura pronta.** Assinatura renderizada é
formatação de vários fatos, não um fato. Produzi-la no Producer seria
interpretar (OB-01). O precedente é `type.generic-parameter`: o C04 decompôs e
deixou a composição de `Cache<TKey, TValue>` para a projeção.

**Vocabulários fechados.** Valor fora do vocabulário é erro de compilação
(IV-04).

```
member.declared / kind        method  constructor  property  field
                              event  operator  indexer
member.accessibility / value  o mesmo vocabulário de type.accessibility
member.modifier / name        static  abstract  virtual  override  sealed
                              readonly  required  extern  const  volatile
member.accessor / kind        get  set  init  add  remove
member.parameter / modifier   ref  out  in  params  ref-readonly
constraint / form             keyword  type  type-parameter
```

**Restrição é `kind` próprio, e não campo** (ADR-043). `where T : class,
IFoo, new()` é conjunto, e conjunto em campo só caberia como texto delimitado
— o oposto do que OB-03 pede ao exigir forma fixa de payload.

O parâmetro é referenciado **por nome**. Parâmetro de tipo não é entidade: não
tem identidade na §3, e criar uma custaria formato novo e invariante de
referência para substituir um nome que já é único no escopo que o declara.

`form` existe para distinguir `T : U` de `T : class` sem depender da ausência
de `typeId` — ausência como discriminante é o que a §6.1.4 já rejeitou ao
observar todo membro em vez de só o público.

`ordinal` é a posição na cláusula **como escrita**. A linguagem exige ordem, e
guardá-la deixa a projeção reproduzir a cláusula sem embutir a gramática do C#
no Publisher.

**Restrições, modificadores de parâmetro e valores padrão vêm da sintaxe**,
pelo motivo já estabelecido para modificadores: o símbolo expõe a forma de
metadados. `default` e `null` são a mesma coisa nos metadados e coisas
diferentes na declaração, e `ref readonly` não se distingue de `in` por
`RefKind` sem conhecer o valor que a linguagem reserva para ele.

**Construtor estático não tem valor próprio** (ADR-042). A declaração escreve
`static X()`: é `constructor` com `member.modifier` de valor `static`. Criar
`static-constructor` duplicaria em vocabulário o que já está em modificador.

**Implementação explícita de interface também não.** Continua sendo `method`,
`property` ou `event`, e o que a distingue é `member.explicit-interface`. A
acessibilidade dela permanece `private`: é o que o símbolo responde e o que
está nos metadados, e C# proíbe modificador de acesso ali, de modo que não há
declaração para espelhar — publicar `public` seria fabricar. Quem decide que
ela é superfície é a projeção, pela presença do fato, e não pela
acessibilidade. É a mesma separação de planos de §9.1: o modelo observa, a
projeção filtra.

**O valor de um `const` é observado** (ADR-044, que revoga a exclusão da
ADR-042). Ele é embutido no chamador em tempo de compilação: trocá-lo quebra
quem já compilou, sem recompilação e sem aviso. É contrato, e mais rígido que
o valor padrão de parâmetro — que ao menos o chamador pode ignorar.

A condição é o modificador **escrito**, e não `IsConst`: membro de enum também
é constante para o símbolo, mas a declaração dele não escreve `const` e
`public MyEnum Valor` já é declaração válida. `static readonly` não entra: o
valor ali é inicialização, lida em tempo de execução, e a declaração já é
válida sem ele.

Quem encontrou o erro foi a conferência de assinatura: `public const string
Kind` não é C# válido, e uma assinatura que não é declaração válida não
descreve nada.

Ficam declaradamente de fora: `async`, detalhe de implementação e não
superfície; `new`, que descreve ocultação e não é exposto pelo símbolo;
`partial`, pelo motivo já registrado em `type.modifier`.

**Exclusões declaradas.** Não são observados os membros que o compilador gera
e ninguém escreve: construtor padrão implícito, `Equals`, `GetHashCode`,
`ToString`, `Deconstruct` e `<Clone>$` sintetizados de `record`, e os métodos
`get_X`/`set_X` da propriedade — estes representados por `member.accessor`, e
não como membros próprios. É o argumento das bases implícitas do C04: membro
implícito é derivável da regra da linguagem, e publicá-lo encheria a Base de
conteúdo que o leitor já sabe.

**Todos os membros declarados são observados.** O modelo registra membro de
qualquer acessibilidade; a projeção do C05 é que filtra a superfície pública
(§9.1). Observar só o público tornaria a ausência ambígua — *não existe* e
*existe e é privado* ficariam indistinguíveis — e o C06 depende de membro não
público. Ampliar depois o alcance de um `kind` existente é alterá-lo, que
EX-01 proíbe.

**A superfície declarada está completa.** A `acquisition.limitation` de escopo
`type-members-partial` deixou de ser produzida na fatia C. Limitação que
sobrevive ao que a justificava vira ruído e treina o leitor a ignorar as
demais.

### 6.2 Evidence

#### 6.2.1 C02

| `kind` | Agrupa |
|---|---|
| `project.graph-position` | O grafo inteiro: `project.declared` e `project.references-project` |
| `project.cycle-path` | Referências que fecham um ciclo |

#### 6.2.2 C04

| `kind` | Agrupa |
|---|---|
| `type.declaration-sites` | As `type.location` de um mesmo tipo |

Esta Evidence só é registrada quando agrupa **duas ou mais** Observations
(IV-17). Um único local de declaração não sustenta conclusão alguma sobre
parcialidade.

### 6.3 Inference

#### 6.3.1 C02

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

#### 6.3.2 C04

| `kind` | `subject` | `payload` | Confidence | Regra |
|---|---|---|---|---|
| `type.is-partial` | Tipo | `{}` | `Asserted` | `partial-by-multiple-declaration-sites` |

`Asserted` porque a implicação é exata: um tipo com mais de um local de
declaração é necessariamente `partial`. A regra não admite exceção.

**Limitação declarada — obrigatória.** A recíproca é falsa. Um tipo declarado
`partial` com um único local existe e é comum, e esta regra não o detecta.
Ausência de `type.is-partial` significa portanto *não detectado*, nunca *não
parcial*. Toda compilação registra `acquisition.limitation` com escopo
`type-partial-single-site`, porque ausência silenciosa é proibida (6.1).

**Nota de revisão.** Esta é a única Inference do modelo que existe por
indisponibilidade, e não por natureza derivada: `partial` é modificador
sintático e não aparece no símbolo. A observação direta é possível — ler os
modificadores da declaração — e produziria `type.modifier` com valor
`partial`, completo e sem limitação. Se o custo dessa leitura se mostrar
aceitável, ou se a limitação acima começar a distorcer alguma resposta do
benchmark, a substituição é: acrescentar `partial` ao vocabulário de
`type.modifier`, remover este `kind` e a limitação correspondente. A troca
custa incremento de versão maior (EX-03) e ADR, por remoção de `kind`.

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
| `Behavior/*` | **Um arquivo por tipo**, em pasta do projeto, mais `INDEX.md` |

Relação de tipo é publicada **separada do inventário**, e não como coluna a
mais na tabela de tipos. Motivo medido: com as duas juntas, uma pergunta do
tipo "onde está o tipo X" paga pela informação de herança sem usá-la — o CR
dessa pergunta subiu de 5410‰ para 6731‰ quando as colunas foram acrescentadas.

A regra geral: **o que não é consultado junto não é publicado junto.**

**Corolário — eixo de seção (ADR-036).** Dentro de um arquivo, secciona-se
pelo campo **mais caro de repetir**, não pelo que parece organizar melhor. O
eixo de seção é o único campo que deixa de aparecer linha a linha.

Medido: seccionar `Structure/Types/{projeto}.md` por classificação obriga o
namespace a virar coluna, escrita uma vez por tipo a 6–8 tokens. O `T_kb` de
"onde está o tipo X" subiu de 2034 para 2912 tokens. Seccionado por namespace,
com a classificação em coluna de um token, o custo volta ao patamar do C03.

Pelo mesmo critério, acessibilidade e modificadores não são publicados no
inventário: existem no KnowledgeModel e são publicados no C05, junto da
superfície pública, que é onde são consultados. IV-07 proíbe projeção com
informação **ausente** do modelo; não exige que todo fato seja projetado em
toda parte.

**Segunda regra — a unidade de partição é a unidade de consulta, e ela muda
por projeção, não por capacidade (ADR-040).** `Structure/Types/` é
particionado por projeto porque responde *onde está o tipo X*, e quem pergunta
ainda não sabe qual é o tipo: precisa varrer um inventário. `Behavior/` é
particionado por tipo porque responde *o que o tipo X expõe*, e quem pergunta
já sabe o tipo. Não é inconsistência de layout; são perguntas de granularidade
diferente, e a regra manda seguir a pergunta.

Estimado sobre a solução de referência, para a Q09 (`T_code` de 712 tokens):
`Behavior/` por projeto custaria de 4.000 a 6.000 tokens, por namespace cerca
de 1.200, e por tipo cerca de 120. Os dois primeiros publicam Base mais cara
que o código que ela substitui.

O índice lista projeto, contagem e link. Nunca nomes de tipo: repetir conteúdo
no índice anularia o ganho da partição.

Em `Behavior/` o nome do arquivo é derivado da identidade do tipo — nome
qualificado com `` ` `` de aridade trocado por `-` e aninhamento por `+`,
nenhum dos dois válido em identificador C#, logo injetivo por construção.
Quem sabe o nome do tipo chega ao arquivo sem ler índice. **Suposição de
medição, declarada:** isso desloca o custo de localização para fora do `T_kb`,
que conta arquivo lido. É legítimo — listar diretório não custa token — mas
fica registrado aqui e no `BENCHMARK.md`, porque suposição de medição não
declarada é o começo de fraude de métrica.

---

## 10. Estrutura publicada

Estado após a primeira fatia do C05. Cada capacidade acrescenta; nenhuma
remove.

```
Knowledge/
  README.md                     visão geral e manifesto legível
  Structure/
    Solution.md                 solução, projetos, frameworks, árvore
    Namespaces.md               hierarquia de namespaces
    Types/
      INDEX.md                  projeto, contagem, link
      {projeto}.md              inventário de tipos do projeto
  Architecture/
    Architecture.md
    ProjectDependencies.md
  Relations/
    INDEX.md
    {projeto}.md                herança e implementação
  Behavior/
    INDEX.md                    projeto, contagem, convenção de nome
    {projeto}/
      {nomeQualificado}.md      superfície pública de um tipo
  model/
    knowledge.model.json        forma canônica
```

A separação entre `Structure/Types/` e `Relations/` é exigida pela §9.1 e
registrada em ADR-035. A partição de `Behavior/` por tipo é a segunda regra da
§9.1, registrada em ADR-040.

O arquivo de tipo abre com a declaração do próprio tipo — classificação,
acessibilidade, modificadores, parâmetros genéricos, namespace, projeto e
arquivo — e é onde acessibilidade e modificadores finalmente aparecem
publicados, quitando o prazo declarado na ADR-036. Dentro de um arquivo de um
tipo só, a espécie do membro pode ser eixo de seção sem contrariar o corolário
da §9.1: o campo caro não aparece em linha nenhuma, porque é o próprio
arquivo.

A projeção publica `public`, `protected` e `protected internal`. Tipo sem
membro publicável não gera arquivo e conta zero no índice.

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
- **IV-13** Referência a tipo dentro de payload (`baseTypeId`, `interfaceId`,
  `containerId`, `typeId`) aponta para tipo existente no modelo.
- **IV-14** Quando o manifesto declara C04, todo tipo presente no modelo
  possui exatamente uma `type.kind` e exatamente uma `type.accessibility`.
  A condição é lida do manifesto, e não da presença de `type.kind`: só assim
  "capacidade não executada" se distingue de "capacidade executada e omissa",
  e a segunda tem de falhar.
- **IV-15** O grafo formado por `type.nested-in` é acíclico, e nenhum tipo
  possui mais de um contentor.
- **IV-16** Os `ordinal` de `type.generic-parameter` de um mesmo tipo formam
  a sequência `0..n-1`, sem repetição e sem lacuna.
- **IV-17** Evidence de kind `type.declaration-sites` referencia ao menos duas
  Observations.
- **IV-18** Todo membro possui exatamente um `member.declared`, exatamente uma
  `member.accessibility`, e é alvo de exatamente um `type.declares-member`
  cujo `subject` existe no modelo.
- **IV-19** Todo membro possui exatamente um `member.type`, exceto o de
  `kind` `constructor` — inclusive o estático —, que não possui nenhum: a
  declaração não escreve tipo ali.
- **IV-20** Os `ordinal` de `member.parameter` de um mesmo membro formam a
  sequência `0..n-1`, sem repetição e sem lacuna. O mesmo vale para
  `member.generic-parameter`.
- **IV-21** `member.accessor` ocorre apenas em membro de `kind` `property`,
  `indexer` ou `event`, no máximo um por valor de `kind`.
- **IV-22** `member.parameter` ocorre apenas em membro de `kind` `method`,
  `constructor`, `operator` ou `indexer`. Campo, evento e propriedade não
  admitem parâmetro, e uma Observation dessas ali significaria que o Producer
  confundiu a forma do membro.
- **IV-24** `member.constant-value` ocorre apenas em membro de `kind`
  `field`, no máximo um por membro.
- **IV-23** Toda restrição referencia, pelo `parameter`, um parâmetro genérico
  declarado no mesmo sujeito; e os `ordinal` das restrições de um mesmo par
  (sujeito, `parameter`) formam `0..n-1`, sem repetição e sem lacuna. É IV-20
  em outro agrupamento: lá a sequência é por membro, aqui é por parâmetro
  dentro dele.

IV-14 é o que torna testável o critério 1 do C04 — *todo tipo possui
representação própria e completa*. Sem ela, "completa" seria julgamento
subjetivo, e PL-05 não admite julgamento subjetivo como conclusão.

**IV-18 a IV-21 são invariantes de consistência, não de cobertura, e a
diferença importa.** A IV-14 pôde exigir que *todo tipo* tenha `type.kind`
porque todo tipo tem classificação. Não há equivalente para membro: tipo sem
membro é legítimo, e o modelo não sabe o que ficou de fora. O critério 1 do
C05 não fica verificável por invariante e passa a depender do critério 2 — a
conferência de assinatura contra o compilador de referência, que é teste.
Registrado porque fingir simetria com o C04 esconderia que a verificação mudou
de lugar (ADR-039).

**Compilação truncada.** O compilador aceita produzir a Base de uma capacidade
anterior sobre a entrada atual (`--until`). Isso não é uma Base degradada nem
um modo de operação alternativo: capacidades são aditivas, então o prefixo da
lista de Producers é exatamente a Base daquela capacidade. O manifesto declara
o prefixo, e os invariantes de capacidades não executadas não se aplicam.
Existe para satisfazer MT-02, que exige comparar duas capacidades sobre a
mesma entrada.

---

## 11.1 Histórico de esquema

Registra alterações do **esquema**. Alteração deste documento que não toque em
`kind`, campo, payload, vocabulário ou texto de invariante está na §11.2.


| Versão | Mudança | Compatibilidade |
|---|---|---|
| `0.1.0` | Substrato de Observations. Catálogo C01 | — |
| `0.2.0` | `Evidence`, `Inference`, `Confidence`, `Frequency`; catálogos 6.2 e 6.3; IV-09..IV-12 | **Aditiva** (EX-01, EX-04). Nenhum `kind` de v0.1 alterado; Observations de v0.1 permanecem válidas |
| `0.3.0` | Kinds de Observation de C02 (6.1.1); `project.graph-position` passa a agrupar nós e arestas | **Aditiva**. Nenhum `kind` anterior alterado |
| `0.4.0` | Kinds de C03 (6.1.2); identidades `ns:` e `type:`; regra de granularidade (9.1) | **Aditiva**. Nenhum `kind` anterior alterado |
| `0.5.0` | `msBuildVersion` no manifesto, presente apenas em nível S | **Aditiva**. Campo opcional (EX-02) |
| `0.6.0` | Kinds de C04 (6.1.3); IV-13 | **Aditiva**. Nenhum `kind` anterior alterado |
| `0.7.0` | Estrutura do tipo em C04 (6.1.3.b): `type.kind`, `type.accessibility`, `type.modifier`, `type.generic-parameter`, `type.nested-in`; Evidence `type.declaration-sites`; Inference `type.is-partial`; IV-14..IV-17; §10 atualizada | **Aditiva** (EX-01, EX-04). Nenhum `kind` anterior alterado; `IV-13` teve escopo ampliado para `containerId`, sem alterar payload existente |
| `1.0.0` | Congelamento (ADR-037). Nenhum `kind`, campo ou invariante alterado | **Idêntica a `0.7.0` em conteúdo.** A mudança de versão maior declara o fim do estado provisório, não uma quebra de compatibilidade |
| `1.4.0` | `member.constant-value` e IV-24 (ADR-044, que revoga a exclusão da ADR-042 §2) | **Aditiva** (EX-01, EX-04). Encontrada pela conferência de assinatura do critério 2, não por invariante: `public const string Kind` não é declaração C# válida |
| `1.3.0` | Terceira fatia do C05 (ADR-043): `kind`s `member.generic-constraint` e `type.generic-constraint`; campo `defaultValue` em `member.parameter`; vocabulário `form`; IV-23 | **Aditiva** (EX-01, EX-02, EX-04). `defaultValue` é campo novo em entidade existente, e portanto opcional. `type.generic-constraint` é a primeira adição do C05 ao território do C04: adição pura, nenhum `kind` do C04 alterado |
| `1.2.0` | Segunda fatia do C05 (ADR-042): `kind` `member.explicit-interface`; quatro valores em `member.declared`, dois em `member.modifier`, dois em `member.accessor`; IV-19 e IV-21 ampliadas; IV-22 | **Aditiva** (EX-01, EX-04). Nenhum `kind`, campo ou payload existente alterado. `member.type`, `member.parameter` e `member.accessor` receberam as formas novas sem alteração — foi para isso que `member.type` nasceu com esse nome |
| `1.1.0` | Primeira fatia do C05 (ADR-039, ADR-040): identidade `member:` (§3.1); oito `kind`s em 6.1.4; IV-18..IV-21; IV-13 ampliado para `typeId`; segunda regra da §9.1; §10 atualizada | **Aditiva** (EX-01, EX-04). Nenhum `kind` anterior alterado. Primeira alteração sob o modelo congelado, e por ADR, como a ADR-037 exige |

---

## 11.2 Alterações sem efeito de esquema

Toda alteração deste documento exige ADR desde o congelamento (ADR-037),
inclusive as que não mexem no esquema. Estas ficam registradas aqui, e não na
tabela acima, porque incrementar a versão do esquema sem que o esquema tenha
mudado tornaria a própria versão um dado sem significado.

| ADR | Alteração | Efeito no esquema |
|---|---|---|
| ADR-041 | Fronteira do que é observado, declarada na §6.1 | Nenhum. Nenhum `kind`, campo, payload, vocabulário ou texto de invariante foi tocado. A implementação da IV-08 foi corrigida; o texto da IV-08 é o mesmo desde `0.1.0` |

---

## 12. Congelamento — registro

Este documento passou de `PROVISÓRIO` a `CONGELADO` pela ADR-037, com as
quatro condições verificadas:

| Condição | Estado |
|---|---|
| C01, C02 e C03 concluídas conforme seus critérios | Satisfeita — C04 também |
| IV-01 a IV-08 passando em execução automatizada | Satisfeita — rodam dentro da compilação, não só em teste |
| Nenhuma alteração incompatível nas últimas duas iterações | Satisfeita — sete versões, todas aditivas |
| Context Ratio de linha de base medido e registrado | Satisfeita — `benchmark/results-c04` |

A partir daqui vale a regra de extensão da Seção 8, e toda alteração exige
ADR.

---
*Fim de `KNOWLEDGE_MODEL.md` v1.4 — CONGELADO.*
*Alterado por ADR-039, ADR-040, ADR-042, ADR-043 e ADR-044. Fronteira de observação por ADR-041 (§11.2).*
