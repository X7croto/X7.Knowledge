# BENCHMARK.md

**Projeto:** X7.Knowledge
**Versão do conjunto:** 2
**Status:** Normativo por referência — implementa `PROJECT_CONSTITUTION.md` §7
**Solução de referência:** `X7.ProjectIndexer.slnx` (12 projetos, nível X)

---

## 1. Por que esta solução de referência

Escolhida por disponibilidade e conhecimento: está versionada, é C#, tem
hierarquia de pastas, multi-projeto, projetos de teste e código legado real.

**Viés declarado.** Medir o compilador contra a própria solução que o hospeda
favorece o compilador: as convenções que ele vai inferir são as convenções de
quem o escreveu. Enquanto o conhecimento é puramente estrutural (C01–C07) o
viés é pequeno, porque estrutura física não admite interpretação. A partir de
**C08 (Convenções)** o viés passa a distorcer, e a solução de referência deve
migrar para um sistema de produção não relacionado ao compilador.

Isso não invalida a linha de base: pela MT-04 o conjunto de perguntas só
cresce, então perguntas sobre a nova referência somam-se às atuais em vez de
substituí-las.

---

## 2. Regras do conjunto

- **BM-01** Toda pergunta é uma tarefa real de desenvolvimento, não uma
  consulta a fato isolado. "Quantos projetos existem?" não é pergunta-tarefa.
- **BM-02** Toda pergunta declara `codeFiles`: os arquivos que um desenvolvedor
  precisaria abrir para responder **sem** a Base. Essa lista é julgamento
  humano, feita uma vez e versionada.
- **BM-03** Toda pergunta declara `kbFiles`: o subconjunto da Base suficiente
  para responder. Vazio significa que a Base não sustenta a resposta.
- **BM-04** Pergunta que a Base não sustenta conta como **falha** (MT-03),
  nunca como CR baixo. Falha não entra no cálculo da mediana; entra na taxa
  de cobertura.
- **BM-05** O conjunto nunca encolhe (MT-04). Pergunta obsoleta é marcada
  `retired`, não apagada, e para de contar.
- **BM-06** `codeFiles` de uma pergunta só muda se a solução de referência
  mudar. Ajustar a lista para melhorar o número é fraude de medição.
- **BM-07** Duas medições só são comparáveis se a **solução de referência for
  a mesma**. Mudou a solução — projeto adicionado, removido ou renomeado — a
  linha de base é refeita, e a medição anterior deixa de ser termo de
  comparação. `results.json` registra `solutionDigest` e `projectCount` para
  que a incomparabilidade seja detectável, e não uma conclusão errada.
- **BM-08** MT-02 se aplica a **capacidades**, não a mudanças da solução de
  referência. Aumento de CR causado por crescimento da solução não bloqueia
  conclusão de capacidade; aumento causado por capacidade nova, sim.
- **BM-09** A verificação de MT-02 é feita por **comparação pareada** sobre as
  perguntas sustentadas em ambas as medições. Comparar medianas de populações
  diferentes não mede nada.
- **BM-10** `codeFiles` que pressupõe a resposta é defeito, não escolha.
  Listar só os projetos que participam da resposta a uma pergunta do tipo
  "quem depende de X" exige já saber a resposta. A correção desse tipo de erro
  é legítima, deve ser registrada em `codeFilesNote` e incrementa a versão do
  conjunto — distinguindo-a de ajuste para melhorar número, vedado por BM-06.

---

## 3. Medida

Para cada pergunta com resposta sustentada:

```
T_code = tokens(codeFiles)
T_kb   = tokens(kbFiles)
CR     = T_kb / T_code
```

Métrica do projeto: **mediana de CR** sobre as perguntas sustentadas.
Métrica secundária: **cobertura** = sustentadas / total.

Ambas são registradas. Uma capacidade pode melhorar a cobertura sem melhorar a
mediana, e isso é progresso legítimo — mas os dois números são publicados
sempre juntos, porque melhorar um às custas do outro é fácil e enganoso.

### Contagem de tokens

Aproximação declarada e determinística, aplicada identicamente aos dois lados
da razão:

```
tokens(arquivo) = quantidade de sequências separadas por espaço em branco
                + quantidade de caracteres de pontuação isolados
```

**Justificativa.** CR é uma razão. Um contador aproximado, aplicado ao
numerador e ao denominador com a mesma regra, preserva a razão dentro de uma
margem estreita. O que importa é a comparabilidade entre medições, não a
fidelidade ao tokenizador de um modelo específico.

**Limitação declarada.** Código e Markdown não tokenizam na mesma proporção,
então o CR absoluto tem viés. A comparação válida é **entre versões da Base
sobre o mesmo conjunto de perguntas**, nunca entre projetos diferentes.

---

## 4. Conjunto de perguntas — versão 1

Estado esperado em C01: quase tudo `unsupported`. É o zero honesto contra o
qual C02–C12 provam ganho.

| ID | Pergunta-tarefa | Capacidade que deve sustentar |
|---|---|---|
| Q01 | Quais projetos compõem a solução e como estão organizados? | C01 |
| Q02 | Onde fica o projeto que contém a lógica de aquisição de solução? | C01 |
| Q03 | Quais projetos são de teste e o que os identifica? | C01 |
| Q04 | Quais projetos dependem de `X7.ProjectIndexer.Core`? | C02 |
| Q05 | Se eu mudar `X7.Knowledge`, que projetos são impactados? | C02 |
| Q06 | Existe ciclo de dependência entre projetos? | C02 |
| Q07 | Onde está definido o tipo que representa uma unidade de conhecimento? | C03 |
| Q08 | Que tipos implementam `IProducer` e onde estão? | C04 |
| Q09 | Qual é a superfície pública de `KnowledgeModelBuilder`? | C05 |
| Q10 | Quem consome `Observation` e de que forma? | C06 |
| Q11 | O que acontece, do começo ao fim, quando uma compilação é executada? | C07 |
| Q12 | Que convenção devo seguir para criar um novo Producer? | C08 |
| Q13 | Que padrão a solução usa para separar produção de publicação? | C09 |
| Q14 | O que significa "Observation" no vocabulário deste projeto? | C10 |
| Q15 | Que regras impedem a publicação de um modelo inválido? | C11 |

O arquivo `questions.json` contém a forma executável, com `codeFiles` e
`kbFiles` por pergunta.

---

## 5. Procedimento

```
dotnet run --project X7.Knowledge.Benchmark -- \
    --solution X7.ProjectIndexer.slnx \
    --questions benchmark/questions.json \
    --knowledge Knowledge \
    --output benchmark/results
```

Produz:

```
benchmark/results/
  results.json      forma canônica, versionável
  REPORT.md         legível, CR por pergunta + mediana + cobertura
```

`results.json` obedece às mesmas regras de canonicidade do KnowledgeModel:
UTF-8 sem BOM, LF, chaves ordenadas, sem timestamp. É versionado no
repositório e comparado entre capacidades.

---

## 6. Critério de não regressão

Antes de concluir qualquer capacidade:

1. Verificar que `solutionDigest` das duas medições é o mesmo (BM-07). Se não
   for, refazer a linha de base antes de comparar.
2. Rodar o benchmark contra a Base anterior → `mediana_antes`, `cobertura_antes`
3. Rodar contra a Base nova → `mediana_depois`, `cobertura_depois`
4. **Bloqueia a conclusão** se `mediana_depois > mediana_antes` (MT-02)
5. Registrar ambos no fechamento da capacidade

### Comparação pareada (obrigatória)

Medianas de conjuntos diferentes **não são comparáveis**. Quando a cobertura
muda — perguntas novas passam a ser sustentadas — a mediana antes e a mediana
depois são calculadas sobre populações distintas, e a diferença entre elas não
mede melhora nem piora.

O procedimento válido:

1. Tomar as perguntas sustentadas **nas duas** medições.
2. Comparar a mediana de CR apenas sobre esse conjunto comum.
3. Relatar a cobertura à parte, como métrica independente.

Uma capacidade pode, legitimamente, aumentar a mediana global ao trazer para
dentro do cálculo perguntas de CR ruim que antes eram falha. Isso é ganho de
cobertura, não regressão — e a comparação pareada é o que separa os dois casos.

`x7k-bench --baseline <results.json>` faz essa comparação.

### Limitação conhecida da medição

`T_kb` é contado por **arquivo inteiro**. Enquanto uma projeção for monolítica
— um `Solution.md` para a solução toda — o CR de perguntas sobre um subconjunto
cresce proporcionalmente ao tamanho da solução, mesmo sem nenhuma piora da
Base.

Isso não é ruído de medição: é sinal de design. Ou a publicação ganha
granularidade, ou o consumidor precisa de um mecanismo de seleção — que está
fora do escopo do compilador (ADR-030). A decisão pertence a C12; registrada
aqui para não ser confundida com regressão.

---
*Fim de `BENCHMARK.md` v1.*
