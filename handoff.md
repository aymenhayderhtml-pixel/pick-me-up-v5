# Pick Me Up — Handoff for DeepSeek Code Generation

## Project Overview
Unity 6 mobile gacha game. Android portrait 1080×2340, Samsung Galaxy A34.
**You are used ONLY for code generation.** Copy-paste the code from this file to Unity via VS Code.
No MCP access — you write files, human applies them.

## Current State
All scenes built and functional: Boot, Hub, Summon, Roster, Tower.
Roster overhaul completed with dark-themed dropdowns and proper layout.

## CRITICAL RULES (DO NOT BREAK)
- No DOTween — Coroutines only
- No 3D models — 2D sprites only
- No manual Inspector wiring — SerializedObject in Editor scripts only
- Never rewrite completed files — extend or fix only
- New services must be registered in BootLoader.cs in correct order
- Check Unity console after every change — fix errors before next task
- All UI built procedurally via Editor scripts
- All services are plain classes (not MonoBehaviour) unless noted
- Save files use Application.persistentDataPath + "/filename.json"
- Prefabs are created via Editor scripts using PrefabUtility.SaveAsPrefabAsset

## TODO (Priority Order)
1. Test full summon → roster flow with real hero data
2. Add object pooling to Roster (LoopScrollRect — install from GitHub)
3. Migrate Resources.Load to Addressables
4. Add AudioManager service
5. Add global morale penalty after synthesis

---

## TASK 2: Add object pooling to Roster with LoopScrollRect

### What to do:
Install the LoopScrollRect package from GitHub (https://github.com/qiankanglai/LoopScrollRect), then modify the Roster to use it instead of the standard ScrollRect. This prevents creating/destroying cards every time the list changes.

### Files to create/modify:

**1. Install package:**
Add to Packages/manifest.json dependencies:
```
"com.qiankanglai.loopscrollrect": "https://github.com/qiankanglai/LoopScrollRect.git?path=Assets/LoopScrollRect"
```

**2. Modify Assets/Scripts/Editor/SetupRosterUI.cs:**
Replace the standard ScrollRect creation with LoopScrollRect. The prefab cell size stays 380×560. Wire the loopScrollRect.prefabSource and cellSize.

**3. Modify Assets/Scripts/UI/RosterView.cs:**
- Change scrollRect to loopScrollRect (LoopScrollRect type)
- Change gridContent to loopScrollRect.content
- Implement `void ProvideData(Transform transform, int index)` to recycle cards
- Keep existing filter/sort logic but update to work with object pool
- Remove manual card instantiation/destruction

**4. Modify Assets/Scripts/UI/RosterHeroCard.cs:**
Ensure SetupCard() fully resets ALL state at the top (already done per CLAUDE.md line 165).

### Requirements:
- Cards must reuse properly — all visual state reset in SetupCard()
- Filter/sort changes trigger loopScrollRect.RefillCells()
- Scroll position resets on filter change
- Dead heroes still show gray + DEAD overlay
- Detail panel behavior unchanged

---

## TASK 3: Migrate Resources.Load to Addressables

### What to do:
Replace all Resources.Load calls with Addressables.LoadAssetAsync. This means:
- HeroDefinitions (loaded via Resources.LoadAll<HeroDefinition>("Heroes"))
- Hero portraits (loaded via Resources.Load<Sprite>(path))
- UI prefabs (RosterHeroCardPrefab, StarIconPrefab)
- Card frame sprites

### Files to modify:

**Assets/Scripts/Services/GachaService.cs:**
- HeroRoster class currently uses Resources.LoadAll<HeroDefinition>("Heroes")
- Change to Addressables.LoadAssetsAsync<HeroDefinition>("heroes", ...)
- Need to mark all hero .asset files with addressable label "heroes"

**Assets/Scripts/UI/SummonView.cs:**
- PopulateCard() uses Resources.Load<HeroDefinition>($"Heroes/{hero.HeroDefId}")
- Portrait loading uses Resources.Load<Sprite>(def.PortraitSpritePath)
- Change both to Addressables

**Assets/Scripts/Editor/SetupRosterUI.cs:**
- HeroCardPrefab loading uses Resources.Load<GameObject>("UI/RosterHeroCardPrefab")
- Change to Addressables

### Requirements:
- Install Addressables package (com.unity.addressables)
- Create Addressables Groups for Resources/Heroes/ and Resources/UI/
- Use async loading with coroutines
- Handle loading failures gracefully (show "???" for missing data)
- Register Addressables initialization in BootLoader if needed

---

## TASK 4: Add AudioManager service

### Files to create:

**Assets/Scripts/Core/IAudioService.cs**
```csharp
public interface IAudioService
{
    void PlaySFX(string clipName);
    void PlayMusic(string clipName);
    void StopMusic();
    void SetSFXVolume(float volume);
    void SetMusicVolume(float volume);
}
```

**Assets/Scripts/Services/AudioService.cs**
- Plain class, implements IAudioService
- Uses Resources.Load or Addressables to load AudioClips
- Requires AudioSource component — either create one on a GameObject or accept the main camera's AudioSource
- Store SFX volume and music volume in PlayerPrefs
- Simple pool of AudioSources for SFX (allow up to 3 overlapping sounds)

### Files to modify:

**Assets/Scripts/Core/BootLoader.cs**
- Register after TowerService:
```csharp
IAudioService audioService = new AudioService();
ServiceRegistry.Instance.Register<IAudioService>(audioService);
```

**Assets/Scripts/UI/SummonView.cs**
- Add SFX calls: PlaySFX("crack_open") when crack opens, PlaySFX("card_drop") when card drops, PlaySFX("card_flip") at flip midpoint, PlaySFX("star_pop") for each star
- PlaySFX("summon_standard") / PlaySFX("summon_premium") on button press

**Assets/Scripts/UI/HubView.cs**
- PlayMusic("hub_theme") in Start()

### Audio clip locations:
- Place audio clips at Assets/Resources/Audio/SFX/ and Assets/Resources/Audio/Music/
- Load via Resources.Load (or Addressables if Task 3 is done first)

---

## TASK 5: Global morale penalty after synthesis

### File to modify:
**Assets/Scripts/UI/RosterView.cs**

### What to change:
After confirming synthesis (hero is removed), apply a morale penalty to ALL surviving heroes:
- Each surviving hero's morale decreases by a random amount between 5-15
- If a hero's personality is "Loyal", the penalty is doubled (10-30)
- If a hero's personality is "Traumatized", halve the penalty (2-7, rounded down)
- Morale cannot go below 0 or above 100
- Show a brief toast/notification: "All heroes' morale decreased by X-Y"
- Save after applying penalties

### Where in the code:
Find the synthesis confirmation handler (likely in OnSynthConfirm or similar). After calling rosterService.Remove() and closing the synth panel, iterate through all remaining heroes and apply the morale penalty.

---

## EXISTING FILES SUMMARY

### Core (Assets/Scripts/Core/)
| File | Description |
|------|-------------|
| GameSaveData.cs | Serializable save data (Gold, Gems, Stones, Heroes, Pity) |
| HeroInstance.cs | Serializable hero with InstanceId, HeroDefId, Morale, Stars, etc. |
| HeroDefinition.cs | ScriptableObject with HeroId, HeroName, BaseStarRank, Stats |
| BootLoader.cs | Entry point, registers all services, loads Hub scene |
| ITowerService.cs | Interface for tower floor tracking |

### Services (Assets/Scripts/Services/)
| File | Description |
|------|-------------|
| ServiceRegistry.cs | Singleton MonoBehaviour, Register/Resolve/HasService |
| ISaveLoadService.cs / SaveLoadService.cs | JSON persistence to pickmeup_save.json |
| GameStateService.cs | In-memory GameSaveData holder, Save() method |
| ICurrencyService.cs / CurrencyService.cs | Gold/Gems/Stones management, auto-saves |
| IGachaService.cs / GachaService.cs | Standard/Premium summon with pity system |
| IRosterService.cs / RosterService.cs | Hero list CRUD, GetAll/Remove/MarkDead |
| ITowerService.cs / TowerService.cs | Tower floor progress, tower_save.json |

### UI (Assets/Scripts/UI/)
| File | Scene | Description |
|------|-------|-------------|
| HubView.cs | Hub | Currency display, navigation buttons |
| SummonView.cs | Summon | Card reveal animation (3 phases), summon buttons |
| RosterView.cs | Roster | Hero grid, filter/sort, detail panel, promote/synthesize |
| TowerView.cs | Tower | Floor display, enter/continue tower button |
| RosterHeroCard.cs | Roster (prefab) | Card component with SetupCard(), dead overlay |

### Editor (Assets/Scripts/Editor/)
| File | Menu Path | Description |
|------|-----------|-------------|
| ProjectSetupTool.cs | Tools/Pick Me Up/Setup Project | Creates Boot + Hub scenes |
| SetupHubUI.cs | Tools/Pick Me Up/Setup Hub UI | Builds Hub canvas |
| SetupSummonUI.cs | Tools/Pick Me Up/Setup Summon UI | Builds Summon canvas |
| SetupRosterUI.cs | Tools/Pick Me Up/Setup Roster UI | Builds Roster canvas with grid + detail |
| SetupTowerUI.cs | Tools/Pick Me Up/Setup Tower UI | Builds Tower canvas |
| CreateRosterHeroCardPrefab.cs | Tools/Pick Me Up/Create Roster Hero Card Prefab | Creates 380×560 card prefab |

### Scenes
| Scene | Index | Status |
|-------|-------|--------|
| Boot.unity | 0 | ✅ Done |
| Hub.unity | 1 | ✅ Done |
| Summon.unity | 2 | ✅ Done |
| Roster.unity | 3 | ✅ Done |
| Tower.unity | 4 | ✅ Done |

### Manhwa Data Rules (DO NOT BREAK)
- HiddenPotential: 0.0-0.3→"limited", 0.3-0.7→"stirs", 0.7-1.0→"struggles"
- Morale: below 30=red, 30-60=yellow, 60-100=green
- Permadeath: IsAlive=false → gray card + DEAD overlay
- Synthesis: Remove(), NOT MarkDead(). Loyal OR Morale>70 → severe warning
- Promotion: cost = CurrentStarRank * 10 AttributeStones, max 5★

### Registration order in BootLoader.Start():
```
1. ServiceRegistry (auto-create if missing)
2. SaveLoadService
3. GameStateService
4. CurrencyService
5. GachaService
6. RosterService
7. TowerService
8. SceneManager.LoadScene("Hub")
```

---

## HOW TO SUBMIT CODE
For each task, output the COMPLETE file content for each modified/created file, clearly separated:

```
// === FILE: Assets/Scripts/Services/AudioService.cs ===
[full code here]

// === FILE: Assets/Scripts/Core/IAudioService.cs ===
[full code here]
```

Also include BootLoader changes as a SEARCH/REPLACE diff block if needed.
Do NOT include files that don't need changes.