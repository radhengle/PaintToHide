# Game Design

## Core loop
Players enter the large connected mansion. A lobby fills with real players and bots. Teams are balanced into Hiders and Seekers. Hiders paint a shared human character, sample nearby object colours/material cues, choose a supported pose and validate a hiding position. Seekers may customize their appearance too. During the active round, Seekers search and tag Hiders with paint blasters. Reveal exposes remaining Hiders and scores.

## Match phases
- Lobby: 90s.
- Team assignment: immediate after roster lock; one Hider and one Seeker minimum.
- Preparation/Paint: 90s.
- Active round: 180s.
- Reveal: 30s.
- Return to lobby or next round.
- Scheduled cycle after lobby creation: 6m30s, excluding loading/optional waiting.

## Rules
Hiders win if one or more survive until timeout. Seekers win by catching all Hiders. Friendly fire disabled. A valid server-confirmed direct hit eliminates a Hider. Eliminated players spectate/reveal until next round.

## Character
Same neutral faceless humanoid mannequin rig for all players and bots; 1.75m nominal height. Stable hitbox regardless of paint or pose. Supported poses: stand, sit, crouch, kneel, lean, folded arms, raised arms, lie down. No resizing/scaling to fit inside props.

## Camouflage studio
Object reference selection; RGB/HSV picker and eyedropper; standard/soft/hard/spray/texture/detail brushes; size, opacity, hardness and flow; eraser, undo/redo/reset; per-body-part UV texture painting. Material families: wood, marble, concrete, fabric, ceramic, stone, painted wall, metal and foliage. Rating considers colour, texture, pose, position and silhouette, never paint quantity.

## Blaster
Playful toy/pichkari design; single primary weapon. One shot per trigger; 1s reload. Hit requests and elimination server-authoritative. Splash particles and surface decals last 2s. VFX cannot eliminate without a valid direct hit.

## Hiding validation
Each slot has a pose anchor, supported-pose mask and clearance. Validate complete character collider against solid environment geometry, ground, ceiling, neighbouring players and access route. Reject clips/trapped placement.

## Bots
Hider bots rank valid slots by environmental colour/texture, silhouette, exposure, route risk and distance. Seeker bots explore rooms, inspect visual mismatches and investigate evidence. Difficulty Easy/Normal/Hard. Bots obey normal physics, timers, line-of-sight and cooldowns; they receive no exact hidden-player locations.

## Mobile UX
Landscape orientation. Left movement joystick; right camera/look, jump/crouch and shoot; paint phase uses touch paint, colour wheel, brush options, pose picker, zoom and rotation. UI safe-area and scaling are required.
