# Notas de segurança — supressões de auditoria

Registro das supressões declaradas em `Directory.Build.props`, com a análise
que as justifica e as condições que obrigam a revisá-las.

## Avisos suprimidos

Dez avisos, todos oriundos da árvore de dependências do MSBuild.

| Aviso | Pacote |
|---|---|
| `GHSA-w3q9-fxm7-j8fq` | `Microsoft.Build`, `Build.Tasks.Core`, `Build.Utilities.Core` |
| `GHSA-h4j7-5rxr-p4wc` | `Microsoft.Build.Tasks.Core` |
| `GHSA-23rf-6693-g89p` | `System.Security.Cryptography.Xml` |
| `GHSA-37gx-xxp4-5rgx` | `System.Security.Cryptography.Xml` |
| `GHSA-6588-8gv4-xfgh` | `System.Security.Cryptography.Xml` |
| `GHSA-8q5v-6pqq-x66h` | `System.Security.Cryptography.Xml` |
| `GHSA-cvvh-rhrc-wg4q` | `System.Security.Cryptography.Xml` |
| `GHSA-g8r8-53c2-pm3f` | `System.Security.Cryptography.Xml` |
| `GHSA-mmjf-rqrv-855v` | `System.Security.Cryptography.Xml` |
| `GHSA-w3x6-4m5h-cxqf` | `System.Security.Cryptography.Xml` |

## Por que não foram corrigidos por atualização

Foi tentado. Fixar a linha `17.14.8` não resolveu: essa versão também é
afetada por `GHSA-w3q9-fxm7-j8fq`.

O motivo é estrutural. Os pacotes `Microsoft.Build.*` no NuGet existem para
autores de ferramentas que precisam compilar contra a API do MSBuild. A
correção oficial destes avisos, segundo o próprio boletim da Microsoft, é
**atualizar o SDK .NET** — não o pacote. Os pacotes acompanham o SDK com
atraso e, na prática, estão quase sempre sinalizados.

Perseguir versão corrigida seria trabalho recorrente sem ganho de segurança.

## Por que o risco é aceitável aqui

O `MSBuildLocator` carrega o MSBuild **do SDK instalado na máquina**, em tempo
de execução. Os assemblies vindos do NuGet servem para compilar e não são os
que executam. Quem mantém o SDK atualizado está executando código corrigido,
independentemente da versão do pacote.

Para `CVE-2025-26646` há um fator adicional: o boletim declara que projetos
que não utilizam a task `DownloadFile` não são suscetíveis. O X7.Knowledge não
a utiliza.

## O que **não** foi feito

- Nenhum `NoWarn`.
- Nenhum `WarningsNotAsErrors`.
- Nenhum `NuGetAudit=false`.
- Nenhum `NuGetAuditLevel` rebaixado.

`TreatWarningsAsErrors` permanece ligado e a auditoria continua ativa para
todo o resto. Um aviso novo, em qualquer outro pacote, volta a quebrar o
build — que é o comportamento desejado.

### Tentativas de correção por atualização

Duas, ambas sem sucesso, registradas para não serem repetidas:

1. `Microsoft.Build.*` fixado em `17.14.8` — versão também afetada por
   `GHSA-w3q9-fxm7-j8fq`.
2. `System.Security.Cryptography.Xml` fixado em `10.0.0` — versão também
   afetada pelos oito avisos.

Não existe, hoje, combinação de versões destes pacotes livre de aviso.

## Condições que obrigam a revisar

1. **Se o compilador passar a rodar sobre repositório não confiável.** O
   X7.Knowledge abre e avalia arquivos de projeto de terceiros. Um `.csproj`
   hostil pode acionar tasks durante a avaliação do MSBuild. Hoje ele roda
   sobre a própria solução; se virar serviço, ou processar repositório
   arbitrário, a análise acima deixa de valer.
2. **Se surgir versão do `Microsoft.CodeAnalysis.Workspaces.MSBuild` sem
   dependência sinalizada.** Nesse caso, remover as supressões.
3. **Se o `MSBuildWorkspace` for abandonado.** A alternativa é construir a
   `CSharpCompilation` a partir das árvores sintáticas e das referências do
   `project.assets.json`, o que elimina toda a árvore do MSBuild.

   **Avaliada e rejeitada por ora.** O argumento a favor seria determinismo
   (PR-02): o workspace depende do SDK instalado. Mas o `project.assets.json`
   é produzido pelo `restore`, que também depende do SDK e do NuGet — a
   alternativa troca uma dependência de ambiente por outra, sem resolver o
   problema, ao custo de resolver referências, multi-targeting e frameworks
   manualmente.

   Reavaliar apenas se a lista de supressões crescer a ponto de deixar de ser
   auditável, ou se a condição 1 acima passar a valer.

## Verificação periódica

```
dotnet list package --vulnerable --include-transitive
```

Lista tudo, inclusive o que está suprimido no build. Vale rodar antes de cada
capacidade concluída.
