# C01 — Aquisição da Estrutura Física

Implementação completa da capacidade C01, conforme `COMPILATION_PLAN.md` v2.0
e `KNOWLEDGE_MODEL.md` v0.

**35 arquivos de código + 5 de teste. 2.655 linhas.**

> **Não compilado.** Escrito sem SDK .NET disponível. A lógica foi validada por
> um espelho independente rodado contra a sua solução real (ver `PREVIEW-*`),
> mas a primeira compilação C# ainda não aconteceu. Espere erros na primeira
> passada, mais provavelmente em `EntityIndexProjector` e `MarkdownPublisher`.

---

## 1. O fluxo, em uma tela

```
X7_ProjectIndexer.slnx
        │
        ▼
  SolutionReader ──► SlnxReader / SlnReader        Acquisition/
        │            (leitura bruta, sem conhecimento ainda)
        ▼
  SolutionFile ──────────────────────────────────┐
        │                                         │
        ▼                                         ▼
  SolutionProducer                        ProjectProducer
  (solução, projetos,                     (lê cada .csproj:
   pastas, hierarquia)                     frameworks, output,
        │                                   propriedades, teste)
        └──────────────┬──────────────────────────┘
                       ▼
              KnowledgeModelBuilder            Model/
              (dedup por id de conteúdo,
               ordenação OB-04)
                       │
                       ▼
              EntityIndexProjector
              (deriva Solution/Project/SolutionFolder
               EXCLUSIVAMENTE das Observations → IV-02)
                       │
                       ▼
              ModelInvariants.Validate          IV-01..IV-08
              (falhou? lança, não publica nada)
                       │
        ┌──────────────┴──────────────┐
        ▼                             ▼
 KnowledgeModelPublisher       MarkdownPublisher    Publishing/
        │                             │
        ▼                             ▼
 model/knowledge.model.json    README.md
                               Structure/Solution.md
```

---

## 2. Mapa de arquivos

### `Model/` — o substrato

| Arquivo | Papel |
|---|---|
| `KnowledgeId.cs` | Identidade estável e legível. `sln:`, `proj:`, `slnfolder:`, `obs:` |
| `Observation.cs` | Unidade atômica. Id derivado de `sha256(kind+subject+payload)` |
| `ObservationPayload.cs` | Mapa ordenado + forma canônica usada no hash |
| `ObservationKinds.cs` | Catálogo fechado da v0. Kind fora dele **lança** (OB-02) |
| `Provenance.cs` | Origem, produtor, capacidade, nível. Obrigatória (PR-04) |
| `Manifest.cs` | Versões, nível, digest das entradas. Sem timestamp (D-03) |
| `KnowledgeModelBuilder.cs` | Acumula e deduplica. Só adiciona (PR-05) |
| `EntityIndexProjector.cs` | **Deriva as entidades das Observations.** É o que torna IV-02 estrutural em vez de disciplinar |
| `Entities/` | `Solution`, `Project`, `SolutionFolder`, `EntityIndex` |

### `Acquisition/` — leitura bruta

| Arquivo | Papel |
|---|---|
| `SolutionReader.cs` | Despacha por extensão |
| `SlnxReader.cs` | Formato XML. Reconstrói cadeia de pastas: `/src/Core/` cria `src` e `src/Core` |
| `SlnReader.cs` | Formato clássico. GUID de pasta, seção `NestedProjects`, proteção contra ciclo |
| `ProjectFileReader.cs` | `.csproj` como XML puro. Detecta teste por `IsTestProject` ou pacote |
| `PathNormalizer.cs` | Relativo à raiz, separador `/` (D-02) |
| `SolutionFile.cs` | Representação bruta. Ainda não é conhecimento |

### `Compilation/` — produção

| Arquivo | Papel |
|---|---|
| `Producers/SolutionProducer.cs` | Observa solução, projetos contidos, pastas e hierarquia |
| `Producers/ProjectProducer.cs` | Observa cada projeto lendo o `.csproj` |
| `KnowledgePipeline.cs` | Executa Producers em ordem declarada |
| `CompilationContext.cs` | Estado de um único ciclo |
| `InputDigest.cs` | Hash de caminho relativo + conteúdo. Nunca timestamp (D-07) |
| `ModelInvariants.cs` | IV-01..IV-08 **dentro da compilação** |

### `Serialization/` e `Publishing/`

| Arquivo | Papel |
|---|---|
| `CanonicalJson.cs` | Árvore de valores + serializador recursivo. Chaves ordenadas, LF, nulos omitidos (D-06) |
| `CanonicalFile.cs` | UTF-8 sem BOM, LF |
| `KnowledgeModelPublisher.cs` | Forma canônica JSON |
| `MarkdownPublisher.cs` | `README.md` e `Structure/Solution.md` |

### `X7.KnowledgeTests/`

| Arquivo | Cobre |
|---|---|
| `SolutionFixture.cs` | Solução de referência em disco temporário: pasta aninhada, projeto solto, multi-target, projeto de teste, propriedade não resolvida |
| `DeterminismTests.cs` | **Byte-a-byte**, BOM, CRLF, caminho absoluto, sensibilidade do digest |
| `ModelTests.cs` | Invariantes, proveniência, rastreabilidade IV-02, ordenação, hierarquia, dedup, catálogo |
| `CanonicalJsonTests.cs` | Ordenação de chaves, omissão de nulo, LF |

---

## 3. Instalação

O projeto `X7.Knowledge` atual não compila (chamadas para membros inexistentes:
`ScanAsync`, `PublishAsync`, `AddIdentity`, `ISource`). Esta fatia o substitui.

Apagar do projeto atual:

```
X7.Knowledge/Identity.cs
X7.Knowledge/IdentityId.cs
X7.Knowledge/KnowledgeModel.cs
X7.Knowledge/KnowledgeBuilder.cs
X7.Knowledge/KnowledgeEngine.cs
X7.Knowledge/KnowledgeSession.cs
X7.Knowledge/ProjectIndex.cs
X7.Knowledge/ProjectIndexItem.cs
X7.Knowledge/ProjectSummary.cs
X7.Knowledge/AssemblyInfo.cs
X7.Knowledge/Knowledge/       (pasta)
X7.Knowledge/Compilation/     (pasta)
X7.Knowledge/Publishing/      (pasta)
X7.Knowledge/Scanning/        (pasta)
X7.KnowledgeTests/*.cs
```

Copiar por cima o conteúdo de `X7.Knowledge/` e `X7.KnowledgeTests/` deste
pacote. Depois:

```
dotnet build
dotnet test
```

Uso:

```csharp
var model = await KnowledgeCompiler.CompileAsync(
    "X7_ProjectIndexer.slnx",
    "Knowledge");
```

---

## 4. Decisões tomadas nesta fatia

1. **`.csproj` lido como XML, sem MSBuild.** C01 é capacidade de nível X.
   Depender de SDK instalado a contrabandearia para nível S. Import,
   `Directory.Build.props` e `$(...)` não resolvido viram
   `acquisition.limitation` declarada — nunca ausência silenciosa.

2. **Payload textual.** Todo o catálogo C01 cabe em pares chave/valor. Payload
   tipado entra quando algum kind exigir (EX-01).

3. **JSON canônico escrito à mão.** O determinismo da saída não pode depender
   de política interna de biblioteca.

4. **Invariantes verificados na compilação**, não só em teste. Violação lança
   `InvariantViolationException` e nada é publicado. Base parcial inválida é
   pior que Base ausente.

5. **Saída apagada antes de republicar.** ADR-031: cada compilação substitui
   integralmente a anterior.

6. **Detecção de projeto de teste por evidência**, nunca por sufixo do nome.
   Deduzir por nome seria inferência semântica operando em nível X.

---

## 5. Validação já feita

Espelho independente da lógica, rodado contra `X7_ProjectIndexer.slnx`:

- 11 projetos, 2 pastas, **69 Observations**
- duas execuções → arquivo **byte-idêntico** (39.843 bytes)
- sem BOM, sem CR, sem caminho absoluto

Resultado em `PREVIEW-Solution.md` e `PREVIEW-knowledge.model.json`.

---

## 6. Pendente

- **CR de linha de base** (critério 5 do C01). Depende do conjunto de
  perguntas-tarefa da Constituição §7, que ainda não existe. Até lá, C01 está
  *implementada* mas não *concluída* pelo próprio plano.
- **`.sln` clássico**: implementado, nunca exercitado contra arquivo real.
