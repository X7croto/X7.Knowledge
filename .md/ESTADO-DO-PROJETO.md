# X7.Knowledge — estado do projeto

Documento de passagem. Serve para retomar o trabalho em outra conversa sem
reconstruir o contexto.

Atualizado no fechamento do C04.

---

## 1. Documentos normativos

| Documento | Versão | Status |
|---|---|---|
| `PROJECT_CONSTITUTION.md` | **2.3** | Normativo, autoridade 1 |
| `COMPILATION_PLAN.md` | **2.2** | Normativo, autoridade 2 |
| `KNOWLEDGE_MODEL.md` | **1.0** | Normativo, autoridade 3 — **CONGELADO** |
| `BENCHMARK.md` | conjunto v5 | `benchmark/` |
| `SECURITY-NOTES.md` | — | informativo |
| `MIGRATION_v1_to_v2.md` | — | histórico |

ADRs 027 a 038 registradas na Constituição §8. As de 034 em diante decorrem de
medição ou de amadurecimento do modelo, e têm texto completo em `.md/`.

**O modelo está congelado (ADR-037).** Toda alteração dele exige ADR, inclusive
as aditivas. Acrescentar um `kind` no C05 exige ADR própria.

---

## 2. Capacidades

| Capacidade | Estado |
|---|---|
| C01 Estrutura Física | concluída |
| C02 Arquitetural | concluída |
| C03 Estrutura do Código | concluída |
| C04 Modelo Estrutural | **concluída** |
| C05 Modelo Comportamental | próxima |
| C06 em diante | não iniciadas |

C04 entregue em duas fatias: relações (`type.inherits`, `type.implements`) e
estrutura do tipo (`type.kind`, `type.accessibility`, `type.modifier`,
`type.generic-parameter`, `type.nested-in`), mais a Inference
`type.is-partial`.

Critério de conclusão verificado nos três itens. O item 1 — *representação
própria e completa* — é verificado por **IV-14** dentro da compilação, e não
por julgamento.

---

## 3. Números

Solução de referência: o próprio compilador, 5 projetos, nível S.

```
Observations 830
Evidence     3
Inferences   12
Limitações   6
Digest       a0e430d38833d497…
```

**Benchmark, linha de base `benchmark/results-c04`:**

```
Perguntas    15
Sustentadas  8   (era 7 no C03)
Cobertura    53% (era 47%)
Mediana CR   429‰
```

Comparação pareada C03→C04, **sete de sete perguntas, nenhuma exclusão**:
mediana 196‰ → 196‰, sem regressão. Primeira comparação estruturalmente
válida do projeto.

Quatro perguntas pioraram individualmente, todas com a mesma causa registrada:
a Base cortada em C03 não emite a limitação `type-partial-single-site`, e
Q01/Q02/Q03 pagam uma linha a mais em `Structure/Solution.md`. Q07 subiu 5‰ —
o C04 custou dois tokens no arquivo de tipos.

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

1. **C05 — Modelo Comportamental.** Ver §8.
2. **Migrar a solução de referência** para um sistema de produção antes do
   C08. A ADR-038 resolveu a incomparabilidade entre medições; não resolve o
   viés de medir o compilador contra o próprio código, que a partir de
   "Convenções" distorce por outra razão.
3. **Q10 e Q11 seguem sem sustentação.** Custam 7.472 e 5.148 tokens de
   código-fonte e são as que decidem se a tese do projeto se sustenta.
4. **`partial` é Inference por decisão, não por necessidade.** O Producer já lê
   os modificadores da declaração; incluir `partial` no vocabulário eliminaria
   a limitação declarada. Com o modelo congelado, a troca custa versão maior e
   ADR (EX-03). Nota em `KNOWLEDGE_MODEL.md` §6.3.2.
5. **`takeown` na pasta do repositório**, com Drive pausado e VS fechado.
6. **`ARQUIVAR-LEGADO.ps1` tem acentos** e vai quebrar se for usado. Opcional:
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

---

## 8. Próximo passo — C05

O plano define C05 como métodos, construtores, propriedades, campos, eventos,
operadores, assinaturas, parâmetros, tipos de retorno, modificadores e
restrições genéricas. É grande demais para uma fatia.

**Primeira fatia sugerida: superfície pública — métodos e propriedades, com
assinatura.** É o que a Q09 pede (*"Qual é a superfície pública de
KnowledgeModelBuilder?"*) e a menor fatia que sustenta uma pergunta nova.
Campos, eventos, operadores e restrições genéricas vêm depois.

Ordem de trabalho:

1. **ADR dos `kind`s novos** — agora obrigatória, o modelo está congelado.
2. Definir a projeção sob a §9.1 e seu corolário: `Behavior/` particionado por
   projeto, seccionado pelo campo mais caro de repetir. É onde acessibilidade
   e modificadores de tipo finalmente aparecem publicados (ADR-036).
3. Producer, com teste de reprodutibilidade byte-a-byte (D-08).
4. Invariantes novos, verificáveis dentro da compilação.
5. Medição por corte: `--until C04` sobre a árvore do dia.

É a capacidade em que a compressão deve finalmente aparecer, porque o
equivalente em código-fonte passa a ser corpo de método.
