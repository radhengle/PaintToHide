#!/usr/bin/env python3
"""Fast repository checks. This does not replace Unity Editor compilation or device QA."""
from __future__ import annotations
import ast
import json
from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "UnityProject"
MANIFEST = PROJECT / "Packages" / "manifest.json"
BLENDER_SCRIPT = ROOT / "tools" / "blender" / "generate_reference_assets.py"
BUILDER = PROJECT / "Assets" / "PaintBlend" / "Editor" / "GrandHallSceneBuilder.cs"
ROOMS = [
    "Bedroom", "Bathroom", "Library", "Cinema", "RoyalPalace",
    "ArtGallery", "LivingRoom", "DiningRoom", "Kitchen", "Office",
    "StorageRoom", "Classroom", "GrandHall", "Laboratory", "Garage",
    "LaundryRoom", "Workshop", "Arcade", "Gym", "MusicStudio",
    "Attic", "KidsRoom", "GardenCourtyard", "Basement", "RooftopTerrace",
]
ASSET_BUILDERS = [
    "build_living_room", "build_kitchen", "build_royal_palace",
    "build_gallery_and_storage", "build_office", "build_hall_and_garden",
    "build_dining_room", "build_bedroom", "build_bathroom", "build_library",
    "build_classroom", "build_arcade", "build_cinema", "build_gym",
    "build_music_studio", "build_laboratory", "build_garage", "build_kids_room",
    "build_laundry", "build_workshop", "build_attic", "build_basement", "build_rooftop",
]

def require(condition: bool, message: str) -> None:
    if not condition:
        raise RuntimeError(message)

def main() -> int:
    require(MANIFEST.is_file(), "Unity package manifest is missing.")
    dependencies = json.loads(MANIFEST.read_text(encoding="utf-8"))["dependencies"]
    for package in ("com.unity.render-pipelines.universal", "com.unity.ai.navigation",
                    "com.unity.inputsystem", "com.unity.ugui"):
        require(package in dependencies, f"Required Unity package missing: {package}")

    require(BUILDER.is_file(), "Grand Hall editor world builder is missing.")
    builder = BUILDER.read_text(encoding="utf-8")
    missing_rooms = [room for room in ROOMS if f'"{room}"' not in builder]
    require(not missing_rooms, "World builder room IDs missing: " + ", ".join(missing_rooms))

    require(BLENDER_SCRIPT.is_file(), "Blender asset generator is missing.")
    blender_source = BLENDER_SCRIPT.read_text(encoding="utf-8")
    ast.parse(blender_source, filename=str(BLENDER_SCRIPT))
    missing_builders = [name for name in ASSET_BUILDERS if not re.search(rf"^def {name}\\(", blender_source, re.M)]
    require(not missing_builders, "Blender room asset builders missing: " + ", ".join(missing_builders))

    for relative in (
        "Assets/PaintBlend/Scripts/Domain/LocalPracticeMatchDirector.cs",
        "Assets/PaintBlend/Scripts/Domain/TeamAllocator.cs",
        "Assets/PaintBlend/Scripts/Gameplay/HidePositionValidator.cs",
        "Assets/PaintBlend/Scripts/Painting/PaintSurface.cs",
        "Assets/PaintBlend/Scripts/AI/HiderBotPlanner.cs",
        "Assets/PaintBlend/Scripts/AI/SeekerBotPlanner.cs",
        "Assets/PaintBlend/Scripts/World/WorldGraph.cs",
        "Assets/PaintBlend/Shaders/UVPaintStamp.shader",
    ):
        require((PROJECT / relative).is_file(), f"Required runtime source missing: {relative}")

    print(f"PASS: package manifest, {len(ROOMS)} room modules, {len(ASSET_BUILDERS)} Blender kit builders, and core source files.")
    print("NOTE: This is repository/static validation only. Open the project in Unity 2022.3.50f1 to compile, bake navigation, render and run EditMode/PlayMode tests.")
    return 0

if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print("FAIL:", exc, file=sys.stderr)
        raise SystemExit(1)
