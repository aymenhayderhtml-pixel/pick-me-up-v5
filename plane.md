# Pick Me Up Infinite Gacha Upgrade Roadmap

## Summary
This roadmap upgrades the current Unity 6 prototype into a stronger mobile game by focusing on the loop that matters most: summon, attach, risk, lose, memorialize, and grow. The priority is not adding more systems for their own sake, but making the roster more readable, Memorial Hall more meaningful, dungeon pacing more live-game-like, and the UI more ritualized and one-thumb friendly on 1080x2340 Android.

## Roadmap

| Phase | Timeline | Focus | Done When |
|---|---|---|---|
| 1 | Week 1-2 | Identity lock | Hero names, roles, and presentation rules are standardized; no near-duplicate names remain in the main roster; the Master fantasy is clear across all core screens. |
| 2 | Week 2-4 | Memorial and consequence | Memorial Hall grants real legacy value through Echoes, lore, or passive bonuses; synthesis feels like a deliberate sacrifice with visible cost and aftermath. |
| 3 | Week 4-6 | Economy and dungeon rhythm | The 7-day dungeon schedule is clear in UI; facilities have distinct gameplay purposes; Gold, Gems, Stamina, and upgrade materials create meaningful daily decisions. |
| 4 | Week 6-8 | UI and mobile polish | The lobby, summon, roster, memorial, and dungeon screens feel premium, portrait-first, and thumb-reachable; transitions feel ritualistic instead of generic. |
| 5 | Week 8+ | Retention and scale | Seasonal events, rotating dungeon modifiers, challenge modes, and extra content extend replayability without changing the core architecture. |

## Implementation Priorities

- Canonicalize hero naming, faction tags, role badges, and display text so the roster feels distinct on a small screen.
- Keep Memorial Hall as an emotional progression space, not just a graveyard, and make every death leave something behind.
- Keep synthesis as a moral decision, not a simple upgrade button, and make the warning/result flow clear.
- Keep the current Gold vs Gems split and add stronger sinks through summons, facilities, dorm upkeep, and synthesis.
- Keep stamina as the main short-session gate for now, and reserve special tickets for later event modes.
- Keep the dungeon weekly cadence visible in the scene UI so players understand the schedule at a glance.
- Make dungeon cards show whether they are open today and disable the run button when the weekly schedule closes them.
- Make the Hub facility button read `FACILITIES` so the command-center wording matches the new base overhaul.
- Keep all UI procedural, portrait-first, and wired through the existing editor pipeline, with no manual inspector setup.
- Keep save/load compatible with older saves while extending the data model for echoes, dungeon progress, and future content.

## Test Plan

- Boot to Hub loads cleanly and the service stack registers in the expected order.
- Summon, Roster, Tower, Dungeon, and Memorial Hall all share consistent naming, visual hierarchy, and navigation behavior.
- Memorial Echo progress persists after saving and loading.
- The dungeon week schedule is readable without needing extra explanation.
- Synthesis warnings clearly communicate loss, especially for high-value or loyal heroes.
- Main actions remain reachable with one thumb on a 1080x2340 portrait layout.
- Old saves still load correctly after each schema change.
- The project remains clean in Unity Console after each phase.

## Assumptions

- The current Unity 6 scene/service architecture stays in place and is the base for all future work.
- No multiplayer, server authority, or combat-engine rewrite is included in this roadmap.
- The target filename is `plane.md`, and this roadmap is intended as its content.
- The game stays portrait-first and mobile-first, with procedural UI generation and no DOTween.
- The roadmap favors emotional clarity, consequence, and readability over adding more systems too early.
