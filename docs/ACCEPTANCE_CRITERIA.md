# Full-Game Acceptance Criteria

This is the release gate for Paint & Blend. The goal is a complete, polished, playable Android 3D game matching the approved images and game design—not a prototype or a source-code-only handoff.

## 1. Reference-matched visual quality
- One coherent, continuous building with all 25 approved room environments.
- Grand Hall must visibly match the reference: huge bright atrium, glass dome, pink stone arches, balcony/mezzanine, twin red carpet staircases, chandelier, tiered blue fountain, banners, planters, polished floor and warm daylight.
- Every room must preserve its reference-specific layout, dominant palette, furniture silhouettes and recognisable prop assortment.
- No generic furniture kit repeated in every room; no unrelated furniture in themed rooms.
- All props and architecture must be authored as finished meshes with correct scale, clean topology, bevels/normals, UVs, PBR materials, collision proxies, LODs and mobile performance settings.
- No primitive cubes, placeholder texturing, missing assets, floating props, mesh intersections, clipping, z-fighting or unfinished lighting in release scenes.
- Room and prop proportions must be validated against the same 1.75m mannequin, doors, counters, chairs, beds, shelves and hiding spaces.

## 2. Complete connected world
- The 25 rooms are part of one navigable complex, not unrelated menu thumbnails or separate disconnected scenes.
- Doors/openings, corridors, stairs, landings, balconies, garden, basement, attic and rooftop connections are walkable.
- No invisible blockers in valid doorways and no pass-through gaps in walls.
- Every floor and prop has correct collision; player and seeker AI cannot enter solid objects.
- Every legal hiding position has a tested pose anchor, body clearance, floor support, ceiling clearance and a reachable navigation path.
- Validate all hiding anchors using the complete character volume for each supported pose.

## 3. Character and animation
- Same shared humanoid character proportions and rig for humans and bots, nominal standing height 1.75m.
- Complete production rig, skin weights, materials, animations and LODs.
- All poses in the design are available and animate correctly: stand, sit, crouch, kneel, lean, folded arms, arms raised and lie down; add custom supported poses where authored.
- Movement, camera, weapon grip, aiming, paint application and collision remain correct during animation.
- Paint changes only character appearance; it never scales the character or alters the hitbox.

## 4. Camouflage and painting
- Hiders and Seekers can paint during the 90-second preparation phase.
- Colour picker, eyedropper/object colour sampling, palette, recent colours, brush types, brush-level/size/opacity/hardness/flow, eraser, undo, redo and reset all work.
- Paint covers the full body UV layout, remains attached to the correct body parts while animated and is consistent for other network players.
- Surface matching supports wood, marble, stone, concrete, fabric, ceramic, painted wall, metal, foliage, plastic and glass.
- Camouflage scoring considers colour, texture, pose, silhouette, lighting and position; it cannot be increased by dumping paint indiscriminately.
- Paint data is replicated/validated without allowing clients to spoof hidden state.

## 5. Match rules and timings
- 90-second lobby countdown, team assignment, 90-second paint/hide preparation, 180-second active round, 30-second reveal/results.
- Minimum two participants with at least one Hider and one Seeker; teams balanced as evenly as possible.
- Hiders win when at least one survives until the active timer expires.
- Seekers win when every Hider has been caught.
- A match ends cleanly on all terminal conditions; there is no stuck or contradictory phase.
- Round results, surviving/caught counts, score/XP, rematch and return-to-lobby work.

## 6. Paint blaster combat
- Finished colourful toy/pichkari weapon model with correct hand grip, aim, firing, reload, animation, muzzle flash, projectile/impact and audio.
- One shot per trigger and one-second reload per player.
- Paint splash effect and ground/object decal last two seconds.
- A splash visual alone is not a hit; only a valid direct server-confirmed hit catches a Hider.
- Friendly fire is disabled. All clients obey the same range, cooldown and team rules.
- Seekers can catch Hiders; Hiders cannot eliminate Seekers using an unintended exploit.

## 7. Bots
- Bots fill vacant slots and behave as normal participants.
- Hider bots choose legal slots and poses, match body paint to the reference object's colour/material and avoid obvious silhouettes.
- Seeker bots explore rooms, inspect suspicious objects and react to visual/audio/paint evidence.
- Bots must not receive hidden transforms or private hider locations.
- Bots obey collision, doors, pathfinding, line of sight, timings, reload and hit rules.
- Easy/Normal/Hard are distinguishable and fair.

## 8. Online rooms and multiplayer
- Quick Play finds an appropriate live session.
- Create Room generates a private room with a unique ID and shareable link.
- Join Room accepts either the ID or a valid invitation link.
- Match capacity, late join policy, bot backfill and player roster are deterministic.
- Production server authority validates movement, teams, phase transitions, shot cadence, hits, score and elimination.
- Disconnect/reconnect, server loss, expired invites, invalid room IDs and latency have clear handling.
- Two or more physical Android devices finish repeated games across real independent network connections.

## 9. Full mobile UX
- Complete main menu, profile, player customization, map/room selection, play online, create/join room, practice, settings, results and progression screens.
- All visible buttons/actions are implemented; no mock navigation, inert controls, fake session-success messages or unfinished panels.
- Landscape controls include movement, camera look, interact, jump/crouch, shoot and spectate.
- UI adapts to safe areas, aspect ratios, touch sizes and performance constraints.
- HUD includes team counts, phase/timer, minimap, paint options and useful feedback.

## 10. Audio, effects and visual polish
- Complete original or properly licensed game audio: music, room ambience, footsteps, UI, paint weapon, splash, hit, timer, reveal, results and menu.
- Production VFX for paint trails, splash, impact, hit confirmation, elimination and reveal.
- Coherent mobile URP lighting, shadow quality, reflections, post-processing, environment audio and colour grading.
- Consistent frame rate, memory usage, thermal behaviour and battery consumption on agreed minimum Android devices.

## 11. QA and release evidence
- Unity 2022.3.50f1 imports and compiles cleanly.
- Automated EditMode/PlayMode tests and end-to-end match regression pass.
- Hiding/collision/pathfinding test coverage includes all rooms and pose classes.
- Online security, latency, reconnect and abuse testing passes.
- Physical Android device testing passes at multiple screen sizes and performance tiers.
- Reproducible debug APK and signed release APK/AAB exist and install/run.
- App signing, permissions, privacy policy/disclosures, release metadata, changelog and known issues are ready.

## Completion rule
The game is finished only when all mandatory criteria have implementation evidence and relevant tests pass. A Unity project folder, a blockout scene, a successful static CI job, a Blender script or an APK that merely launches does not satisfy this release gate.
