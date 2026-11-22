# Checklist Rápido — Ambiente Windows de Produção Visual (PARALLAX)

Todos os arquivos de configuração MCP do repositório já foram convertidos para os caminhos do Windows (`C:/opt/Project-Daedalus` e `C:/opt/Project-Daedalus/client/parallax`):

- `.agents/plugins/daedalus-parallax/mcp_config.json`
- `studio/ide-antigravity/mcp_config.json`
- `.cursor/mcp.json`
- `studio/ide-cursor/mcp.json`

---

## 1. Instalar as Ferramentas Gratuitas (US\$ 0)

Execute no PowerShell na raiz do repositório (`c:\opt\Project-Daedalus`):

```powershell
# 1. Conferir o que falta
.\studio\scripts\setup_windows_pipeline.ps1

# 2. Instalar automaticamente via winget (Godot 4, Blender 4.x, Blockbench, Krita, Inkscape, TrenchBroom)
.\studio\scripts\setup_windows_pipeline.ps1 -Install

# 3. Sincronizar mcp_config.json para o perfil global do Antigravity
.\studio\scripts\setup_windows_pipeline.ps1 -SyncMcp
```

- **Material Maker** (texturas PBR procedurais): baixar o executável gratuito em `https://www.materialmaker.org/`.

---

## 2. Validar os Módulos 01 e 02 no Godot 4.4+

1. Abra o Godot 4.4+ e importe `c:\opt\Project-Daedalus\client\parallax\project.godot`.
2. Pressione **F5** para rodar [MainMenuSalaDeMaquinas.tscn](../../client/parallax/01_ui_menu_beta/scenes/MainMenuSalaDeMaquinas.tscn):
   - Pressione qualquer tecla para ver as chapas do logotipo **PARALLAX** deslizarem e travarem.
   - Use **Setas** para alternar entre as 5 válvulas (note a troca automática de emblema corporativo, Marca de 4 Hastes e retrato de rádio Candeia ↔ Brandt).
   - Pressione **TAB** para deslizar as 3 camadas de paralaxe (Goliath / Phantom / Cipher).
   - Segure **ENTER** por `0,4 s` para acionar o medidor **Três Mãos**.
3. Pressione **F2** (ou clique em **Testar Lentes & Câmera 2.5D**) para abrir [OrthoCamera25D.tscn](../../client/parallax/02_core_shaders_e_camera/scenes/OrthoCamera25D.tscn):
   - **`[1]`** Olhar do Goliath (LUT térmico + ondas sísmicas da Batida + vinheta de capacete).
   - **`[2]`** Olhar do Phantom (Sobel ciano + refração N2 + retícula telemétrica vertical).
   - **`[3]`** Olhar do Cipher (Pele de Rede desaturada).
   - **`[O]`** Alternar **Override Cego** (`override_terminal_frame.png` com pico da Fonte P).
   - **`[L]`** Aumentar a Contagem de Vazamento (`0` a `5`) — em `3+`, dispara o **Tremor Vermelho da Diretriz Ômega**.
   - **`[B]`** Disparar pulso manual da Batida; **`[H]`/`[C]`** testar aquecimento e choque criogênico no `ThermalNode`.

---

## 3. Ligar o Blender MCP para o Módulo 03 (Rigs + 3 Auxiliares)

O addon não vem pré-instalado no Blender. Para instalá-lo automaticamente via `uvx`, rode no PowerShell:

```powershell
uvx mcp-for-blender install-addon
```

Depois, no **Blender**:
1. Abra **Edit > Preferences > Add-ons** (ou instale manualmente `addon.py` pelo menu `v` no canto superior direito > **Install from Disk...**).
2. Busque por **MCP for Blender** (ou **Blender MCP**) e marque a caixa para ativar.
3. Na Viewport 3D, pressione **`N`** para abrir a barra lateral direita, clique na aba **MCP for Blender** (ou **BlenderMCP**) e clique em **Start MCP Server** / **Connect to MCP server**.
4. De volta ao Antigravity, peça para modelarmos os **3 Auxiliares** (`Macaco`, `Pipa`, `Cartucho de Override`) e os **2 Rigs base** (`Rig_Bipede_Pesado` 2,6 m e `Rig_Bipede_Padrao` 1,8 m) diretamente em `client/parallax/03_classes_e_auxiliares/`.

---

## 4. Nota de Propriedade Intelectual (Regra HTL Games)

Os 4 arquivos raster gerados neste sprint inicial (`portrait_candeia_radio.png`, `portrait_brandt_radio.png`, `chapa_aco_tritao_albedo.png` e `override_terminal_frame.png`) são **bases de protótipo/vertical slice**. Antes do lançamento comercial na Steam, passe cada um por acabamento/paintover humano no **Krita** ou **Aseprite** conforme [blueprint-de-investimento.md](../../jogos/parallax/blueprint-de-investimento.md).
