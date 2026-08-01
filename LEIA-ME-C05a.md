# C05, fatia A — superfície pública

Métodos, construtores e propriedades, com assinatura. Implementa ADR-039
(modelo) e ADR-040 (projeção).

---

## 1. Antes de aplicar

Feche o Visual Studio. Aplique o zip por cima da raiz da solução. Rode
`VERIFICAR-ESTADO.ps1` **depois** de copiar e **antes** de buildar.

O script vai reprovar de novo na linha do modelo: ele procura a string
`0.7.0`, que já estava desatualizada antes deste zip — o congelamento tinha
levado o esquema a `1.0.0` — e agora o valor correto é `1.1.0`.

Vale trocar a verificação por uma que não quebre na próxima capacidade, que é
o padrão que o projeto já aprendeu com os testes de lista fechada. Em ASCII:

```powershell
$declarado = (Select-String -Path 'X7.Knowledge\KnowledgeCompiler.cs' `
    -Pattern 'ModelVersion = "(\d+\.\d+\.\d+)"').Matches[0].Groups[1].Value

$normativo = (Select-String -Path '.md\KNOWLEDGE_MODEL.md' `
    -Pattern '^\*\*Esquema:\*\* `(\d+\.\d+\.\d+)`').Matches[0].Groups[1].Value

if ($declarado -eq $normativo) {
    "ok     modelo $declarado confere com KNOWLEDGE_MODEL.md"
} else {
    "FALTA  modelo $declarado no codigo, $normativo no documento"
}
```

Assim a verificação passa a testar a propriedade — código e documento
concordam — em vez de uma versão específica.

---

## 2. O que entra

Novos:

| Arquivo | Papel |
|---|---|
| `X7.Knowledge/Model/MemberVocabulary.cs` | vocabulários fechados de membro |
| `X7.Knowledge/Compilation/Producers/MemberIdentity.cs` | identidade `member:`, em um lugar só |
| `X7.Knowledge/Compilation/Producers/MemberSurfaceProducer.cs` | o Producer do C05 |
| `X7.Knowledge/Publishing/BehaviorPublisher.cs` | `Behavior/`, um arquivo por tipo |
| `X7.KnowledgeTests/MemberSurfaceTests.cs` | quinze testes |

Alterados:

- `KnowledgeId.cs` — `ForMember`.
- `ObservationKinds.cs` — oito `kind`s novos no catálogo.
- `TypeIdentity.cs` — `Display` passa a aceitar qualquer `ITypeSymbol`; o C05
  precisa exibir `T`, arranjo e ponteiro, que não são `INamedTypeSymbol`.
- `ModelInvariants.cs` — IV-18 a IV-21, e IV-13 ampliado para `typeId`.
- `KnowledgeCompiler.cs` — modelo `1.1.0`, `C05` na lista de capacidades, o
  Producer e o Publisher novos, e o parâmetro `behaviorLayout`.
- `X7.Knowledge.Cli/*` — opção `--behavior-layout`.
- `SolutionFixture.cs` — a solução de teste ganhou membros: sobrecarga,
  parâmetro opcional, `out`, método genérico, propriedade `init`, membro não
  público e tipo aninhado com superfície.

**`C05` entrar na lista de capacidades é o que faz `--until C04` valer como
linha de base.** Sem isso não há o que cortar.

---

## 3. Rodar

```powershell
dotnet build
```
```powershell
dotnet test
```
```powershell
dotnet run --project X7.Knowledge.Cli -- -o C:\Temp\X7Knowledge
```

Confira em `C:\Temp\X7Knowledge\Behavior\`:

- `INDEX.md` com projeto e contagem, **sem nome de tipo nenhum**;
- `X7.Knowledge\X7.Knowledge.Model.KnowledgeModelBuilder.md` com a superfície
  do builder e, no cabeçalho, `public sealed class KnowledgeModelBuilder` —
  é a acessibilidade que a ADR-036 tirou do inventário com prazo, e este é o
  prazo vencendo.

---

## 4. Medição — obrigatória antes de fechar a capacidade

São duas coisas diferentes, e as duas precisam rodar.

**MT-02, comparação pareada por corte (ADR-038).**

```powershell
dotnet run --project X7.Knowledge.Cli -- --until C04 -o C:\Temp\Base-C04
```
```powershell
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge C:\Temp\Base-C04 --output C:\Temp\results-c04-resnap
```
```powershell
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge Knowledge --output benchmark\results-c05 --baseline C:\Temp\results-c04-resnap\results.json
```

**ADR-040 §6, os dois layouts de `Behavior/`.** A decisão de particionar por
tipo foi tomada sobre estimativa, e a ADR-036 estabeleceu que granularidade
não fecha sem número.

```powershell
dotnet run --project X7.Knowledge.Cli -- --behavior-layout project -o C:\Temp\Base-C05-projeto
```
```powershell
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge C:\Temp\Base-C05-projeto --output C:\Temp\results-c05-projeto
```

A Q09 vai aparecer como **não sustentada** nessa segunda medição: o
`kbFiles` dela aponta `Behavior/X7.Knowledge/…KnowledgeModelBuilder.md`, que o
layout por projeto não produz. Para comparar de verdade, o `T_kb` a olhar é o
do arquivo `Behavior/X7.Knowledge.md` — some os tokens dele e ponha ao lado do
número da medição por tipo. Se a estimativa da ADR estiver certa, a diferença
fica perto de uma ordem de grandeza.

Um comando por vez no terminal.

---

## 5. O que esperar que dê errado primeiro

- **`--until C04` sobre a árvore de hoje** é a primeira execução real do corte
  com uma capacidade a mais. Se alguma invariante do C05 disparar na Base
  cortada, é defeito: as IV-18 a IV-21 são de consistência e devem ser
  vacuamente verdadeiras sem membros.
- **Caminho longo no Windows.** Um arquivo por tipo, dentro de pasta do
  projeto, dentro do Drive sincronizado. Publicar em `C:\Temp` deixou de ser
  conveniência.
- **Contagem de Observations sobe muito.** Todos os membros declarados são
  observados, de qualquer acessibilidade — a projeção é que filtra. Isso pesa
  em `knowledge.model.json`, não em `T_kb`.
- **Implementação explícita de interface não é observada** nesta fatia, junto
  com campo, evento, operador, indexador e construtor estático. Está declarado
  em `acquisition.limitation` de escopo `type-members-partial`, e aparece no
  `Structure/Solution.md`.
