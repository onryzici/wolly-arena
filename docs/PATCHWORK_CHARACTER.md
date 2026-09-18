# Patchwork playable character

Added as character ID 2, after Woolly and Punk Vera. The selection screen, lobby portrait, gameplay avatar, checkpoint validation and independent accessory equipment support all three characters. Existing character IDs and save versions are unchanged.

Patchwork currently uses Woolly's durable stat profile and shield dash: 100 initial health, +2 armor and +2 maximum health per level. This keeps the addition within the existing combat system.

## Model work

Source: user-provided `Meshy_AI_Patchwork_Lamb_Doll_biped.zip`, extracted under `art/woolly/patchwork/source`. Original GLBs are retained.

The supplied rig has a nearly horizontal head marker and a reversed short spine segment. The mesh is rebound to the actual corrected Woolly skeleton, using its unchanged Idle, Walk and Run clips. Torso transfer uses the hips-to-neck span rather than the malformed spine markers. Head volume is preserved with an 8-degree upward bind-space correction. Extra head-marker weights are merged into the head, torso weights follow the corrected spine heights, skin weights are normalized, and boots are rigidly weighted and planted in idle. The supplied base-color artwork is retained. The open right mitten beyond the cuff is replaced by an authored closed grip; the remaining body mesh is preserved.

Runtime assets are `Assets/Woolly/Resources/Characters/Patchwork.fbx`, `Patchwork_BaseColor.png` and `PatchworkPortrait.png`. Their Unity metadata and the existing character asset postprocessor configure generic animation, looping clips and the portrait sprite.

Patchwork carries an authored cartoon knife with red blood marks on both blade faces. `tools/build_patchwork_knife.py` constructs its geometry and vertex palette; it is included in the character FBX and rigidly weighted to the right hand. The player factory supplies its vertex-color material and skips the copied lobby revolver for Patchwork. The knife appears in both the lobby and gameplay avatar; existing inventory attacks are unchanged. The portrait is regenerated from the equipped model.

## Offline workflow

Run these scripts in order with Blender's `--background --python` options:

1. `tools/inspect_patchwork_character.py`
2. `tools/build_patchwork_character.py`
3. `tools/validate_patchwork_character.py`

The editable result is `art/woolly/patchwork/Patchwork_WoollyRig.blend`. Front, side, quarter, walking and running renders are stored beside it. The validator writes `validation.json` with skeleton comparison, skin-weight, deformation, loop and FBX round-trip results.

`python3 tools/validate_equipment_offline.py` compiles runtime and Editor C# sources without launching Unity and runs the model tests, including Patchwork stats, progression, checkpoint round-trip and independent accessory equipment.

Final offline results: both C# assemblies compiled and 164 model checks passed. All 36 bones and every animation-frame pose match the Woolly reference; Idle/Walk/Run close without a vertex seam. Idle soles remain planted, footwear stays rigid, and the exported FBX reimports in Blender with all three clips. Final walking/running renders were visually inspected after correcting coat deformation.

Unity import, in-game presentation, weapon grip, touch selection and device performance still require in-Editor/device checks. No Unity Editor or phone build was launched for this revision.

## Closed grip and accessory fit revision

`tools/fit_patchwork_grip.py` authors a closed palm, four curled fingers and a thumb around the knife handle. Both grip and knife use rigid right-hand weights and vertex-color palettes, verified in the exported FBX. The portrait was regenerated.

`Characters/AccessoryFits.json` gives Woolly, Vera and Patchwork separate hat, glasses and necklace dimensions and offsets. Accessories are parented to the animated head or upper spine, so they inherit rotation and subsequent avatar scaling. The hat crown covers the forelock/mohawk; glasses include temple arms; the necklace uses a draped ellipse. `tools/render_accessory_fit.py` generates offline geometry previews for all three characters in `art/woolly/accessory_review`. Unity importer, animation-time fit and phone rendering remain unverified.

A one-time 1,000-coin test grant was applied to the Mac local career preference: 15 → 1,015. The previous career JSON was backed up under `WoollyArenaTest/Logs`. No automatic grant, economy change or phone-save modification was added.
