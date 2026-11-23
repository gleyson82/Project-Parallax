# Checklist Rápido — Ambiente Windows de Produção (PARALLAX em Unity 6 LTS)

Os arquivos de configuração MCP do repositório usam o caminho `C:/opt/Project-Parallax` e o projeto Unity em `client/parallax-unity`:

- `.cursor/mcp.json`
- `studio/ide-cursor/mcp.json`
- `.agents/plugins/daedalus-parallax/mcp_config.json`
- `studio/ide-antigravity/mcp_config.json`

Os quatro são idênticos. Edite `.cursor/mcp.json` e copie para os outros três.

---

## 1. Auditar e instalar a pilha gratuita (US\$ 0)

```powershell
# Conferir o que falta (Unity 6000.3 LTS + IL2CPP, Blender, Krita, Material Maker etc.)
.\studio\scripts\setup_windows_pipeline.ps1

# Instalar o que faltar (winget + Unity CLI)
.\studio\scripts\setup_windows_pipeline.ps1 -Install

# Zerar o mcp_config.json global do Antigravity (os MCPs ficam no plugin local)
.\studio\scripts\setup_windows_pipeline.ps1 -SyncMcp
```

---

## 2. Validar o projeto Unity

1. Abra com o Unity CLI: `unity open client/parallax-unity` (Unity **6000.3.29f1 LTS**).
2. Rode `Parallax/Bootstrap/Run Full Bootstrap` uma vez (é idempotente). Ele garante Force Text, Visible Meta Files, Input System, URP com os 4 renderers dos Olhares, dados base, Localization e as cenas `Boot` e `Lab_Olhares`.
3. Abra `Assets/_Project/Scenes/Labs/Lab_Olhares.unity` e dê Play:
   - **`[1]`** Olhar do Goliath (LUT térmico + ondas sísmicas da Batida + vinheta de capacete).
   - **`[2]`** Olhar do Phantom (Sobel ciano + refração N2).
   - **`[3]`** Olhar do Cipher (Pele de Rede desaturada). **`[0]`** volta ao renderer neutro.
   - **`[O]`** Override Cego (moldura do terminal isolado).
   - **`[L]`** Contagem de Vazamento (`0` a `5`); em `3+`, dispara o **Tremor Vermelho da Diretriz Ômega**.
   - **`[B]`** Pulso manual da Batida.
4. Testes: `unity test --project-path client/parallax-unity` (24 EditMode + 1 PlayMode que hospeda a sessão, nasce o jogador e troca de classe).
5. **Laboratório jogável** (`Assets/_Project/Scenes/Labs/Lab_Jogavel.unity`, primeira cena do build):
   - Play → **Hospedar sessão (Host)** no painel do canto. O primeiro operador vira Goliath.
   - WASD move, mouse olha, Espaço pula, **Q** habilidade (Goliath ancora e sustenta a porta de 10 t; Phantom usa o arpéu nos pontos da passarela alta; Cipher invade o terminal da Omni-Cripto), **E** prende cabo (Phantom), botão direito ou **F** passa para a 1ª pessoa, **F1/F2/F3** trocam de classe, **Esc** solta o cursor.
   - Para 2 ou 3 jogadores: **Window > Multiplayer Play Mode**, ative os jogadores virtuais e dê Play; uma instância hospeda e as outras clicam em **Entrar como cliente**. Cada uma recebe a próxima classe livre e enxerga só o próprio Olhar.
   - Regerar prefab e cena: `Parallax/Bootstrap/Playable Lab (Player + Lab_Jogavel)`.

---

## 3. Ligar o Blender MCP

O addon `blender_mcp.py` já está instalado no Blender 5.3 desta máquina. Para reinstalar:

```powershell
uvx blender-mcp
```

No **Blender**:
1. **Edit > Preferences > Add-ons**, busque **Blender MCP** e ative.
2. Na Viewport 3D, pressione **`N`**, abra a aba **BlenderMCP** e clique em **Connect**.
3. Exporte para `client/parallax-unity/Assets/_Project/Art/<categoria>/` (FBX para personagens com rig, GLB para props). Os `.blend` ficam em `art-pipeline/source/blender/`.

---

## 4. Nota de Propriedade Intelectual (Regra HTL Games)

Os rasters gerados no sprint inicial (`portrait_candeia_radio.png`, `portrait_brandt_radio.png`, `chapa_aco_tritao_albedo.png`, `override_terminal_frame.png`) são **bases de protótipo**. Antes do lançamento comercial na Steam, cada um passa por paintover humano no **Krita** ou **Aseprite**, conforme [blueprint-de-investimento.md](../../jogos/parallax/blueprint-de-investimento.md).
