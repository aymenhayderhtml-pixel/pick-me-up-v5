# Pick Me Up - Infinite Gacha
## Unity 6 | Android Portrait 1080x2340 | Samsung Galaxy A34

---

## Team Rules
- Use Codex to audit all new code before Unity import
- Use Continue.dev + Unity MCP to apply changes into Unity
- Never apply unaudited code directly to the project
- Check the Unity Console after every change and fix errors before moving on
- Prefer extend/fix over rewrite for completed files

---

## Hard Constraints
- No DOTween, coroutines only
- No 3D models, only 2D sprites
- No manual Inspector wiring, use SerializedObject in Editor scripts only
- No namespaces in game scripts
- Use ServiceRegistry, not ServiceLocator
- Register new services in BootLoader.cs in the correct order
- Keep UI procedurally built through Editor tools

---

## Game Direction
This is a mobile-first 2D gacha RPG inspired by Pick Me Up Infinite Gacha.
The main experience should revolve around:
- Summoning heroes
- Managing a roster
- Entering dungeons and towers
- Upgrading facilities
- Equipping and using inventory items
- Discovering heroes in a memorial hall
- Synthesizing or removing heroes through a dedicated lab

---

## Canon Hero Notes
Use these notes as the current content target when adding or naming heroes.

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

---

## Facility Notes
Facilities to support long-term progression:
1. Workshop - synthesis chamber and armory
2. Square - crack of space and time
3. Dorms
4. Holding Facility
5. Training Hall
6. Flying Dock

Implementation intent:
- Workshop = synthesis, crafting, equipment prep
- Square = anomaly/event hub
- Dorms = recovery and comfort
- Holding Facility = lockup / storage / pending state
- Training Hall = growth and stat development
- Flying Dock = travel/world access for future content

---

## Summon Notes
- Regular Summoning uses Gold
- Advanced Summoning uses Gems
- Keep banner and pity logic data-driven where possible

---

## Daily Dungeon Notes
| Name | Day |
| --- | --- |
| Isralta Mine | Monday and Thursday |
| Kendert Forst | Wednesday and Tuesday |
| Sinmiel Plateau | Friday |
| All Dungeons | Every day |

---

## Big Plan
### 1. Keep the current playable loop stable
- Verify Boot, Hub, Summon, Roster, Tower, Dungeon, Inventory, Memorial Hall, Facilities, and Synthesis Lab scenes all load
- Verify save/load, currencies, stamina, and pity persist correctly
- Keep the current service architecture stable

### 2. Finish the base management layer
- Flesh out facility upgrades and passive generation
- Finish inventory/equipment flows
- Finish memorial hall discovery tracking
- Polish synthesis behavior and warnings

### 3. Add content and progression depth
- Finalize hero names, classes, portraits, lore, and stats
- Populate dungeons with daily availability
- Tune summon costs, drop rates, promotion costs, and stamina pacing
- Add more hero and item content over time

### 4. Improve usability and scale
- Add audio manager
- Add object pooling for roster and item-heavy screens
- Move large content loading from Resources to Addressables later
- Add validation to editor setup scripts and save system edge cases

---

## Low-Token Research Prompt
Use this when asking another AI to research without using many tokens:

```text
Research only the most useful current best practices for a Unity 6 Android portrait gacha RPG.
Keep it short and practical.
Focus on:
- mobile gacha UI patterns
- daily dungeon scheduling
- facility progression systems
- inventory/equipment UX
- memorial hall / codex UX
- synthesis lab UX

Output format:
- 5 to 10 short bullets
- mention any risks or tradeoffs
- avoid long explanations
- include a short implementation checklist at the end
```

---

## Quick Reminders
- Prefer simple data structures
- Prefer editor-generated UI over hand-wired inspector setup
- Prefer scene names and service names that match the current codebase
- If a change affects save data, preserve old saves and fill defaults safely
- If a change affects UI, keep it portrait-first and mobile-friendly
