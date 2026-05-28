# Pick Me Up Infinite Gacha Roadmap

## Goal
Build a stronger Unity 6 mobile gacha game around the core loop of summon, attachment, risk, loss, memorialization, and growth.

## Phase 1: Identity Lock
Timeline: Week 1-2

Focus:
- Standardize hero names, roles, and display rules.
- Remove confusing near-duplicate names from the main roster.
- Make the Master fantasy obvious across Hub, Roster, Summon, and Tower.

Exit Criteria:
- Roster feels readable on a small portrait screen.
- Hero presentation is consistent across all scenes.
- No naming collisions confuse the player.

## Phase 2: Memorial and Consequence
Timeline: Week 2-4

Focus:
- Turn Memorial Hall into a real progression space.
- Add Echo rewards, lore unlocks, or passive benefits for fallen heroes.
- Make synthesis feel like a meaningful sacrifice with clear consequences.

Exit Criteria:
- Death creates lasting value, not just deletion.
- Memorial Hall has emotional and mechanical purpose.
- Synthesis warnings clearly communicate loss.

## Phase 3: Economy and Dungeon Rhythm
Timeline: Week 4-6

Focus:
- Keep the 7-day dungeon schedule visible in UI.
- Give facilities distinct gameplay purposes.
- Strengthen Gold, Gems, Stamina, and upgrade-material sinks.

Exit Criteria:
- The weekly dungeon cadence is understandable at a glance.
- Facilities feel different from each other.
- Resource decisions create tension instead of clutter.

## Phase 4: UI and Mobile Polish
Timeline: Week 6-8

Focus:
- Make all major screens feel premium and ritualized.
- Keep the layout portrait-first and thumb-reachable.
- Improve transitions so they feel intentional, not generic.

Exit Criteria:
- Main actions are usable with one thumb on 1080x2340.
- Lobby, Summon, Roster, Memorial, and Dungeon feel visually consistent.
- UI flows are procedurally built and editor-driven.

## Phase 5: Retention and Scale
Timeline: Week 8+

Focus:
- Add seasonal events.
- Add rotating dungeon modifiers.
- Add challenge modes and limited-time rewards.

Exit Criteria:
- The game has live-ops hooks without changing the core architecture.
- New content extends replayability instead of replacing the main loop.

## Priority Checklist
- Canonicalize hero naming, faction tags, role badges, and display text.
- Keep Memorial Hall as an emotional progression space.
- Make synthesis a moral choice, not a simple upgrade action.
- Preserve the Gold vs Gems economy split.
- Keep stamina as the primary short-session gate.
- Make the dungeon weekly cadence visible in scene UI.
- Make dungeon cards show whether they are open today and disable the run button when the weekly schedule closes them.
- Make the Hub facility button read `FACILITIES` so the command-center wording matches the new base overhaul.
- Keep all UI procedural and wired through the existing editor pipeline.
- Preserve save/load compatibility while extending the data model.

## Validation
- Boot to Hub loads cleanly and service registration stays in the correct order.
- Summon, Roster, Tower, Dungeon, and Memorial Hall use consistent naming and hierarchy.
- Memorial Echo progress persists across save/load.
- The dungeon schedule is readable without extra explanation.
- Synthesis warnings clearly communicate cost.
- Old saves remain compatible after each schema change.
- The Unity Console stays clean after each phase.

## Assumptions
- The current Unity 6 scene/service architecture remains the base.
- No multiplayer, server-authority, or combat-engine rewrite is included.
- The game stays portrait-first, mobile-first, and DOTween-free.
- Emotional clarity, consequence, and readability matter more than adding systems too early.
