# Arquitetura — X7.Knowledge

Derivado do grafo de dependências entre projetos. Toda afirmação abaixo aponta sua Evidence no KnowledgeModel.

## Camadas

Profundidade é a maior distância até um projeto que não referencia nenhum outro da solução. Regra `layer-by-graph-depth`, Confidence `Asserted`.

### Camada 0

- X7.Knowledge

### Camada 1

- X7.Knowledge.Benchmark
- X7.Knowledge.Cli
- X7.Knowledge.Specifications
- X7.KnowledgeTests

## Projetos-raiz

Nenhum projeto da solução depende deles. Regra `root-by-absence-of-dependents`.

- X7.Knowledge.Benchmark
- X7.Knowledge.Cli
- X7.Knowledge.Specifications
- X7.KnowledgeTests

## Projetos-folha

Não referenciam nenhum projeto da solução. Regra `leaf-by-absence-of-references`.

- X7.Knowledge

## Ciclos de dependência

Nenhum ciclo entre projetos.
