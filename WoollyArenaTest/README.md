# Woolly Training Arena

Unity 6000.5.6f1 • URP • macOS Editor test prototype.

Open `Assets/Woolly/Scenes/TrainingArena.unity`, press Play, and click the Game view.

| Input | Action |
|---|---|
| WASD | Walk |
| Shift + WASD | Run |
| Mouse | Aim |
| Left mouse / hold | Fire |
| R | Reload |

Five target dummies take four hits, then reset after 1.5 seconds. Cover blocks movement and shots. Magazine: 6 rounds; fire interval: 0.28 s; reload: 1.15 s. Speeds, timing and camera framing are editable in the Inspector.

## Animation and character

`Assets/Woolly/Prefabs/Woolly_Player.prefab` contains the 36-bone generic rig, eight editable finger bones, hand-mounted revolver and muzzle socket. Original editable Blender source: `../art/woolly/Woolly_Adventurer_GripRig_v9.blend`; game animation source: `../art/woolly/Woolly_Unity_AnimationSource_v2.blend`.

`Idle`, `Walk`, `Run` blend by actual movement speed. `Shoot` uses an upper-body mask so firing can overlap locomotion. Lower-body travel direction is adapted procedurally, including reverse playback when backing up; these are not separately authored eight-direction mocap clips. Root motion is disabled; CharacterController handles collisions. Walk playback is tuned to stride length, run speed to the planted-foot section of the source clip.

Baked repaired base color and normal textures are included. Weapon parts are merged for export; editable parts remain in the Blender source. Generic rig is retained intentionally to preserve the stylized hand.

## Verification

`WoollyArena.Editor.ArenaValidation.Run()` checks weapon timing/reload, diagonal speed clamp, references, clips, masks, finger bones and HUD fonts. Results: `Logs/arena-edit-tests.txt`.

In Play mode, attach `ArenaSmokeProbe` temporarily to the player to exercise walking, running, simultaneous fire, targets, reload and cover. It removes itself and writes `Logs/arena-smoke-test.txt`. The saved scene does not include this component.

This is a local single-player input/animation test. Touch controls and an iOS Xcode export are included. Multiplayer, sound design and a full device performance pass are not included. No cloud services or monetization are configured.

Animation revision 2: walk foot tracks narrowed to 0.21 m; knees point forward; left shoulder/elbow swing with walk and run. Recoil is authored in character pitch, with no lateral muzzle drift. Camera pitch: 43 degrees, orthographic size 5.8.

## iPhone test

- Open `Builds/iOS/Unity-iPhone.xcodeproj` in Xcode.
- Scheme: **Unity-iPhone**, destination: connected iPhone.
- Signing: Automatic, existing local development team. Bundle ID: `com.trexoinnovation.woollyarena`.
- Press Run. Unlock the device; approve device trust/developer mode prompts if iOS requests them.
- Left stick: walk; push beyond 72% for run. Right stick: aim and hold to fire. RELOAD: refill magazine. Two simultaneous fingers are supported. Controls reset when focus is lost.
- Landscape orientation; safe-area anchors accommodate the notch/home indicator. 60 FPS target, Metal, IL2CPP/ARM64, iOS 15+.
- Lighting: warm directional key, cool ambient fill, soft shadows, neutral tone mapping, subtle contrast/saturation. Mobile URP uses 2x MSAA and two shadow cascades.

Regenerate Xcode after scene/code changes using `WoollyArena.Editor.MobileSetup.Export()` from the Editor integration, with iOS as the active build target. Generated Xcode output is ignored by Git; source of truth remains Unity Assets/Packages/ProjectSettings.

### Desert palette revision (iOS build 2)
Environment uses `Woolly/ArenaToon`: stable base colors, a small directional shading range and warm main-light shadows. Ground is peach sand, cover is terracotta/coral, and the checker/path overlays are disabled. Character materials retain their textured Lit shader. Tone mapping and grading boosts are disabled for this palette. Reapply via `Woolly > Apply Warm Desert Palette`; `MobileSetup.Configure()` also reapplies the palette. The test-arena geometry remains a simple blockout.

### Ground and character presentation
`PaintedSand.png` is a generated painted albedo texture, mapped in world space by `Woolly/PaintedGround` with a 12 m repeat. Loose polygon floor decorations are disabled. CharacterVitals exposes name, maximum/current health and damage/heal methods; the billboard displays name, health and magazine pips. The scene currently has target dummies, so incoming player damage is test/API-driven.

FootstepDust samples each animated ankle after pose updates and emits on downward crossings near ground height, only when grounded and actually moving. Particles use world space, fade in under 0.5 seconds, and are capped at 48. PresentationProbe verified running emissions, live particles, no idle emissions, fading, bar updates and maximum-health clamping; see `Logs/presentation-tests.txt`.

Footstep revision: dust uses 3 particles per walking contact / 6 per running contact with a subtle opacity increase. FootprintTrail stamps a five-segment boot tread on contact, aligned with the foot-to-toe direction; minimum spacing prevents overdraw while pivoting. Pool: 48 marks, fade begins at 5 seconds and finishes at 10. Marks remain in world space and are removed with the player. No prints are emitted while stationary.

### Enclosed arena and free-arm revision
Raised layered canyon perimeter replaces the flat boundary appearance; boundary colliders enclose the playable area. Camera follow is confined near arena center. Golden foliage uses root-pinned, phase-offset GrassSway deformation.

Current editable character source: `../art/woolly/Woolly_Unity_AnimationSource_v4.blend`. Left-arm walk/run motion is rebuilt from neutral, with shoulder swing synchronized against the left thigh and a flexed elbow. Shoulder tuck is reduced from 13 to 9 degrees so the arm retains clearance and a visible swing. Run hand fore/aft travel is approximately 34 cm; walk 17 cm. Lower-body and right-hand weapon tracks are preserved from v2. Previous source revisions remain available. These changes are in the Unity project; regenerate the iOS export to update a phone build.

### Illustrated arena pass
The current map is `Assets/Woolly/Art/Environment/IllustratedArena.png`, generated with the built-in ImageGen tool. Exact prompt is saved in `../art/arena/imagegen-prompt.txt`. The 24 m square background replaces visible environment blockout models; invisible cover/boundary colliders are aligned to its six painted rock silhouettes. This is a painted 2.5D backdrop, not editable 3D environment geometry. Shader preserves painted colors, receives character shadows, and subtly warps the grass bands for wind. Previous 3D environment remains inactive in the scene.

Green translucent selection ring follows the player. Walking/running speeds are 1.08/3.36 m/s (20% increase), with locomotion Playback increased 20% and blend-tree speed normalized so gait selection remains correct. Right-arm shooting playback is unchanged. Phone build requires a fresh export.

### Warm character and cartoon controls
Character body uses WarmCharacter shader: cream wool, amber clothing and gently lifted warm charcoal leather, retaining original texture detail. Original Lit material copied to `Woolly Body Before Warm.mat`. Weapon palette is desaturated blue steel, brown grip and brass; contour uses dark brown. Source texture is untouched.

UI uses Lilita One (Google Fonts, SIL OFL; bundled license in `Assets/Woolly/UI/Fonts/OFL.txt`), one baked 1024 SDF atlas, shared outline material. Blue movement and coral aim controls have layered circular sprites with directional/reticle icons; reload uses a circular arrow. Existing touch regions, separate pointer ownership and safe-area layout retained. Background Play verification passed simultaneous pointer hold, independent release and reset. UI visual check captured at 1600x900 with temporary camera canvas mode; saved scene remains ScreenSpaceOverlay.

### Wide-screen arena edges
`IllustratedArenaWide.png` is a built-in ImageGen outpaint of the square map, extended to 1536x1024 with canyon shelves on both sides. Exact edit prompt: `../art/arena/imagegen-wide-prompt.txt`. Material now maps this image to 36x24 world units, rather than clamping the square map edge across wide phone viewports. Ground expanded to cover the background; six collision boxes realigned to the new painted silhouettes. Original square image preserved. Background view inspected at 1948x900.

### 4K presentation, solid breakable props and effects
Current ground: `IllustratedArena4K.png`, 3840x2560. ImageGen enhanced the 1536x1024 input; Core Image Lanczos scales it 2.5x. This is upscaled, not native generated 4K. Unity preserves 4096 max texture size, trilinear filtering, 8x anisotropy; iOS override ASTC 4x4/high quality. Enhancement/prop prompts are in `../art/arena/imagegen-4k-and-props-prompts.txt`.

Eight breakable objects now use actual WoodBarrel/SupplyCrate FBX meshes, Lit wood/steel materials, real shadow casting and additional soft ground contact shadows. Positioned in mirrored groups next to existing stone covers; central paths stay clear. Barrel: 75 health (three hits), crate: 50 (two). Broken objects disable collision/visuals immediately, emit dust/wood chips, then clean up after 1.8 seconds. Generated card art is retained as an unused experiment, not used in the scene.

Selection marker: green filled disc, lime rim, thick cyan outer segments and orbiting white dot. Shader-driven, no per-frame object creation. ShotEffects pools 12 moving short gold trails; shared capped particle systems emit muzzle stars and impact sparks. Magazine capacity is eight; HUD/nameplate updated. Touch controls have thin translucent bases, compact blue thumb, translucent red crosshair and no redundant labels.

### Enemy spawn test
`Woolly_Enemy.prefab` is an independent red-tinted rigged enemy variant. EnemySpawnDirector keeps up to four enemies alive across ten candidate points, rejects occupied/blocked positions and positions within 2.5 m of the player, and respawns after a three-second delay. Each enemy has 100 health, name/health display, spawn/death scale animation and faces the player. This initial enemy pass does not yet chase or shoot. Player hits deal 25 damage. Live inspection verified four enemies at four distinct valid positions.

Latest breakable tests: all eight checks passed (crate/barrel raycast damage, death removes visuals/collider, dust/debris emitted, delayed cleanup). Crates enlarged to 1.55x so their solid geometry intersects the muzzle's firing height. Ring shader's reserved HLSL identifier corrected; shader compile messages cleared. Eight-round weapon state/editor checks passed.

User workflow constraint: do not open/focus Unity or trigger Play/Stop without a subsequent explicit request. Continue file-only background work where possible. No new phone build has been exported for these changes.

### Enemy pursuit and return fire (file-only update)
EnemySpawnDirector removes legacy TargetDummy objects at scene startup and builds a runtime NavMesh from the ground, Cover and Boundary BoxColliders, bounded to the arena. Agent settings: 0.35 m radius, 1.6 m height, 0.15 m step. Breakable props carve the mesh; destroying them disables carving. Enemy CharacterControllers remain disabled; NavMeshAgents handle movement and capsule hitboxes receive shots.

Enemies pursue at 2.35 m/s, repath every 0.2 seconds, stop around 4.9 m with clear sight and fire within 6.5 m after turning to aim. Shots deal 125 damage approximately every 1.35 seconds, with slight spread. Body-to-barrel and barrel-to-target ray checks block firing through cover. Existing locomotion/fire animations and pooled shot effects are reused. Player death pauses movement for two seconds, then restores position, health and ammo with two seconds of protection.

Validation: runtime scripts compiled offline with Unity's bundled Roslyn and installed Unity reference assemblies; output is temporary and not injected into the Editor. No Unity UI, Play/Stop or in-game AI test was used for this update. Runtime navigation, pursuit and firing require a user-started Play session for visual validation. Legacy target deletion takes effect at that startup; serialized scenes were not hand-edited. See Logs/combat-offline-compile.txt.

### Device update — build 3
On 2026-09-16, a headless APFS-cloned build workspace exported current sources to Builds/iOS with zero Unity build errors. Xcode Debug/arm64 build succeeded, code signature verified, and devicectl installed com.trexoinnovation.woollyarena build 3 on the paired iPhone 14 Pro Max. Application was not launched. Reports: Logs/phone-build-report.txt, Logs/phone-xcode-build.log and Logs/phone-install-build3.json. Interactive Unity session was not used for the build.

### Survival and cartoon powers — 2026-09-17

The arena now defaults to a Brotato-style survival loop: automatic targeting/fire, movement and dodge controls, ten timed waves, escalating melee enemies, and a between-wave shop. Kills credit three materials immediately. Six offers include damage, attack speed, vitality, Galaxy area blast, Energy chain lightning and Star meteor. Bought powers cast automatically; repeated purchases in later shops improve their level. Each offer can be purchased once per shop; multiple different offers can be bought before Next Wave, or all can be skipped. Materials and upgrades last for the current run only. Death ends the run; replay resets it.

CartoonPowerVFX draws pooled curved ribbons, jagged silhouettes, colored auras, orbit rings and four-point stars. The Resource shader PowerInk is included in builds without texture assets or bloom. The user-supplied cartoon VFX image informed the purple/cyan/gold palette and arrival/impact/dissipation phases. This is a procedural in-game interpretation, not the reference image pasted onto a sprite.

Review commands: Woolly > Review Survival and Woolly > Capture Cartoon Powers. See the root README for current controls and parameters. This update has not been exported to an iPhone build.

Validation: Unity compilation completed without C# errors. The final survival integration run passed 62 checks, including automatic special-power damage with regular gunfire disabled, shop spending/duplicate protection, all ten wave transitions, defeat/replay, victory/replay, lobby return and upgrade layout at 16:9, 20:9 and 4:3. The final run logged no runtime errors. The VFX contact sheet was rendered and visually checked in Unity. Device performance and audio playback have not been validated for this update.

### Equipment, stats and wave shop — current source update

Replaced the six fixed upgrades with a twenty-wave run, XP-driven free stat choices, four randomized shop offers, six weapon slots, tier I–IV weapon combining, offer locking across shops, priced rerolls and half-price equipment recycling. The catalog has six weapon types and ten passive items with positive/negative modifiers. Character stats affect maximum/current HP, incoming armor mitigation, critical damage, movement, attack speed, regeneration and wave-end harvesting. Guns have independent cooldowns; the three cartoon powers occupy equipment slots.

The user's current instruction prohibits opening Unity without an explicit request, including batch/headless/test sessions. A previously launched background compile session was stopped immediately when this instruction arrived. Subsequent validation was offline only. Runtime and Editor source compilation and 40 shared model checks passed. The new UI, runtime combat and device performance have NOT been visually/in-Editor validated; previous survival test results belong to the earlier implementation. Run `python3 tools/validate_equipment_offline.py` from the repository root to reproduce the offline checks. See the root README and AGENTS.md for current behavior and workflow.

### Persistence and pause quality pass

Added versioned wave-start/shop checkpoints, atomic replacement with backup recovery and checksum verification, deterministic shop RNG continuation, lobby Continue/New Run confirmation, manual/background pause, explicit resume protection and persistent sound preference. Shop cards now explain insufficient funds and full weapon slots. HUD counters were changed from internal shot debugging to player-facing defeat/weapon information; the survival header refreshes at most ten times per second.

No Unity Editor was started for this pass. Offline runtime/Editor compilation and 63 economy/save-file checks passed. The new pause UI, resume lifecycle, audio preference and iOS file behavior still require in-Editor/device testing when explicitly authorized. RELEASE_READINESS.md records the remaining work; this is not a release-ready claim.

### iOS build 5 — 2026-09-17

With explicit user authorization, Unity exported the current equipment/stat/save implementation using `WoollyArena.Editor.PhoneBuild.Export`. The export ran 63 model checks plus lobby checks; the separate Unity scene review passed 116 checks including those model checks. Shop renders cover three aspect ratios. Xcode Debug/arm64 build and code-signature verification succeeded. Build 5 was installed and launched on the paired iPhone 14 Pro Max, and its process was observed running. Full device gameplay, touch, lifecycle, performance and sound validation remain pending. Reports: `Logs/phone-build-report.txt`, `Logs/survival-review.txt`, `Logs/phone-xcode-build5.log`, `Logs/phone-install-build5.json`, `Logs/phone-launch-build5.json`.

### Build 6 and offline dash/UI revision

Build 6 exported and built successfully, passed code-sign verification, and was installed/launched on the paired phone. The additional scene review did not pass (shop/stat and attack assertions); no new integration pass is claimed. Following user feedback, the source now uses a 2.2 m / 0.22 s dash, last movement direction, continuous running on exit, short speed trails, and a light cream/mint/amber/sky pause and shop palette. These subsequent changes passed offline compilation and the existing 63 model checks only. Unity remains closed by user instruction; the phone still has Build 6.

## Combat and loot revision (2026-09-17)

- Waves 1–19 last 30–44 seconds; the final wave lasts 90 seconds, with a remaining-time bar and final-five-second color cue.
- Four procedural creature silhouettes replace survival raiders: gnawer, fast spine runner, slower high-health brute, and stitched dummy.
- The hand-held revolver renderers and firing overlay are disabled during survival. Six equipment types have distinct orbiting models; physical guns recoil, and powers originate at their equipped core.
- Compact bolts/shock rings replace the old oversized power ribbons. Actual health loss appears as animated numbers, with gold critical-hit numbers and a brief creature hit flash.
- Defeats leave bounded ground splats and flying fragments. Gold pickups are worth one and drop at 100% initially, tapering to 75%. Normal chests have a 1% chance capped at one per wave. No healing pickups spawn, and chests grant only gold. See `../docs/BROTATO_BALANCE_RESEARCH.md` for the economy and XP formulas.
- Loot attracts within 2.5 units; uncollected rewards are banked before successful wave transitions and shop saves. Health drops wait when health is full. Defeat discards uncollected loot.
- Shop cards use a dark slate background, rarity accents, higher-contrast prices, and a clearer next-wave action.

Validation: runtime and editor C# compile offline, and all 63 economy/checkpoint checks pass. The existing Play Mode review now checks dropped/collected gold and end-of-wave banking, but has not been run. Unity was not launched per AGENTS.md. Shader rendering, creature/weapon appearance, UI fit, runtime pickup behavior, and phone performance still need an in-Editor/device review.

## Visual UI and lobby pass

Combat HUD panels are transparent, with outlined numbers and heart/coin illustrations. Dash is anchored 105 units from the right and 110 from the bottom of the safe area. Blood uses seven smooth, feathered silhouettes and smaller directional droplets, with no cast shadows and a gradual opacity fade.

The shop and pause flow are Turkish. Offer/equipment/stat illustrations are resolution-independent uGUI geometry; passive cards show every positive and negative modifier with a stat icon and signed value. Detailed descriptions remain available by selecting owned equipment. Turkish glyph coverage is checked when the shop initializes.

Lobby buttons use a shared rounded nine-slice sprite. Dragging on the character rotates it around its vertical axis without snapping back. The hit area stays inside the character frame, tracks one pointer, and ignores rotation while a modal is open.

Validation: runtime/editor offline compilation and 63 economy/checkpoint checks pass. In the open Editor, the shop's Turkish cards and icons, background-free HUD, dash position, and rounded lobby buttons were visually inspected. EventSystem raycast hit the character drag area; simulated pointer down/drag/up changed yaw from 160 to 43.18 degrees. Full-turn wrap returned zero angular error. Native automated mouse drags did not reach Unity's Input System, so physical mouse/touch dragging and phone layout still need hands-on verification. Two runtime issues found during this review (missing CanvasRenderer on icons and premature MaterialPropertyBlock allocation) were fixed before rechecking the shop. Lobby left open with checkpoint saving re-enabled.

## Cartoon artwork and original UI package restoration

The lobby now exclusively uses the existing package sprites for its button surfaces: original round glossy yellow/blue controls, dark round panels, and framed currency/profile bars. Procedural button textures and tint overlays were removed; character drag rotation remains intact.

The in-game vector placeholders were replaced with 25 original cartoon inventory illustrations, generated using built-in ImageGen. Asset: `Assets/Woolly/Resources/CombatArt/CartoonInventory.png`; exact prompt and grid order: `../art/woolly/cartoon_inventory_prompt.txt`. The 1254×1254 RGBA atlas preserves transparent pixels; no image background removal or repainting was applied. `ArenaIcon` shares the atlas and creates sprite regions for all weapons, items, stats and gold. Empty slots stay empty.

Validated in the running Editor: generated illustrations on shop offers, signed stat modifiers and inventory; original UI package button surfaces in lobby; rotation component retained. Offline runtime/editor compilation and 63 economy/checkpoint tests passed. Lobby left open with normal checkpoint saving restored.

## Wave ending and bloody cartoon impacts

Wave expiry now enters a 2.8-second celebration before level rewards and the shop. Combat stops, survivors dissolve in a staggered bloody sweep, remaining loot moves to the player, and a transparent Turkish banner shows kills and collected gold. Cleanup adds no kill rewards. Pausing freezes the sequence; quitting during it retains the existing start-of-wave checkpoint.

Hits spray directional droplets; deaths create larger stains and bouncing chunks that leave smaller ground marks. Critical hits and deaths add bounded camera impulses. A gold expanding ring marks wave completion. Effects use fixed pools; stains fade and debris expires.

Validation: offline runtime/editor compilation and 72 checks passed, including nine transition timing/pause/reset checks. In the existing Editor session, a manually triggered wave end displayed the banner, then advanced to level rewards and the shop; survivor cleanup preserved eight kills and collected remaining gold (18 to 24). Phone performance and full-run progression have not been retested for this revision.

## Combat feedback refinement

Camera impulses now cover normal hits, criticals, deaths and player damage with a light default intensity (0.55), strongest-impact cap and short decay. Disabling the camera removes its last offset. Loot pops upward on spawn, bobs on the ground and accelerates toward the player once attracted. Full-health hearts remain available until needed or wave settlement. Reward values remain unchanged.

Critical damage uses larger, longer-lived numbers with slight sideways movement. Rapid kills show occasional Turkish streak milestones; new levels and chest collection receive distinct feedback. Small synthesized pickup, healing, chest, level and critical sounds follow the existing global sound toggle; coin sounds are rate limited. Health, gold and level readouts briefly pulse on changes; the last five seconds pulse once per second. No new HUD backgrounds were added.

This refinement was validated through offline runtime/editor compilation and the existing 72 model checks. These checks cover economy, saves and wave timing; the new animation and audio quality still require in-Editor/device review. Unity was not launched and Play/Stop was not triggered for this pass.

## Bosses, crafted models and Buzul Korusu

Ordinary enemy health starts at 1.2× baseline and gains another 0.14× per wave after five. Population caps at 60 and spawn interval at 0.20 seconds. Waves 5/10/15/20 introduce Buz Muhafızı / Diken Kraliçe bosses (1800/6300/13800/24300 health). Their existing ground warnings precede area attacks. Mid-run bosses must be killed and ordinary spawns continue after timer expiry. The final ends on boss defeat or surviving 90 seconds, after a successful boss spawn. Boss rewards are gold-only chests. Repeated hits interrupt regular enemies at most once per 0.45 seconds, bosses once per 0.9 seconds.

Four ordinary creatures, two bosses and all six orbit weapons now load authored beveled/rounded FBX models from `Resources/Models`, with explicit URP material remapping. Original procedural geometry remains a fallback if an asset is missing. Model source generator and Blender inspection render: `../art/woolly/expansion/`. These are new geometry assets, not a character animation retarget.

The lobby's map picker now offers Kızıl Kanyon and Buzul Korusu using existing UI-package surfaces and visual previews. The snow biome uses a generated painted ground, crafted fir trees, ice crystals and a different six-cover layout. It is assembled from the existing arena scene before runtime navigation is built, so visual cover and baked obstacles agree. Save format v2 stores the biome; v1 saves remain supported and default to the canyon. Choosing a map affects new runs, while Continue uses the saved biome.

Validation: runtime and editor scripts compile offline; 84 model checks pass, including difficulty caps, boss-wave gating, biome persistence and v1 migration. Exported models and biome preview were rendered and inspected in Blender. Unity Play mode, actual imported-model orientation, boss combat balance, map navigation and phone performance have not been tested for this expansion. Existing scene review was updated for the celebration delay and scripted boss defeats; it was not executed.

## Punk Vera — second playable character

The user-supplied `Meshy_AI_Punk_Bunny_Rebel_biped.zip` provides Vera's model and source walking/running clips. Corrected assets live in `Resources/Characters`; editable Blender source, generation scripts, rear-view inspection and deformation reports are in `../art/woolly/punk_vera/`. The source mesh's original albedo is retained; the game uses CombatModel cartoon shading and a skinned 0.012-unit dark outline. All weapons remain in orbit. Both characters share the same starting stats and equipment.

Boot influences below the ankle were reassigned to the foot, with a smooth calf transition. Weight sums are normalized, lateral foot twist is corrected, sole contact is rechecked after closing animation seams, and a relaxed standing Idle was authored alongside Walk and Run. The original pompom was separated from its off-center socket, the old socket capped, and the pompom moved to the rear center and bound entirely to Hips; trousers were not stretched to connect the two positions.

The lobby Character/Profile buttons open a two-portrait picker; selecting Vera swaps the lobby model and retains drag rotation. Character selection applies to new runs. Save format v3 stores character and biome; v1/v2 records retain Woolly and preserve their existing progress/biome.

Validation: 87 standalone economy/save/progression checks and runtime/editor compilation pass. Blender sampled 181 poses, including half-frames: minimum sole clearance was 0.008 m (Idle), 0.01699 m (Walk), 0.01254 m (Run), with maximum sole length error below 0.000001 m. Front, rear and gait renders were inspected. In the already-open Unity Editor, resource loading found Idle/Run/Walk clips, the model's identity root transform, and both shaders; temporary preview-scene instantiation produced the body plus matching skinned outline and a valid baked mesh. No Play/Stop was triggered. Full gameplay, character-picker interaction and phone checks remain unperformed. The later Unity render-preview attempt could not reconnect to the Editor; no new Editor was launched.

### Punk Vera idle balance correction (2026-09-17)
Rebuilt the idle stance in armature space: upright torso/head, relaxed matching arms, individually grounded boots, and chest breathing with stationary hips/feet. Existing run/walk correction and centered pompom are retained. Regenerated PunkVera.fbx and portrait. Front preview: `art/woolly/punk_vera/idle-balanced-front.png` (workspace root).

Offline Blender validation sampled 181 poses across Idle/Walk/Run. All passed: idle sole contact difference <0.001 mm, foot drift <0.001 mm, torso lean <0.001 degrees, no sole penetration or boot stretching. Visually inspected front render. This correction has not been checked in Unity Play mode or on device.


## September 18 balance and lobby revision

Research, formulas, assumptions and offline simulation results: [Brotato balance report](../docs/BROTATO_BALANCE_RESEARCH.md). Existing saves are preserved; evaluate progression on a fresh run. No Play/Stop, Unity launch or device test was performed for this revision.

Character selection now has its own full screen, following the GUI pack collection/detail references. Lobby shadows use baked sole bounds; Vera is 10% larger and uses a separate boxing Idle with brass knuckles. Gameplay continues using the approved Woolly-bound locomotion rig. The lobby FBX and transparent portrait are generated offline with Blender (`tools/create_vera_lobby.py`). Rendered Unity layout, touch interactions and shader output still require in-Editor/device verification.

Boxing model regression: 145 Idle samples × 11 lower-body bones exactly match the approved Vera/Woolly rig (maximum world-matrix difference 0 in both Blender source and reimported FBX). Runtime and Editor C# compilation passed with 101 offline model checks.


## Character selection and armed Vera correction

Supersedes the boxing presentation above. Vera now uses the original approved `PunkVera.fbx` Idle again. The lobby clones Woolly's actual `Revolver_HandSocket` hierarchy under Vera's matching hand bone, preserving local pose, gun materials and outline. The unused boxing FBX was removed from Resources. Vera copies Woolly's actual body and outline materials (with her own texture), including the warm outline color and width. Both armed portraits are rendered by `tools/render_character_portraits.py` with the same camera and ink settings; portraits are visible in the selection footer and main profile.

The separate character screen now browses one character at a time via horizontal swipes, arrows or portrait buttons. Main-lobby dragging still rotates the character. The selection view displays eight starting stats and the passive trait from `CharacterDefinition`, also used by the actual new-run build. Woolly: 100 HP, 2 armor. Vera: 85 HP, 0 armor, 95% damage, 112% attack speed, 108% movement. Both start with 5% critical, 0 regeneration, 5 harvesting and one tier-I revolver. These are starting identities, not earned account upgrades.

Checkpoint v4 persists the stat profile separately from earned bonuses. Versions 1–3 retain their previous neutral stats; no existing run loses health or receives duplicate bonuses when loaded. Selection changes affect new runs. Offline runtime/editor C# compilation and 110 model checks pass, including shared UI/game stat values, character multipliers, trait/gear save round-trips and version-three migration. Portrait renders were visually inspected. Unity Play, rendered selection layout, shader appearance and physical touch gestures remain untested; no Editor launch or Play/Stop was triggered.


## Character-screen screenshot corrections

The independent character screen hides bottom lobby navigation and restores it on return. Its larger character preview stays within the frame, including Vera's mohawk; footer cards and the short gold confirmation button have separate space. Portraits use imported Single/FullRect Sprite assets, refreshed when the screen opens or character changes; missing assets disable the image instead of drawing a white rectangle. The portrait importer revision forces alpha-aware, uncompressed UI sprites. Card portraits fit within their cards. Selection background is a texture-free dark purple gradient with a soft spotlight, removing the tiled pattern. Turkish stat headings use Turkish casing. Offline C# compilation/model checks and PNG alpha/layout containment checks pass; no new Unity render, Play/Stop or device interaction test was performed.


## Action music, lasers and upgrade progression

See [implementation and validation notes](../docs/ACTION_AND_UPGRADES.md). Supersedes the prior 10-HP between-wave recovery: each new wave now starts at full current maximum health. Added original looping combat music, four combat cues, pooled impact sparks and telegraphed ranged laser enemies from wave 3. Level choices now have four rarity tiers and paid rerolls; early shops favor weapon acquisition/upgrades and introduce two category-specific special items. Checkpoint v5 preserves offered rarity and migrates previous saves. 122 offline checks pass; runtime/audio/device checks remain pending.
