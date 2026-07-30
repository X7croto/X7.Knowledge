# MIGRATION_v1_to_v2.md

**Status:** Informativo — não normativo
**Objetivo:** registrar o que mudou na consolidação, para que nenhuma decisão de v1 se perca sem rastro.

---

## 1. Documentos

| Documento v1 | Situação | Destino |
|---|---|---|
| `PROJECT_CONSTITUTION.md` v1.0 | **REVOGADO** | Consolidado em `PROJECT_CONSTITUTION.md` v2.0 |
| `ObjetivoX7.docx` | **REVOGADO** | Ontologia e princípios absorvidos na Constituição v2.0 |
| `ROADMAP.md` | **REVOGADO** | Absorvido por `COMPILATION_PLAN.md` v2.0 (ADR-032) |
| `COMPILATION_PLAN.md` v1.0 | **REVOGADO** | Substituído por v2.0 |
| — | **NOVO** | `KNOWLEDGE_MODEL.md` v0 |

Recomendação: mover os quatro originais para `.md/legacy/` com cabeçalho `REVOGADO — ver v2.0`. Não apagar.

---

## 2. Renomeações

| v1 | v2 | Motivo |
|---|---|---|
| `X7.ProjectIndexer`, `ProjectIndexer` | **`X7.Knowledge`** | ADR-027. "Indexer" contradiz o próprio não objetivo N-01 |
| `Knowledge IR` | **`KnowledgeModel`** | ADR-028. Não é intermediário; é o produto final |
| `Fact` | **`Observation`** | ADR-029. Ontologia única |
| `Structural Knowledge` | Camada de Observations | Deixa de ser nome de etapa e passa a ser conteúdo do modelo |
| `Rule Inference` | `Inference` (com `Evidence` e `Confidence`) | ADR-029 |
| `Knowledge View` | `Projection` | Unifica com o vocabulário de Publishers |

---

## 3. Mapeamento de evolução

Os três planos concorrentes de v1 colapsam em uma cadeia única.

| Constituição v1 §11 | ROADMAP v1 | COMPILATION_PLAN v1 | **Plano v2** |
|---|---|---|---|
| R-001..R-004 | — | — | `KNOWLEDGE_MODEL.md` v0 |
| — | Macroetapa 1 | C01 | **C01** Estrutura Física |
| — | Macroetapa 2 | C02 | **C02** Arquitetural |
| — | Macroetapa 3 | C03 (parcial) | **C03** Estrutura do Código |
| — | Macroetapa 4 | C03 (parcial) | **C04** Modelo Estrutural |
| — | Macroetapa 5 | C04 | **C05** Comportamental |
| R-005 | Macroetapa 6 | C05 | **C06** Relações |
| — | — | C06 | **C07** Fluxos |
| — | Macroetapa 7 | C07 | **C08** Convenções |
| — | Macroetapa 8 | C08 | **C09** Padrões |
| — | Macroetapa 9 (parcial) | C09 | **C10** Linguagem de Domínio |
| — | Macroetapa 9 (parcial) | C10 | **C11** Regras de Negócio |
| — | Macroetapa 10 | C11 + C12 | **C12** Consolidação e Publicação |
| R-008, R-009 | Macroetapa 11 | C13 + C14 | **Fora do escopo** (ADR-030) |
| R-006 | Critério Determinismo | 4.4 | Constituição §6, regras D-01..D-08 |
| R-007 | — | — | Constituição §7, métrica CR |

---

## 4. Conflitos resolvidos

| # | Conflito em v1 | Resolução |
|---|---|---|
| 1 | Dois documentos declarando-se autoridade máxima | Hierarquia explícita de três documentos (Constituição §0) |
| 2 | Duas ontologias concorrentes | Cadeia única `Observation → Evidence → Inference` (ADR-029) |
| 3 | Dois nomes para o artefato central | `KnowledgeModel` (ADR-028) |
| 4 | Três nomes para o projeto | `X7.Knowledge` (ADR-027) |
| 5 | Três roadmaps sobrepostos sem mapeamento | Documento único de evolução (ADR-032) |
| 6 | Escopo termina na compilação, mas o plano incluía consulta e consumo | C13/C14 retiradas (ADR-030) |
| 7 | Monotonicidade tornaria a Base divergente do código | Monotonicidade intra-compilação (ADR-031) |
| 8 | Domínio e regras prometidos sem IA e sem escopo verificável | Escopo fechado por catálogo declarado (ADR-033) |
| 9 | "Solução suportada" indefinida | Níveis de aquisição S e X (Constituição §5.3) |
| 10 | Determinismo declarado, não especificado | Regras D-01..D-08 |
| 11 | "Reduzir contexto" sem métrica | Context Ratio e benchmark (Constituição §7) |
| 12 | Artefato central inexistente bloqueando tudo | `KNOWLEDGE_MODEL.md` v0, mínimo e provisório |

---

## 5. O que não mudou

Preservado integralmente:

- A missão e o problema que o projeto resolve.
- Todos os não objetivos.
- Determinismo e ausência de IA na compilação.
- Independência de qualquer LLM.
- Independência do mecanismo de armazenamento.
- Separação entre compilação, publicação e consumo.
- Proveniência e explicabilidade obrigatórias.
- Todas as decisões rejeitadas DR-001 a DR-015.
- Todas as armadilhas AC-001 a AC-014.

A consolidação não afrouxou nenhum princípio. Fechou escopo, nomeou o que estava ambíguo e tornou mensurável o que era retórico.

---

## 6. Primeiro passo recomendado

Uma fatia vertical completa de C01, contra uma solução real:

```
solução real → Producer de estrutura física
             → Observations com proveniência
             → KnowledgeModel v0
             → knowledge.model.json + Structure/Solution.md
             → teste de reprodutibilidade byte-a-byte
             → CR de linha de base medido
```

Pequena de propósito. É ela que valida se o modelo v0 aguenta o mundo real antes de qualquer congelamento.

---
*Fim de `MIGRATION_v1_to_v2.md`.*
