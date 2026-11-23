# Roteiro 06 — Arsenal das 3 Corporações e Relíquias de Grau-Zero

**Destino de Exportação na Unity:** `client/parallax-unity/Assets/_Project/Art/Characters/` (sockets de arma), `client/parallax-unity/Assets/_Project/Art/Weapons/` e `client/parallax-unity/Assets/_Project/Art/VFX/`
**Referências de Lore:** `armamento-therma-tek.md`, `armamento-kinesys.md`, `armamento-omni-cripto.md`, `coracao-de-hefesto.md`, `lagrima-do-leviata.md`, `codigo-fonte-omega.md`, `marca-de-quatro-hastes.md`

---

## 1. Armas Corporativas (Modelagem Low-Poly Afiada no Blockbench ou Blender MCP)

### A. Arsenal Therma-Tek (Sinergia Goliath — Fogo, Escória e Aço)
- **Ferramenta:** **Blockbench** (gratuito) ou **Blender MCP**.
- **Formato de Saída:** `arma_thermatek_canhao_escoria.glb` e `arma_thermatek_escopeta_impacto.glb`.
- **Prompt para o Agente via Blender MCP / Blockbench:**
  ```text
  Modele em 3D low-poly industrial o `arma_thermatek_canhao_escoria` (acoplado ao antebraço do Goliath):
  - Cano duplo largo com camisa de resfriamento perfurada que brilha de laranja a vermelho conforme o medidor de superaquecimento sobe.
  - Tambor inferior de escória líquida com pintura amarela/laranja desgastada da Therma-Tek e pistão de recuo pesado.
  Exporte como `.glb` com ponto de origem no encaixe do antebraço (`Muzzle_Point` na ponta do cano).
  ```

### B. Arsenal Kinesys (Sinergia Phantom — Gelo, Precisão e Silêncio)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `arma_kinesys_rifle_criogenico.glb` e `arma_kinesys_arpel_magnetico.glb`.
- **Prompt para o Agente via Blender MCP / Blockbench:**
  ```text
  Modele em 3D low-poly o `arma_kinesys_rifle_criogenico`:
  - Chassi longo, fino e esqueletizado em titânio branco-gelo e grafite, com cilindro lateral transparente de nitrogênio líquido pressurizado (#00E5FF) e supressor magnético longo na ponta.
  Exporte como `.glb`.
  ```

### C. Arsenal Omni-Cripto (Sinergia Cipher — Feixe de Íons, EMP e Dados)
- **Ferramenta:** **Blockbench** ou **Blender MCP**.
- **Formato de Saída:** `arma_omnicripto_feixe_ions.glb` e `arma_omnicripto_granada_emp.glb`.
- **Prompt para o Agente via Blender MCP / Blockbench:**
  ```text
  Modele em 3D low-poly o `arma_omnicripto_feixe_ions`:
  - Carabina bullpup angular sem partes móveis de combustão, com três trilhos condutores abertos na frente que convergem em um anel emissor de plasma violeta (#B388FF) e pequeno visor lateral de sincronia quântica.
  Exporte como `.glb`.
  ```

---

## 2. As 3 Relíquias de Grau-Zero (Peças Únicas da Primeira Colonização)
Regra obrigatória de lore (`marca-de-quatro-hastes.md`): **Todas as três Relíquias possuem a Marca de Quatro Hastes gravada em um ponto oculto**.

### Relíquia 1 (Goliath): O Coração de Hefesto (`coracao-de-hefesto.md`)
- **Ferramenta:** **Blender 4.x via Blender MCP** + Shader de Incandescência na URP (HLSL ou Shader Graph).
- **Formato de Saída:** `reliquia_coracao_de_hefesto.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele a Relíquia de Grau-Zero `reliquia_coracao_de_hefesto` (módulo de reator peitoral para o Goliath):
  1. Carcaça cilíndrica de tântalo escurecido pelo magma com aletas de contenção abertas revelando um núcleo pulsante branco-laranja de fusão bruta.
  2. Detalhe de Lore obrigatório: Na face interna da tampa de tântalo, grave em baixo-relevo **onze riscos verticais feitos a faca (os 11 pilotos anteriores) e um décimo segundo risco pela metade**, além da **Marca de Quatro Hastes**.
  Exporte como `reliquia_coracao_de_hefesto.glb`.
  ```

### Relíquia 2 (Phantom): A Lágrima do Leviatã (`lagrima-do-leviata.md`)
- **Ferramenta:** **Blender 4.x via Blender MCP** + Shader de Cristal sem Sombra (`Cast Shadows: Off`) na URP.
- **Formato de Saída:** `reliquia_lagrima_do_leviata.glb` + `Parallax/World/LeviataCrystal` (shader URP).
- **Prompt para o Agente via Blender MCP & Shader:**
  ```text
  1. Via Blender MCP, modele o rifle sniper `reliquia_lagrima_do_leviata`: o corpo e o cano inteiro são esculpidos a partir de **um único cristal contínuo de nitrogênio facetado**, sem nenhum parafuso ou peça de metal tradicional. Sob o cano, esculpa internamente a **Marca de Quatro Hastes**.
  2. Na Unity, gere o shader `Parallax/World/LeviataCrystal` com refração pura de vidro/gelo e configure o MeshRenderer com `ShadowCastingMode.Off` (pois na lore a Lágrima do Leviatã não projeta sombra).
  ```

### Relíquia 3 (Cipher): O Código-Fonte Ômega (`codigo-fonte-omega.md`)
- **Ferramenta:** **Blockbench / Blender MCP** + **Krita** (textura da escrita a lápis).
- **Formato de Saída:** `reliquia_codigo_fonte_omega.glb`.
- **Prompt para o Agente via Blender MCP:**
  ```text
  Via Blender MCP, modele a Relíquia `reliquia_codigo_fonte_omega` (acoplada à manopla do Cipher):
  - Um disco óptico-magnético pesado e chamuscado do Setor Nulo dentro de um estojo aberto de metal.
  - Na tampa externa, adicione uma etiqueta gasta com a inscrição manuscrita a lápis: "NÃO LER EM VOZ ALTA".
  - No verso do disco, grave a **Marca de Quatro Hastes**.
  - Adicione um pequeno display LED vermelho preso ao cabo que exibe fixamente: `VAZAMENTO DETECTADO: 1`.
  ```
