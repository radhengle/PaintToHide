#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PaintBlend.Gameplay;
using PaintBlend.World;

namespace PaintBlend.Editor
{
    /// <summary>
    /// Builds an editable first-pass connected-world scene from the approved room plan.
    /// This is an architectural/prop blockout generator; final reference-matched
    /// production meshes, textures and animation are separate art-production tasks.
    /// </summary>
    public static class GrandHallSceneBuilder
    {
        private const string RootName = "PAINT_BLEND_GRAND_HALL_COMPLEX";
        private const string MaterialFolder = "Assets/PaintBlend/Generated/Materials";
        private const string SceneFolder = "Assets/PaintBlend/Scenes";
        private const float RoomWidth = 16f;
        private const float RoomDepth = 14f;
        private const float StepX = 22f;
        private const float StepZ = 20f;
        private const float WallThickness = 0.35f;
        private const float DoorWidth = 3.4f;
        private const float DoorHeight = 2.8f;

        private static readonly List<RoomData> Rooms = CreateLayout();
        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        private sealed class RoomData
        {
            public string Id;
            public string Title;
            public string Theme;
            public int Column;
            public int Row;
            public bool Outdoor;
            public bool Atrium;
            public Vector3 Center => new Vector3((Column - 2) * StepX, 0f, (2 - Row) * StepZ);
        }

        [MenuItem("Tools/Paint & Blend/Build Grand Hall Blockout")]
        public static void BuildGrandHallBlockout()
        {
            if (!EditorUtility.DisplayDialog(
                "Build Paint & Blend World",
                "This replaces the existing generated PAINT_BLEND_GRAND_HALL_COMPLEX root in the open scene. Continue?",
                "Build World", "Cancel"))
                return;

            EnsureFolder("Assets/PaintBlend");
            EnsureFolder("Assets/PaintBlend/Generated");
            EnsureFolder(MaterialFolder);
            EnsureFolder(SceneFolder);
            Materials.Clear();

            GameObject oldRoot = GameObject.Find(RootName);
            if (oldRoot != null) Undo.DestroyObjectImmediate(oldRoot);

            GameObject root = new GameObject(RootName);
            GameObject architecture = Child(root.transform, "Architecture");
            GameObject corridors = Child(root.transform, "Connected Corridors");
            GameObject props = Child(root.transform, "Room Props");
            GameObject slots = Child(root.transform, "Hiding Slot Anchors");
            GameObject lighting = Child(root.transform, "Lighting and Atmosphere");

            CreateMaterials();
            BuildRoomShells(architecture.transform, props.transform, slots.transform);
            WorldGraph worldGraph = root.AddComponent<WorldGraph>();
            worldGraph.Rebuild();
            BuildCorridorGrid(corridors.transform);
            BuildGrandHallSignature(props.transform);
            BuildGardenSignature(props.transform);
            BuildWorldLighting(lighting.transform);
            CreateSpawnMarkers(root.transform);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            string path = SceneFolder + "/GrandHallComplex_Blockout.unity";
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Paint & Blend: generated 25 connected room modules in one editable scene at " + path);
        }

        private static List<RoomData> CreateLayout()
        {
            string[,] ids =
            {
                { "Bedroom", "Bathroom", "Library", "Cinema", "RoyalPalace" },
                { "ArtGallery", "LivingRoom", "DiningRoom", "Kitchen", "Office" },
                { "StorageRoom", "Classroom", "GrandHall", "Laboratory", "Garage" },
                { "LaundryRoom", "Workshop", "Arcade", "Gym", "MusicStudio" },
                { "Attic", "KidsRoom", "GardenCourtyard", "Basement", "RooftopTerrace" }
            };

            var result = new List<RoomData>();
            for (int row = 0; row < 5; row++)
            for (int col = 0; col < 5; col++)
            {
                string id = ids[row, col];
                result.Add(new RoomData
                {
                    Id = id,
                    Title = MakeTitle(id),
                    Theme = ThemeFor(id),
                    Column = col,
                    Row = row,
                    Outdoor = id == "GardenCourtyard" || id == "RooftopTerrace",
                    Atrium = id == "GrandHall"
                });
            }
            return result;
        }

        private static string MakeTitle(string id)
        {
            switch (id)
            {
                case "RoyalPalace": return "Royal Palace";
                case "LivingRoom": return "Living Room";
                case "DiningRoom": return "Dining Room";
                case "ArtGallery": return "Art Gallery";
                case "StorageRoom": return "Storage Room";
                case "Classroom": return "Classroom";
                case "Laboratory": return "Laboratory";
                case "Garage": return "Garage";
                case "LaundryRoom": return "Laundry Room";
                case "MusicStudio": return "Music Studio";
                case "KidsRoom": return "Kids' Room";
                case "GardenCourtyard": return "Garden Courtyard";
                case "RooftopTerrace": return "Rooftop Terrace";
                default: return id;
            }
        }

        private static string ThemeFor(string id)
        {
            switch (id)
            {
                case "GrandHall": return "Atrium • pink stone, ivory mouldings, glass and royal red";
                case "LivingRoom": return "Cream upholstery, teal drapes, warm wood and blue rug";
                case "RoyalPalace": return "Royal red velvet, gold accents and pale marble";
                case "Kitchen": return "Teal cabinetry, blue appliance accents and warm timber";
                case "GardenCourtyard": return "Lush green foliage, blossom pink and sandstone";
                case "ArtGallery": return "Ivory marble, gold frames and gallery red";
                case "StorageRoom": return "Industrial grey, warm crates and saturated storage bins";
                case "Office": return "Purple walls, warm wood, blue task chairs";
                default: return "Bright playful stylized 3D with room-specific materials";
            }
        }

        private static void BuildRoomShells(Transform architecture, Transform props, Transform slots)
        {
            foreach (RoomData room in Rooms)
            {
                GameObject roomRoot = Child(architecture, room.Id + " — " + room.Title);
                roomRoot.transform.position = room.Center;
                RoomMarker marker = roomRoot.AddComponent<RoomMarker>();
                marker.Configure(room.Id, room.Title, room.Theme, 0);

                float height = room.Atrium ? 11f : 5.2f;
                float width = RoomWidth;
                float depth = RoomDepth;
                Material wall = GetRoomWallMaterial(room.Id);

                Cube("Floor", roomRoot.transform, new Vector3(0f, -0.22f, 0f),
                    new Vector3(width, 0.44f, depth), GetRoomFloorMaterial(room.Id));
                if (!room.Outdoor)
                    Cube("Ceiling", roomRoot.transform, new Vector3(0f, height, 0f),
                        new Vector3(width, 0.3f, depth), room.Atrium ? Mat("AtriumCeiling", new Color(0.96f, 0.84f, 0.78f)) : wall);

                BuildSideWall(roomRoot.transform, room, "North", width, depth, height, wall);
                BuildSideWall(roomRoot.transform, room, "South", width, depth, height, wall);
                BuildSideWall(roomRoot.transform, room, "East", width, depth, height, wall);
                BuildSideWall(roomRoot.transform, room, "West", width, depth, height, wall);

                BuildRoomTrim(roomRoot.transform, width, depth, height, room.Atrium);
                BuildRoomProps(room, props);
                AddHidingSlots(room, slots);
            }
        }

        private static void BuildSideWall(Transform parent, RoomData room, string side,
            float width, float depth, float height, Material wallMaterial)
        {
            Vector3 neighbourOffset = side == "North" ? Vector3.forward :
                side == "South" ? Vector3.back :
                side == "East" ? Vector3.right : Vector3.left;
            bool connected = HasNeighbour(room, neighbourOffset);

            if (connected)
            {
                if (side == "North" || side == "South")
                    BuildWallDoor(parent, side, width, height, wallMaterial);
                else
                    BuildWallDoor(parent, side, depth, height, wallMaterial);
            }
            else
            {
                BuildExteriorWindowWall(parent, side, side == "North" || side == "South" ? width : depth,
                    height, wallMaterial);
            }
        }

        private static bool HasNeighbour(RoomData room, Vector3 direction)
        {
            int dc = Mathf.RoundToInt(direction.x);
            int dr = -Mathf.RoundToInt(direction.z);
            for (int i = 0; i < Rooms.Count; i++)
                if (Rooms[i].Column == room.Column + dc && Rooms[i].Row == room.Row + dr)
                    return true;
            return false;
        }

        private static void BuildWallDoor(Transform parent, string side, float length, float height, Material wall)
        {
            float sideLength = (length - DoorWidth) * 0.5f;
            float edge = DoorWidth * 0.5f + sideLength * 0.5f;
            float surface = side == "North" ? RoomDepth * 0.5f :
                side == "South" ? -RoomDepth * 0.5f :
                side == "East" ? RoomWidth * 0.5f : -RoomWidth * 0.5f;

            if (side == "North" || side == "South")
            {
                float z = surface;
                Cube(side + " Wall Left", parent, new Vector3(-edge, height * 0.5f, z),
                    new Vector3(sideLength, height, WallThickness), wall);
                Cube(side + " Wall Right", parent, new Vector3(edge, height * 0.5f, z),
                    new Vector3(sideLength, height, WallThickness), wall);
                Cube(side + " Door Header", parent, new Vector3(0f, DoorHeight + (height - DoorHeight) * 0.5f, z),
                    new Vector3(DoorWidth, height - DoorHeight, WallThickness), wall);
                AddDoorFrame(parent, new Vector3(0f, 0f, z), false);
            }
            else
            {
                float x = surface;
                Cube(side + " Wall Left", parent, new Vector3(x, height * 0.5f, -edge),
                    new Vector3(WallThickness, height, sideLength), wall);
                Cube(side + " Wall Right", parent, new Vector3(x, height * 0.5f, edge),
                    new Vector3(WallThickness, height, sideLength), wall);
                Cube(side + " Door Header", parent, new Vector3(x, DoorHeight + (height - DoorHeight) * 0.5f, 0f),
                    new Vector3(WallThickness, height - DoorHeight, DoorWidth), wall);
                AddDoorFrame(parent, new Vector3(x, 0f, 0f), true);
            }
        }

        private static void BuildExteriorWindowWall(Transform parent, string side,
            float length, float height, Material wall)
        {
            bool alongX = side == "North" || side == "South";
            float surface = side == "North" ? RoomDepth * 0.5f :
                side == "South" ? -RoomDepth * 0.5f :
                side == "East" ? RoomWidth * 0.5f : -RoomWidth * 0.5f;
            float glassWidth = Mathf.Min(5.0f, length * 0.44f);
            float glassHeight = Mathf.Min(2.7f, height * 0.55f);
            float baseY = 1.45f;
            float sideLength = (length - glassWidth) * 0.5f;
            float edge = glassWidth * 0.5f + sideLength * 0.5f;

            if (alongX)
            {
                Cube(side + " Wall Left", parent, new Vector3(-edge, height * 0.5f, surface),
                    new Vector3(sideLength, height, WallThickness), wall);
                Cube(side + " Wall Right", parent, new Vector3(edge, height * 0.5f, surface),
                    new Vector3(sideLength, height, WallThickness), wall);
                Cube(side + " Window Sill", parent, new Vector3(0f, baseY * 0.5f, surface),
                    new Vector3(glassWidth, baseY, WallThickness), wall);
                Cube(side + " Window Top", parent, new Vector3(0f, baseY + glassHeight + (height - baseY - glassHeight) * 0.5f, surface),
                    new Vector3(glassWidth, height - baseY - glassHeight, WallThickness), wall);
                Cube(side + " Glass", parent, new Vector3(0f, baseY + glassHeight * 0.5f, surface),
                    new Vector3(glassWidth - 0.18f, glassHeight - 0.18f, 0.08f), Mat("WindowGlass", new Color(0.26f, 0.78f, 0.96f)));
            }
            else
            {
                Cube(side + " Wall Left", parent, new Vector3(surface, height * 0.5f, -edge),
                    new Vector3(WallThickness, height, sideLength), wall);
                Cube(side + " Wall Right", parent, new Vector3(surface, height * 0.5f, edge),
                    new Vector3(WallThickness, height, sideLength), wall);
                Cube(side + " Window Sill", parent, new Vector3(surface, baseY * 0.5f, 0f),
                    new Vector3(WallThickness, baseY, glassWidth), wall);
                Cube(side + " Window Top", parent, new Vector3(surface, baseY + glassHeight + (height - baseY - glassHeight) * 0.5f, 0f),
                    new Vector3(WallThickness, height - baseY - glassHeight, glassWidth), wall);
                Cube(side + " Glass", parent, new Vector3(surface, baseY + glassHeight * 0.5f, 0f),
                    new Vector3(0.08f, glassHeight - 0.18f, glassWidth - 0.18f), Mat("WindowGlass", new Color(0.26f, 0.78f, 0.96f)));
            }
        }

        private static void AddDoorFrame(Transform parent, Vector3 center, bool alongZ)
        {
            Material trim = Mat("WarmIvoryTrim", new Color(0.98f, 0.83f, 0.68f));
            Vector3 left = alongZ ? new Vector3(0f, DoorHeight * 0.5f, -DoorWidth * 0.5f) :
                new Vector3(-DoorWidth * 0.5f, DoorHeight * 0.5f, 0f);
            Vector3 right = alongZ ? new Vector3(0f, DoorHeight * 0.5f, DoorWidth * 0.5f) :
                new Vector3(DoorWidth * 0.5f, DoorHeight * 0.5f, 0f);
            Vector3 scale = alongZ ? new Vector3(0.22f, DoorHeight, 0.22f) :
                new Vector3(0.22f, DoorHeight, 0.22f);
            Cube("Door Trim Left", parent, center + left, scale, trim);
            Cube("Door Trim Right", parent, center + right, scale, trim);
            Cube("Door Arch Header", parent, center + Vector3.up * DoorHeight,
                alongZ ? new Vector3(0.22f, 0.22f, DoorWidth) : new Vector3(DoorWidth, 0.22f, 0.22f), trim);
        }

        private static void BuildRoomTrim(Transform parent, float width, float depth, float height, bool atrium)
        {
            Material trim = Mat("WarmIvoryTrim", new Color(0.98f, 0.83f, 0.68f));
            float beamY = atrium ? height - 0.5f : height - 0.35f;
            Cube("North Crown Moulding", parent, new Vector3(0f, beamY, depth * 0.5f - 0.12f),
                new Vector3(width, 0.3f, 0.35f), trim);
            Cube("South Crown Moulding", parent, new Vector3(0f, beamY, -depth * 0.5f + 0.12f),
                new Vector3(width, 0.3f, 0.35f), trim);
            Cube("East Crown Moulding", parent, new Vector3(width * 0.5f - 0.12f, beamY, 0f),
                new Vector3(0.35f, 0.3f, depth), trim);
            Cube("West Crown Moulding", parent, new Vector3(-width * 0.5f + 0.12f, beamY, 0f),
                new Vector3(0.35f, 0.3f, depth), trim);
        }

        private static void BuildCorridorGrid(Transform parent)
        {
            Material floor = Mat("CorridorFloor", new Color(0.89f, 0.78f, 0.72f), 0.04f, 0.65f);
            Material border = Mat("CorridorBorder", new Color(0.58f, 0.34f, 0.22f));
            for (int row = 0; row < 5; row++)
            for (int col = 0; col < 4; col++)
            {
                float x = (col - 1.5f) * StepX;
                float z = (2 - row) * StepZ;
                Cube("East-West Corridor", parent, new Vector3(x, -0.12f, z),
                    new Vector3(StepX - RoomWidth, 0.24f, RoomDepth), floor);
                Cube("Corridor Edge North", parent, new Vector3(x, 0.08f, z + RoomDepth * 0.5f),
                    new Vector3(StepX - RoomWidth, 0.16f, 0.18f), border);
                Cube("Corridor Edge South", parent, new Vector3(x, 0.08f, z - RoomDepth * 0.5f),
                    new Vector3(StepX - RoomWidth, 0.16f, 0.18f), border);
            }

            for (int row = 0; row < 4; row++)
            for (int col = 0; col < 5; col++)
            {
                float x = (col - 2) * StepX;
                float z = ((2 - row) * StepZ) - StepZ * 0.5f;
                Cube("North-South Corridor", parent, new Vector3(x, -0.12f, z),
                    new Vector3(RoomWidth, 0.24f, StepZ - RoomDepth), floor);
                Cube("Corridor Edge East", parent, new Vector3(x + RoomWidth * 0.5f, 0.08f, z),
                    new Vector3(0.18f, 0.16f, StepZ - RoomDepth), border);
                Cube("Corridor Edge West", parent, new Vector3(x - RoomWidth * 0.5f, 0.08f, z),
                    new Vector3(0.18f, 0.16f, StepZ - RoomDepth), border);
            }

            // Wider junction pads connect the orthogonal corridor strips.
            for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
            {
                float x = (col - 1.5f) * StepX;
                float z = ((2 - row) * StepZ) - StepZ * 0.5f;
                Cube("Corridor Junction", parent, new Vector3(x, -0.11f, z),
                    new Vector3(StepX - RoomWidth, 0.22f, StepZ - RoomDepth), floor);
            }
        }

        private static void BuildRoomProps(RoomData room, Transform props)
        {
            GameObject roomProps = Child(props, room.Id + " Props");
            roomProps.transform.position = room.Center;
            Transform p = roomProps.transform;
            Material wood = Mat("WarmOak", new Color(0.43f, 0.22f, 0.10f), 0.08f, 0.45f);
            Material cream = Mat("WarmCream", new Color(0.96f, 0.86f, 0.72f));
            Material blue = Mat("BrightBlue", new Color(0.08f, 0.43f, 0.90f));
            Material teal = Mat("Teal", new Color(0.08f, 0.58f, 0.56f));
            Material gold = Mat("PolishedGold", new Color(1.0f, 0.66f, 0.10f), 0.72f, 0.72f);
            Material red = Mat("RoyalRed", new Color(0.67f, 0.025f, 0.09f));
            Material plant = Mat("LeafGreen", new Color(0.10f, 0.42f, 0.16f));
            Material white = Mat("Porcelain", new Color(0.95f, 0.94f, 0.89f), 0.05f, 0.65f);
            Material grey = Mat("StorageGrey", new Color(0.36f, 0.40f, 0.44f), 0.32f, 0.42f);
            Material accent = Mat("PaintPink", new Color(1f, 0.14f, 0.53f));

            switch (room.Id)
            {
                case "GrandHall":
                    BuildHallProps(p, wood, cream, gold, red, blue, plant);
                    break;
                case "LivingRoom":
                    BuildSofa(p, new Vector3(-2.2f, 0f, 1.0f), cream, blue, accent, wood);
                    Cube("Blue Geometric Rug", p, new Vector3(0f, 0.02f, -1.4f), new Vector3(8.0f, 0.08f, 5.0f), blue);
                    BuildCoffeeTable(p, new Vector3(0f, 0.48f, -1.4f), wood, cream);
                    BuildBookshelf(p, new Vector3(5.2f, 0f, 2.2f), wood, gold);
                    BuildPlant(p, new Vector3(5.2f, 0f, -3.8f), cream, plant);
                    BuildTVUnit(p, new Vector3(4.8f, 0f, 0f), wood, grey, blue);
                    break;
                case "RoyalPalace":
                    BuildRoyalThrone(p, wood, gold, red, cream);
                    BuildStatuePair(p, gold, cream);
                    Cube("Royal Carpet", p, new Vector3(0f, 0.04f, -1.2f), new Vector3(7.6f, 0.08f, 8.0f), red);
                    break;
                case "Kitchen":
                    BuildKitchen(p, wood, teal, blue, cream, white, plant);
                    break;
                case "DiningRoom":
                    BuildDining(p, wood, cream, gold);
                    break;
                case "Bedroom":
                    BuildBedroom(p, wood, cream, blue, accent);
                    break;
                case "Bathroom":
                    BuildBathroom(p, white, blue, wood, plant);
                    break;
                case "Library":
                    BuildBookshelf(p, new Vector3(-5.4f, 0f, 0f), wood, gold);
                    BuildBookshelf(p, new Vector3(5.4f, 0f, 0f), wood, gold);
                    BuildDesk(p, new Vector3(0f, 0f, -1.4f), wood, cream, blue);
                    BuildPlant(p, new Vector3(-5.4f, 0f, -4.2f), cream, plant);
                    break;
                case "Office":
                    BuildOffice(p, wood, blue, cream, grey, plant);
                    break;
                case "ArtGallery":
                    BuildGallery(p, cream, gold, red, wood);
                    break;
                case "GardenCourtyard":
                    BuildGarden(p, wood, cream, gold, plant, blue);
                    break;
                case "RooftopTerrace":
                    BuildRooftop(p, wood, cream, gold, plant, blue);
                    break;
                case "StorageRoom":
                    BuildStorage(p, grey, wood, blue, red, accent);
                    break;
                case "Classroom":
                    BuildClassroom(p, wood, blue, cream, greenMaterial: plant);
                    break;
                case "Arcade":
                    BuildArcade(p, grey, blue, accent, gold);
                    break;
                case "Cinema":
                    BuildCinema(p, red, gold, grey);
                    break;
                case "Gym":
                    BuildGym(p, grey, blue, accent);
                    break;
                case "MusicStudio":
                    BuildMusicStudio(p, wood, red, gold, grey);
                    break;
                case "Laboratory":
                    BuildLaboratory(p, white, blue, teal, grey);
                    break;
                case "Garage":
                    BuildGarage(p, grey, red, gold, blue, wood);
                    break;
                case "KidsRoom":
                    BuildKidsRoom(p, wood, blue, accent, cream, gold);
                    break;
                case "LaundryRoom":
                    BuildLaundry(p, white, blue, wood, teal);
                    break;
                case "Workshop":
                    BuildWorkshop(p, wood, grey, gold, red);
                    break;
                case "Attic":
                    BuildAttic(p, wood, grey, cream, gold);
                    break;
                case "Basement":
                    BuildBasement(p, grey, teal, red);
                    break;
            }
        }

        private static void BuildHallProps(Transform p, Material wood, Material cream, Material gold,
            Material red, Material blue, Material plant)
        {
            // Fountain, paired staircases, columns, banners and warm chandelier are
            // deliberately composed as recognizable forms instead of generic cubes.
            Cylinder("Fountain Lower Basin", p, new Vector3(0f, 0.35f, 0f), new Vector3(4.3f, 0.45f, 4.3f), cream);
            Cylinder("Fountain Water", p, new Vector3(0f, 0.63f, 0f), new Vector3(3.55f, 0.08f, 3.55f), blue);
            Cylinder("Fountain Pedestal", p, new Vector3(0f, 1.25f, 0f), new Vector3(1.3f, 1.2f, 1.3f), cream);
            Cylinder("Fountain Upper Bowl", p, new Vector3(0f, 1.95f, 0f), new Vector3(2.1f, 0.32f, 2.1f), cream);
            Sphere("Fountain Finial", p, new Vector3(0f, 2.55f, 0f), new Vector3(0.55f, 0.7f, 0.55f), blue);

            for (int side = -1; side <= 1; side += 2)
            {
                BuildStaircase(p, side, red, cream, gold);
                for (int i = 0; i < 3; i++)
                    Cylinder("Grand Hall Column " + side + "-" + i, p,
                        new Vector3(side * 6.7f, 3.8f, -4f + i * 4f),
                        new Vector3(0.62f, 7.6f, 0.62f), cream);
                BuildPlant(p, new Vector3(side * 4.6f, 0f, 4.5f), cream, plant);
            }

            BuildChandelier(p, gold);
            BuildBookshelf(p, new Vector3(-6.2f, 0f, -3.9f), wood, gold);
            BuildSofa(p, new Vector3(5.2f, 0f, 3.0f), cream, blue, red, wood);
            Cube("Central Red Runner", p, new Vector3(0f, 0.03f, -5.4f), new Vector3(5.2f, 0.06f, 3.8f), red);
            for (int i = 0; i < 4; i++)
            {
                Cube("Paint Banner " + i, p, new Vector3(-6.8f + i * 4.5f, 5.0f, 6.7f),
                    new Vector3(1.15f, 3.1f, 0.08f), i % 2 == 0 ? blue : Mat("BannerPink", new Color(0.92f, 0.05f, 0.48f)));
            }

            // Stylized glass-dome ribs; final dome glass and mouldings are replaced
            // by custom curved production meshes in the art pass.
            Material sky = Mat("DomeGlassBlue", new Color(0.18f, 0.58f, 0.88f), 0.05f, 0.8f);
            Sphere("Glass Dome Core", p, new Vector3(0f, 9.5f, 0f), new Vector3(16f, 3.0f, 13.5f), sky);
        }

        private static void BuildStaircase(Transform p, int side, Material red, Material cream, Material gold)
        {
            for (int i = 0; i < 11; i++)
            {
                float x = side * (3.9f + i * 0.32f);
                float y = 0.18f + i * 0.27f;
                float z = 4.6f - i * 0.62f;
                Cube("Grand Stair Step " + side + "-" + i, p, new Vector3(x, y, z),
                    new Vector3(3.1f, 0.25f, 0.78f), red);
                Cube("Stair Nosing " + side + "-" + i, p, new Vector3(x, y + 0.13f, z + 0.34f),
                    new Vector3(3.1f, 0.06f, 0.08f), gold);
            }
            Cube("Stair Landing " + side, p, new Vector3(side * 7.2f, 3.2f, -2.4f),
                new Vector3(4.8f, 0.4f, 2.5f), cream);
        }

        private static void BuildChandelier(Transform p, Material gold)
        {
            Cylinder("Chandelier Stem", p, new Vector3(0f, 8.2f, 0f), new Vector3(0.18f, 2.1f, 0.18f), gold);
            Sphere("Chandelier Hub", p, new Vector3(0f, 7.0f, 0f), new Vector3(1.0f, 0.45f, 1.0f), gold);
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 2f / 8f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 2.0f, 6.8f, Mathf.Sin(angle) * 2.0f);
                Cylinder("Chandelier Arm " + i, p, Vector3.Lerp(new Vector3(0f, 7f, 0f), pos, 0.55f),
                    new Vector3(0.11f, 0.13f, 0.11f), gold);
                Sphere("Chandelier Bulb " + i, p, pos + Vector3.down * 0.1f,
                    new Vector3(0.38f, 0.42f, 0.38f), Mat("WarmGlow", new Color(1f, 0.79f, 0.34f), 0f, 0.8f, true));
            }
        }

        private static void BuildSofa(Transform p, Vector3 at, Material upholstery, Material blue,
            Material accent, Material wood)
        {
            Cube("Sofa Base", p, at + new Vector3(0f, 0.48f, 0f), new Vector3(5.4f, 0.8f, 1.65f), upholstery);
            Cube("Sofa Back", p, at + new Vector3(0f, 1.25f, 0.68f), new Vector3(5.25f, 1.5f, 0.34f), upholstery);
            Cube("Sofa Arm Left", p, at + new Vector3(-2.55f, 0.86f, 0f), new Vector3(0.45f, 1.2f, 1.7f), upholstery);
            Cube("Sofa Arm Right", p, at + new Vector3(2.55f, 0.86f, 0f), new Vector3(0.45f, 1.2f, 1.7f), upholstery);
            for (int i = 0; i < 3; i++)
            {
                Material cushion = i == 0 ? accent : (i == 1 ? blue : Mat("YellowCushion", new Color(1f, 0.73f, 0.08f)));
                Cube("Sofa Seat Cushion " + i, p, at + new Vector3(-1.55f + i * 1.55f, 0.92f, -0.08f),
                    new Vector3(1.45f, 0.28f, 1.3f), cushion);
                Cube("Sofa Back Cushion " + i, p, at + new Vector3(-1.55f + i * 1.55f, 1.45f, 0.43f),
                    new Vector3(1.35f, 0.82f, 0.22f), cushion);
            }
            for (int i = 0; i < 4; i++)
                Cube("Sofa Leg " + i, p, at + new Vector3(i < 2 ? -2.15f : 2.15f, 0.15f, i % 2 == 0 ? -0.55f : 0.55f),
                    new Vector3(0.16f, 0.3f, 0.16f), wood);
        }

        private static void BuildCoffeeTable(Transform p, Vector3 at, Material wood, Material cream)
        {
            Cube("Coffee Table Top", p, at + Vector3.up * 0.48f, new Vector3(3.3f, 0.18f, 1.55f), wood);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Cube("Coffee Table Leg", p, at + new Vector3(x * 1.35f, 0.22f, z * 0.55f),
                    new Vector3(0.13f, 0.45f, 0.13f), wood);
            Sphere("Flower Vase", p, at + new Vector3(0f, 0.82f, 0f),
                new Vector3(0.42f, 0.5f, 0.42f), cream);
        }

        private static void BuildBookshelf(Transform p, Vector3 at, Material wood, Material trim)
        {
            Cube("Bookcase Back", p, at + new Vector3(0f, 2.0f, 0f), new Vector3(3.5f, 4f, 0.18f), wood);
            Cube("Bookcase Left Side", p, at + new Vector3(-1.72f, 2.0f, -0.18f), new Vector3(0.16f, 4f, 0.45f), wood);
            Cube("Bookcase Right Side", p, at + new Vector3(1.72f, 2.0f, -0.18f), new Vector3(0.16f, 4f, 0.45f), wood);
            for (int shelf = 0; shelf < 5; shelf++)
                Cube("Bookcase Shelf " + shelf, p, at + new Vector3(0f, 0.45f + shelf * 0.88f, -0.18f),
                    new Vector3(3.35f, 0.13f, 0.48f), trim);
            Material[] bookMats =
            {
                Mat("BookBlue", new Color(0.08f, 0.32f, 0.74f)),
                Mat("BookRed", new Color(0.78f, 0.07f, 0.13f)),
                Mat("BookGold", new Color(0.95f, 0.58f, 0.08f)),
                Mat("BookGreen", new Color(0.1f, 0.48f, 0.24f))
            };
            for (int shelf = 0; shelf < 4; shelf++)
            for (int book = 0; book < 7; book++)
                Cube("Book " + shelf + "-" + book, p,
                    at + new Vector3(-1.4f + book * 0.45f, 0.8f + shelf * 0.88f, -0.15f),
                    new Vector3(0.32f, 0.62f + (book % 3) * 0.07f, 0.28f), bookMats[(shelf + book) % bookMats.Length]);
        }

        private static void BuildTVUnit(Transform p, Vector3 at, Material wood, Material grey, Material blue)
        {
            Cube("Media Console", p, at + new Vector3(0f, 0.55f, 0f), new Vector3(4.5f, 1.1f, 0.8f), wood);
            Cube("Television Frame", p, at + new Vector3(0f, 2.25f, 0.3f), new Vector3(3.6f, 2.1f, 0.18f), grey);
            Cube("Television Screen", p, at + new Vector3(0f, 2.25f, 0.19f), new Vector3(3.36f, 1.86f, 0.04f), blue);
        }

        private static void BuildPlant(Transform p, Vector3 at, Material pot, Material leaves)
        {
            Cylinder("Planter Pot", p, at + Vector3.up * 0.42f, new Vector3(0.76f, 0.82f, 0.76f), pot);
            Cylinder("Plant Stem", p, at + Vector3.up * 1.5f, new Vector3(0.12f, 1.3f, 0.12f), leaves);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2f / 7f;
                Sphere("Plant Leaf " + i, p, at + new Vector3(Mathf.Cos(angle) * 0.48f, 1.75f + (i % 3) * 0.25f, Mathf.Sin(angle) * 0.48f),
                    new Vector3(0.65f, 0.25f, 0.38f), leaves);
            }
        }

        private static void BuildRoyalThrone(Transform p, Material wood, Material gold, Material red, Material cream)
        {
            Cube("Throne Dais", p, new Vector3(0f, 0.28f, 4.0f), new Vector3(7f, 0.56f, 4.0f), cream);
            for (int step = 0; step < 3; step++)
                Cube("Throne Step " + step, p, new Vector3(0f, 0.16f + step * 0.22f, 1.5f + step * 0.72f),
                    new Vector3(6f - step * 0.6f, 0.25f, 0.8f), red);
            Cube("Throne Seat", p, new Vector3(0f, 1.5f, 4.2f), new Vector3(2.9f, 0.55f, 2.0f), red);
            Cube("Throne Back", p, new Vector3(0f, 3.0f, 5.05f), new Vector3(2.85f, 3.0f, 0.45f), red);
            Cube("Throne Crown Crest", p, new Vector3(0f, 4.75f, 5.05f), new Vector3(3.5f, 0.35f, 0.55f), gold);
            for (int side = -1; side <= 1; side += 2)
            {
                Cube("Throne Arm", p, new Vector3(side * 1.65f, 2.05f, 4.2f), new Vector3(0.38f, 1.4f, 1.7f), gold);
                Sphere("Throne Finial", p, new Vector3(side * 1.68f, 2.85f, 4.25f), new Vector3(0.35f, 0.45f, 0.35f), gold);
                Sphere("Lion Statue Body", p, new Vector3(side * 3.2f, 1.0f, 4.0f), new Vector3(1.25f, 1.2f, 1.4f), gold);
                Sphere("Lion Statue Head", p, new Vector3(side * 3.2f, 1.8f, 4.45f), new Vector3(0.9f, 0.9f, 0.85f), gold);
            }
            Cube("Royal Wall Banner", p, new Vector3(0f, 4.7f, 6.6f), new Vector3(3.8f, 2.5f, 0.1f), red);
        }

        private static void BuildStatuePair(Transform p, Material gold, Material cream)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 at = new Vector3(side * 6.0f, 0f, -3.5f);
                Cylinder("Statue Plinth", p, at + Vector3.up * 0.5f, new Vector3(1.4f, 1f, 1.4f), cream);
                Capsule("Armor Display", p, at + Vector3.up * 2.1f, new Vector3(0.85f, 1.8f, 0.65f), gold);
                Sphere("Armor Helmet", p, at + Vector3.up * 3.2f, new Vector3(0.72f, 0.8f, 0.7f), gold);
            }
        }

        private static void BuildKitchen(Transform p, Material wood, Material teal, Material blue,
            Material cream, Material white, Material plant)
        {
            for (int i = 0; i < 6; i++)
            {
                float x = -6f + i * 2.0f;
                Cube("Kitchen Base Cabinet " + i, p, new Vector3(x, 0.9f, 5.3f), new Vector3(1.85f, 1.8f, 1.0f), teal);
                Cube("Kitchen Upper Cabinet " + i, p, new Vector3(x, 3.6f, 5.2f), new Vector3(1.85f, 1.65f, 0.8f), teal);
                Cube("Cabinet Handle " + i, p, new Vector3(x + 0.5f, 0.9f, 4.75f), new Vector3(0.08f, 0.32f, 0.08f), cream);
            }
            Cube("Kitchen Countertop", p, new Vector3(0f, 1.95f, 5.15f), new Vector3(13f, 0.18f, 1.25f), white);
            Cube("Refrigerator", p, new Vector3(-6.0f, 2.3f, -3.6f), new Vector3(2.2f, 4.4f, 2.1f), blue);
            Cube("Kitchen Island", p, new Vector3(0f, 1.0f, -1.3f), new Vector3(5.8f, 1.0f, 2.5f), wood);
            Cube("Island Top", p, new Vector3(0f, 1.58f, -1.3f), new Vector3(6.1f, 0.18f, 2.7f), white);
            for (int i = -1; i <= 1; i++)
            {
                Cube("Yellow Bar Stool Seat " + i, p, new Vector3(i * 2.3f, 0.9f, -3.6f), new Vector3(1.0f, 0.28f, 1.0f), Mat("YellowSeat", new Color(1f, 0.73f, 0.08f)));
                Cylinder("Bar Stool Pedestal " + i, p, new Vector3(i * 2.3f, 0.45f, -3.6f), new Vector3(0.12f, 0.9f, 0.12f), greyMaterial());
            }
            for (int i = 0; i < 4; i++) BuildPlant(p, new Vector3(-5f + i * 3.3f, 0f, 3.4f), white, plant);
        }

        private static Material greyMaterial() { return Mat("BrushedSteel", new Color(0.52f, 0.58f, 0.63f), 0.55f, 0.72f); }

        private static void BuildDining(Transform p, Material wood, Material cream, Material gold)
        {
            Cube("Dining Table Top", p, new Vector3(0f, 1.2f, 0f), new Vector3(8f, 0.25f, 3.4f), wood);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Cube("Dining Table Leg", p, new Vector3(x * 3.4f, 0.58f, z * 1.25f), new Vector3(0.28f, 1.15f, 0.28f), gold);
            int index = 0;
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                Vector3 at = new Vector3(x * 4.8f, 0f, z * 2.2f);
                Cube("Dining Chair Seat " + index, p, at + new Vector3(0f, 0.9f, 0f), new Vector3(1.35f, 0.3f, 1.25f), cream);
                Cube("Dining Chair Back " + index, p, at + new Vector3(0f, 1.65f, 0.48f), new Vector3(1.35f, 1.4f, 0.22f), cream);
                index++;
            }
        }

        private static void BuildBedroom(Transform p, Material wood, Material cream, Material blue, Material accent)
        {
            Cube("Bedroom Bed Base", p, new Vector3(0f, 0.55f, 1.1f), new Vector3(5.6f, 1.0f, 7.0f), wood);
            Cube("Bedroom Mattress", p, new Vector3(0f, 1.18f, 1.0f), new Vector3(5.5f, 0.42f, 6.8f), cream);
            Cube("Blue Duvet", p, new Vector3(0f, 1.45f, -0.7f), new Vector3(5.35f, 0.18f, 3.8f), blue);
            Cube("Upholstered Headboard", p, new Vector3(0f, 2.0f, 4.65f), new Vector3(6.1f, 2.1f, 0.35f), accent);
            for (int side = -1; side <= 1; side += 2)
            {
                Cube("Bedside Table", p, new Vector3(side * 4.0f, 0.7f, 3.6f), new Vector3(1.5f, 1.4f, 1.3f), wood);
                Cube("Bedside Lamp Shade", p, new Vector3(side * 4.0f, 1.8f, 3.6f), new Vector3(0.75f, 0.65f, 0.75f), cream);
            }
            Cube("Wardrobe", p, new Vector3(-5.7f, 2.2f, -3.0f), new Vector3(2.6f, 4.4f, 2.0f), wood);
        }

        private static void BuildBathroom(Transform p, Material white, Material blue, Material wood, Material plant)
        {
            Cube("Bathroom Tub Body", p, new Vector3(3.7f, 0.6f, 2.8f), new Vector3(4.5f, 1.1f, 2.1f), white);
            Cube("Bath Water", p, new Vector3(3.7f, 1.05f, 2.8f), new Vector3(3.9f, 0.1f, 1.6f), blue);
            Cube("Bathroom Vanity", p, new Vector3(-3.8f, 0.75f, 4.5f), new Vector3(4f, 1.5f, 1.4f), wood);
            Cube("Bathroom Mirror", p, new Vector3(-3.8f, 2.7f, 5.9f), new Vector3(2.5f, 2.0f, 0.12f), blue);
            Cube("Shower Glass", p, new Vector3(-4.8f, 2.4f, -2.4f), new Vector3(0.12f, 4.8f, 4.3f), Mat("ShowerGlass", new Color(0.53f, 0.84f, 0.92f)));
            BuildPlant(p, new Vector3(5.2f, 0f, -4.0f), white, plant);
        }

        private static void BuildDesk(Transform p, Vector3 at, Material wood, Material cream, Material blue)
        {
            Cube("Desk Top", p, at + new Vector3(0f, 1.0f, 0f), new Vector3(4.0f, 0.18f, 1.8f), wood);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Cube("Desk Leg", p, at + new Vector3(x * 1.7f, 0.48f, z * 0.68f), new Vector3(0.16f, 0.95f, 0.16f), wood);
            Cube("Desk Chair Seat", p, at + new Vector3(0f, 0.75f, -1.6f), new Vector3(1.2f, 0.25f, 1.0f), blue);
            Cube("Desk Chair Back", p, at + new Vector3(0f, 1.45f, -2.0f), new Vector3(1.15f, 1.35f, 0.22f), blue);
            Cube("Desk Monitor", p, at + new Vector3(0f, 1.9f, 0.48f), new Vector3(1.5f, 1.0f, 0.12f), cream);
        }

        private static void BuildOffice(Transform p, Material wood, Material blue, Material cream, Material grey, Material plant)
        {
            BuildDesk(p, new Vector3(-3.5f, 0f, 0f), wood, grey, blue);
            BuildDesk(p, new Vector3(3.5f, 0f, 0f), wood, grey, blue);
            BuildBookshelf(p, new Vector3(5.2f, 0f, 4.8f), wood, cream);
            Cube("Office Filing Cabinet", p, new Vector3(-5.5f, 1.0f, 4.7f), new Vector3(1.6f, 2.0f, 1.5f), grey);
            Cube("Office Printer", p, new Vector3(0f, 1.2f, 5.5f), new Vector3(1.8f, 1.2f, 1.4f), cream);
            BuildPlant(p, new Vector3(5.5f, 0f, -4.5f), cream, plant);
            Cube("Office Notice Board", p, new Vector3(0f, 2.8f, 6.7f), new Vector3(4.8f, 2.0f, 0.12f), Mat("NoticeBoard", new Color(0.56f, 0.28f, 0.18f)));
        }

        private static void BuildGallery(Transform p, Material cream, Material gold, Material red, Material wood)
        {
            for (int i = 0; i < 5; i++)
            {
                float x = -5.4f + i * 2.7f;
                Cylinder("Gallery Display Plinth", p, new Vector3(x, 0.55f, 0f), new Vector3(1.3f, 1.1f, 1.3f), cream);
                Capsule("Gallery Statue", p, new Vector3(x, 1.9f, 0f), new Vector3(0.65f, 1.5f, 0.55f), gold);
                Sphere("Gallery Bust Head", p, new Vector3(x, 3.0f, 0f), new Vector3(0.5f, 0.65f, 0.5f), cream);
                Cube("Gallery Bench", p, new Vector3(x, 0.65f, -4.3f), new Vector3(2.0f, 0.4f, 0.8f), red);
                Cube("Gallery Art Frame", p, new Vector3(x, 3.4f, 6.7f), new Vector3(1.8f, 2.4f, 0.18f), gold);
                Cube("Gallery Art Canvas", p, new Vector3(x, 3.4f, 6.58f), new Vector3(1.48f, 2.08f, 0.04f), i % 2 == 0 ? blueMaterial() : wood);
            }
        }

        private static Material blueMaterial() { return Mat("GalleryBlue", new Color(0.04f, 0.30f, 0.78f)); }

        private static void BuildStorage(Transform p, Material grey, Material wood, Material blue, Material red, Material accent)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * 5.3f;
                for (int level = 0; level < 4; level++)
                {
                    Cube("Storage Rack Shelf", p, new Vector3(x, 0.8f + level * 1.15f, 0f), new Vector3(0.35f, 0.12f, 10.8f), grey);
                    for (int z = -1; z <= 1; z++)
                        Cube("Storage Box", p, new Vector3(x - side * 0.8f, 1.3f + level * 1.15f, z * 3.0f),
                            new Vector3(1.5f, 0.9f, 2.2f), (level + z) % 2 == 0 ? blue : red);
                }
            }
            Cube("Wooden Storage Crate", p, new Vector3(0f, 0.65f, -3.9f), new Vector3(2.8f, 1.3f, 2.4f), wood);
            Cube("Large Storage Trunk", p, new Vector3(0f, 0.75f, 0.0f), new Vector3(3.2f, 1.5f, 2.1f), grey);
            Cylinder("Paint Bucket Pink", p, new Vector3(3.4f, 0.65f, 4.2f), new Vector3(0.85f, 1.3f, 0.85f), accent);
            Cylinder("Paint Bucket Blue", p, new Vector3(5.2f, 0.65f, 4.2f), new Vector3(0.85f, 1.3f, 0.85f), blue);
            Cube("Red Step Ladder", p, new Vector3(-1.7f, 2.2f, 5.0f), new Vector3(0.25f, 4.2f, 0.3f), red);
        }

        private static void BuildClassroom(Transform p, Material wood, Material blue, Material cream, Material greenMaterial)
        {
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 3; col++)
            {
                Vector3 at = new Vector3(-4f + col * 4f, 0f, 2.6f - row * 4.2f);
                Cube("Student Desk", p, at + Vector3.up * 1f, new Vector3(2.2f, 0.2f, 1.3f), wood);
                Cube("Student Chair Seat", p, at + new Vector3(0f, 0.6f, -1.3f), new Vector3(1.2f, 0.2f, 1.0f), blue);
                Cube("Student Chair Back", p, at + new Vector3(0f, 1.15f, -1.75f), new Vector3(1.2f, 1.0f, 0.18f), blue);
            }
            Cube("Classroom Chalkboard", p, new Vector3(0f, 2.9f, 6.7f), new Vector3(7.8f, 2.5f, 0.16f), greenMaterial);
            Sphere("Classroom Globe", p, new Vector3(-5.2f, 2.0f, 5f), new Vector3(0.9f, 0.9f, 0.9f), blue);
        }

        private static void BuildArcade(Transform p, Material grey, Material blue, Material accent, Material gold)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = -5.4f + i * 3.6f;
                Cube("Arcade Cabinet Body", p, new Vector3(x, 1.8f, 4.8f), new Vector3(2.2f, 3.6f, 1.4f), i % 2 == 0 ? blue : accent);
                Cube("Arcade Screen", p, new Vector3(x, 2.4f, 4.05f), new Vector3(1.6f, 1.2f, 0.08f), grey);
                Sphere("Arcade Joystick", p, new Vector3(x - 0.45f, 1.55f, 3.95f), new Vector3(0.22f, 0.35f, 0.22f), gold);
            }
            Cube("Foosball Table", p, new Vector3(0f, 1.0f, -2.2f), new Vector3(5.5f, 1.0f, 2.8f), woodMaterial());
        }

        private static Material woodMaterial() { return Mat("GameRoomWood", new Color(0.42f, 0.22f, 0.12f)); }

        private static void BuildCinema(Transform p, Material red, Material gold, Material grey)
        {
            Cube("Cinema Screen Frame", p, new Vector3(0f, 3.2f, 6.5f), new Vector3(10f, 5.4f, 0.3f), grey);
            Cube("Cinema Screen", p, new Vector3(0f, 3.2f, 6.3f), new Vector3(9.5f, 4.9f, 0.06f), Mat("CinemaBlueScreen", new Color(0.08f, 0.22f, 0.54f), 0f, 0.4f, true));
            for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
            {
                Vector3 at = new Vector3(-4.5f + col * 4.5f, row * 0.5f, -4.2f + row * 2.5f);
                Cube("Cinema Seat Base", p, at + new Vector3(0f, 0.65f, 0f), new Vector3(2.1f, 0.75f, 2.0f), red);
                Cube("Cinema Seat Back", p, at + new Vector3(0f, 1.5f, 0.8f), new Vector3(2.1f, 1.2f, 0.45f), red);
                Cube("Cinema Armrest", p, at + new Vector3(-1.0f, 1.0f, 0f), new Vector3(0.22f, 0.9f, 1.6f), gold);
            }
        }

        private static void BuildGym(Transform p, Material grey, Material blue, Material accent)
        {
            Cube("Gym Treadmill Base", p, new Vector3(-4.0f, 0.6f, 2.2f), new Vector3(3.2f, 0.45f, 1.4f), grey);
            Cube("Gym Treadmill Console", p, new Vector3(-4.0f, 2.0f, 3.0f), new Vector3(1.4f, 1.6f, 0.4f), blue);
            for (int i = 0; i < 4; i++)
                Cylinder("Gym Weight Plate", p, new Vector3(2.0f + i * 0.55f, 0.7f, 3.8f),
                    new Vector3(0.8f, 0.3f, 0.8f), grey);
            Sphere("Gym Exercise Ball", p, new Vector3(4.0f, 0.9f, -2.8f), new Vector3(1.6f, 1.6f, 1.6f), accent);
            Cube("Gym Yoga Mat", p, new Vector3(-2f, 0.08f, -3.5f), new Vector3(3.0f, 0.12f, 1.6f), blue);
        }

        private static void BuildMusicStudio(Transform p, Material wood, Material red, Material gold, Material grey)
        {
            Cube("Piano Body", p, new Vector3(-3.4f, 1.0f, 3.0f), new Vector3(4.0f, 1.8f, 1.6f), grey);
            Cube("Piano Keys", p, new Vector3(-3.4f, 1.95f, 2.12f), new Vector3(3.2f, 0.12f, 0.25f), creamMaterial());
            Cylinder("Drum Bass", p, new Vector3(2.6f, 1.0f, 2.0f), new Vector3(1.7f, 1.4f, 1.7f), red);
            for (int i = 0; i < 3; i++)
                Cylinder("Drum Tom", p, new Vector3(1.3f + i * 1.2f, 1.7f, 2.0f), new Vector3(0.75f, 0.65f, 0.75f), grey);
            for (int i = 0; i < 2; i++)
                Cube("Amplifier Cabinet", p, new Vector3(-5.0f + i * 10f, 1.4f, -4.2f), new Vector3(1.8f, 2.8f, 1.2f), grey);
            Cube("Keyboard Synth", p, new Vector3(0f, 1.1f, -4.8f), new Vector3(5f, 0.25f, 1.0f), wood);
            Cylinder("Guitar Stand", p, new Vector3(5.6f, 0.8f, 0f), new Vector3(0.6f, 1.6f, 0.6f), gold);
        }

        private static Material creamMaterial() { return Mat("MusicIvory", new Color(0.96f, 0.91f, 0.77f)); }

        private static void BuildLaboratory(Transform p, Material white, Material blue, Material teal, Material grey)
        {
            for (int i = 0; i < 3; i++)
            {
                Cube("Lab Bench", p, new Vector3(-4.5f + i * 4.5f, 1.1f, 4.0f), new Vector3(3.8f, 0.22f, 1.6f), grey);
                Cylinder("Lab Flask", p, new Vector3(-5.2f + i * 4.5f, 1.75f, 4.0f), new Vector3(0.65f, 1.2f, 0.65f), i % 2 == 0 ? blue : teal);
                Sphere("Lab Bubble", p, new Vector3(-3.7f + i * 4.5f, 1.8f, 4.0f), new Vector3(0.45f, 0.45f, 0.45f), white);
            }
            Cube("Lab Cabinet", p, new Vector3(-5.4f, 1.9f, -4.8f), new Vector3(2.2f, 3.8f, 1.8f), white);
            Sphere("Microscope Base", p, new Vector3(4.5f, 1.5f, -3.8f), new Vector3(1.0f, 0.3f, 0.8f), grey);
            Cylinder("Microscope Column", p, new Vector3(4.5f, 2.3f, -3.8f), new Vector3(0.15f, 1.2f, 0.15f), grey);
        }

        private static void BuildGarage(Transform p, Material grey, Material red, Material gold, Material blue, Material wood)
        {
            Cube("Car Chassis", p, new Vector3(0f, 0.8f, 0f), new Vector3(7.5f, 1.0f, 3.7f), red);
            Cube("Car Cabin", p, new Vector3(0f, 1.85f, 0.4f), new Vector3(4.4f, 1.4f, 3.1f), blue);
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Cylinder("Car Tire", p, new Vector3(x * 3.0f, 0.65f, z * 2.0f), new Vector3(0.95f, 0.45f, 0.95f), grey);
            Cube("Garage Tool Bench", p, new Vector3(-5.0f, 1.1f, 4.8f), new Vector3(3.0f, 2.2f, 1.4f), wood);
            Cube("Garage Tool Cabinet", p, new Vector3(5.4f, 1.7f, 4.9f), new Vector3(2.2f, 3.4f, 1.5f), gold);
        }

        private static void BuildKidsRoom(Transform p, Material wood, Material blue, Material accent, Material cream, Material gold)
        {
            Cube("Bunk Bed Lower Mattress", p, new Vector3(-4f, 1f, 1f), new Vector3(3.8f, 0.5f, 5.4f), blue);
            Cube("Bunk Bed Upper Mattress", p, new Vector3(-4f, 3.0f, 1f), new Vector3(3.8f, 0.5f, 5.4f), accent);
            Cube("Bunk Bed Side Rail", p, new Vector3(-4f, 3.45f, 3.6f), new Vector3(4.0f, 0.35f, 0.2f), wood);
            Sphere("Large Plush Toy", p, new Vector3(3.0f, 0.85f, 1.8f), new Vector3(1.3f, 1.3f, 1.2f), gold);
            Cube("Toy Chest", p, new Vector3(4.7f, 0.6f, -3.2f), new Vector3(2.5f, 1.2f, 1.8f), wood);
            Cube("Play Mat", p, new Vector3(0f, 0.04f, -2.2f), new Vector3(5f, 0.08f, 4f), cream);
        }

        private static void BuildLaundry(Transform p, Material white, Material blue, Material wood, Material teal)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 at = new Vector3(-4.8f + i * 4.8f, 1.4f, 4.5f);
                Cube("Washer Dryer", p, at, new Vector3(3.0f, 2.8f, 2.1f), white);
                Cylinder("Washer Door", p, at + new Vector3(0f, 0f, -1.1f), new Vector3(1.35f, 0.18f, 1.35f), blue);
                Cylinder("Laundry Basket", p, new Vector3(-5f + i * 5f, 0.6f, -3.5f), new Vector3(1.2f, 1.2f, 1.2f), teal);
            }
            Cube("Laundry Folding Table", p, new Vector3(0f, 1.0f, 0f), new Vector3(5f, 0.2f, 2.0f), wood);
        }

        private static void BuildWorkshop(Transform p, Material wood, Material grey, Material gold, Material red)
        {
            Cube("Workshop Bench Top", p, new Vector3(0f, 1.25f, 4.7f), new Vector3(10f, 0.25f, 1.7f), wood);
            Cube("Workshop Pegboard", p, new Vector3(0f, 3.0f, 6.55f), new Vector3(10f, 3.2f, 0.15f), grey);
            for (int i = 0; i < 8; i++)
                Cube("Workshop Tool " + i, p, new Vector3(-4.2f + i * 1.2f, 3.0f + (i % 2) * 0.7f, 6.35f),
                    new Vector3(0.22f, 0.9f, 0.12f), i % 2 == 0 ? gold : red);
            Cube("Tool Chest", p, new Vector3(-4.8f, 0.8f, -4.2f), new Vector3(2.3f, 1.6f, 1.6f), red);
            Cube("Timber Stack", p, new Vector3(3.8f, 0.75f, -4.2f), new Vector3(3.0f, 1.5f, 2f), wood);
        }

        private static void BuildAttic(Transform p, Material wood, Material grey, Material cream, Material gold)
        {
            Cube("Attic Trunk", p, new Vector3(-4f, 0.8f, 0f), new Vector3(3.0f, 1.6f, 2.2f), wood);
            Cube("Covered Furniture", p, new Vector3(3.8f, 1.2f, 1.8f), new Vector3(3.2f, 2.4f, 2.2f), cream);
            Sphere("Attic Globe", p, new Vector3(4.8f, 1.2f, -4.2f), new Vector3(1.0f, 1.0f, 1.0f), gold);
            Cube("Attic Stored Boxes", p, new Vector3(-2f, 0.65f, -4.8f), new Vector3(4.0f, 1.3f, 1.4f), grey);
        }

        private static void BuildBasement(Transform p, Material grey, Material teal, Material red)
        {
            for (int i = 0; i < 4; i++)
            {
                Cylinder("Basement Utility Pipe", p, new Vector3(-5f + i * 3.3f, 3.4f, 5.7f),
                    new Vector3(0.2f, 0.25f, 0.2f), grey);
            }
            Cube("Basement Control Panel", p, new Vector3(-5.3f, 2.1f, -4.8f), new Vector3(2.4f, 3.6f, 1.2f), teal);
            Cylinder("Basement Utility Tank", p, new Vector3(4.5f, 1.9f, -4.3f), new Vector3(1.7f, 3.8f, 1.7f), grey);
            Cube("Basement Warning Light", p, new Vector3(0f, 4.2f, 6.5f), new Vector3(1.2f, 0.45f, 0.15f), red);
        }

        private static void BuildGarden(Transform p, Material wood, Material cream, Material gold, Material plant, Material blue)
        {
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2f / 7f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 5.0f, 0f, Mathf.Sin(angle) * 4.4f);
                Sphere("Garden Flower Bed", p, pos + Vector3.up * 0.2f, new Vector3(2.0f, 0.4f, 1.8f), plant);
                Sphere("Garden Flower Cluster", p, pos + Vector3.up * 0.7f, new Vector3(1.2f, 0.65f, 1.2f),
                    i % 2 == 0 ? Mat("FlowerPink", new Color(1f, 0.24f, 0.59f)) : Mat("FlowerYellow", new Color(1f, 0.78f, 0.08f)));
            }
            Cylinder("Garden Fountain Base", p, new Vector3(0f, 0.3f, 0f), new Vector3(3.3f, 0.5f, 3.3f), cream);
            Cylinder("Garden Fountain Water", p, new Vector3(0f, 0.6f, 0f), new Vector3(2.8f, 0.1f, 2.8f), blue);
            Cylinder("Garden Fountain Stem", p, new Vector3(0f, 1.2f, 0f), new Vector3(0.5f, 1.2f, 0.5f), cream);
            Sphere("Garden Fountain Bowl", p, new Vector3(0f, 1.85f, 0f), new Vector3(1.7f, 0.32f, 1.7f), cream);
            Cylinder("Garden Tree Trunk", p, new Vector3(-5.2f, 2.5f, 4.5f), new Vector3(0.9f, 5f, 0.9f), wood);
            for (int i = 0; i < 5; i++)
                Sphere("Blossom Tree Canopy", p, new Vector3(-5.2f + (i - 2) * 1.2f, 5.0f + (i % 2) * 0.7f, 4.5f),
                    new Vector3(2.1f, 1.7f, 2.0f), Mat("BlossomPink", new Color(1f, 0.43f, 0.67f)));
            BuildPergola(p, wood, gold);
        }

        private static void BuildPergola(Transform p, Material wood, Material gold)
        {
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                Cube("Pergola Post", p, new Vector3(x * 5.5f, 2.5f, z * 4.2f), new Vector3(0.35f, 5f, 0.35f), wood);
            for (int i = 0; i < 7; i++)
                Cube("Pergola Roof Slat " + i, p, new Vector3(-5.0f + i * 1.65f, 5.1f, 0f),
                    new Vector3(0.28f, 0.3f, 9.0f), gold);
        }

        private static void AddHidingSlots(RoomData room, Transform slots)
        {
            Transform roomSlotParent = Child(slots, room.Id + " Validated Slot Anchors").transform;
            EnvironmentObjectMetadata reference = roomSlotParent.gameObject.AddComponent<EnvironmentObjectMetadata>();
            SurfaceFamily surface = room.Id == "GardenCourtyard" ? SurfaceFamily.Foliage :
                (room.Id == "RoyalPalace" || room.Id == "ArtGallery" || room.Id == "GrandHall"
                    ? SurfaceFamily.Marble : (room.Id == "StorageRoom" || room.Id == "Garage"
                        ? SurfaceFamily.Metal : SurfaceFamily.Wood));
            Color roomReferenceColour = room.Id == "GardenCourtyard" ? new Color(0.20f, 0.44f, 0.18f) :
                (room.Id == "RoyalPalace" ? new Color(0.66f, 0.035f, 0.08f) :
                    (room.Id == "Kitchen" ? new Color(0.08f, 0.48f, 0.44f) :
                        new Color(0.48f, 0.30f, 0.18f)));
            reference.Configure(room.Id + "_reference", room.Title + " reference surface", surface,
                roomReferenceColour, 0.55f);
            Vector3 center = room.Center;
            for (int i = 0; i < 3; i++)
            {
                GameObject slotObject = new GameObject(room.Id + "_Slot_" + (i + 1));
                slotObject.transform.SetParent(roomSlotParent, false);
                slotObject.transform.position = center + new Vector3(-4.4f + i * 4.4f, 0.02f, -4.4f);
                HidingSlot slot = slotObject.AddComponent<HidingSlot>();
                // These are placement candidates only. They still require NavMesh,
                // collider and target-prop validation in the finished scene.
            }
        }

        private static void CreateSpawnMarkers(Transform root)
        {
            GameObject spawnRoot = Child(root, "Spawn Markers");
            GameObject hider = new GameObject("Practice Hider Spawn");
            hider.transform.SetParent(spawnRoot.transform, false);
            hider.transform.position = new Vector3(-2.5f, 0.1f, -1f);
            GameObject seeker = new GameObject("Practice Seeker Spawn");
            seeker.transform.SetParent(spawnRoot.transform, false);
            seeker.transform.position = new Vector3(2.5f, 0.1f, -1f);

            GameObject cameraObject = new GameObject("Main Camera — Preview");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root, false);
            cameraObject.transform.position = new Vector3(0f, 18f, -33f);
            cameraObject.transform.LookAt(new Vector3(0f, 3.5f, 0f));
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            cameraObject.AddComponent<AudioListener>();
        }

        private static void BuildRooftop(Transform p, Material wood, Material cream, Material gold,
            Material plant, Material blue)
        {
            Material parapet = Mat("RooftopWarmStone", new Color(0.78f, 0.66f, 0.57f));
            Cube("Rooftop North Parapet", p, new Vector3(0f, 1.0f, 6.6f), new Vector3(15.5f, 2f, 0.35f), parapet);
            Cube("Rooftop South Parapet", p, new Vector3(0f, 1.0f, -6.6f), new Vector3(15.5f, 2f, 0.35f), parapet);
            Cube("Rooftop East Parapet", p, new Vector3(7.8f, 1.0f, 0f), new Vector3(0.35f, 2f, 13.5f), parapet);
            Cube("Rooftop West Parapet", p, new Vector3(-7.8f, 1.0f, 0f), new Vector3(0.35f, 2f, 13.5f), parapet);
            BuildPergola(p, wood, gold);
            BuildPlant(p, new Vector3(-6.0f, 0f, -4.6f), cream, plant);
            BuildPlant(p, new Vector3(6.0f, 0f, -4.6f), cream, plant);
            Cube("Rooftop Outdoor Sofa Base", p, new Vector3(0f, 0.6f, -3.7f), new Vector3(5f, 0.8f, 1.3f), cream);
            Cube("Rooftop Outdoor Sofa Back", p, new Vector3(0f, 1.4f, -4.2f), new Vector3(5f, 1.1f, 0.28f), blue);
            Cylinder("Rooftop Telescope Tripod", p, new Vector3(5.3f, 0.9f, 4.5f), new Vector3(0.18f, 1.8f, 0.18f), gold);
            Cylinder("Rooftop Telescope Tube", p, new Vector3(5.3f, 2.0f, 4.5f), new Vector3(0.28f, 1.6f, 0.28f), blue);
        }

        private static void BuildWorldLighting(Transform parent)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.54f, 0.68f, 0.86f);
            RenderSettings.ambientEquatorColor = new Color(0.86f, 0.75f, 0.68f);
            RenderSettings.ambientGroundColor = new Color(0.34f, 0.29f, 0.27f);
            GameObject sun = new GameObject("Warm Directional Key");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(48f, -34f, 0f);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(1f, 0.86f, 0.71f);
            light.shadows = LightShadows.Soft;

            foreach (RoomData room in Rooms)
            {
                GameObject roomLight = new GameObject(room.Id + " Warm Ceiling Light");
                roomLight.transform.SetParent(parent, false);
                roomLight.transform.position = room.Center + new Vector3(0f, room.Atrium ? 8.0f : 4.3f, 0f);
                Light point = roomLight.AddComponent<Light>();
                point.type = LightType.Point;
                point.range = room.Atrium ? 22f : 13f;
                point.intensity = room.Atrium ? 2.2f : 1.1f;
                point.color = new Color(1f, 0.78f, 0.55f);
                point.shadows = LightShadows.None;
            }
        }

        private static GameObject Cube(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            return Primitive(name, PrimitiveType.Cube, parent, localPosition, scale, material);
        }

        private static GameObject Sphere(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            return Primitive(name, PrimitiveType.Sphere, parent, localPosition, scale, material);
        }

        private static GameObject Cylinder(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            return Primitive(name, PrimitiveType.Cylinder, parent, localPosition, scale, material);
        }

        private static GameObject Capsule(string name, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            return Primitive(name, PrimitiveType.Capsule, parent, localPosition, scale, material);
        }

        private static GameObject Primitive(string name, PrimitiveType type, Transform parent,
            Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            return go;
        }

        private static GameObject Child(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void CreateMaterials()
        {
            Mat("WallPink", new Color(0.91f, 0.69f, 0.68f));
            Mat("WallPurple", new Color(0.55f, 0.40f, 0.72f));
            Mat("FloorIvory", new Color(0.88f, 0.79f, 0.73f), 0.05f, 0.72f);
            Mat("FloorBlue", new Color(0.15f, 0.36f, 0.68f), 0.08f, 0.55f);
        }

        private static Material GetRoomWallMaterial(string id)
        {
            if (id == "Office" || id == "Library" || id == "Classroom") return Mat("RoomPurpleWall", new Color(0.63f, 0.48f, 0.75f));
            if (id == "StorageRoom" || id == "Basement" || id == "Garage" || id == "Workshop") return Mat("RoomIndustrialWall", new Color(0.49f, 0.51f, 0.54f));
            if (id == "GardenCourtyard" || id == "RooftopTerrace") return Mat("GardenLowWall", new Color(0.80f, 0.72f, 0.58f));
            return Mat("RoomWarmPinkWall", new Color(0.91f, 0.72f, 0.70f));
        }

        private static Material GetRoomFloorMaterial(string id)
        {
            if (id == "GardenCourtyard" || id == "RooftopTerrace") return Mat("GardenGrass", new Color(0.23f, 0.48f, 0.20f));
            if (id == "RoyalPalace" || id == "GrandHall" || id == "ArtGallery") return Mat("PolishedMarble", new Color(0.89f, 0.83f, 0.79f), 0.1f, 0.88f);
            if (id == "Office" || id == "Library" || id == "LivingRoom") return Mat("WarmWoodFloor", new Color(0.54f, 0.32f, 0.19f), 0.04f, 0.56f);
            return Mat("RoomTile", new Color(0.81f, 0.75f, 0.73f), 0.04f, 0.66f);
        }

        private static Material Mat(string name, Color color, float metallic = 0f, float smoothness = 0.48f, bool emissive = false)
        {
            if (Materials.TryGetValue(name, out Material existing) && existing != null) return existing;
            string path = MaterialFolder + "/" + name.Replace(" ", "_") + ".mat";
            Material asset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (asset == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                asset = new Material(shader);
                asset.name = name;
                AssetDatabase.CreateAsset(asset, path);
            }

            if (asset.HasProperty("_BaseColor")) asset.SetColor("_BaseColor", color);
            if (asset.HasProperty("_Color")) asset.SetColor("_Color", color);
            if (asset.HasProperty("_Metallic")) asset.SetFloat("_Metallic", metallic);
            if (asset.HasProperty("_Smoothness")) asset.SetFloat("_Smoothness", smoothness);
            if (emissive && asset.HasProperty("_EmissionColor"))
            {
                asset.SetColor("_EmissionColor", color * 1.6f);
                asset.EnableKeyword("_EMISSION");
            }
            EditorUtility.SetDirty(asset);
            Materials[name] = asset;
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
