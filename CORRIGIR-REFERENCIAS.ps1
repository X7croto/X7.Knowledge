# Corrige referencias ao nome antigo da solucao (ADR-027).
#
# O questions.json apontava X7.ProjectIndexer.slnx em codeFiles da Q01.
# Renomeada a solucao, o arquivo deixou de existir, T_code foi a zero e a Q01
# saiu do calculo da mediana sem que nada acusasse: a metrica caiu de 448 para
# 196 por milhar por perda de uma pergunta, nao por compressao.
#
# Tambem atualiza o marcador de versao do modelo em VERIFICAR-ESTADO.ps1, que
# ainda procurava 0.6.0.
#
# Sem acentos de proposito: PowerShell 5.1 le .ps1 como ANSI quando o arquivo
# nao tem BOM, e ai qualquer caractere fora de ASCII quebra o parser.

$ErrorActionPreference = 'Stop'

function Atualizar([string] $Caminho, [string] $De, [string] $Para) {
    if (-not (Test-Path $Caminho)) {
        Write-Warning "nao encontrado: $Caminho"
        return
    }

    $conteudo = Get-Content $Caminho -Raw -Encoding UTF8

    $ocorrencias = ([regex]::Matches($conteudo, [regex]::Escape($De))).Count

    if ($ocorrencias -eq 0) {
        Write-Host "  nada a fazer: $Caminho"
        return
    }

    $novo = $conteudo.Replace($De, $Para)

    # UTF-8 sem BOM: o questions.json e lido por System.Text.Json, e BOM em
    # arquivo reescrito por PowerShell 5 e causa classica de falha.
    $utf8SemBom = New-Object System.Text.UTF8Encoding $false

    [System.IO.File]::WriteAllText((Resolve-Path $Caminho), $novo, $utf8SemBom)

    Write-Host "  $Caminho - $ocorrencias ocorrencia(s)"
}

Write-Host 'Referencias ao nome antigo da solucao (ADR-027):'
Atualizar 'benchmark/questions.json' 'X7.ProjectIndexer.slnx' 'X7.Knowledge.slnx'

Write-Host 'Marcador de versao do modelo:'
Atualizar 'VERIFICAR-ESTADO.ps1' '0.6.0' '0.7.0'

Write-Host ''
Write-Host 'Confira antes de commitar: git diff benchmark/questions.json'
