# Como rodar

## 1. Adicionar o CLI

Copie a pasta `X7.Knowledge.Cli/` para a raiz da solução (ao lado de
`X7.Knowledge/`) e substitua `X7.Knowledge/KnowledgeCompiler.cs` pela versão
deste pacote — ela traz uma trava contra apagar diretório errado.

Registre o projeto na solução:

```
dotnet sln X7_ProjectIndexer.slnx add X7.Knowledge.Cli\X7.Knowledge.Cli.csproj
```

Ou edite o `.slnx` à mão, dentro de `<Folder Name="/src/">`:

```xml
<Project Path="X7.Knowledge.Cli/X7.Knowledge.Cli.csproj" />
```

## 2. Compilar

```
dotnet build
```

## 3. Rodar

Do diretório da solução:

```
dotnet run --project X7.Knowledge.Cli -- X7_ProjectIndexer.slnx -o Knowledge
```

O `--` separa os argumentos do `dotnet run` dos argumentos do programa. Sem
ele, o `dotnet` engole o `-o` e você recebe um erro confuso.

Sem argumentos, ele procura uma única solução no diretório atual:

```
dotnet run --project X7.Knowledge.Cli
```

## 4. Saída esperada

```
Solução      X7_ProjectIndexer
Nível        X (sintático)
Capacidades  C01
Projetos     11
Pastas       2
Observations 69
Digest       a3f1c07e9b24d5ef…

Base publicada em C:\...\Knowledge
Tempo 41 ms
```

E em disco:

```
Knowledge/
  README.md
  Structure/Solution.md
  model/knowledge.model.json
```

## 5. Instalar como comando `x7k` (opcional)

Para rodar de qualquer lugar, sem `dotnet run`:

```
dotnet publish X7.Knowledge.Cli -c Release -o publish
```

O executável fica em `publish\x7k.exe`. Adicione a pasta ao `PATH` e use:

```
x7k C:\caminho\MinhaSolucao.slnx -o C:\caminho\Knowledge
```

## 6. Verificação de determinismo à mão

No PowerShell:

```powershell
dotnet run --project X7.Knowledge.Cli -- X7_ProjectIndexer.slnx -o Base1
dotnet run --project X7.Knowledge.Cli -- X7_ProjectIndexer.slnx -o Base2

$a = Get-FileHash Base1\model\knowledge.model.json -Algorithm SHA256
$b = Get-FileHash Base2\model\knowledge.model.json -Algorithm SHA256

if ($a.Hash -eq $b.Hash) { "IDÊNTICO" } else { "DIVERGENTE" }
```

No cmd:

```
certutil -hashfile Base1\model\knowledge.model.json SHA256
certutil -hashfile Base2\model\knowledge.model.json SHA256
```

## 7. Códigos de retorno

| Código | Significado |
|---|---|
| 0 | Compilação concluída |
| 1 | Erro de uso ou entrada inválida |
| 2 | Invariante do modelo violado — nada foi publicado |

## Nota sobre o diretório de saída

Cada compilação substitui integralmente a anterior (ADR-031). Para evitar
perda de dados por argumento errado, o compilador **recusa apagar** um
diretório que exista, não esteja vazio e não contenha
`model/knowledge.model.json`. Aponte `-o` para uma pasta nova ou para uma Base
já publicada.
