checklist.md

Phase 1 – Core Foundations (Week 1)

✅ World & Chunk System

[ ] Implement chunk grid (16×128×16).

[ ] Add greedy meshing algorithm.

[ ] Async chunk generation with Unity Jobs + Burst.

[ ] Frustum culling + LOD system.

[ ] Flat 32×32 safe spawn area.


✅ Block & Registry

[ ] Create BlockType registry (ScriptableObject).

[ ] Add material atlas (2048×2048).

[ ] Implement durability + break times.

[ ] Implement block tinting system for Rainbow Painter.

[ ] Add special block types (Candy, Robot, Ice).


✅ Inventory & Hotbar

[ ] ItemStack system (blocks + tools).

[ ] 9-slot hotbar with selection highlight.

[ ] Main inventory (27+ slots).

[ ] Tool/Block placement from hotbar.

[ ] Cross-platform input (PC + Android).



---

Phase 2 – Tools & Gameplay (Week 2–3)

✅ Auto-Builder Wand

[ ] StructureAsset blueprint format (RLE voxel data).

[ ] Preview system (transparent ghost).

[ ] Animated placement coroutine (with particles).

[ ] Rotation + scale options.

[ ] Child-friendly structure selection UI.


✅ Adventure Tools

[ ] Jetpack (fuel system + particles).

[ ] Grapple Glove (rope physics + swing).

[ ] Bubble Shield (10s invulnerability + shader).

[ ] Hologram Map (3D minimap + waypoints).


✅ Enhanced Mining Tools

[ ] Rainbow Pickaxe (instant break + rainbow trail).

[ ] Super Shovel (3×3×3 clearing).

[ ] Paint Brush (block recoloring + flood-fill).

[ ] Shape Creator (spheres, pyramids, arches).



---

Phase 3 – UI & Cross-Platform (Week 3)

✅ User Interface

[ ] Cross-platform adaptive scaling.

[ ] Touch gestures: tap break, long-press place, swipe hotbar.

[ ] Virtual joystick for Android.

[ ] Settings menu (platform-specific).

[ ] ParentalControls panel (time limit, friend approval, safe messages).


✅ Character & Pets

[ ] Robbie player model (cube-head).

[ ] Cube Dog pet with follow AI.

[ ] Pet customization (colors, accessories).

[ ] Extendable system for robot/slime pets.



---

Phase 4 – Content & Mini-Games (Week 4)

✅ Biomes

[ ] Forest, Desert, Snow, Beach.

[ ] Candy Kingdom.

[ ] Robot Factory.

[ ] Underwater Palace.

[ ] Biome-specific structures + decorations.


✅ Mini-Games

[ ] Block Dash (parkour + checkpoints).

[ ] Build Battle (timed building challenges).

[ ] Treasure Hunt (clues + hidden chests).

[ ] Creative Challenges (daily prompts + rewards).



---

Phase 5 – Polish & Safety (Week 5)

✅ Optimization

[ ] AndroidOptimization (dynamic scaling, texture streaming).

[ ] PlatformAdapter (device detection, memory management).

[ ] Object pooling for blocks + items.

[ ] Save/load modified chunks.


✅ Safety

[ ] Time limit system.

[ ] Safe multiplayer toggle (off by default).

[ ] Pre-set messages for communication.

[ ] PIN-locked parental panel.

[ ] Autosave every 2 minutes.



---

Phase 6 – QA & Delivery (Week 6)

✅ Testing

[ ] PC low-spec test (4GB RAM, integrated GPU).

[ ] Android mid-tier test (3GB RAM, Adreno 530+).

[ ] Battery usage 1-hour session.

[ ] Child playtest (6–12 year olds).

[ ] Parental control verification.


✅ Build Targets

[ ] Windows 64-bit build.

[ ] macOS Universal build.

[ ] Android ARM64 + ARMv7 APK.

[ ] Development builds with profiler.

[ ] Release builds optimized for store submission.



---

⚡ Completion Rule:
Mark a phase complete only when:

Code compiles without errors.

Runs at target FPS on test hardware.

Matches design intent in agents.md and PDR.



---

Do you want me to also create a tasks.json (machine-readable version of this checklist for codex to parse and schedule builds), or keep it human-readable in markdown?

