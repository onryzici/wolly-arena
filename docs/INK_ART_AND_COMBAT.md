# Illustrated inventory and combat presentation

The inventory now uses one transparent illustrated atlas for all 20 weapons and 16 passive items. Each catalog ID maps to a distinct sprite. Shop cards use larger art regions; stat glyphs remain separate. The generated atlas, exact prompt and tool provenance are recorded in [INVENTORY_ART_PROMPT.md](INVENTORY_ART_PROMPT.md).

Overhead ammunition pips are disabled when character billboards initialize. The player marker is a single pale mint ring with an empty center and antialiased edges. Spawn warnings use a muted red circular boundary and a filling countdown arc. Their existing warning duration, safe distance, pause handling and cancellation rules are unchanged. The combat HUD still has no background panels.

## KayKit additions and scene cleanup

Eight additional models from the free CC0 KayKit Dungeon Remastered pack are used in the actual arena: decorated barrel, stacked boxes, decorated keg, broken sword/shield, gold sword/shield, rubble, stacked coins and green bottle. Breakable models fit the existing colliders; edge decoration stays outside the walkable arena. Existing gameplay geometry and navigation remain authoritative.

Source: https://kaylousberg.itch.io/kaykit-dungeon-pack

Pinned download URLs and SHA-256 hashes: `Assets/Woolly/ThirdParty/KayKit/arena-expansion-assets.json`. The existing `dungeon-LICENSE.txt` covers these models.

Only `Assets/Scenes/SampleScene.unity` and its meta file were unused and removed. The template default now points to Startup. Startup, Lobby and TrainingArena remain the three enabled build scenes.

## Effects and budgets

Melee arcs have a thicker colored stroke and a bright inner edge. Rail beams and area impacts are more visible. Grenade, mortar and hammer impacts emit expanding dust billows; skeleton deaths emit short dust bursts and larger recognizable bone/skull/rib fragments. Hit flashes are larger. Effects still have fixed pools: 48 weapon strokes, 192 elemental particles, 96 defeat dust particles, 144 skeleton fragments, 24 hit cards and 96 sparks. These are capacity limits, not measured phone performance guarantees.

## Validation

All runtime and Editor C# compiles offline; 268 model checks pass. The Unity Survivor review also exercises all 36 icon mappings, imported and placed KayKit assets, hidden ammo pips, marker shader compilation, scene/build references, real UI buttons and joystick input, three screen ratios, spawning/pause/navigation, and one full timed wave. It includes a staged wave-16 crowd and simultaneous deaths to inspect the shared VFX pools. That fixture uses invulnerability and is a presentation/integration test, not a difficulty benchmark. Physical device performance is not measured in this revision.

Reports and captures are under `WoollyArenaTest/Logs/`: `equipment-offline-review.txt`, `survivor-runtime-review.txt`, `survivor-shop-1600x900.png`, `survivor-spawn-warnings-1600x900.png`, `survivor-crowded-combat-1600x900.png`, and `survivor-death-effects-1600x900.png`.
