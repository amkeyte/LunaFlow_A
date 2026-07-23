#!/usr/bin/env pwsh
<#
.SYNOPSIS
    .trx Triage Engine wrapper for PowerShell
.DESCRIPTION
    Runs the triage.py script with the latest.trx from the default test results directory.
    Automatically detects the repo root and latest.trx path.
.PARAMETER Output
    Output format: 'markdown' (default) or 'json'
.PARAMETER TrxPath
    Path to the .trx file. If omitted, uses tests/LunaFlow.EngineTests/TestResults/latest.trx
.EXAMPLE
    .\triage.ps1
    .\triage.ps1 -Output json
    .\triage.ps1 -TrxPath C:\path\to\results.trx
#>

param(
    [ValidateSet('markdown', 'json')]
    [string]$Output = 'markdown',

    [string]$TrxPath = $null
)

# Find repo root (contains CLAUDE.md)
$repoRoot = Get-Location
while ($repoRoot -ne [System.IO.Path]::GetPathRoot($repoRoot)) {
    if (Test-Path (Join-Path $repoRoot 'CLAUDE.md')) {
        break
    }
    $repoRoot = Split-Path $repoRoot
}

if (-not (Test-Path (Join-Path $repoRoot 'CLAUDE.md'))) {
    Write-Error "Could not find repo root (CLAUDE.md not found)"
    exit 1
}

# Resolve .trx path
if (-not $TrxPath) {
    $TrxPath = Join-Path $repoRoot 'tests\LunaFlow.EngineTests\TestResults\latest.trx'
}

if (-not (Test-Path $TrxPath)) {
    Write-Error "Test results file not found: $TrxPath`nRun the test suite first to generate results."
    exit 1
}

# Find Python (prefer 3.x)
$python = $null
foreach ($cmd in @('python3', 'python')) {
    try {
        $version = & $cmd --version 2>&1
        if ($version -match 'Python 3') {
            $python = $cmd
            break
        }
    }
    catch { }
}

if (-not $python) {
    Write-Error "Python 3 not found. Install Python 3.x and add it to PATH."
    exit 1
}

# Run triage
$triagePath = Split-Path -Parent $MyInvocation.MyCommand.Path
$triageScript = Join-Path $triagePath 'triage.py'

& $python $triageScript $TrxPath --output $Output
