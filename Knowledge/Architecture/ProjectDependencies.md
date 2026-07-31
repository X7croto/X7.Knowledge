# Dependências entre projetos

## Referências declaradas

| Projeto | Referencia |
|---|---|
| X7.Knowledge.Benchmark | X7.Knowledge |
| X7.Knowledge.Cli | X7.Knowledge |
| X7.Knowledge.Specifications | X7.Knowledge |
| X7.KnowledgeTests | X7.Knowledge |
| X7.ProjectIndexer.CLI | X7.ProjectIndexer.CSharp, X7.ProjectIndexer.Core, X7.ProjectIndexer.Graph, X7.ProjectIndexer.Knowledge, X7.ProjectIndexer.Markdown, X7.ProjectIndexer.Output |
| X7.ProjectIndexer.CSharp | X7.ProjectIndexer.Core |
| X7.ProjectIndexer.Graph | X7.ProjectIndexer.Core |
| X7.ProjectIndexer.Knowledge | X7.ProjectIndexer.Core |
| X7.ProjectIndexer.Markdown | X7.ProjectIndexer.Core |
| X7.ProjectIndexer.Output | X7.ProjectIndexer.Core |
| X7.ProjectIndexer.Tests | X7.ProjectIndexer.CSharp, X7.ProjectIndexer.Core |

## Quem depende de quem

| Projeto | É referenciado por |
|---|---|
| X7.Knowledge.Benchmark | — |
| X7.Knowledge.Cli | — |
| X7.Knowledge.Specifications | — |
| X7.Knowledge | X7.Knowledge.Benchmark, X7.Knowledge.Cli, X7.Knowledge.Specifications, X7.KnowledgeTests |
| X7.KnowledgeTests | — |
| X7.ProjectIndexer.CLI | — |
| X7.ProjectIndexer.CSharp | X7.ProjectIndexer.CLI, X7.ProjectIndexer.Tests |
| X7.ProjectIndexer.Core | X7.ProjectIndexer.CLI, X7.ProjectIndexer.CSharp, X7.ProjectIndexer.Graph, X7.ProjectIndexer.Knowledge, X7.ProjectIndexer.Markdown, X7.ProjectIndexer.Output, X7.ProjectIndexer.Tests |
| X7.ProjectIndexer.Graph | X7.ProjectIndexer.CLI |
| X7.ProjectIndexer.Knowledge | X7.ProjectIndexer.CLI |
| X7.ProjectIndexer.Markdown | X7.ProjectIndexer.CLI |
| X7.ProjectIndexer.Output | X7.ProjectIndexer.CLI |
| X7.ProjectIndexer.Tests | — |

## Pacotes externos declarados

Identidade e versão apenas; conteúdo não é resolvido.

| Projeto | Pacote | Versão |
|---|---|---|
| X7.Knowledge.Specifications | Microsoft.NET.Test.Sdk | 17.14.1 |
| X7.Knowledge.Specifications | coverlet.collector | 6.0.4 |
| X7.Knowledge.Specifications | xunit | 2.9.3 |
| X7.Knowledge.Specifications | xunit.runner.visualstudio | 3.1.4 |
| X7.Knowledge | Microsoft.Build.Locator | 1.9.1 |
| X7.Knowledge | Microsoft.CodeAnalysis.CSharp | 5.3.0 |
| X7.Knowledge | Microsoft.CodeAnalysis.Workspaces.MSBuild | 5.3.0 |
| X7.KnowledgeTests | Microsoft.NET.Test.Sdk | 17.14.1 |
| X7.KnowledgeTests | xunit | 2.9.3 |
| X7.KnowledgeTests | xunit.runner.visualstudio | 3.1.0 |
| X7.ProjectIndexer.CLI | Spectre.Console | 0.57.1 |
| X7.ProjectIndexer.CLI | System.CommandLine | 3.0.0-preview.5.26302.115 |
| X7.ProjectIndexer.CSharp | Microsoft.CodeAnalysis.CSharp | 5.3.0 |
| X7.ProjectIndexer.CSharp | Microsoft.CodeAnalysis.Workspaces.Common | 5.3.0 |
| X7.ProjectIndexer.Core | Microsoft.CodeAnalysis.CSharp | 5.3.0 |
| X7.ProjectIndexer.Markdown | Markdig | 1.3.2 |
| X7.ProjectIndexer.Tests | FluentAssertions | 8.10.0 |
| X7.ProjectIndexer.Tests | Microsoft.NET.Test.Sdk | 17.14.1 |
| X7.ProjectIndexer.Tests | NSubstitute | 5.3.0 |
| X7.ProjectIndexer.Tests | coverlet.collector | 6.0.4 |
| X7.ProjectIndexer.Tests | xunit | 2.9.3 |
| X7.ProjectIndexer.Tests | xunit.runner.visualstudio | 3.1.4 |
