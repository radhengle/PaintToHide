# Acceptance Criteria

## Art and world
- One continuous scene with room connections, upper/lower floors and outdoor links.
- Room layout matches the approved floorplan.
- Grand Hall signature features match the reference: dome, pink arches, balcony, twin stairs, fountain, chandelier, carpet, banners and plants.
- Each room has its own reference-matched prop set and material palette.
- No office furniture copied into unrelated rooms.
- Production assets have approved topology, UVs, materials, normals, collision, LODs and scale.
- Character height/door/furniture proportions are consistent.

## Gameplay
- Two participants can play one per team.
- Match phase transitions follow 90/90/180/30 seconds.
- UV painting works on the full character while animating.
- Brush, colour sampling, eraser, undo/redo and material profiles work.
- Hiding slots reject full-volume overlaps, floor/ceiling clipping and unreachable positions.
- Weapon reload is 1.0s; splash effect lifetime 2.0s.
- Friendly fire impossible; server validates tags.
- Bots search/hide without hidden-location privileges.

## Online
- Two Android devices complete repeated matches over a real network.
- Private room IDs and links resolve to the right session.
- Reconnect and match authority survive transient network loss.
- Client cannot force hit, kill, team or phase changes.

## Release
- Android device matrix and frame-time profiling.
- Memory, heat and bandwidth tests.
- Regression and anti-cheat suite.
- Debug and signed release APK.
- Release notes and known issues.
