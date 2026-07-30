# Context Ratio — resultado

Conjunto v1 · referência `X7.ProjectIndexer.slnx`

| Métrica | Valor |
|---|---|
| Perguntas | 15 |
| Sustentadas pela Base | 3 |
| Cobertura | 20% |
| **Mediana de CR** | **780‰** |

## Por pergunta

| ID | Capacidade | Sustentada | T_code | T_kb | CR |
|---|---|---|---|---|---|
| Q01 | C01 | sim | 352 | 475 | 1349‰ |
| Q02 | C01 | sim | 3203 | 475 | 148‰ |
| Q03 | C01 | sim | 609 | 475 | 780‰ |
| Q04 | C02 | **não** | 503 | — | — |
| Q05 | C02 | **não** | 295 | — | — |
| Q06 | C02 | **não** | 301 | — | — |
| Q07 | C03 | **não** | 376 | — | — |
| Q08 | C04 | **não** | 1239 | — | — |
| Q09 | C05 | **não** | 376 | — | — |
| Q10 | C06 | **não** | 4716 | — | — |
| Q11 | C07 | **não** | 3256 | — | — |
| Q12 | C08 | **não** | 1332 | — | — |
| Q13 | C09 | **não** | 727 | — | — |
| Q14 | C10 | **não** | 1068 | — | — |
| Q15 | C11 | **não** | 1341 | — | — |

## Não sustentadas

Contam como falha (MT-03), não como CR baixo. Cada uma indica a capacidade que precisa existir.

- **Q04** (C02) — Quais projetos dependem de X7.ProjectIndexer.Core?
- **Q05** (C02) — Se eu mudar X7.Knowledge, que projetos são impactados?
- **Q06** (C02) — Existe ciclo de dependência entre projetos?
- **Q07** (C03) — Onde está definido o tipo que representa uma unidade de conhecimento?
- **Q08** (C04) — Que tipos implementam IProducer e onde estão?
- **Q09** (C05) — Qual é a superfície pública de KnowledgeModelBuilder?
- **Q10** (C06) — Quem consome Observation e de que forma?
- **Q11** (C07) — O que acontece, do começo ao fim, quando uma compilação é executada?
- **Q12** (C08) — Que convenção devo seguir para criar um novo Producer?
- **Q13** (C09) — Que padrão a solução usa para separar produção de publicação?
- **Q14** (C10) — O que significa 'Observation' no vocabulário deste projeto?
- **Q15** (C11) — Que regras impedem a publicação de um modelo inválido?
