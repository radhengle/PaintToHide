# PAINT & BLEND

**A polished, bright-cartoon 3D multiplayer camouflage hide-and-seek game.** Android first. One enormous connected mansion complex. Hiders paint their human character to blend into environmental objects; Seekers identify and tag them with colorful paint blasters.

> **Project status:** Active foundation implementation. This branch establishes a Unity project shell, game architecture, map plan, and first runtime systems. It is not yet a finished game or verified APK. Production-grade custom meshes, animation, final camouflage shader pipeline, server hosting, and device QA remain explicit work items.

## Locked creative direction

The attached reference images are the visual source of truth:
- Grand Hall: glass dome, pink stone arches, double staircase, chandelier, central blue fountain, red carpets, planters and paint-splash accents.
- Living room: cream sectional, colored cushions, blue rug, wooden coffee table, arched windows, teal curtains, media wall and indoor plants.
- Royal palace: red velvet throne, raised dais, gold trim, lion sculptures, suits of armor, tall windows and red carpet.
- Kitchen: teal cabinetry, blue refrigerator, wooden dining table, yellow chairs, open shelves, cookware, plants and bright tiled floor.
- Garden: pink blossom tree, stone fountain, pergola, lanterns, flower beds, gazebo, stone paths and topiary.
- Art gallery: skylit dome, framed artworks, marble floor, statues, red benches, display plinths and roped displays.
- Storage: metal racks, colored bins, crates, boxes, rolled rugs, ladder, lockers, paint buckets and wooden mannequin-style prop.
- Office: purple/blue walls, wood desks, blue swivel chairs, dual monitors, bookshelves, filing cabinets, printer, noticeboard, arched windows and plants.
- Match UI: 2-team score header, 3-minute timer, circular minimap, left movement stick, right action buttons and bottom paint palette.

Use bright colorful stylized 3D, refined bevels, believable proportions, carefully authored material response, appealing bounce light and finished silhouettes. Do not substitute primitive cubes or placeholder art for final assets.

## Game rules
- Minimum 2 participants; one Hider and one Seeker.
- Lobby 90 seconds; preparation/painting 90 seconds; active round 180 seconds; reveal 30 seconds.
- Hiders win if at least one survives the active round. Seekers win when all Hiders are caught.
- One toy-like Holi colour blaster per player; server-enforced 1-second reload.
- Impact splash VFX lifetime: 2 seconds.
- All players use the same base humanoid rig and nominal 1.75m height.
- Bots may fill empty slots but must obey normal rules and receive no privileged hidden-player positions.
- Players remain human. Camouflage changes body paint, texture, pose and positioning; players never become furniture or clip into solid objects.

## World scope
A single persistent interconnected Grand Hall Complex with wings, floors, corridors, large open doorways, staircases and outdoor-connected areas. Planned rooms: Grand Hall, living room, royal palace, kitchen, dining room, bedroom, bathroom, library, office, art gallery, garden/courtyard, storage, classroom, arcade/game room, cinema, gym, music studio, laboratory, garage, kids' room, laundry, workshop, attic, basement and rooftop. See [room catalogue](docs/ROOM_CATALOG.md) and [floorplan](docs/GRAND_HALL_FLOORPLAN.md).

## Tech baseline
- Unity 2022.3.50f1 LTS target
- Universal Render Pipeline (URP)
- C#
- Android landscape first
- Blender-authored production meshes and animations
- Server-authoritative networking; transport/hosting selection is a tracked decision, not a fake local multiplayer implementation

## Repository layout
- `UnityProject/` — Unity project shell and source.
- `docs/` — art direction, room catalogue, floorplan, game design, architecture, setup, roadmap, QA criteria.

## Open in Unity
1. Install Unity Hub and Unity **2022.3.50f1** with Android Build Support, SDK & NDK Tools and OpenJDK.
2. Open `UnityProject` through Unity Hub.
3. Allow Package Manager to resolve the pinned packages.
4. Run **Tools → Paint & Blend → Build Grand Hall Blockout** to generate the connected-world first-pass scene.
5. See [Unity setup](docs/UNITY_SETUP.md).

The generated world builder is an editable first-pass architectural/prop blockout, not a claim that final reference-matched production art is complete. Final meshes, UVs, textures, rigging, VFX and lighting need the dedicated art-production pass.

## Milestone policy
Features are complete only after implementation and tests. A successful project open or compile is not equivalent to a finished game. See [roadmap](docs/ROADMAP.md) and [acceptance criteria](docs/ACCEPTANCE_CRITERIA.md).
