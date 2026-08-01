# Estrutura Física — X7.Knowledge

## Projetos

| Projeto | Caminho | Frameworks | Saída | Teste |
|---|---|---|---|---|
| X7.Knowledge.Benchmark | `X7.Knowledge.Benchmark/X7.Knowledge.Benchmark.csproj` | net10.0 | Exe | — |
| X7.Knowledge.Cli | `X7.Knowledge.Cli/X7.Knowledge.Cli.csproj` | net10.0 | Exe | — |
| X7.Knowledge.Specifications | `X7.Knowledge.Specifications/X7.Knowledge.Specifications.csproj` | net10.0 | — | sim |
| X7.Knowledge | `X7.Knowledge/X7.Knowledge.csproj` | net10.0 | — | — |
| X7.KnowledgeTests | `X7.KnowledgeTests/X7.KnowledgeTests.csproj` | net10.0 | — | sim |

## Árvore lógica

```
X7.Knowledge
  src/
    X7.Knowledge.Cli
    X7.Knowledge
  tests/
    X7.Knowledge.Specifications
    X7.KnowledgeTests
  X7.Knowledge.Benchmark
```

## Limitações de aquisição

O que o compilador não conseguiu obter, declarado explicitamente.

| Escopo | Motivo | Origem |
|---|---|---|
| project-property | Directory.Build.props presente e não resolvido (leitura sintática) | `Directory.Build.props` |
| project-property | Directory.Build.props presente e não resolvido (leitura sintática) | `Directory.Build.props` |
| project-property | Directory.Build.props presente e não resolvido (leitura sintática) | `Directory.Build.props` |
| project-property | Directory.Build.props presente e não resolvido (leitura sintática) | `Directory.Build.props` |
| project-property | Directory.Build.props presente e não resolvido (leitura sintática) | `Directory.Build.props` |
| type-partial-single-site | Tipo `partial` declarado em um único arquivo não é detectado; a regra deriva parcialidade de múltiplos locais de declaração | `X7.Knowledge.slnx` |
