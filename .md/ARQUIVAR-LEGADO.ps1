<#
    Arquiva as oito pastas do v1 revogado em .md/legacy/.

    OPCIONAL. A Base já ignora esses projetos assim que eles saem do .slnx —
    o compilador lê a lista de projetos da solução, não do disco. Este script
    é só arrumação da árvore de trabalho.

    Rode com o Visual Studio fechado. Se o repositório estiver dentro do
    Google Drive, pause a sincronização antes: mover pasta que o cliente de
    sync observa é a operação que já travou quatro vezes neste projeto.
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $Destino = '.md/legacy'
)

$ErrorActionPreference = 'Stop'

$legado = @(
    'X7.ProjectIndexer.CLI'
    'X7.ProjectIndexer.Core'
    'X7.ProjectIndexer.CSharp'
    'X7.ProjectIndexer.Graph'
    'X7.ProjectIndexer.Knowledge'
    'X7.ProjectIndexer.Markdown'
    'X7.ProjectIndexer.Output'
    'X7.ProjectIndexer.Tests'
)

if (Test-Path 'X7.ProjectIndexer.slnx') {
    Write-Warning 'X7.ProjectIndexer.slnx ainda existe. Renomeie ou remova antes: duas soluções na raiz fazem o CLI sem argumento não saber qual abrir.'
}

$git = (Test-Path '.git') -and (Get-Command git -ErrorAction SilentlyContinue)

if (-not (Test-Path $Destino)) {
    New-Item -ItemType Directory -Path $Destino -Force | Out-Null
}

foreach ($pasta in $legado) {
    if (-not (Test-Path $pasta)) {
        Write-Host "  já ausente: $pasta"
        continue
    }

    $alvo = Join-Path $Destino $pasta

    if ($PSCmdlet.ShouldProcess($pasta, "mover para $alvo")) {
        if ($git) {
            # git mv preserva o rastro no histórico.
            git mv -- $pasta $alvo
        }
        else {
            Move-Item -Path $pasta -Destination $alvo
        }

        Write-Host "  arquivado: $pasta"
    }
}

Write-Host ''
Write-Host 'Nada foi apagado. As pastas estão em ' -NoNewline
Write-Host $Destino -ForegroundColor Cyan
