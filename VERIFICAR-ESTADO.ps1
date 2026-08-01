# Verifica o que está de fato no disco. Sem adivinhação.
# Rodar na raiz da solução.

$ok = $true

function Check($descricao, $arquivo, $padrao) {
    if (-not (Test-Path $arquivo)) {
        Write-Host "FALTA  $descricao -> arquivo inexistente: $arquivo" -ForegroundColor Red
        $script:ok = $false
        return
    }
    if (Select-String -Path $arquivo -Pattern $padrao -Quiet) {
        Write-Host "ok     $descricao" -ForegroundColor Green
    } else {
        Write-Host "FALTA  $descricao -> em $arquivo" -ForegroundColor Red
        $script:ok = $false
    }
}

Write-Host "=== Estado dos arquivos ===" -ForegroundColor Cyan

Check "pacote de linguagem C#"      "X7.Knowledge\X7.Knowledge.csproj"                        "CSharp.Workspaces"
Check "using System.Reflection"     "X7.Knowledge\Acquisition\Roslyn\CompilationProvider.cs"  "using System.Reflection"
Check "saneamento de caminho"       "X7.Knowledge\Acquisition\PathNormalizer.cs"              "Sanitize"
Check "versao do MSBuild no manifesto" "X7.Knowledge\Model\Manifest.cs"                       "MsBuildVersion"
Check "modelo 0.7.0"                "X7.Knowledge\KnowledgeCompiler.cs"                       'ModelVersion = "0.7.0"'
Check "produtor de relacoes C04"    "X7.Knowledge\Compilation\Producers\TypeRelationProducer.cs" "TypeImplements"
Check "motivo da queda de nivel"    "X7.Knowledge.Cli\Program.cs"                             "Modelo semantico indisponivel|Modelo sem.ntico indispon.vel"
Check "supressoes de auditoria"     "Directory.Build.props"                                   "NuGetAuditSuppress"
Check "publicacao sem mover pasta"  "X7.Knowledge\KnowledgeCompiler.cs"                       "RemoveEmptyDirectories"

Write-Host ""
if ($ok) {
    Write-Host "Tudo aplicado. Pode buildar." -ForegroundColor Green
} else {
    Write-Host "Ha arquivos nao aplicados. Copie-os do zip antes de buildar." -ForegroundColor Yellow
}
