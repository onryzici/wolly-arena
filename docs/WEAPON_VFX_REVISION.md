# Cartoon weapon VFX revision

The earlier neon line/echo effects were rejected. This revision replaces the flamethrower with animated painted fire and melee arcs with short tapered filled trails. This is a visual revision; weapon damage, attack intervals, build bonuses and enemy pressure values are unchanged.

- Flame uses `Resources/VFX/CartoonFire.png`, an eight-frame transparent atlas. A shared 144-element pool emits overlapping animated puffs from each actual weapon muzzle. Puffs expand into a short directional plume and fade at the end. Cover queries ignore character colliders and clip the plume against scenery. Emitters follow weapon direction and cancel with the attack system.
- Axe and sword use a brief cream-colored crescent with tapered ends and a fading inner edge; the trail stays in the forward strike region. Saw uses a shorter rotating cut. One 24-element mesh pool also handles filled, fading impact waves for frost, grenade, mortar and hammer. No full-duration wire rings or flame sine curves remain in the new weapon VFX.
- Rail and ricochet traces are shorter lived; spear/crossbow traces travel as short streaks.
- Inventory images use the new `FlatInventory.png`: simpler matte color areas, thicker contours and fewer surface details for all 36 catalog entries. This replaces the rejected glossy atlas. HUD background and marker preferences remain intact.

Generated assets were made with built-in imagegen. Exact prompts and original/project paths: [fire atlas](CARTOON_FIRE_PROMPT.md), [flat inventory atlas](FLAT_INVENTORY_PROMPT.md).

## Validation

Offline runtime/Editor compile and 268 model checks pass. `Logs/weapon-vfx-review.txt` exercises real axe, flame, sword and saw attacks, eight close frames each, correct world placement with a translated player, shared capacity with six flamethrowers, cover clipping, cancellation and pause; the final run reports no errors. These are staged presentation checks, not a phone benchmark. `Logs/vfx-review.html` plays the close frames; individual images are named `vfx-close-<weapon>-<0..7>.png`.

The Survivor review also checks all 36 atlas mappings and the shop at three aspect ratios, followed by the existing UI, spawn and crowd integration scenarios. Its report remains `Logs/survivor-runtime-review.txt`.
