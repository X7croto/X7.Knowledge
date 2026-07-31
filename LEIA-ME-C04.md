# C04 — Herança e implementação

Fatia estreita de propósito. Modelo `0.5.0 → 0.6.0`, aditivo.

Primeira capacidade que **só existe em nível S**.

> Não compilado aqui. Antes de aplicar: feche a IDE, copie, rode
> `VERIFICAR-ESTADO.ps1`, e só então builde.

## Conhecimento novo

| `kind` | `payload` |
|---|---|
| `type.inherits` | `{ baseTypeName, baseTypeId?, external? }` |
| `type.implements` | `{ interfaceName, interfaceId?, external? }` |

## Decisões

**Base implícita não é observada.** `System.Object`, `ValueType`, `Enum`,
`Delegate`, `MulticastDelegate` ficam de fora. Toda classe deriva de `Object`:
observar isso somaria mais de mil Observations sem informar nada, e inflaria o
CR sem contrapartida. Exclusão declarada no catálogo, não omissão.

**Só interface declarada diretamente.** A herdada da classe base é derivável do
conjunto; computá-la aqui seria inferência disfarçada de observação (OB-01).

**Alvo fora da solução guarda o nome, com `external: "true"`.** Saber que algo
deriva de `Exception` é conhecimento legítimo. Descartar perderia informação;
forjar identidade inexistente seria pior.

**IV-13 novo:** referência a tipo no payload aponta para tipo existente. Sem
isso, uma relação poderia apontar para o vazio e a Base pareceria completa.

**Nível X não produz nada** e declara `acquisition.limitation` com escopo
`type-relations`.

## Projeção

`Structure/Types/{Projeto}.md` ganha duas colunas: *Herda de* e *Implementa*.
Sem arquivo novo — a granularidade por projeto já está certa.

## Ordem

```
.\VERIFICAR-ESTADO.ps1
dotnet build
dotnet test
dotnet run --project X7.Knowledge.Cli -- -o C:\Temp\X7Knowledge
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge C:\Temp\X7Knowledge --output benchmark\results --baseline benchmark\results-c03\results.json
```

O `--baseline` vale desta vez: nenhum `.csproj` mudou, então o `solutionDigest`
deve bater e a comparação pareada é válida. Código 5 significa regressão e
bloqueia a conclusão de C04.

Conjunto de perguntas vai à v4: Q08 passa a ser sustentada.
