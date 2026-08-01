# X7.Knowledge — estado do projeto

Documento de passagem. Serve para retomar o trabalho em outra conversa sem
reconstruir o contexto.

---

## 1. Documentos normativos — versões corretas

| Documento | Versão | Onde |
|---|---|---|
| `PROJECT_CONSTITUTION.md` | **2.1** | workspace |
| `COMPILATION_PLAN.md` | **2.1** | workspace |
| `KNOWLEDGE_MODEL.md` | **0.7** | workspace |
| `BENCHMARK.md` | conjunto v5 | `benchmark/` |
| `SECURITY-NOTES.md` | — | raiz da solução |
| `MIGRATION_v1_to_v2.md` | — | histórico |

ADR-034 (comparação pareada) e ADR-035 (projeções do C04) estão **registradas
na Constituição §8**. MT-02 e PL-07 já refletem a ADR-034.

---

## 2. Capacidades

| Capacidade | Estado | Observação |
|---|---|---|
| C01 Estrutura Física | concluída | |
| C02 Arquitetural | concluída | |
| C03 Estrutura do Código | concluída | nível S alcançado |
| C04 Modelo Estrutural | **implementada; falta medir** | duas fatias: relações (C04-a) e estrutura do tipo (C04-b) |
| C05 em diante | não iniciadas | |

C04 cobre agora `type.inherits`, `type.implements`, `type.kind`,
`type.accessibility`, `type.modifier`, `type.generic-parameter` e
`type.nested-in`, mais a Inference `type.is-partial`. O critério 1 do C04
("representação própria e completa") é verificado por **IV-14**, não por
julgamento.

Membros, assinaturas e restrições genéricas são C05, conforme o plano.

---

## 3. Números atuais

Última compilação **antes** da remoção do legado (13 projetos):

```
Nível        S (semântico)
Projetos     13
Observations 2459
Evidence     4
Inferences   24
Limitações   14
```

Evidence e Inferences subiram de 1 e 21 para 4 e 24: três tipos parciais,
cada um com uma `type.declaration-sites` sustentando uma `type.is-partial`.
A limitação nova é `type-partial-single-site`.

**Benchmark: sem medição válida no momento.** A última rodada comparou 4 de 7
perguntas pareadas — no limite de validade que a própria ADR-034 define — e
a solução de referência mudou de novo desde então.

---

## 4. Ambiente — armadilhas conhecidas

**O repositório está dentro do Google Drive.** Causou quatro incidentes:
travamento de exclusão (duas vezes), publicação bloqueada e reversão de
arquivo. O compilador foi endurecido contra isso — publica substituindo
conteúdo, nunca movendo a pasta — mas a origem do problema permanece.

**Feche o Visual Studio antes de aplicar qualquer zip.** A IDE reescreve
`.csproj` e arquivos abertos, desfazendo a cópia.

**Rode `VERIFICAR-ESTADO.ps1` depois de copiar e antes de buildar.**

**Publicar fora da pasta sincronizada funciona** e é opção legítima:
`-o C:\Temp\X7Knowledge`. A Base é função total da entrada e regenera em
segundos; o que precisa ser versionado é `benchmark/results-*`.

---

## 5. Pendências abertas

1. **Regravar a linha de base e medir o C04.** A solução de referência mudou
   por três motivos acumulados: remoção do legado v1, renomeação, e o próprio
   crescimento do compilador. Nenhuma comparação com `results-c03` vale mais.
   O caminho é gravar linha de base nova sobre o snapshot atual e medir o C05
   contra ela.
2. **Q07 deixou de ser pendência.** A pergunta "foi o C04 ou foi o projeto que
   cresceu?" morre junto com a linha de base velha.
3. **Congelar o `KNOWLEDGE_MODEL`.** Os quatro critérios do §12 estão
   satisfeitos desde o C03. Congelar **antes** do C05, que é a maior adição de
   kinds do projeto — congelar depois seria congelar sob pressão. Falta ADR.
4. **Migrar a solução de referência** para um sistema de produção antes do
   C08. Com o legado removido restam 5 projetos coesos: é referência pequena,
   mas real. A partir de "Convenções", medir o compilador contra o próprio
   código de quem o escreveu continua distorcendo o resultado.
5. **Q10 e Q11 continuam sem sustentação.** Custam 4.716 e 3.256 tokens de
   código-fonte e são as que vão decidir se a tese do projeto se sustenta.
   A compressão real depende de C05 em diante.
6. **`partial` é Inference por decisão, não por necessidade.** O Producer já
   lê os modificadores da declaração; incluir `partial` no vocabulário e
   apagar o `PartialTypeProducer` elimina a limitação declarada. Custa remoção
   de kind: versão maior e ADR (EX-03). Nota registrada em
   `KNOWLEDGE_MODEL.md` §6.3.2.

---

## 6. Padrões que se provaram

**Invariantes na compilação, não só em teste.** Barraram Bases inválidas antes
de publicar. IV-14 é o caso mais claro: transformou "representação completa",
que era julgamento, em condição verificável.

**Benchmark dirigindo design.** A separação entre inventário de tipos e
relações veio de uma medição — Q07 saltando 24% — e não de intuição.

**Fatias estreitas.** C04-b entrou com cinco kinds, dois Producers e onze
testes, e compilou na primeira rodada.

**Testes agnósticos à capacidade.** Todo teste que fixou o conjunto exato de
capacidades quebrou na capacidade seguinte. O último foi
`ArchitectureTests.Toda_inference_aponta_evidence_existente_e_regra`, que
afirmava que toda Inference é do C02. Verificar formato e propriedade, nunca
lista fechada.

**`Assert.All` sobre coleção vazia passa.** Um helper de teste com filtro
errado deu falso verde até a asserção seguinte falhar por outro motivo.
Todo `Assert.All` merece um `Assert.NotEmpty` antes.

**O que o modelo não promete, o teste não exige.** OB-04 ordena Observations
por id, que é hash de conteúdo. Um teste esperava ordem de arquivo. Quem
ordena para exibir é o Publisher.

**Identidade calculada em um lugar só.** Havia duas cópias do cálculo de
identidade de tipo e quase entrou a terceira. Duas implementações da mesma
identidade divergem em silêncio: o modelo passa a ter dois tipos onde há um.

---

## 7. Próximo passo

1. Aplicar a remoção do legado e a renomeação (`X7.Knowledge.slnx`).
2. Build, `dotnet test`, compilar a Base — conferir 5 projetos.
3. Gravar `benchmark/results-c04` como **linha de base nova**, não como
   comparação.
4. ADR de congelamento do `KNOWLEDGE_MODEL`.
5. C05 (Modelo Comportamental) em fatias: membros públicos primeiro,
   assinaturas depois. É a capacidade em que a compressão deve finalmente
   aparecer, porque o equivalente em código-fonte passa a ser corpo de método.
