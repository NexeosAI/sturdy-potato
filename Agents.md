agents.md

Project Identity

Project Name: RobbieCraft

Type: Voxel-based sandbox adventure game (child-friendly Minecraft-style)

Engine: Unity 2022 LTS, URP

Platforms: Windows, macOS, Android

Target Audience: Children ages 6–12


RobbieCraft is a safe, creative game that combines Minecraft-like building with unique kid-friendly tools, colorful art, and simple mechanics.


---

Role of codex

codex is the build agent responsible for generating Unity scripts, assets, and scene wiring that match the RobbieCraft PDR specifications.
Think of codex as the game engineer assistant:

Convert design requirements into production-ready Unity C# code.

Generate systems that are modular, documented, and easy for humans to extend.

Always optimize for child safety, simplicity, and performance.



---

Scope of Work

codex must implement the following core systems:

1. World & Chunk System

Infinite chunk-based world (16×128×16).

Greedy meshing for optimized chunk meshes.

Async chunk generation (Unity Jobs + Burst).

LODs + frustum culling.

Perlin noise biomes (Forest, Desert, Snow, Beach, Candy Kingdom, Robot Factory, Underwater Palace).



2. Block & Tool Systems

Registry of 20+ block types with durability.

Special blocks: Candy, Robot, Ice.

Rainbow Painter, Auto-Builder Wand, Shape Creator.

Adventure tools: Jetpack, Grapple Glove, Bubble Shield, Hologram Map.



3. Inventory & Hotbar

9-slot hotbar.

Inventory (27 slots+).

Tool/Block placement system driven by hotbar.



4. UI & Controls

Adaptive cross-platform UI.

Touch support on Android (joystick, tap/place, swipe hotbar).

Keyboard/mouse support on PC.



5. Characters & Pets

Robbie (cube-head) player character.

Cube Dog companion with simple AI.

Pet system extensible to robots/slimes.



6. Mini-Games & Extras

Block Dash, Build Battle, Treasure Hunt, Creative Challenges.

Parental controls (time limits, safe chat, friend approval).





---

Performance & Safety Targets

Frame Rate: 60+ FPS on low-end PC, 30+ FPS on mid-tier Android.

Loading: <3s PC, <5s Android.

Memory: <4GB PC, <2GB Android.

Battery: <10%/hr laptops, <15%/hr Android.

Child-friendly:

No ads, violence, or unsafe communication.

Simple tools, low frustration, no complex crafting.

Autosaves every 2 minutes (no permanent loss).




---

Do’s

✅ Follow Unity best practices (MonoBehaviours for runtime, ScriptableObjects for data).
✅ Keep systems modular and documented (XML comments).
✅ Use object pooling, async jobs, and greedy meshing for performance.
✅ Make UI touch-friendly (≥44dp buttons, large fonts).
✅ Ensure safe defaults: flat spawn, friendly pets, no fail states that frustrate kids.
✅ Favor clarity over complexity in code structure.
✅ Include example prefabs, ScriptableObjects, and demo scenes.


---

Don’ts

❌ Don’t implement unsafe communication (no open chat).
❌ Don’t add monetization or ads.
❌ Don’t exceed performance budgets (triangle counts, memory).
❌ Don’t introduce frustrating mechanics (steep learning curve, harsh penalties).
❌ Don’t hardcode values when they can be data-driven (blocks, tools, biomes).
❌ Don’t break cross-platform input (must work on PC + Android).
❌ Don’t skip autosave or parental controls.


---

Implementation Guidelines

File Organization:

Assets/Scripts/
├── World/
├── Tools/
├── Systems/
├── Player/
├── UI/
├── Safety/
├── MiniGames/
└── Characters/

Code Quality:

Use C# naming conventions.

Public APIs documented with XML.

Unit tests for core (WorldGenerator, Inventory, ToolBase).


Scene Setup:

Bootstrap objects: GameManager, WorldManager, AudioManager.

UI Canvas with Hotbar, Inventory, ParentalControls.

Player prefab with controller, camera, inventory.




---

Success Criteria

Kids can place their first block in <5 minutes.

90%+ tool discovery rate without tutorials.

Play sessions average 20–30 minutes.

Smooth 30–60 FPS across target devices.

Parents report safe, fun, educational experience.



---

⚡ Summary:
codex is building RobbieCraft, a Minecraft-like but child-safe sandbox.
The priority is simplicity, safety, performance, and fun.
All code must be Unity 2022-compatible, modular, optimized, and accessible for ages 6–12.


---

Would you like me to also write a checklist.md for codex (a task-by-task implementation guide it can tick off), or keep agents.md as the only meta-file?

