# C02 — Representação Arquitetural

Modelo `0.2.0 → 0.3.0`. Aditivo: nenhum `kind` anterior mudou.

Inclui a correção de comparabilidade do benchmark descrita na Seção 5.

> Não compilado — sem SDK .NET disponível aqui.

---

## 1. Conhecimento novo

**Observations** (`6.1.1` do modelo):

| `kind` | `payload` |
|---|---|
| `project.references-project` | `{ targetId }` |
| `project.package-reference` | `{ name, version? }` |

**Evidence:**

| `kind` | Agrupa |
|---|---|
| `project.graph-position` | O grafo inteiro: nós e arestas |
| `project.cycle-path` | Referências internas a um ciclo |

**Inferences** — todas `Asserted`:

| `kind` | Regra |
|---|---|
| `project.layer` | `layer-by-graph-depth` |
| `project.is-root` | `root-by-absence-of-dependents` |
| `project.is-leaf` | `leaf-by-absence-of-references` |
| `project.participates-in-cycle` | `cycle-by-strongly-connected-component` |

**Projeções:** `Architecture/Architecture.md` e
`Architecture/ProjectDependencies.md`.

---

## 2. Arquivos novos

| Arquivo | Papel |
|---|---|
| `Compilation/ProjectGraph.cs` | Grafo, SCC por Tarjan, profundidade por condensação |
| `Compilation/Producers/ProjectReferenceProducer.cs` | Observa referências de projeto e pacote |
| `Compilation/Producers/ArchitectureProducer.cs` | Conclui camada, raiz, folha, ciclo |
| `Publishing/ArchitecturePublisher.cs` | Publica as duas projeções |
| `AssemblyInfo.cs` | `InternalsVisibleTo` para os testes |
| `X7.KnowledgeTests/ArchitectureTests.cs` | 10 testes de C02 |

## 3. Arquivos alterados

| Arquivo | Mudança |
|---|---|
| `Model/ObservationKinds.cs` | Dois kinds de C02 |
| `Model/EvidenceKinds.cs` | Reduzido ao que é de fato produzido |
| `Acquisition/ProjectFile.cs` | `ProjectReferences`, `PackageReferences` |
| `Acquisition/ProjectFileReader.cs` | Lê `ProjectReference` e `PackageReference`; resolve caminho relativo ao `.csproj` |
| `KnowledgeCompiler.cs` | Dois Producers e um Publisher novos; `["C01","C02"]`; modelo `0.3.0` |
| `X7.KnowledgeTests/SolutionFixture.cs` | Referências entre projetos na solução de teste |

---

## 4. Decisões

**Todas as Inferences são `Asserted`.** Posição no grafo é exata dada a
estrutura — não é regularidade estatística. `Observed` só faz sentido a partir
de C08, onde convenção é frequência. Marcar profundidade como `Observed`
inflaria incerteza onde não existe.

**Camada é profundidade numérica, não nome arquitetural.** Chamar um projeto
de "domínio" ou "infraestrutura" exigiria interpretação semântica, que o
compilador não faz em nível X. `depth` é o que o grafo de fato determina.

**Profundidade é calculada sobre a condensação em componentes fortemente
conexos.** Assim um ciclo não torna a profundidade indefinida: membros de um
mesmo ciclo compartilham profundidade, que é exatamente o que a posição no
grafo determina.

**Evidence de posição é o grafo inteiro.** A profundidade de um projeto depende
de toda a estrutura, não de um trecho. Apontar só as referências do próprio
projeto seria proveniência incompleta. Inclui `project.declared` além das
arestas — um grafo são nós e arestas, e assim a Evidence nunca é vazia mesmo em
solução sem referências.

**Referência para fora da solução não vira aresta.** Vira
`acquisition.limitation`. Critério 2 do C02 exige que nenhuma dependência
inventada apareça.

---

## 5. Correção: comparabilidade do benchmark

O CR subiu de 780‰ para 839‰ quando um projeto foi adicionado à solução. Não
era regressão: o `Solution.md` cresceu ~8% e o `T_code` das perguntas
sustentadas não mudou.

O `BENCHMARK.md` não distinguia os dois casos. Agora distingue:

- **BM-07** — duas medições só são comparáveis com a mesma solução de
  referência. `results.json` passa a registrar `solutionDigest` e
  `projectCount`, lidos do manifesto da Base.
- **BM-08** — MT-02 se aplica a capacidades, não a mudanças da solução de
  referência.

Também está registrada a **limitação conhecida**: `T_kb` é contado por arquivo
inteiro, então enquanto uma projeção for monolítica o CR de perguntas sobre um
subconjunto cresce com o tamanho da solução. Isso é sinal de design sobre
granularidade de publicação, decisão que pertence a C12.

---

## 6. Ordem de execução

```
dotnet build
dotnet test
dotnet run --project X7.Knowledge.Cli
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge Knowledge --output benchmark\results
```

Esperado no CLI: `Capacidades C01, C02` e contagens de Evidence e Inferences
maiores que zero.

Esperado no benchmark: **6 sustentadas de 15** (cobertura 40%), com Q04, Q05 e
Q06 entrando. A mediana deve **cair** — as perguntas de dependência têm
`T_code` alto e `T_kb` baixo.

Se cair, é o primeiro ganho medido do projeto.
