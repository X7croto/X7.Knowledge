# KNOWLEDGE_MODEL.md

**Projeto:** X7.Knowledge
**Versão do modelo:** v0 (provisório, deliberadamente mínimo)
**Status:** Normativo (autoridade 3)
**Derivado de:** `PROJECT_CONSTITUTION.md` v2.0, `COMPILATION_PLAN.md` v2.0

---

## 0. Por que v0 é provisório

A Constituição v1 exigia definir o modelo canônico completo antes de qualquer implementação. Isso contraria AC-07 e AC-15: um modelo desenhado sem nenhum Producer real é um modelo desenhado contra suposições, e PL-01 impediria corrigi-lo depois.

Portanto:

- **v0 cobre exclusivamente C01.** Nada além disso é modelado.
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
| `modelVersion` | string | Versão deste esquema. `0.1.0` |
| `compilerVersion` | string | Versão do compilador que produziu |
| `solutionId` | KnowledgeId | Identidade da solução |
| `acquisitionLevel` | `S` \| `X` | Nível alcançado (Constituição §5.3) |
| `capabilities` | string[] | Capacidades executadas. Ex.: `["C01"]` |
| `inputDigest` | string | Hash canônico das entradas consideradas |
| `observationCount` | int | Total de Observations |

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
| Observation | `obs:{sha256(kind + subjectId + payloadCanônico)[0..16]}` | `obs:9f2c41ab77e0d3b5` |

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

## 6. Catálogo de `kind` — v0 (C01)

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

Cada nova capacidade adiciona `kind`s ao catálogo. Nenhuma capacidade altera `kind` existente.

---

## 7. Entidades tipadas — v0

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
- Números com formatação invariante; sem notação científica.
- Campos nulos omitidos, nunca escritos como `null`.
- Sem timestamp, caminho absoluto ou dado de ambiente.

**Teste obrigatório:** duas compilações da mesma entrada produzem arquivos byte-idênticos.

O JSON é a forma de referência, não o produto (PR-07). Markdown, SQLite ou grafo são projeções equivalentes.

---

## 10. Estrutura publicada — v0

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

---

## 12. Critério de congelamento

Este documento passa de `PROVISÓRIO` a `CONGELADO` quando, simultaneamente:

1. C01, C02 e C03 estiverem concluídas conforme seus critérios;
2. os invariantes IV-01 a IV-08 passarem em execução automatizada;
3. nenhuma das três capacidades tiver exigido alteração incompatível do modelo nas últimas duas iterações;
4. o Context Ratio de linha de base estiver medido e registrado.

O congelamento é formalizado por ADR e, a partir dele, vale a regra de extensão da Seção 8.

---
*Fim de `KNOWLEDGE_MODEL.md` v0.*
