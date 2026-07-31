# Como rodar o benchmark

## Instalação

1. Copie `X7.Knowledge.Benchmark/` para a raiz da solução.
2. Copie `BENCHMARK.md` e `questions.json` para `benchmark/`.
3. Registre na solução:

```
dotnet sln X7.ProjectIndexer.slnx add X7.Knowledge.Benchmark\X7.Knowledge.Benchmark.csproj
dotnet build
```

## Executar

```
dotnet build
dotnet run --project X7.Knowledge.Cli
```
```
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge Knowledge --output benchmark\results
```
    
Em PowerShell troque `^` por `` ` `` ou escreva em uma linha só.

## Saída esperada (linha de base C01)

```
Perguntas    15
Sustentadas  3
Cobertura    20%
Mediana CR   777‰
```

Gera `benchmark/results/results.json` e `benchmark/results/REPORT.md`.

Ambos são versionados. É a comparação entre eles, capacidade após capacidade,
que aplica MT-02.

## Nota sobre o número

`777‰` significa que a Base gasta 78% dos tokens que o código gastaria. É um
resultado ruim, e é o resultado certo para C01: três perguntas estruturais
sustentadas, doze não sustentadas, e nenhuma compressão real ainda.

O CR só melhora quando a Base passa a responder perguntas cujo equivalente em
código-fonte é caro. Q10 (4.716 tokens de código) e Q11 (3.256) são os alvos
grandes — quando C06 e C07 existirem, elas entram no cálculo e puxam a mediana
para baixo.
