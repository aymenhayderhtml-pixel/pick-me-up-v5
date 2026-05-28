# Pick Me Up - Infinite Gacha
## Unity 6 | Android Portrait 1080x2340 | Samsung Galaxy A34

---

## Project Intent
Build a mobile-first 2D gacha RPG inspired by Pick Me Up Infinite Gacha.
The game should feel like a survival-combat management sim with roster growth,
facility progression, summoning, dungeons, synthesis, and memorial/collection systems.

---

## Workflow Rules
- Code generation: Qwen 3.7 Max / DeepSeek V4 for large drafts
- Code auditing: Codex before anything is applied to Unity
- Apply to Unity: Continue.dev + Unity MCP via VS Code
- Never apply unaudited code to Unity
- Check Unity console after every change and fix errors before the next task

---

## Non-Negotiables
- No DOTween, use coroutines only
- No 3D models, only 2D sprites
- No manual Inspector wiring, use SerializedObject in Editor scripts
- Prefer extend/fix over rewrite for completed files
- All UI must be procedurally built through Editor tools
- New services must be registered in BootLoader.cs in the correct order
- Use ServiceRegistry, not ServiceLocator
- Keep all game scripts in the global namespace

---

## Current Gameplay Pillars
- Hub: central navigation, currencies, stamina, scene access
- Summon: regular gold summon and advanced gem summon
- Roster: hero management, filtering, promotion, synthesis entry point
- Tower: progression climb mode
- Dungeon: daily dungeon runs and stamina spending
- Inventory: equipment, consumables, materials
- Memorial Hall: hero collection / discovery log
- Facilities: passive generation, recovery, upgrade queues
- Synthesis Lab: hero merging and progression rewriting

---

## Hero Data Notes
The following hero roster notes are the current design target for the project.
Use these as the source of truth for content and naming.

| Name | Class | Strength | Intelligence | HP | Agility | Star |
| --- | --- | --- | --- | --- | --- | --- |
| Islat Han | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Enok | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Chloe | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Gide | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Hansen | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Dika | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Jenna Cirai | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Han Israt | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Aaron Delcut | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |
| Antaris | Novice | 14/14 | 10/10 | 12/12 | 12/12 | 1 |

Notes:
- Keep the existing core model unless a new field is explicitly required.
- Base combat stats still belong to HeroDefinition.
- HeroInstance should remain focused on ownership, state, morale, equipment, and progression flags.

---

## Facility Plan
Use these facilities as the main long-term base systems.

1. Workshop - synthesis chamber and armory
2. Square - crack of space and time
3. Dorms
4. Holding Facility
5. Training Hall
6. Flying Dock

Design intent:
- Workshop handles synthesis, crafting, and equipment prep
- Square acts as a story/event hub and anomaly space
- Dorms support recovery, morale, and passive comfort systems
- Holding Facility manages locked, captured, or pending heroes/items if needed later
- Training Hall increases stats, rank, or passive growth systems
- Flying Dock is a travel or world-selection hub for future expansions

---

## Summon Rules
- Regular Summoning uses Gold
- Advanced Summoning uses Gems

Current cost direction:
- Regular summon = gold sink
- Advanced summon = premium gem sink
- Keep pity and banner logic in code consistent with the existing summon architecture

---

## Daily Dungeon Schedule
Use this as the content schedule for dungeon availability.

| Name | Day |
| --- | --- |
| Isralta Mine | Monday and Thursday |
| Kendert Forst | Wednesday and Tuesday |
| Sinmiel Plateau | Friday |
| All Dungeons | Every day |

Implementation note:
- Treat schedule data as content, not hardcoded scene rules.
- Prefer data-driven availability so later tuning does not require rewriting scenes.

---

## Big Plan
### Phase 1 - Solidify Core Loop
- Keep Boot -> Hub -> Summon -> Roster -> Tower working cleanly
- Make sure the new service set stays stable after Unity reimport
- Verify save/load, currency, pity, roster, and tower progress all survive restart
- Make hero discovery consistent between Gacha, Roster, and Memorial Hall

### Phase 2 - Expand Management Systems
- Finish Dungeon flow and daily dungeon availability
- Expand Inventory so equipment can be equipped, unequipped, sold, and previewed
- Expand Facilities so passive generation and recovery feel meaningful
- Add stronger synthesis rules and better warning UX

### Phase 3 - Content and Identity
- Populate hero content with final names, traits, portraits, and lore
- Add Memorial Hall entries and discovery completion rewards
- Tune summon rates, gold sinks, gem sinks, and promotion costs
- Add localized or flavor text for the manhwa-inspired tone

### Phase 4 - Progression and Balance
- Add daily dungeon rewards and stamina tuning
- Balance facility upgrade pacing
- Balance synthesis outcomes, morale penalties, and promotion costs
- Add late-game scarcity and sink systems so progression stays interesting

### Phase 5 - Polish and Scale
- Add audio manager and combat feedback polish
- Add object pooling for roster and card-heavy screens
- Move from Resources.Load to Addressables when content volume grows
- Add test coverage or editor validation for save/load and UI setup scripts

---

## Research Prompt For Another AI
Use this if you want an external model to help with research without burning many tokens:

```text
You are helping design a Unity 6 mobile gacha RPG inspired by Pick Me Up Infinite Gacha.
I need concise research notes, not long explanations.
Focus only on:
1. Mobile gacha RPG UI/UX patterns for portrait Android games
2. Data-driven daily dungeon scheduling systems
3. Facility/passive progression systems in gacha management games
4. Hero collection / memorial hall / codex UX patterns
5. Inventory and equipment management patterns for mobile RPGs

Constraints:
- Give short bullet points only
- Prefer current best practices and examples
- Avoid long essays
- Do not repeat generic advice
- If something is uncertain, say so directly
- Give a small implementation checklist at the end
```

---

## Reference Notes For The Project
- Keep scene names stable unless the project explicitly changes them
- Keep UI procedural and editor-driven
- Keep service resolution centralized through ServiceRegistry
- Keep game state in GameSaveData and avoid introducing parallel save systems unless required
- Prefer simple, testable data structures over clever abstractions

---

## Development Priority
1. Keep the current playable loop stable
2. Expand daily dungeon and facility systems
3. Add inventory/equipment depth
4. Add memorial hall discovery completion
5. Add balance and polish
6. Add content, story, and late-game systems
