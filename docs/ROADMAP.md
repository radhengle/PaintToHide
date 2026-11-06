# Development Roadmap — Full Game Release

## Release objective

Ship **Paint & Blend as a complete, polished, online-capable Android 3D game** that matches the approved reference images. The deliverable is the finished game, not a prototype, scene generator, technical demo or source-only handoff.

No milestone is accepted because code exists or a project compiles. Every milestone requires working implementation, visual review and relevant automated/manual test evidence.

## M0 — Foundation and production pipeline
- [x] Dedicated repository and separate Unity project.
- [x] Visual direction and reference catalogue.
- [x] 25-location map catalogue and connected-building plan.
- [x] Unity runtime/editor code foundation.
- [x] Static CI validation for project manifest, room catalogue and Blender generator syntax.
- [x] Deterministic Unity batch-mode scene generation and Android ARM64 IL2CPP build entry point.
- [x] Unity EditMode test and Android APK CI workflow added; execution is gated on repository Unity license configuration.
- [ ] Unity Editor import/compilation, EditMode test run and Android APK build verified on a licensed Unity runner.
- [ ] Unity 2022.3.50f1 project opens cleanly and compiles with zero errors/warnings requiring action.
- [ ] Blender generator executes successfully and exports all declared asset kits.
- [ ] Generated files are imported into Unity and reviewed in the target URP renderer.

## M1 — Complete world and environment art
- [ ] Build one continuous, navigable Grand Hall Complex containing all 25 locations.
- [ ] Model every room against the approved individual image references, not generic repeated room kits.
- [ ] Complete signature architecture: glass dome, pink arches, mezzanine balconies, twin red staircases, chandelier, fountains, banners, windows, doors, corridors and stair links.
- [ ] Complete and dress every listed room: Grand Hall, Living Room, Royal Palace, Kitchen, Dining Room, Bedroom, Bathroom, Library, Office, Art Gallery, Garden, Storage Room, Classroom, Game Room/Arcade, Cinema, Gym, Music Studio, Laboratory, Garage, Kids Room, Laundry Room, Workshop, Attic, Basement and Rooftop.
- [ ] Build all unique furniture, decorations, clutter, plants, appliances, vehicles, displays and room-specific props shown in references.
- [ ] Every mesh has clean topology, bevel/normal treatment, UVs, authored materials, correct pivot, collision proxy and appropriate LOD/culling.
- [ ] All props have believable scale relative to the 1.75m player, doors and room architecture.
- [ ] Doorways, corridors, stairs and transitions remain physically open and traversable.
- [ ] Every room has tested collision, NavMesh, hiding anchors and seeker routes.
- [ ] Visual acceptance review against each reference image; no obvious placeholder primitives, floating props, intersections, clipping or missing key assets.

## M2 — Finished character, animation and mobile controls
- [ ] One consistent, rigged humanoid mannequin character for all players and bots.
- [ ] Production skinning, facial/character presentation, materials, LODs and animation controller.
- [ ] Complete locomotion: idle, walk, run, turn, stop, jump/land, hit, caught, spectate and victory/defeat.
- [ ] Complete pose set: stand, sit, crouch, kneel, lean, arms folded, arms raised, lie down and supported custom poses.
- [ ] Character silhouette, height and collision capsule remain consistent in every pose and paint state.
- [ ] Mobile landscape camera, movement joystick, camera look, jump, crouch, interact, shoot and spectate controls.
- [ ] Touch controls support different screen sizes, safe areas and user-adjustable sensitivity.

## M3 — Complete camouflage and paint studio
- [ ] Runtime UV paint layer for the complete character mesh, including moving/animated body parts.
- [ ] Colour wheel, RGB/HSV selection, sampled object colour/texture, recent colours and palette presets.
- [ ] Brushes: round, soft, hard, spray, texture/stipple, detail and eraser.
- [ ] Brush size, opacity, hardness, flow and brush-level controls.
- [ ] Undo, redo, reset and stroke grouping; no dropped touch strokes or accidental UI painting.
- [ ] Object/surface reference selection and material families for wood, marble, concrete, fabric, ceramic, stone, wall paint, metal, foliage, plastic and glass.
- [ ] Camouflage evaluation accounts for colour, surface texture, pose, silhouette, lighting and position.
- [ ] Painting is available to Hiders and Seekers during the preparation phase.
- [ ] Character scale, hitbox and collision never change when paint/texture is applied.

## M4 — Complete match, combat and presentation
- [x] Offline practice match phase clock, balanced team assignment, participant catch state and Hider/Seeker win-condition logic implemented with EditMode test coverage (Unity execution still pending).
- [ ] Main menu, profile, customize, theme/map selection, shop/rewards entry points, settings, help and exit flows are functional.
- [ ] Quick Play, Create Private Room, Join by Room ID, Join by invitation link and Practice with Bots are implemented.
- [ ] Match lifecycle: 90-second lobby, team assignment, 90-second preparation, 180-second active round and 30-second reveal/results.
- [ ] Minimum two participants; balanced Hider and Seeker teams; support configured player capacity.
- [ ] Seeker/Hider role presentation, score, player counts, timers, minimap, objective feedback and spectating.
- [ ] One toy-like Holi/pichkari colour blaster per player, with polished model, recoil, muzzle flash, projectile/impact, audio and hit feedback.
- [ ] Exactly one shot per trigger; enforced 1.0-second reload; splash VFX and decal lifetime of 2.0 seconds.
- [ ] Server validates shot cadence, hit, team, distance and elimination. Friendly fire is disabled.
- [ ] Reveal highlights remaining Hiders for 30 seconds, followed by results, XP/score and rematch/lobby flow.
- [ ] No inert controls, fake confirmations, missing screens, debug UI or unhandled error paths.

## M5 — Competent bots and fair play
- [ ] Bots fill unoccupied roster slots without preventing real players from joining.
- [ ] Hider bots identify legal hiding spots, choose appropriate poses, select surface-matched colour/texture and apply plausible camouflage.
- [ ] Seeker bots navigate connected rooms, search likely locations, inspect visual differences and react to legitimate evidence.
- [ ] Bot perception is limited to line of sight, sound, paint evidence and gameplay observations. No direct access to hidden-player positions.
- [ ] Difficulty levels vary decision quality, search strategy and reaction time without cheating.
- [ ] Bot pathfinding, movement, collision and shot cooldown obey the same game rules as players.
- [ ] Test bots against human players over repeated seeded scenarios.

## M6 — Production online multiplayer
- [ ] Select and document the production networking, lobby, relay/transport and hosting solution.
- [ ] Server-authoritative match state, team allocation, movement validation, shooting, hits, eliminations, timers and results.
- [ ] Quick matchmaking, private room creation, unique room IDs and shareable invite links.
- [ ] Human players and bots fill the session to configured capacity.
- [ ] Reconnect, disconnect, host/server loss, late join, session expiry and duplicate requests are handled.
- [ ] Network interpolation/prediction, bandwidth controls, latency handling and abuse protection.
- [ ] Two or more physical Android devices complete repeated matches over separate real network connections.
- [ ] Client cannot forge team, role, position, shot, hit, score, elimination, countdown or phase changes.

## M7 — Audio, VFX, polish and optimization
- [ ] Original/licensed sound effects for UI, movement, footsteps, paint spraying, splash, hit, timer, reveal and results.
- [ ] Music and ambient audio for every environment theme with volume controls.
- [ ] Finished colour-splash particles, hit decals, paint transitions, UI feedback, role/phase transitions and reveal effects.
- [ ] Production lighting, reflections, shadows, post-processing and colour grading tuned for mobile URP.
- [ ] GPU/CPU frame-time, memory, thermal, battery and network profiling on target Android devices.
- [ ] LOD, occlusion culling, light baking, texture compression, pooling and asset loading tuned for the entire complex.
- [ ] Stable target frame rate on the agreed minimum device; no game-breaking frame drops, leaks or crashes.

## M8 — QA, release and handoff
- [ ] Unity compile/import validation with no unresolved errors.
- [ ] EditMode, PlayMode and gameplay automation tests for the core systems.
- [ ] End-to-end match tests for every phase, both teams, all win/loss paths, bots and private/quick rooms.
- [ ] Collision/hiding test sweep for all rooms, poses and hiding slots.
- [ ] Reference-art review and gameplay UX review on multiple Android aspect ratios.
- [ ] Multiplayer load, reconnect, latency, security and anti-cheat test pass.
- [ ] Physical-device smoke, regression, performance and thermal test pass.
- [ ] Android debug APK and signed release APK/AAB built reproducibly.
- [ ] Release configuration, signing, permissions, privacy disclosures and store metadata prepared.
- [ ] README, setup instructions, release notes, known issues and source handoff complete.

## Current truth

The repository currently contains a first implementation foundation, 25-room Unity scene generator, starter runtime systems and first-pass Blender model generators. The CI workflow validates repository structure and Python syntax only. It does **not** prove that Unity compiles, that the Blender models are production-quality, that the game is playable, that online multiplayer works, or that an APK exists.

All unchecked items above remain release work. Keep the pull request in draft until the relevant tests pass. Do not merge to `main` or label the game complete before the release gates are satisfied.
