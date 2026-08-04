# X7.Knowledge — estado do projeto

Documento de passagem. Serve para retomar o trabalho em outra conversa sem
reconstruir o contexto.

Atualizado no fechamento do **C05**, concluída nos três critérios.

---

## 1. Documentos normativos

| Documento | Versão | Status |
|---|---|---|
| `PROJECT_CONSTITUTION.md` | **2.8** | Normativo, autoridade 1 |
| `COMPILATION_PLAN.md` | **2.3** | Normativo, autoridade 2 |
| `KNOWLEDGE_MODEL.md` | **1.4** | Normativo, autoridade 3 — **CONGELADO** |
| `BENCHMARK.md` | conjunto v6 | `benchmark/` |
| `SECURITY-NOTES.md` | — | informativo |
| `MIGRATION_v1_to_v2.md` | — | histórico |

ADRs 027 a 044 registradas na Constituição §8. As de 034 em diante decorrem de
medição, de amadurecimento do modelo, da entrada do C05 ou de defeito
descoberto em produção, e têm texto completo em `.md/`.

A ADR-041 alterou este documento sem alterar o esquema; o registro está na
§11.2 do modelo, e não na tabela de versões. Incrementar a versão do esquema
sem que o esquema mude tornaria a versão um dado sem significado.

**O modelo está congelado (ADR-037).** Toda alteração dele exige ADR, inclusive
as aditivas. A primeira alteração sob o congelamento foi a ADR-039: esquema
`1.1.0`, oito `kind`s novos, nenhum existente alterado. A regra de extensão foi
exercida contra a maior adição do plano e não precisou ser quebrada.

---

## 2. Capacidades

| Capacidade | Estado |
|---|---|
| C01 Estrutura Física | concluída |
| C02 Arquitetural | concluída |
| C03 Estrutura do Código | concluída |
| C04 Modelo Estrutural | **concluída** |
| C05 Modelo Comportamental | **concluída** |
| C06 em diante | não iniciadas |

C04 entregue em duas fatias: relações (`type.inherits`, `type.implements`) e
estrutura do tipo (`type.kind`, `type.accessibility`, `type.modifier`,
`type.generic-parameter`, `type.nested-in`), mais a Inference
`type.is-partial`.

C05 fatia A entregue com oito `kind`s (`type.declares-member`,
`member.declared`, `member.accessibility`, `member.modifier`, `member.type`,
`member.parameter`, `member.generic-parameter`, `member.accessor`), identidade
`member:`, IV-18 a IV-21 e a projeção `Behavior/` por tipo. Fatia B entregue com o `kind`
`member.explicit-interface`, quatro valores em `member.declared`, dois em
`member.modifier`, dois em `member.accessor`, IV-22 e IV-19/IV-21 ampliadas.
Fatia C entregou restrições
genéricas (`member.generic-constraint`, `type.generic-constraint`), o campo
`defaultValue` em `member.parameter`, o modificador `ref-readonly` que o
vocabulário declarava sem produtor, e IV-23. A limitação
`type-members-partial` deixou de existir: a superfície declarada está
completa.

Critério de conclusão verificado nos três itens. O item 1 — *representação
própria e completa* — é verificado por **IV-14** dentro da compilação, e não
por julgamento.

---

## 3. Números

Solução de referência: o próprio compilador, 5 projetos, nível S.

```
Observations 6046
Evidence     1
Inferences   10
Limitações   6
Digest       a0e430d38833d497…
```

Corte em C04 sobre o mesmo snapshot: 845 Observations.

**Benchmark, `benchmark/results-c05`:**

```
Perguntas    15
Sustentadas  9   (era 8 no C04)
Cobertura    60% (era 53%)
Mediana CR   427‰  global, população nova
```

Comparação pareada C04→C05, **oito de oito perguntas, nenhuma exclusão**:
mediana **417‰ → 417‰**, sem regressão, e nenhuma piora individual.

A mediana global (427‰) e a pareada (417‰) não se comparam: são populações
diferentes. A Q09 entrou cara no conjunto, e isso é ganho de cobertura, não
regressão — é o caso que a ADR-034 existe para separar. Quem governa MT-02 é a
pareada.

Três perguntas pioraram individualmente, com a mesma causa registrada: a
sétima limitação em `Structure/Solution.md`, que Q01, Q02 e Q03 leem.

```
Q01  2538‰ -> 2760‰
Q03   713‰ ->  775‰
Q02   106‰ ->  115‰
```

**Q09, e o número que fecha a ADR-040:**

```
Behavior/ por tipo      T_code 712   T_kb   304   CR   427‰
Behavior/ por projeto   T_code 712   T_kb  7179   CR 10083‰
```

**Efeito da ADR-041, medido no corte C04 sobre o mesmo snapshot:** 893
Observations antes, 817 depois; mediana 441‰ → 414‰. Nenhuma projeção foi
otimizada — é a saída de build deixando de ser descrita. As linhas de base
`results-c01` a `results-c04` foram medidas antes disso e não comparam com o
que vem depois (BM-13).

---

## 4. Como medir (ADR-038)

A linha de base **não** é recuperada de medição anterior. É produzida por corte
de capacidade sobre o snapshot atual:

```powershell
dotnet run --project X7.Knowledge.Cli -- --until C04 -o C:\Temp\Base-C04
```
```powershell
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge C:\Temp\Base-C04 --output C:\Temp\results-c04-resnap
```
```powershell
dotnet run --project X7.Knowledge.Benchmark -- --questions benchmark\questions.json --knowledge Knowledge --output benchmark\results-c05 --baseline C:\Temp\results-c04-resnap\results.json
```

Capacidades são aditivas, então o prefixo da lista de Producers é exatamente a
Base daquela capacidade. Mesmo binário, mesma entrada, mesmo snapshot: `T_code`
fica idêntico dos dois lados por construção.

Pergunta cuja capacidade não foi executada conta como **não sustentada**
(MT-03), nunca como medição inválida.

---

## 5. Ambiente — armadilhas conhecidas

**O repositório está dentro do Google Drive.** Quatro incidentes: travamento
de exclusão (duas vezes), publicação bloqueada e reversão de arquivo. O
compilador publica substituindo conteúdo, nunca movendo pasta. A origem
permanece, e agora o `.git` também está lá dentro.

**Feche o Visual Studio antes de aplicar qualquer zip.** A IDE reescreve
`.csproj` e arquivos abertos.

**Rode `VERIFICAR-ESTADO.ps1` depois de copiar e antes de buildar.**

**`.ps1` só com ASCII.** PowerShell 5.1 lê `.ps1` como ANSI quando não há BOM;
acento vira lixo e quebra o parser.

**`Unblock-File` antes de rodar `.ps1` baixado.** A mark-of-the-web faz a
política de execução recusar o arquivo.

**Um comando por vez no terminal.** Colar blocos junta a última linha com a
primeira do bloco seguinte. Custou três rodadas nesta sessão — `pushgit`,
`resnapdotnet`, e um `--until` que nunca rodou.

**Git:** remoto em `github.com/X7croto/X7.Knowledge`. A pasta pertence a um SID
da instalação anterior do Windows; contornado com `safe.directory`, mas a posse
real ainda não foi reassumida (`takeown`).

**Publicar fora da pasta sincronizada funciona:** `-o C:\Temp\X7Knowledge`.

---

## 6. Pendências abertas

1. **C05 fatia B.** Campos, eventos, operadores, indexadores, construtores
   estáticos, implementações explícitas de interface e restrições genéricas.
   Reaproveita `member.type`, `member.parameter` e `member.accessor`; o que
   entra de novo são valores no vocabulário de `member.declared`, e isso exige
   ADR — o modelo está congelado.
2. **Migrar a solução de referência** para um sistema de produção antes do
   C08. A ADR-038 resolveu a incomparabilidade entre medições; não resolve o
   viés de medir o compilador contra o próprio código, que a partir de
   "Convenções" distorce por outra razão.
3. **Q10 e Q11 seguem sem sustentação.** Custam 7.472 e 5.148 tokens de
   código-fonte e são as que decidem se a tese do projeto se sustenta.
4. **O `INDEX.md` é metade do custo da Q09** — 152 dos 304 tokens. Pela BM-12
   ele nem precisaria estar no `kbFiles`, e retirá-lo levaria a Q09 de 427‰
   para cerca de 213‰. Não foi retirado: a pergunta é se um consumidor real
   abre aquele índice, e responder que não sem evidência seria melhorar a
   métrica mexendo no que se declara ler (BM-06). Decidir contra observação de
   uso.
5. **`partial` ficou mais fraco depois da ADR-041.** A segunda declaração dos
   tipos hospedeiros de `[GeneratedRegex]` morava em `obj/`, então a Evidence
   `type.declaration-sites` deixou de agrupar dois locais para eles — de 3
   Evidence para 1. A limitação cobre, e a saída continua sendo ler `partial`
   dos modificadores da declaração.
6. **`partial` é Inference por decisão, não por necessidade.** O Producer já lê
   os modificadores da declaração; incluir `partial` no vocabulário eliminaria
   a limitação declarada. Com o modelo congelado, a troca custa versão maior e
   ADR (EX-03). Nota em `KNOWLEDGE_MODEL.md` §6.3.2.
7. **`takeown` na pasta do repositório**, com Drive pausado e VS fechado.
8. **`ARQUIVAR-LEGADO.ps1` tem acentos** e vai quebrar se for usado. Opcional:
   as 8 pastas do v1 revogado seguem no disco, fora da solução e portanto fora
   da Base.

---

## 7. Padrões que se provaram

**Invariantes dentro da compilação, não só em teste.** Barraram Bases
inválidas antes de publicar. IV-14 é o caso mais claro: transformou
"representação completa", que era julgamento, em condição verificável.

**Decisão de granularidade não fecha sem medição.** Duas vezes o princípio
pareceu claro e o número disse o contrário. A separação entre inventário e
relações veio de Q07 saltando 24%. A ADR-036 corrigiu a ADR-035, que decidira
por analogia: o eixo de seção é o campo **mais caro de repetir**, e seccionar
por classificação obrigava o namespace a ser coluna — 43% de aumento no T_kb.

**Comparação exige população fixa.** A ADR-034 registrou isso para
capacidades; a lição reapareceu duas vezes fora dela. Um arquivo renomeado
tirou a Q01 do cálculo e a mediana caiu 56% sem que nada tivesse melhorado.

**Testes agnósticos à capacidade.** Todo teste que fixou o conjunto exato de
capacidades quebrou na seguinte. Verificar formato e propriedade, nunca lista
fechada.

**`Assert.All` sobre coleção vazia passa.** Um helper com filtro errado deu
falso verde. Todo `Assert.All` merece um `Assert.NotEmpty` antes.

**O que o modelo não promete, o teste não exige.** OB-04 ordena Observations
por id, que é hash de conteúdo. Quem ordena para exibir é o Publisher.

**Identidade calculada em um lugar só.** Havia duas cópias do cálculo de
identidade de tipo e quase entrou a terceira. Duas implementações da mesma
identidade divergem em silêncio.

**Fatias estreitas.** C04-b entrou com cinco kinds, dois Producers e doze
testes, e compilou na primeira rodada.

**O mesmo cálculo copiado tem o mesmo defeito em todas as cópias.** Três
Producers repetiam `?.RelativePath ?? path`, e o `?? path` publicava caminho
absoluto justamente quando o arquivo estava fora da fronteira. Não eram
implementações divergindo — copiar não faz divergir, faz o defeito se
multiplicar sem que nenhuma cópia pareça errada.

**Teste que fixa um caractere proibido erra como teste de lista fechada.**
Proibir `<` em nome de tipo reprovou `IQuery<T>`. O que distingue nome emitido
pelo compilador não é o caractere, é a posição — `<` no início ou depois de
ponto. Verificar propriedade, nunca superfície.

**Verificação que fixa uma versão quebra na versão seguinte.** O
`VERIFICAR-ESTADO.ps1` procurava `0.7.0` e reprovava desde o congelamento.
Agora compara `ModelVersion` com o `**Esquema:**` do `KNOWLEDGE_MODEL.md`:
testa que código e documento concordam.

**Forma não é estrutura e valor não é dado.** Errei nisso duas vezes na mesma
direção: a ADR-042 tirou o valor do `const` chamando-o de dado, e a fatia A
publicava `= …` no lugar do valor padrão de parâmetro. Nos dois casos o que o
consumidor precisa é do valor. A pergunta útil não é *isto é dado?*, e sim
*trocar isto quebra alguém?* — para `const` público a resposta é sim, e sem
recompilação.

**A conferência que concorda com a projeção não verifica nada.** A tentação,
quando a conferência de assinatura acusou `public const string Kind`, era
afrouxá-la para tolerar. Ela existe para discordar; normalizar até os dois
lados coincidirem transforma verificação em tautologia.

**Nem todo critério vira invariante.** A IV-14 funcionou porque todo tipo tem
classificação. Não existe equivalente para membro: tipo sem membro é legítimo,
e o modelo não sabe o que ficou de fora. Quando a cobertura não é verificável
de dentro do modelo, a verificação muda de lugar — vai para teste contra o
compilador de referência — e isso se declara, em vez de se alegar simetria com
a capacidade anterior.

---

## 8. Próximo passo — C06

O C05 fechou nos três critérios, em três fatias mais uma correção:

| Fatia | Entrega | ADR |
|---|---|---|
| A | método, construtor, propriedade; identidade `member:`; `Behavior/` por tipo | 039, 040 |
| B | campo, evento, operador, indexador, construtor estático, implementação explícita | 042 |
| C | restrições genéricas, valor padrão de parâmetro, `ref readonly` | 043 |
| — | valor de constante, achado pela conferência de assinatura | 044 |

Fora do plano do C05, mas no caminho dele: a **ADR-041**, que corrigiu a
fronteira do que é observado depois que a publicação por tipo esbarrou num
nome de arquivo inválido emitido por gerador de código.

**Critério 2 satisfeito por `SignatureConformanceTests`**: toda assinatura
publicada é devolvida ao compilador de referência, e verifica-se que é C#
válido, que nada público foi omitido e que nada publicado foi inventado.

O C06 é *Representação das Relações*, e a pergunta que o mede é a Q10 — *quem
consome `Observation` e de que forma*. Ele herda a segunda regra da §9.1:
`Relations/` hoje é por projeto porque responde *quem implementa X*, que é
varredura; a projeção nova justifica a própria unidade contra a pergunta que
sustenta, e sai medida.

Candidato registrado para o C11: **teste de cobertura de vocabulário** — todo
valor declarado é produzido ao menos uma vez pela fixture. O `ref-readonly`
ficou dois ciclos no vocabulário sem que nada pudesse produzi-lo, e o valor de
`const` foi excluído por decisão errada; nenhum invariante cobre essa classe.

O critério 1 do C05 — *o comportamento público é compreensível sem abrir
código* — não é verificável por invariante: não existe equivalente da IV-14
para membro, porque tipo sem membro é legítimo. Quem o torna objetivo é o
critério 2, a conferência de assinatura contra o compilador de referência
(ADR-039 §6). **Esse teste ainda não existe**, e é o único item que separa o
C05 da conclusão.

Depois do C05, o C06 herda a segunda regra da §9.1: `Relations/` hoje é por
projeto porque responde *quem implementa X*, uma varredura. As projeções novas
justificam a própria unidade contra a pergunta que sustentam.
