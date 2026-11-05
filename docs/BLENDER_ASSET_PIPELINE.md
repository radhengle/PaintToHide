# Blender Production Asset Pipeline

The scene builder is a blockout/layout generator. The final game art must be authored as modular Blender assets using the reference images.

## Required model pack
- Architecture: arch modules, columns/capitals, balustrades, doors, window frames, domed skylight ribs, staircase and fountain pieces.
- Living room: sectional sofa modules, seat/back cushions, coffee table, media console, TV, bookshelf, floor lamp and plant pots.
- Royal palace: throne, dais steps, carved gold trim, paired lion statues, armor stands, royal banners and red carpet trim.
- Kitchen: modular teal cabinets, blue fridge, stove/hood, island/table, yellow chairs, shelves, jars, hanging utensils and flower vase.
- Garden: curved stone fountain, pergola, gazebo, blossom tree, topiary, planters, flower clusters and lanterns.
- Gallery: frames, plinths, busts/statues, red benches, rope barriers and pedestal bases.
- Storage: metal rack, plastic bins, cardboard cartons, crate, trunk, paint bucket, rolled carpet and ladder.
- Office: desks, drawer pedestals, blue swivel chair, monitor, keyboard, printer, bookcase, filing cabinet and noticeboard.

## Modelling checklist per asset
1. Model at real-world metre scale (human reference 1.75m).
2. Apply transforms; use non-destructive bevels and weighted normals where appropriate.
3. UV unwrap; keep texel density consistent by material group.
4. Use principled PBR materials with restrained roughness/metalness.
5. Create clean collision proxies separately from visual meshes.
6. Create LOD0/LOD1 for high-visibility props; add LOD2 for repeated background props.
7. Set pivots at logical interaction points; name objects consistently.
8. Export FBX with applied scale and no cameras/lights; verify forward/up axis in Unity.
9. Import and inspect in Unity under the approved URP lighting before acceptance.

## Export convention
`PB_<CATEGORY>_<ASSET>_v001.fbx`
Examples: `PB_ARCH_GrandHallColumn_v001.fbx`, `PB_LIVING_SectionalSofa_v001.fbx`, `PB_KITCHEN_TealCabinet_900_v001.fbx`.

Never ship primitive blockout models as final art. The visual references are the bar.
