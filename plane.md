# Pick Me Up — v1.2 Roadmap (plane.md)

Version: 1.2
Target: Mobile landscape (2340×1080) — focus on readability, consequence, ritual, and persistence.

## Summary
v1.2 transforms the prototype into a tighter, mobile‑first loop: clearer roster presentation, a meaningful Memorial Hall, readable dungeon cadence, robust facilities & inventory, and polished one‑thumb UI. The goal is not to add systems indiscriminately but to make the existing loop feel deliberate and valuable.

Key themes
- Clarity: readable roster, consistent naming, clear stats and actions.
- Consequence: synthesis decisions feel meaningful and Memorial Hall grants legacy value.
- Reliability: persistent saves, timers, and deterministic synthesis with migration-safe saves.
- Mobile polish: landscape-first UX, one‑thumb reachability, safe touch targets.

## What changed since the previous plane.md
- Removed vague “big overhauls” and replaced with phased, deliverable milestones.
- Promoted immediate blockers (back button, missing sprites, summon animation, empty dungeon schedule) to P0 fixes.
- Added explicit acceptance criteria, test plan, and PR checklist compatible with project constraints (procedural UI, SerializedObject, no DOTween, ServiceRegistry/BootLoader).
- Added save migration strategy and persistent timers as mandatory for facilities and pity.

## Phases & timeline (8 weeks)
Phase 0 — Stabilize & Patch (days 0–7)
- Fix P0 bugs: back button visibility, missing hero sprites fallback, summon animation fallback, dungeon availability binding.
Done when: Mobile hotfix build has no blocker bugs and testers can proceed.

Phase 1 — Identity Lock & Roster Clarity (week 1–2)
- Canonicalize hero names, faction tags, role badges.
- Roster UI: responsive grid, dedupe logic, fallback sprites, larger fonts, explicit stat order:
  NAME | CLASS | STRENGTH | INTELLIGENCE | HP | AGILITY | STAR
- Per-hero actions: Synthesis, Promote, Assign (overflow/menu or hero detail modal).
Done when: Roster loads without duplicates, stats legible on 1080×2340, actions reachable with one thumb.

Phase 2 — Memorial & Synthesis (week 2–4)
- Memorial Hall: discovery state, Echoes (passive or lore rewards), first-acquisition reward.
- Synthesis Lab: deterministic previews, interconnected hero visuals, confirmation modal, rollback/guard for accidental loss.
Done when: First acquisition grants Memorial entry; synthesis shows clear preview & confirmation.

Phase 3 — Facilities & Economy (week 4–6)
- Facilities: full building art, upgrade modal with confirmation, persistent timers, assignable hero slots applying passive buffs, enterable interactions for key buildings.
- Economy: gold/gem sinks through facilities, dorm upkeep, synthesis costs; stamina remains core short-session gate.
Done when: Facilities persist across sessions, assigned hero buffs apply, and upgrade timers survive restarts.

Phase 4 — Dungeon Rhythm & Content (week 6–8)
- DungeonSchedule asset (7-day cadence) editable via Editor tools; dungeon cards show "OPEN TODAY" and disable run when closed. Add squad selection screen.
- Rewards: first‑clear + daily caps, stamina checks, clear UI for weekly schedule.
Done when: Dungeon UI reflects schedule and squad selection flow is functional.

Phase 5 — Polish & Retention (week 8+)
- UI polish: summon ritual, consistent palette, subtle particle focus, landscape transitions.
- Retention features scoped for later: login rewards, offline rewards, events, achievements, messaging.
Done when: Core loop polished and a plan exists for retention features without breaking save/schema.

## Priorities (short)
Priority A (must fix before next test round)
- Back button coverage & safe area fixes
- Hero sprite fallback
- Summon animation fallback/fix
- Dungeon availability binding
- Save/Load hardening + migration scaffolding

Priority B (ship in v1.2)
- Roster improvements (layout, fonts, actions)
- Synthesis preview & confirmation
- Facilities: upgrade modal, persistent timers, assign hero
- Memorial Hall discovery + Echoes

Priority C (polish/content)
- Summon visual tuning & palette
- Inventory materials (manhua assets)
- Tower & Memorial UI overhauls (phase epics)
- Performance pass (sprite atlas, GC reductions)

## Implementation constraints & rules (non‑negotiable)
- Procedural UI only; Editor tools must use SerializedObject to create/persist UI prefabs.
- No DOTween; use Animator for complex animation and coroutines for sequences and timers.
- Keep global namespace (do not add namespace blocks).
- Use ServiceRegistry + BootLoader.cs for service ordering and runtime assertions.
- Save migrations: versioned save schema with upgrader functions on load.
- All configuration data (dungeons, facilities, banners, items) must be data‑driven (ScriptableObjects or JSON).

## Acceptance criteria (per area)
Roster
- No duplicate heroes in a single roster load.
- Missing sprite replaced by fallback silhouette; missing assets logged.
- Stats and labels match canonical order and are legible on 1080×2340.

Summon
- Summon animation completes or falls back cleanly.
- Result mapping, pity counters, and banner rules persist across restarts.

Synthesis
- Deterministic preview shown before commit.
- Confirmation modal prevents accidental loss; high-value hero synth shows extra confirmation.

Facilities
- Upgrade opens modal with cost, timer, confirmation; timer persists across save/load.
- Assigning a hero applies passive buff to combat & UI shows assigned hero.

Dungeon
- DungeonSchedule asset controls availability; open/closed UI state visible.
- Squad selection screen before run.

Save/Load
- Old saves load without fatal errors; migration path applied and logged.
- Timers, pity counters, currencies persist.

## Test Plan
- Automated playmode tests: roster load, summon end-to-end, synthesis preview commit, facility upgrade persistence, dungeon availability.
- Manual QA checklist for each screen (visibility, tappable areas, persistence).
- Run acceptance on a physical Android device (1080×2340) and in Unity Editor with same reference resolution.

## PR & Release checklist
- Branch off: feature/1.2-<short-name>
- PR body: summary, migration notes, affected services, test steps, screenshots.
- Include playmode/unit tests where applicable.
- BootLoader: add and document new services; runtime asserts that list services & versions.
- Editor tools: use SerializedObject only; include small README snippet in PR.
- No DOTween; no new namespace blocks; Unity console clean after scene load.
- Add migration notes in CHANGELOG and a sample upgrader for save schema.

## Suggested immediate tasks (first sprint)
1. Hotfix: back button z-order and SafeArea spacer (P0).
2. Hotfix: roster cell fallback sprite & missing-sprite logging (P0).
3. Hotfix: dungeon availability binding + friendly "none available" state (P0).
4. Implement DungeonSchedule ScriptableObject + Editor creation tool (Phase 4 prep).
5. Create Synthesis preview UI & confirmation modal (Phase 2).

## Next steps & recommendations
- Ship P0 fixes immediately to unblock testers.
- After hotfix, lock hero names/roles and implement roster layout improvements in one focused sprint.
- Use feature branches and small PRs (<500 LOC) so Unity console issues are easy to debug.
- Create one EditorWindow to author DungeonSchedule and one for Facility configs so designers can tune without code.

## Notes for engineers
- Save migrations: store a top-level "saveVersion" integer. On load, run upgrade steps to the current version; write migration unit tests for each upgrader.
- Persistent timers: store (startTimestampUtc, durationSeconds, pausedFlag); compute remaining time on load; rely on system time but log discrepancies for debugging.
- Coroutines: provide a CancellableTimer utility that uses a CancellationToken-like struct to stop on scene unload.
- Roster dedupe: ensure HeroInstance has unique instanceId, and dedupe by instanceId at load.
