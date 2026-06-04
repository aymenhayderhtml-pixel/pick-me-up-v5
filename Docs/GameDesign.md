# Game Design

## Vision
A mobile-first 2D gacha RPG inspired by *Pick Me Up Infinite Gacha*. Players summon heroes, build a roster, explore dungeons and towers, and manage facilities.

## Core Loop

```
Summon → Collect → Upgrade → Enter Dungeon/Tower → Earn Rewards → Repeat
                          ↕
                    Manage Facilities
```

## Hero System

- **Classes**: Warrior, Mage, Archer, Healer, Tank, Novice (base class)
- **Rarity**: 1★ to 5★ via promotion
- **Stats**: Strength, Intelligence, HP, Agility — each with current/max values
- **Discovery**: Heroes are logged in the Memorial Hall on first acquisition

### Starting Heroes (1★ Novice)

| Name | STR | INT | HP | AGI |
|---|---|---|---|---|
| Islat Han | 14 | 10 | 12 | 12 |
| Enok | 14 | 10 | 12 | 12 |
| Chloe | 14 | 10 | 12 | 12 |
| Gide | 14 | 10 | 12 | 12 |
| Hansen | 14 | 10 | 12 | 12 |
| Dika | 14 | 10 | 12 | 12 |
| Jenna Cirai | 14 | 10 | 12 | 12 |
| Han Israt | 14 | 10 | 12 | 12 |
| Aaron Delcut | 14 | 10 | 12 | 12 |
| Antaris | 14 | 10 | 12 | 12 |

## Currencies
- **Gold** — Regular summoning and basic purchases
- **Gems** — Advanced summoning and premium purchases
- **Stamina** — Dungeon/tower entry cost, regenerates over time

## Summoning

| Type | Cost | Feature |
|---|---|---|
| Regular | Gold | Base pool, low pity |
| Advanced | Gems | Higher rarity rates, pity counter |

- Banners and pity thresholds are data-driven for easy tuning.

## Facilities

| Facility | Purpose |
|---|---|
| Workshop | Synthesis, crafting, equipment |
| Square | Anomaly/event hub |
| Dorms | Recovery and comfort |
| Holding Facility | Lockup / storage |
| Training Hall | Growth and stat development |
| Flying Dock | Travel/world access (future) |

## Daily Dungeons

| Name | Available |
|---|---|
| Isralta Mine | Monday, Thursday |
| Kendert Forst | Tuesday, Wednesday |
| Sinmiel Plateau | Friday |
| All Dungeons | Every day |

## Progression Systems

- **Synthesis Lab** — Combine heroes for promotion, with warnings on high-rarity sacrifices
- **Memorial Hall** — Codex of discovered heroes with lore unlock tracking
- **Inventory** — Equipment and items with equip/unequip flow
- **Tower** — Ascending floors with increasing difficulty, rewards at milestones

## Economy Principles

- Early game: Generous summoning to build roster variety
- Mid game: Resource gates encourage daily engagement (dungeons, facility collection)
- Late game: Promotion and equipment depth as primary progression
- Stamina: Regenerates over time, can be reserved with items
