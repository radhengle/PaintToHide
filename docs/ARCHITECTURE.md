# Runtime Architecture

## Layers
- Domain: phases, teams, rules, match configuration and score.
- Gameplay: character motion, pose, UV paint, environment metadata, hiding validation and blaster.
- World: room graph, room metadata, navigation and editor world authoring.
- AI: perception observations, slot evaluation, search planning and difficulty.
- Infrastructure: network session adapter, server authority, persistence and telemetry.
- Presentation: lobby, paint studio, HUD, reveal, audio and VFX.

## Authority boundary
The client sends intent only. A server owns match phase, team roster, movement validity, confirmed hiding positions, firing cooldown, projectile/hit validation, elimination and final scores. Local practice mode may run without a server; it must be visibly separate from online mode. Never label local bots as online multiplayer.

## Painting
Use per-character render textures/material masks driven by UV hits. Do not paint a flat screen overlay. Replicate compact stroke data (surface/material slot, UV, colour, brush parameters, sequence) rather than sending full texture frames. Add anti-abuse limits and server-approved stroke windows.

## Network decision
Transport/hosting service still needs a documented provider selection and cost/security test. Implement interfaces first; integrate only after validating room codes/links, reconnect, host migration, anti-cheat and server-authoritative hit handling on Android.

## Data
Use ScriptableObjects for MatchConfig, RoomDefinition, RoomConnection, EnvironmentObjectDefinition, HidingSlotDefinition, BrushConfig, MaterialProfile, BlasterConfig and BotDifficultyProfile.

## Rendering
URP, mobile shader variants, baked lighting where suitable, LODs, occlusion culling, texture atlases, pooling and profiling. No arbitrary quality promises without physical-device testing.
