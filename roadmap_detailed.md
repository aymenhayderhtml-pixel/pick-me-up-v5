# Pick Me Up Infinite Gacha Detailed Roadmap

## Phase 1: Identity Lock

### Objectives
- Canonicalize hero names, role labels, and presentation text.
- Remove roster ambiguity on small portrait screens.
- Make the Master perspective feel present in every core scene.

### Tasks
- Normalize hero display names through a shared presentation utility.
- Add consistent role badges and hero subtitles to roster, summon, and memorial views.
- Ensure selection and detail panels always show the same identity language.
- Audit any duplicate or near-duplicate roster naming before adding new content.

### Completion Criteria
- Players can identify a hero at a glance without guessing.
- No roster card or detail panel uses conflicting naming rules.
- Hub, Roster, Summon, and Memorial Hall all reinforce the same tone.

## Phase 2: Memorial and Consequence

### Objectives
- Turn Memorial Hall into a progression space.
- Make Echoes feel like earned legacy rather than a passive counter.
- Make synthesis communicate loss clearly.

### Tasks
- Keep dead heroes visible in the memorial layer instead of deleting them from memory.
- Track echo progress from fallen heroes and show it in the Memorial Hall UI.
- Add stronger synthesis warning text for high-value or loyal heroes.
- Preserve save/load compatibility for new echo-related fields.

### Completion Criteria
- Fallen heroes contribute to the account long after death.
- Memorial Hall feels emotionally different from the rest of the game.
- Synthesis reads as a deliberate sacrifice.

## Phase 3: Economy and Dungeon Rhythm

### Objectives
- Make the weekly dungeon structure visible and understandable.
- Give facilities a stronger role in the resource loop.
- Keep Gold, Gems, Stamina, and upgrade materials in active tension.

### Tasks
- Surface dungeon availability days in the dungeon scene.
- Keep facility labels and flows aligned with their mechanical purpose.
- Verify dungeon rewards, stamina costs, and upgrade sinks support repeat play.
- Keep the 7-day cadence readable on the main dungeon cards.

### Completion Criteria
- Players can understand daily dungeon availability without extra explanation.
- Facilities each do something distinct.
- The economy has meaningful sinks and tradeoffs.

## Phase 4: UI and Mobile Polish

### Objectives
- Make the game feel premium and deliberate.
- Keep the experience portrait-first and thumb-friendly.
- Improve pacing, transitions, and scene hierarchy.

### Tasks
- Review top bars, bottom docks, and primary action placement on 1080x2340.
- Make transitions and modal states feel ritualized instead of generic.
- Ensure procedural editor-generated UI remains the source of truth.
- Remove visual clutter that fights readability on mobile.

### Completion Criteria
- Main actions are reachable with one thumb.
- Screens feel coherent across all core scenes.
- UI hierarchy is obvious without tutorials.

## Phase 5: Retention and Scale

### Objectives
- Prepare the game for live-ops without architectural churn.
- Extend replayability with rotating content.
- Keep the current scene/service structure intact.

### Tasks
- Define seasonal event hooks.
- Add dungeon modifiers and challenge mode scaffolding.
- Reserve ticket-gated or limited-time content for later expansion.
- Keep the codebase ready for additional scenes and future content drops.

### Completion Criteria
- New content can be added without rewriting the core game loop.
- The game has room to grow after the initial prototype.

## Shared Validation
- Boot to Hub path loads without console errors.
- Save/load remains compatible after schema changes.
- Memorial Echo data persists correctly.
- Weekly dungeon availability is visible and stable.
- Roster detail panels show consistent hero identity and equipment text.

