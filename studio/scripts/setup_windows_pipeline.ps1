<#
.SYNOPSIS
  Verifica e instala a pilha gratuita (US$ 0) de produção de PARALLAX (Unity 6 LTS) no Windows.
.DESCRIPTION
  Audita as ferramentas no PATH e o Unity 6000.3 LTS com o módulo Windows IL2CPP. Com -Install, instala via
  winget o que falta e, pelo Unity CLI, o editor LTS. Com -SyncMcp, zera o mcp_config.json global do Antigravity
  (os MCPs ficam em .agents/plugins/daedalus-parallax/mcp_config.json).
.EXAMPLE
  .\studio\scripts\setup_windows_pipeline.ps1
  .\studio\scripts\setup_windows_pipeline.ps1 -Install
  .\studio\scripts\setup_windows_pipeline.ps1 -SyncMcp
#>
param(
    [switch]$Install,
    [switch]$SyncMcp
)

$ErrorActionPreference = "Continue"
$UnityVersion = "6000.3.29f1"
$UnityProject = Join-Path $PSScriptRoot "..\..\client\parallax-unity" | Resolve-Path -ErrorAction SilentlyContinue

$tools = @(
    @{ Name = "Unity CLI";       Cmd = "unity";          WingetId = "Unity.UnityHub";                RequiredFrom = "Motor (Unity 6 LTS + MCP)" },
    @{ Name = "Git LFS";         Cmd = "git-lfs";        WingetId = "GitHub.GitLFS";                 RequiredFrom = "Binários de arte e áudio" },
    @{ Name = "Python 3";        Cmd = "python";         WingetId = "Python.Python.3.12";            RequiredFrom = "Validador e sync do Daedalus" },
    @{ Name = "uv / uvx";        Cmd = "uvx";            WingetId = "astral-sh.uv";                  RequiredFrom = "Blender MCP e scripts" },
    @{ Name = "Node.js";         Cmd = "node";           WingetId = "OpenJS.NodeJS.LTS";             RequiredFrom = "MCP Filesystem" },
    @{ Name = "Blender 5.x";     Cmd = "blender";        WingetId = "BlenderFoundation.Blender";     RequiredFrom = "Modelagem, rigs e kits" },
    @{ Name = "Blockbench";      Cmd = "blockbench";     WingetId = "JannisX11.Blockbench";          RequiredFrom = "Auxiliares e armas" },
    @{ Name = "Krita";           Cmd = "krita";          WingetId = "KDE.Krita";                     RequiredFrom = "Paintover humano" },
    @{ Name = "Inkscape";        Cmd = "inkscape";       WingetId = "Inkscape.Inkscape";             RequiredFrom = "UI vetorial" },
    @{ Name = "Material Maker";  Cmd = "material_maker"; WingetId = "RodZill4.MaterialMaker";        RequiredFrom = "Texturas PBR" },
    @{ Name = "Audacity";        Cmd = "audacity";       WingetId = "Audacity.Audacity";             RequiredFrom = "Rádio e áudio" },
    @{ Name = "Obsidian";        Cmd = "obsidian";       WingetId = "Obsidian.Obsidian";             RequiredFrom = "Vault Project Daedalus" }
)

$installDirs = @{
    "blender"  = "C:\Program Files\Blender Foundation"
    "krita"    = "C:\Program Files\Krita (x64)"
    "inkscape" = "C:\Program Files\Inkscape"
    "audacity" = "C:\Program Files\Audacity 4"
    "blockbench" = "$env:LOCALAPPDATA\Programs\Blockbench"
    "obsidian" = "$env:LOCALAPPDATA\Programs\Obsidian"
}

Write-Host "`n=== PARALLAX // AUDITORIA DO PIPELINE WINDOWS (UNITY 6 LTS) ===" -ForegroundColor Cyan
foreach ($t in $tools) {
    $found = Get-Command $t.Cmd -ErrorAction SilentlyContinue
    $dir = $installDirs[$t.Cmd]
    if ($found) {
        Write-Host ("[OK]    {0,-16} -> {1}" -f $t.Name, $found.Source) -ForegroundColor Green
    } elseif ($dir -and (Test-Path $dir)) {
        Write-Host ("[OK]    {0,-16} -> {1} (fora do PATH)" -f $t.Name, $dir) -ForegroundColor Green
    } else {
        Write-Host ("[FALTA] {0,-16} ({1}) | winget id: {2}" -f $t.Name, $t.RequiredFrom, $t.WingetId) -ForegroundColor Yellow
        if ($Install) {
            winget install --id $t.WingetId -e --accept-package-agreements --accept-source-agreements
        }
    }
}

$editor = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$il2cpp = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_player_nondevelopment_il2cpp"
if (Test-Path $editor) {
    Write-Host ("[OK]    Unity {0}" -f $UnityVersion) -ForegroundColor Green
} else {
    Write-Host ("[FALTA] Unity {0} LTS" -f $UnityVersion) -ForegroundColor Yellow
    if ($Install) { unity install $UnityVersion -m windows-il2cpp --accept-eula -y }
}
if ((Test-Path $editor) -and -not (Test-Path $il2cpp)) {
    Write-Host "[FALTA] Módulo Windows Build Support (IL2CPP)" -ForegroundColor Yellow
    if ($Install) { unity install-modules --editor-version $UnityVersion -m windows-il2cpp --accept-eula -y }
}

if ($SyncMcp) {
    $dstDir = Join-Path $env:USERPROFILE ".gemini\config"
    $dstMcp = Join-Path $dstDir "mcp_config.json"
    New-Item -ItemType Directory -Force -Path $dstDir | Out-Null
    '{"mcpServers": {}}' | Set-Content -Path $dstMcp -Encoding UTF8
    Write-Host "`n[OK] Duplicatas removidas de $dstMcp (MCPs concentrados em .agents/plugins/daedalus-parallax/mcp_config.json)" -ForegroundColor Green
}

Write-Host "`nPróximos passos:" -ForegroundColor Cyan
Write-Host "1. Abrir o projeto: unity open $UnityProject"
Write-Host "2. Configurar (idempotente): menu Parallax/Bootstrap/Run Full Bootstrap."
Write-Host "3. Testes: unity test --project-path $UnityProject"
Write-Host "4. No Blender 5.x: ativar o addon 'Blender MCP' e clicar em Connect na barra lateral (N)."
Write-Host "5. Lore: python studio/scripts/sync_daedalus.py e menu Parallax/Daedalus/Import Generated JSON.`n"
