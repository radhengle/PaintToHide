# Visual Direction — Source of Truth

## Overall look
Bright, colourful, premium stylized cartoon 3D with believable real-world scale. Clean silhouettes, precise bevels, rounded architectural mouldings, physically coherent materials, strong but controlled saturation, warm interior practical lights, daylight through tall arched windows, soft contact shadows, polished tiled floors and intentionally composed prop clusters.

The game should look like a finished commercial mobile 3D game. The images in the conversation are visual targets; they are not final mesh/texture assets and do not replace production modelling.

## Reference-by-reference translation

| Reference | Required game elements |
|---|---|
| Vibrant Grand Hall Atrium | Glass dome, pink/ivory arches, ornate balcony railings, twin red staircases, central blue tiered fountain, chandeliers, carpet runners, flowering planters, coloured paint splashes |
| Vibrant Modern Living Room | Cream sectional sofa, yellow/blue/pink cushions, blue geometric rug, wood coffee table, arched windows, teal curtains, media console, bookshelves, lamps and plants |
| Vibrant Royal Palace Throne Room | Raised marble dais, red upholstered throne, gold decorative frame, paired lion statues, red/gold banners, velvet carpet, armor displays, tall arched windows |
| Vibrant Cartoon Kitchen | Teal cabinets, blue refrigerator, central wood table, yellow chairs, checkered table runner, shelves and jars, hanging utensils, stove/hood, plants and tiled floor |
| Vibrant Garden Fountain Courtyard | Blossom tree, fountain, curved stone path, flower beds, pergola, warm lanterns, gazebo, topiary, lush foliage |
| Paint & Blend Gallery Hide-and-Seek | Large skylight, framed colourful paintings, classical statues, marble floor, red benches, roped plinths and decorative banners |
| Paint & Blend Storeroom Hide & Seek | Industrial shelving, colourful plastic tubs, cardboard boxes, trunks, rolled rugs, red ladder, lockers, paint buckets and small props |
| Office visual references | Purple/blue walls, wood desks and shelving, blue swivel chairs, monitors, filing cabinets, printer, bulletin board, plants, arched windows and bright lighting |
| Multiplayer concept/gameplay boards | Consistent faceless mannequin character, paint studio, pose selector, colour wheel, brush tools, round HUD, minimap, pichkari blaster, splash and reveal |

## Character consistency
One shared neutral humanoid base rig for all players and bots. Nominal standing height 1.75m. Faceless/simplified neutral mannequin appearance as shown in references; no different proportions by team. Hider paint is applied over the body surface using UV-aware material layers. A paint layer must not change the collision capsule or character scale.

## Quality bar
- No naked primitive cubes as final furniture.
- No low-resolution blurry textures or stretched UVs.
- No un-bevelled razor edges unless the real object needs them.
- No floating props, intersecting meshes or wall clipping.
- Consistent scale between doors, furniture and the 1.75m character.
- Every visual asset needs a production mesh, UVs, material setup, collision proxy, LOD/culling settings and QA approval.
- Mobile rendering is optimized without flattening the art direction.
