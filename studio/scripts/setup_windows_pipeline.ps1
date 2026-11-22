<#
.SYNOPSIS
  Verifica e instala a pilha gratuita (US$ 0) de produção visual/3D de PARALLAX no Windows.
.DESCRIPTION
  Executa auditoria das ferramentas no PATH e, com -Install, instala via winget:
  Godot 4, Blender 4.x, Krita, Blockbench, Inkscape e Material Maker.
  Também sincroniza o mcp_config.json para o diretório global do Antigravity (~/.gemini/config/mcp_config.json).
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

$tools = @(
    @{ Name = "Godot Engine 4";  Cmd = "godot";          WingetId = "GodotEngine.GodotEngine";       RequiredFrom = "Módulo 01 (Agora)" },
    @{ Name = "Blender 4.x";     Cmd = "blender";        WingetId = "BlenderFoundation.Blender";     RequiredFrom = "Módulo 03 (Rigs & Props)" },
    @{ Name = "Blockbench";      Cmd = "blockbench";     WingetId = "JannisX11.Blockbench";          RequiredFrom = "Módulo 03 (Auxiliares)" },
    @{ Name = "Krita";           Cmd = "krita";          WingetId = "KDE.Krita";                     RequiredFrom = "Módulo 01 (Paintover/UI)" },
    @{ Name = "Inkscape";        Cmd = "inkscape";       WingetId = "Inkscape.Inkscape";             RequiredFrom = "Módulo 01 (SVGs)" },
    @{ Name = "TrenchBroom";     Cmd = "trenchbroom";    WingetId = "TrenchBroom.TrenchBroom";       RequiredFrom = "Módulo 05 (Biomas)" },
    @{ Name = "Material Maker";  Cmd = "material_maker"; WingetId = "RodZill4.MaterialMaker";        RequiredFrom = "Módulo 01/05 (Texturas PBR)" },
    @{ Name = "Audacity";        Cmd = "audacity";       WingetId = "Audacity.Audacity";             RequiredFrom = "Módulo 08 (Áudio/Rádio)" },
    @{ Name = "Obsidian";        Cmd = "obsidian";       WingetId = "Obsidian.Obsidian";             RequiredFrom = "Vault Project Daedalus" },
    @{ Name = "Node.js";         Cmd = "node";           WingetId = "OpenJS.NodeJS.LTS";             RequiredFrom = "MCP Godot/Filesystem" },
    @{ Name = "uv / uvx";        Cmd = "uvx";            WingetId = "astral-sh.uv";                  RequiredFrom = "MCP Blender" }
)

Write-Host "`n=== PARALLAX // AUDITORIA DO PIPELINE WINDOWS ===" -ForegroundColor Cyan
foreach ($t in $tools) {
    $found = Get-Command $t.Cmd -ErrorAction SilentlyContinue
    if ($found) {
        Write-Host ("[OK]    {0,-18} -> {1}" -f $t.Name, $found.Source) -ForegroundColor Green
    } else {
        Write-Host ("[FALTA] {0,-18} ({1})  | winget id: {2}" -f $t.Name, $t.RequiredFrom, $t.WingetId) -ForegroundColor Yellow
        if ($Install) {
            Write-Host ("        Instalando {0} via winget..." -f $t.Name) -ForegroundColor Cyan
            winget install --id $t.WingetId -e --accept-package-agreements --accept-source-agreements
        }
    }
}

Write-Host "`n[NOTA] Material Maker (texturas PBR gratuitas) nao possui pacote oficial no winget:" -ForegroundColor DarkYellow
Write-Host "       Baixe diretamente em: https://www.materialmaker.org/ (ou itch.io/rodzilla/material-maker)" -ForegroundColor Gray

if ($SyncMcp) {
    $dstDir = Join-Path $env:USERPROFILE ".gemini\config"
    $dstMcp = Join-Path $dstDir "mcp_config.json"
    New-Item -ItemType Directory -Force -Path $dstDir | Out-Null
    '{"mcpServers": {}}' | Set-Content -Path $dstMcp -Encoding UTF8
    Write-Host "`n[OK] Duplicatas removidas de $dstMcp (MCPs concentrados em .agents/plugins/daedalus-parallax/mcp_config.json)" -ForegroundColor Green
}

Write-Host "`nProximos passos apos instalar:" -ForegroundColor Cyan
Write-Host "1. Abrir c:\opt\Project-Daedalus\client\parallax\project.godot no Godot 4.4+ e rodar F5 (Menu Beta) + F2 (Lab 2.5D)."
Write-Host "2. No Blender 4.x, instalar o addon 'blender-mcp' (Edit > Preferences > Add-ons) e clicar em 'Connect to Claude/MCP'."
Write-Host "3. Executar '.\studio\scripts\setup_windows_pipeline.ps1 -SyncMcp' para registrar os caminhos Windows no Antigravity.`n"
