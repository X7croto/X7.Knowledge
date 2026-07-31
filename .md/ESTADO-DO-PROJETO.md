# X7.Knowledge — estado do projeto

Documento de passagem. Serve para retomar o trabalho em outra conversa sem
reconstruir o contexto.

---

## 1. Documentos normativos — versões corretas

| Documento | Versão | Onde |
|---|---|---|
| `PROJECT_CONSTITUTION.md` | 2.0 | workspace |
| `COMPILATION_PLAN.md` | 2.0 | workspace |
| `KNOWLEDGE_MODEL.md` | **0.6** | substituir no workspace |
| `BENCHMARK.md` | conjunto v5 | `benchmark/` |
| `SECURITY-NOTES.md` | — | raiz da solução |
| `MIGRATION_v1_to_v2.md` | — | histórico |

**ADR-034 aprovada** (verificação de MT-02 por comparação pareada) — falta
registrar o texto na Constituição §8 e ajustar a redação de MT-02.

---

## 2. Capacidades

| Capacidade | Estado | Observação |
|---|---|---|
| C01 Estrutura Física | **concluída** | |
| C02 Arquitetural | **concluída** | |
| C03 Estrutura do Código | **concluída** | nível S alcançado |
| C04 Herança e implementação | **em fechamento** | fatia estreita; membros e genéricos ainda não |
| C05 em diante | não iniciadas | |

C04 cobre apenas `type.inherits` e `type.implements`. O restante do C04 do
plano — membros, assinaturas, modificadores, restrições genéricas — continua
pendente e provavelmente pertence a uma fatia própria.

---

## 3. Números atuais

```
Nível        S (semântico)
Projetos     13
Observations ~1650 (deve cair após a correção da base implícita)
Evidence     1
Inferences   21
Limitações   13 (todas de Directory.Build.props não resolvido)
```

**Benchmark:** 8 de 15 sustentadas, cobertura 53%.
Linha de base gravada: `benchmark/results-c03/`.
Comparação pareada C03→C04: 2013‰ → 2013‰, sem regressão.

---

## 4. Ambiente — armadilhas conhecidas

**O repositório está dentro do Google Drive.** Causou quatro incidentes:
travamento de exclusão (duas vezes), publicação bloqueada e reversão de
arquivo. O compilador foi endurecido contra isso — publica substituindo
conteúdo, nunca movendo a pasta — mas a origem do problema permanece.

**Feche o Visual Studio antes de aplicar qualquer zip.** A IDE reescreve
`.csproj` e arquivos abertos, desfazendo a cópia. Isso já causou três rodadas
perdidas de diagnóstico.

**Rode `VERIFICAR-ESTADO.ps1` depois de copiar e antes de buildar.** Ele
confere os marcadores no disco e diz exatamente o que não chegou.

**Publicar fora da pasta sincronizada funciona** e é opção legítima:
`-o C:\Temp\X7Knowledge`. A Base é função total da entrada e regenera em
segundos; o que precisa ser versionado é `benchmark/results-*`.

---

## 5. Pendências abertas

1. **Registrar ADR-034** na Constituição.
2. **Q07 subiu de 5410‰ para 5495‰** entre C03 e C04. Provável causa: o
   próprio `X7.Knowledge` ganhou arquivos, então a Base sobre ele cresceu.
   É o viés de auto-medição já declarado em `BENCHMARK.md` §1 — vale confirmar
   antes de fechar C04.
3. **Migrar a solução de referência** para um sistema de produção antes do
   C08. A partir de "Convenções", medir o compilador contra o próprio código
   de quem o escreveu distorce o resultado.
4. **Q10 e Q11 continuam sem sustentação.** Custam 4.716 e 3.256 tokens de
   código-fonte e são as que vão decidir se a tese do projeto se sustenta.
   A compressão real depende de C05 em diante.

---

## 6. Padrões que se provaram

**Invariantes na compilação, não só em teste.** Barraram três Bases inválidas
antes de publicar, incluindo uma com caminho absoluto e outra com referência a
tipo inexistente.

**Benchmark dirigindo design.** A separação entre inventário de tipos e
relações veio de uma medição — Q07 saltando 24% — e não de intuição.

**Fatias estreitas.** C04 entrou com dois kinds e um Producer. As entregas
grandes custaram várias rodadas de diagnóstico; as pequenas, uma.

**Testes agnósticos à capacidade.** Todo teste que fixou o conjunto exato de
capacidades quebrou na capacidade seguinte. Verificar formato e propriedade,
nunca lista fechada.

---

## 7. Próximo passo sugerido

Fechar C04: build, testes, publicar, benchmark com `--baseline`, gravar
`results-c04`, commitar.

Depois, C05 (Modelo Comportamental) em fatias: membros públicos primeiro,
assinaturas depois. É a capacidade em que a compressão deve finalmente
aparecer, porque o equivalente em código-fonte passa a ser corpo de método.
