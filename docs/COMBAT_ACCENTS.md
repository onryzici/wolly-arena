# Additional cartoon combat effects

Six event-driven accents use the existing `PowerInk` shader:

- Critical hit: sharp four-point flash and compact ground ripple.
- Enemy defeat: small five-point pop and a short ripple.
- Gold/chest collection: two gold sparkles near the collected reward.
- Level gain: narrow mint spiral rising from the feet with an expanding ring.
- Dash launch: two directional ground chevrons moving behind the starting point.
- Boss defeat: a rising gold star and a larger ground celebration ring.

`CombatAccentFX` owns 16 pooled accents, with two reusable LineRenderers per accent and one shared material. Four slots are reserved for dash/level/boss events; ordinary hits and pickups cannot replace those slots. Per-event throttles range from 0.075 to 0.25 seconds and lifetimes from 0.3 to 1.05 seconds. The pool does not create objects or consume gameplay randomness when an event fires.

The effects use scaled combat time, freeze while paused, and clear outside the Wave/WaveClear phases. No added smoke, full-screen flashes, post-processing, camera shake or combat-balance changes.

## Offline validation

Runtime and Editor C# compilation passed. All 164 model checks passed, including seven new checks covering throttling boundaries, independent pickup cooldowns, 6,000 scheduled effect attempts, reserved slots, bounded lifetimes and invalid input. Tests run the production `CombatAccentBudget` scheduling code without Unity.

Event connections were inspected in CombatRewards and DodgeAbility. Unity rendering, material appearance, effect timing during actual combat and device performance have not been tested. No Unity Editor, Play/Stop or build was launched.
