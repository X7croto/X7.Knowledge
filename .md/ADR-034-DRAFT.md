# ADR-034 — Verificação de MT-02 por comparação pareada

**Status:** PROPOSTA — requer sua aprovação
**Contexto:** Constituição §7.3, MT-02

---

## Contexto

MT-02 diz: *"Nenhuma capacidade pode aumentar a mediana de CR. Aumento é
regressão e bloqueia a conclusão."*

Aplicada literalmente, a regra é inválida quando a **cobertura muda**.

Medição após C01: 3 perguntas sustentadas de 15, mediana 839‰.
Medição após C02: 6 perguntas sustentadas de 15, mediana esperada acima disso.

As duas medianas são calculadas sobre **populações diferentes**. A segunda
inclui perguntas que antes eram falha (MT-03) e agora têm CR ruim. A mediana
subiu porque o conjunto mudou, não porque a Base piorou. Nenhuma das perguntas
sustentadas em C01 ficou pior.

Pela letra de MT-02, C02 estaria bloqueada. Pelo espírito — a Base responde
mais perguntas e nenhuma resposta anterior degradou — C02 é progresso.

## Decisão

MT-02 é verificada por **comparação pareada**:

1. Tomar as perguntas sustentadas em **ambas** as medições.
2. Comparar a mediana de CR sobre esse conjunto comum.
3. Regressão é aumento **nesse** número.
4. Cobertura é relatada como métrica independente e nunca pode diminuir.

## Consequências

- Uma capacidade que traz perguntas caras para dentro do cálculo não é punida
  por isso.
- Uma capacidade que degrada resposta já existente continua bloqueada.
- Duas métricas passam a ser publicadas em conjunto: mediana pareada e
  cobertura. Melhorar uma às custas da outra permanece detectável.
- `x7k-bench --baseline` implementa a verificação e devolve código 5 em caso
  de regressão.

## Alternativa rejeitada

Manter MT-02 sobre a mediana global. Rejeitada: incentiva não adicionar
perguntas difíceis ao conjunto, contrariando MT-04, e transforma ganho de
cobertura em falha aparente.
