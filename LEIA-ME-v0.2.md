# Extensão do modelo — v0.1.0 → v0.2.0

Adiciona o mecanismo de derivação que C02 exige: `Evidence`, `Inference`,
`Confidence`, `Frequency`.

**Aditiva.** Nenhum `kind` de v0.1 mudou. As 81 Observations do C01 continuam
válidas e com os mesmos ids.

> Não compilado — sem SDK .NET disponível aqui.

## Arquivos novos

| Arquivo | Papel |
|---|---|
| `Model/Evidence.cs` | Agrupamento de Observations que sustenta uma conclusão |
| `Model/Inference.cs` | Conclusão derivada de Evidence por regra declarada |
| `Model/InferenceProvenance.cs` | Proveniência ancorada em regra, não em arquivo |
| `Model/Confidence.cs` | `Asserted` / `Observed` |
| `Model/Frequency.cs` | Conformes sobre total, taxa em milésimos inteiros |
| `Model/EvidenceKinds.cs` | Catálogo fechado, 4 kinds reservados para C02 |
| `Model/InferenceKinds.cs` | Catálogo fechado, 4 kinds reservados para C02 |
| `X7.KnowledgeTests/InferenceTests.cs` | 14 testes do mecanismo |

## Arquivos alterados

| Arquivo | Mudança |
|---|---|
| `Model/KnowledgeId.cs` | `ForEvidence`, `ForInference` |
| `Model/KnowledgeModel.cs` | Coleções `Evidence` e `Inferences` |
| `Model/KnowledgeModelBuilder.cs` | `AddEvidence`, `AddInference` com verificação de referência |
| `Model/Manifest.cs` | `evidenceCount`, `inferenceCount` |
| `Compilation/ModelInvariants.cs` | IV-09 a IV-12 |
| `Publishing/KnowledgeModelPublisher.cs` | Serialização das duas coleções |
| `Publishing/MarkdownPublisher.cs` | Contagens no README, só quando maiores que zero |
| `KnowledgeCompiler.cs` | `ModelVersion` = `0.2.0` |

## Decisões

**1. Evidence não tem `source` nem `locator`.** Sua origem física é
estrutural: aponta para Observations que já declaram proveniência completa.
Sintetizar um `source` seria fabricar dado.

**2. Inference tem proveniência própria, ancorada na regra.** Uma Observation
vem de um arquivo; uma Inference vem de uma regra. A §3.1 da Constituição
exige que a regra seja declarada — `rule` é essa declaração.

**3. `Asserted` com frequência é erro, não aviso.** Se há exceções, a regra
não é exata. A compilação falha em vez de publicar uma contradição.

**4. Frequência sai em milésimos inteiros.** Formatação de `double` na saída
canônica é risco de não determinismo.

**5. Inference não deriva de Inference** (IN-03). A cadeia não se encadeia
sobre si mesma em v0.2. Se C08 precisar disso, é ADR.

## Efeito na Base já publicada

Ao recompilar, `Knowledge/model/knowledge.model.json` muda:

- `manifest.modelVersion` passa a `0.2.0`
- `manifest.evidenceCount` e `inferenceCount` aparecem, valendo `0`
- `evidence` e `inferences` aparecem como arrays vazios

`README.md` e `Structure/Solution.md` **não mudam** — as contagens só entram
quando maiores que zero. Portanto o Context Ratio permanece em 780‰ e a
linha de base do benchmark continua comparável.

Vale recompilar e regravar a Base antes de começar C02.
