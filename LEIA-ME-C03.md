# C03 — Representação Estrutural do Código

Modelo `0.3.0 → 0.4.0`. Aditivo.

**Primeira dependência externa do compilador: Roslyn.** E a primeira vez que a
Base pode alcançar nível S.

> Não compilado — sem SDK .NET disponível aqui. Esta entrega tem mais
> superfície de erro que as anteriores: MSBuildWorkspace, API de símbolos e
> travessia sintática. Espere ajustes na primeira compilação.

---

## 1. Conhecimento novo

| `kind` | `subject` | `payload` |
|---|---|---|
| `namespace.declared` | Namespace | `{ name, parentId? }` |
| `namespace.contains` | Namespace | `{ typeId }` |
| `type.declared` | Tipo | `{ name, metadataName, namespace?, projectId }` |
| `type.location` | Tipo | `{ file }` |

Identidades novas: `ns:{nomeCompleto}` e
`type:{nomeQualificado}@{nomeDoProjeto}`.

**Projeções:** `Structure/Types/{Projeto}.md`, `Structure/Types/INDEX.md`,
`Structure/Namespaces.md`.

---

## 2. Roslyn: nível S com degradação declarada

`CompilationProvider` tenta o caminho semântico via `MSBuildWorkspace`. Se o
SDK não estiver disponível, se a solução não abrir ou se um projeto não
carregar, cai para leitura sintática pura **e declara a queda** como
`acquisition.limitation` com escopo `semantic-model`.

Nunca degrada em silêncio (Constituição §5.3).

**O nível é por projeto.** Uma solução pode ter parte resolvida
semanticamente e parte não. O nível global do manifesto é o **menor**
alcançado: a Base não pode alegar semântica que parte dela não tem.

**C01 e C02 permanecem nível X por item**, mesmo quando S está disponível.
Seus Producers leem apenas `.sln` e `.csproj` — marcar como semântico seria
falso.

**Pacotes novos:** `Microsoft.Build.Locator`,
`Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Workspaces.MSBuild`.

O `MsBuildBootstrap.Ensure()` é chamado na primeira linha do CLI. Restrição do
`MSBuildLocator`: o registro precisa acontecer antes de qualquer tipo do
MSBuild carregar. A escolha da instalação é ordenada explicitamente para não
depender da ordem de resolução do Locator (PR-02).

---

## 3. Granularidade de projeção (modelo §9.1)

`Structure/Types/` é publicado **um arquivo por projeto**, com `INDEX.md` que
lista projeto, contagem e link — e **nunca nomes de tipo**.

Motivo: `T_kb` é contado por arquivo inteiro. Um `Types.Index.md` monolítico
faria qualquer pergunta sobre um tipo pagar a solução inteira. Você já viu o
efeito em escala pequena, com o `Solution.md`.

Repetir nomes de tipo no índice anularia o ganho da partição. Há um teste que
verifica isso.

---

## 4. Arquivos novos

`Acquisition/Roslyn/MsBuildBootstrap.cs`,
`Acquisition/Roslyn/SourceCompilation.cs`,
`Acquisition/Roslyn/CompilationProvider.cs`,
`Compilation/Producers/CodeStructureProducer.cs`,
`Publishing/StructurePublisher.cs`,
`X7.KnowledgeTests/CodeStructureTests.cs`.

## 5. Arquivos alterados

`X7.Knowledge.csproj` (pacotes), `Model/ObservationKinds.cs`,
`Model/KnowledgeId.cs`, `KnowledgeCompiler.cs`,
`Compilation/Producers/{Solution,Project,ProjectReference}Producer.cs`
(nível X explícito), `X7.Knowledge.Cli/Program.cs`,
`X7.KnowledgeTests/SolutionFixture.cs`.

---

## 6. Ordem de execução

```
dotnet restore
dotnet build
dotnet test
dotnet run --project X7.Knowledge.Cli
```

**Confira o `Nível` na saída.** Se aparecer `S (semântico)`, o workspace
carregou tudo. Se aparecer `X (sintático)`, veja a tabela de limitações em
`Structure/Solution.md` — o motivo estará lá, com escopo `semantic-model`.

Expectativa de escala: alguns milhares de observations, contra 126 hoje. O
`knowledge.model.json` deve passar de 40 KB para mais de 1 MB, e a compilação
deve levar segundos em vez de milissegundos.

Depois, benchmark com comparação pareada contra a linha de base do C02:

```
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge Knowledge --output benchmark\results --baseline benchmark\results-c02\results.json
```

O conjunto vai à versão 3: Q07 passa a ser sustentada. Código de retorno 5
significa regressão pareada e bloqueia a conclusão de C03.

---

## 7. Pendências suas

- **ADR-034 aprovada** — registre o texto na Constituição, §8, e atualize
  MT-02 conforme a decisão.
- Se `Nível` vier `X`, decida se vale investigar o motivo antes de C04. **C04
  exige nível S**: herança e implementação precisam ser fatos resolvidos, e em
  nível X seriam dedução por nome, o que §5.3 proíbe.
