# Art Pipeline (A Fábrica Externa de Ativos de PARALLAX)

Este diretório (`art-pipeline/`) contém os **8 Roteiros de Criação Visual e Sonora fora da Unity**, organizados na ordem exata de produção definida pela Diretoria de Game Design em [`jogos/parallax/fundacao/roteiro-componentes-visuais.md`](../jogos/parallax/fundacao/roteiro-componentes-visuais.md).

Cada roteiro especifica **qual ferramenta externa usar**, **o prompt exato (para geradores de IA 2D/3D/Áudio ou para os Agentes via Blender MCP / Inkscape / Material Maker)** e **a pasta de destino dentro do motor (`client/parallax-unity/Assets/_Project/`)**.

---

## Índice dos 8 Roteiros de Criação e Prompts

| Ordem | Pasta e Roteiro | O que cria | Ferramentas Externas Utilizadas | Destino na Unity (`Assets/_Project/`) |
| --- | --- | --- | --- | --- |
| **01** | [`01_menu_e_ui_beta/ROTEIRO-01-MENU-E-UI.md`](01_menu_e_ui_beta/ROTEIRO-01-MENU-E-UI.md) | Chapa de aço escuro, Logo PARALLAX desalinhado, 4 Válvulas do Mapa, Emblemas das 3 Corporações, Marca de 4 Hastes, 3 Relógios e Retratos de Rádio (Candeia/Brandt). | **Material Maker**, **Inkscape**, **Aseprite**, **Krita**, **ComfyUI / Midjourney** | `Art/UI/` |
| **02** | [`02_lentes_e_mascaras_olhar/ROTEIRO-02-LENTES-E-SHADERS.md`](02_lentes_e_mascaras_olhar/ROTEIRO-02-LENTES-E-SHADERS.md) | Os 3 Olhares Assimétricos (`goliath_thermal`, `phantom_optic`, `cipher_net`), Tremor Vermelho da Ômega e Moldura 2D do Terminal de Override Cego. | **Cursor Pro / Antigravity**, **Material Maker**, **Inkscape**, **Aseprite** | `Shaders/Vision/` + `Art/Textures/Vision/` |
| **03** | [`03_classes_e_auxiliares_3d/ROTEIRO-03-CLASSES-E-AUXILIARES.md`](03_classes_e_auxiliares_3d/ROTEIRO-03-CLASSES-E-AUXILIARES.md) | Modelos 3D Low-Poly de **Goliath**, **Phantom** e **Cipher**, os **2 Rigs Universais** e os 3 Auxiliares (**Macaco**, **Pipa**, **Cartucho de Override**). | **Tripo 3D Pro / Meshy Pro / Hunyuan3D**, **Blockbench**, **Blender 4.x + Blender MCP**, **Mixamo** | `Art/Characters/` + `Art/Auxiliaries/` |
| **04** | [`04_props_e_interativos_puzzles/ROTEIRO-04-PROPS-DE-PUZZLE.md`](04_props_e_interativos_puzzles/ROTEIRO-04-PROPS-DE-PUZZLE.md) | Válvula Mestra Therma-Tek (com número no topo), Porta de Chumbo 10t + Chave Tripla + 4º Receptáculo, Linha Viva 140% e Farol de Laser. | **Blender 4.x + Blender MCP**, **Blockbench**, **Material Maker** | `Art/Props/` + `Prefabs/Props/` |
| **05** | [`05_biomas_e_transportes/ROTEIRO-05-BIOMAS-E-TRANSPORTES.md`](05_biomas_e_transportes/ROTEIRO-05-BIOMAS-E-TRANSPORTES.md) | Kits modulares PBR e Skyboxes (Face Voltada vs. Face Cega) na ordem: 1º Fosso de Hefesto, 2º Hub, 3º Estilhaços, 4º Krios-Prime, 5º Setor Nulo e 3 Transportes. | **ProBuilder + kits do Blender**, **Material Maker**, **Blender MCP**, **ComfyUI / Krita** | `Art/Environment/` + `Scenes/Missions/` |
| **06** | [`06_arsenal_e_reliquias/ROTEIRO-06-ARSENAL-E-RELIQUIAS.md`](06_arsenal_e_reliquias/ROTEIRO-06-ARSENAL-E-RELIQUIAS.md) | Armas das 3 Corporações e as 3 Relíquias de Grau-Zero (**Coração de Hefesto**, **Lágrima do Leviatã** sem sombra, **Código-Fonte Ômega**). | **Blockbench**, **Blender 4.x + Blender MCP**, **Krita** | `Art/Weapons/` + `Art/VFX/` |
| **07** | [`07_inimigos_e_bosses_raid/ROTEIRO-07-INIMIGOS-E-BOSSES.md`](07_inimigos_e_bosses_raid/ROTEIRO-07-INIMIGOS-E-BOSSES.md) | Esquadras de Contenção, Drones/Guardião da Ômega e os 3 Chefes (**Mestre de Fornalha**, **Regulador de Brandt** e **Gestor-9**). | **Tripo 3D / Hunyuan3D**, **Blockbench**, **Blender 4.x + Blender MCP** (reusando os 2 Rigs) | `Art/Enemies/` |
| **08** | [`08_audio_vozes_e_batida/ROTEIRO-08-AUDIO-E-VOZES.md`](08_audio_vozes_e_batida/ROTEIRO-08-AUDIO-E-VOZES.md) | Nota do Piso (50 Hz), Pulso da Fonte P (26h56), SFX da Batida e do Choque Térmico, Vozes de Rádio (Candeia/Brandt) e Música Industrial. | **Vital Synth**, **ElevenLabs**, **Suno Pro**, **Audacity** | `Audio/` |
