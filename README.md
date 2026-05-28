# Pick Me Up - Infinite Gacha

A mobile-first 2D gacha RPG built with Unity 6 for Android (1080x2340, Samsung Galaxy A34).

## Overview

Inspired by *Pick Me Up Infinite Gacha*, this game features summoning heroes, managing a roster, entering dungeons and towers, upgrading facilities, equipping inventory items, discovering heroes in a memorial hall, and synthesizing or removing heroes through a dedicated lab.

## Features

- **Summoning** – Regular (Gold) and Advanced (Gems) summoning with banner and pity logic
- **Roster Management** – View, filter, and manage your hero collection
- **Dungeons & Towers** – Daily dungeons with rotating schedules and tower progression
- **Facilities** – Workshop, Square, Dorms, Holding Facility, Training Hall, Flying Dock
- **Inventory & Equipment** – Equip and manage items for your heroes
- **Memorial Hall** – Discover and track hero lore and progress
- **Synthesis Lab** – Combine and manage heroes

## Tech Stack

- **Engine:** Unity 6
- **Platform:** Android (Portrait 1080x2340)
- **Target Device:** Samsung Galaxy A34
- **Architecture:** ServiceRegistry-based dependency injection (no namespaces, no DOTween, 2D sprites only)

## Getting Started

1. Clone the repository
2. Open the project in Unity 6
3. Load the Boot scene
4. Play!

## Project Structure

- `Assets/Scenes/` – All game scenes
- `Assets/Scripts/` – Game scripts and services
- `Assets/ScriptableObjects/` – Data-driven configuration assets
- `Assets/Resources/` – Runtime-loaded resources
- `Assets/Settings/` – Project settings
- `ProjectSettings/` – Unity project configuration

## Development

This project follows strict conventions:
- No DOTween (coroutines only)
- No 3D models (2D sprites only)
- No manual Inspector wiring (Editor scripts use SerializedObject)
- No namespaces in game scripts
- UI is procedurally built through Editor tools