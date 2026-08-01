# C04-b — estrutura do tipo

Fecha a lacuna entre o que o C04 promete no plano e o que o catálogo tinha:
classificação, acessibilidade, modificadores, parâmetros genéricos e
aninhamento. Esquema passa a `0.7.0`.

## Antes de aplicar

1. **Feche o Visual Studio.** A IDE reescreve `.csproj` e arquivos abertos.
2. Copie o zip sobre a raiz da solução.
3. `.\VERIFICAR-ESTADO.ps1`
4. `dotnet build`

## Arquivos

**Novos**

| Arquivo | Papel |
|---|---|
| `X7.Knowledge/Model/TypeVocabulary.cs` | Vocabulários fechados de classificação, acessibilidade e modificador |
| `X7.Knowledge/Compilation/Producers/TypeIdentity.cs` | Cálculo da identidade de tipo, em um lugar só |
| `X7.Knowledge/Compilation/Producers/TypeStructureProducer.cs` | Os cinco kinds novos, nos dois níveis |
| `X7.Knowledge/Compilation/Producers/PartialTypeProducer.cs` | Inference `type.is-partial` e sua limitação |
| `X7.KnowledgeTests/TypeStructureTests.cs` | Onze testes |

**Alterados**

| Arquivo | Mudança |
|---|---|
| `Model/ObservationKinds.cs` | 5 kinds |
| `Model/EvidenceKinds.cs` | `type.declaration-sites` |
| `Model/InferenceKinds.cs` | `type.is-partial` |
| `Compilation/Producers/CodeStructureProducer.cs` | Todas as localizações; `namespace.contains` só para tipo de nível superior |
| `Compilation/Producers/TypeRelationProducer.cs` | Passa a usar `TypeIdentity` |
| `Compilation/ModelInvariants.cs` | IV-14 a IV-17; IV-13 cobre `containerId` |
| `Publishing/StructurePublisher.cs` | Seções por classificação; índices montados uma vez |
| `KnowledgeCompiler.cs` | Esquema `0.7.0`; dois Producers novos |
| `X7.KnowledgeTests/SolutionFixture.cs` | Tipo parcial, variância e delegate na solução de referência |

## O que muda na saída

- **Limitações: 13 → 14.** A nova é `type-partial-single-site`, declarada em
  toda compilação. Ela informa o alcance da regra de parcialidade, não o
  resultado desta compilação.
- **Observations sobem bastante.** São cinco kinds novos, e a maior parte é
  uma Observation por tipo.
- **`namespace.contains` cai.** Tipo aninhado deixa de contar como conteúdo
  direto do namespace. `Structure/Namespaces.md` mostra números menores, e
  eles é que estão certos.
- **`Structure/Types/{projeto}.md` muda de forma.** Seções por classificação
  em vez de por namespace, com colunas Tipo, Namespace, Declaração e Arquivo.
  Nomes de genéricos aparecem como `IQuery<T>` e não como `IQuery\`1`.
- **Evidence e Inferences sobem** conforme o número de tipos parciais.

## Depois de buildar

```
dotnet test
dotnet run --project X7.Knowledge.Cli
```

O benchmark **não** deve ser rodado ainda com `--baseline` contra
`results-c03`: a solução de referência mudou de novo, e a comparação pareada
exige snapshot fixo (ADR-034). Ver a ordem de fechamento na resposta.
