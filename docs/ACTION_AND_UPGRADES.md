# Combat audio, lasers and progression

18 September 2026. Implemented offline; no Unity Editor launch, Play/Stop or phone build.

## Audio

`Resources/Audio/ArenaRush.wav`: original 144 BPM, 32-bar / 53.333-second stereo composition. Synthesized drums, picked minor-pentatonic lead, bass, response phrases and breakdown bars; no third-party samples. Reproduce with `tools/compose_arena_music.py` in a Python environment with NumPy. Four short mono cues cover charge, laser fire, impact and wave start.

PCM validation: peak 0.562, RMS 0.107, loop-boundary sample discontinuity 0 after a 6 ms edge ramp. This verifies signal integrity, not subjective sound quality. No listening/device mix review was performed. Unity imports the music as streaming Vorbis and cues as decompressed PCM. The run owns the sources, so scene changes dispose them. Music rises in boss combat and drops in shops/pause; the existing sound toggle mutes everything.

## Combat

- Every `BeginWave` applies the build then heals to the current maximum, including wave restarts. Buying more maximum HP before proceeding therefore starts the next wave full. Ground healing drops remain disabled.
- Prism Hunters appear from wave 3, on every seventh eligible spawn, capped at 1–4 living laser enemies depending on wave. Pink emitter/crystal accessories distinguish them.
- They approach to firing distance, telegraph a locked target for 0.9 seconds, and launch an energy bolt at 9 units/second. The projectile does not track the player. It expires after 16 units and collides with scenery/player using a swept sphere. Other enemies are ignored. Laser damage uses the existing armor and dodge invulnerability paths.
- Death, wave clear and component disable cancel the warning/projectile. Pausing freezes simulation. The charging enemy stops moving; scenery blocks firing. The next attack waits 3.4 seconds.
- A fixed 96-spark pool adds hit/critical sparks, energy impacts, boss bursts and wave-start feedback. No per-hit particle GameObject allocation. Each laser enemy owns three reusable line renderers.

## Upgrade and shop rules

Brotato offers four primary-stat choices with four rarities and paid rerolls. It also guarantees upgraded rarity on specific milestone levels. We implemented those mechanics for this game's eight existing primary stats, with amounts scaled to its 100-HP baseline. This does not add every Brotato stat or mechanic. [Upgrade reference](https://brotato.wiki.spellsandguns.com/Upgrades)

Each queued reward uses its own earned level (not the player's newest level), so multiple pending choices cannot all inherit one high-level rarity. Level 5 guarantees tier II; levels 10/15/20 tier III; 25 and subsequent multiples of five tier IV. Other choices use rising tier chances. Tier I–IV: HP 8/12/16/20; damage and attack speed 5/8/12/16%; movement 4/6/8/10%; armor/regen 1/2/3/4; crit/harvest 3/5/7/9. Each choice shows its rarity color and actual amount. Stat rerolls charge gold, increase cost, and retain the pending choice count; shop reroll cost resets once all stat choices are consumed.

The first two shops guarantee two weapon offers, excluding preserved locked slots. Already-owned weapon types receive 3× selection weight in the candidate list; offers remain unique. This supports intentional upgrading without granting free weapons. Existing six-slot cap, matching-tier combination, recycling and locked prices remain. Brotato similarly biases early shops and owned-weapon selection, but this game's probabilities/prices are its own. [Shop reference](https://brotato.wiki.spellsandguns.com/Shop)

From shop 4 two special items join the pool:

- **Düellocu Rozeti**: +12% firearm damage per tier; −3% attack speed per tier. Applies to revolver/repeater/shotgun only.
- **Prizma Çekirdeği**: +18% power damage per tier; −8 max HP per tier. Applies to Galaxy/Energy/Star only.

Effects are computed from owned gear, included in descriptions and removed when recycled. Existing thematic icons are reused. Their category-specific modifiers do not change the generic Damage stat displayed in the sidebar.

Checkpoint version 5 stores all four offered rarities alongside the existing RNG state and reroll count. Old versions 1–4 still load; old offers default to tier I. Character stat profiles remain intact.

## Validation

Offline runtime and Editor C# compilation plus 122 model checks pass. Added tests cover milestone rarity with queued rewards, paid/failed rerolls, exact save/resume choices and RNG, special-item effects and recycling, starter shops and v4 migration. Existing economy scenarios still run; they are sensitivity models, not gameplay measurements. New health/refill behavior, raycast collision, warning visibility, full-run difficulty, rendered stat cards, audio playback/mix and mobile performance still require runtime/device checks.


## 18 Eylül: düşman sunumu ikinci revizyonu

ArenaRush artık kullanılmıyor; Robo-Western ve lisansı THIRD_PARTY_NOTICES.md içindedir. Önceki sentez müzik değerlendirmesi tarihçedir. Yeni modeller tools/build_articulated_enemies.py ile üretilir; eski art/woolly/expansion/build_models.py dosyası düşmanları yeniden üretmek için kullanılmamalıdır, çünkü eski statik modellerin üstüne yazar. Yeni FBX dosyalarında Head/Torso/ArmL/ArmR/LegL/LegR (kraliçede altı Spider parçası) pivotları korunmalıdır. EnemyModelImport bunu sağlar.

Hareket kat edilen mesafeye bağlıdır; boşta bütün modeli sallama kaldırılmıştır. MeleeStrike hazırlık/darbe/bitiriş zamanını yönetir. 7 yeni kontrol kaçış, iptal, tek darbe ve uzun kare davranışını sınar. Final dalgası barı CurrentWaveDuration kullanır. Toplam 129 çevrimdışı model kontrolü geçti. Yeni shader, animasyon ve sesin Unity/cihaz kontrolü henüz yapılmadı.
