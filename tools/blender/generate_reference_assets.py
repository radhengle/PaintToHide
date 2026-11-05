"""
Paint & Blend — reference-driven Blender asset kit generator.

Run in Blender 3.6+ / 4.x:
  blender --background --python tools/blender/generate_reference_assets.py

Writes:
  <output>/PaintBlend_ReferenceAssetKit.blend
  <output>/FBX/PB_<ROOM>_AssetKit_v001.fbx

This builds beveled, UV-unwrapped modular base assets at metre scale. Artists
must refine sculptural details, topology, textures and LODs against the image
references before the assets are approved for release.
"""
import bpy
import math
import os
from mathutils import Vector

OUT = os.environ.get("PAINTBLEND_EXPORT_DIR", os.path.abspath("./PaintBlend_ArtOutput"))
FBX_OUT = os.path.join(OUT, "FBX")
os.makedirs(FBX_OUT, exist_ok=True)

def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for block in bpy.data.collections:
        if block.name.startswith("PB_"):
            bpy.data.collections.remove(block)

def material(name, color, metallic=0.0, roughness=0.48):
    mat = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    mat.diffuse_color = (*color, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        bsdf.inputs["Metallic"].default_value = metallic
        bsdf.inputs["Roughness"].default_value = roughness
    return mat

MAT = {
    "ivory": material("PB_Ivory_Stone", (0.92, 0.82, 0.74), 0.0, 0.38),
    "pink": material("PB_Petal_Pink", (0.88, 0.53, 0.57), 0.0, 0.42),
    "cream": material("PB_Cream_Fabric", (0.94, 0.87, 0.76), 0.0, 0.78),
    "red": material("PB_Royal_Red_Velvet", (0.56, 0.025, 0.07), 0.0, 0.7),
    "gold": material("PB_Polished_Gold", (0.92, 0.57, 0.11), 0.72, 0.25),
    "oak": material("PB_Warm_Oak", (0.34, 0.16, 0.075), 0.0, 0.42),
    "teal": material("PB_Teal_Cabinet", (0.035, 0.40, 0.39), 0.0, 0.32),
    "blue": material("PB_Bright_Blue", (0.035, 0.27, 0.78), 0.0, 0.36),
    "yellow": material("PB_Sunshine_Yellow", (0.98, 0.67, 0.06), 0.0, 0.45),
    "green": material("PB_Leaf_Green", (0.065, 0.29, 0.10), 0.0, 0.78),
    "metal": material("PB_Brushed_Metal", (0.40, 0.46, 0.50), 0.72, 0.3),
    "marble": material("PB_Marble", (0.83, 0.80, 0.78), 0.05, 0.25),
    "glass": material("PB_Glass_Blue", (0.10, 0.45, 0.68), 0.05, 0.14),
    "pinkpaint": material("PB_Paint_Pink", (0.96, 0.04, 0.39), 0.0, 0.25),
}

def new_collection(name):
    coll = bpy.data.collections.new("PB_" + name)
    bpy.context.scene.collection.children.link(coll)
    return coll

def move_to_collection(obj, coll):
    for old in list(obj.users_collection):
        old.objects.unlink(obj)
    coll.objects.link(obj)
    return obj

def asset_root(name, coll):
    root = bpy.data.objects.new("PB_" + name, None)
    coll.objects.link(root)
    root.empty_display_type = "PLAIN_AXES"
    root.empty_display_size = 0.25
    return root

def finish_mesh(obj, name, mat, bevel=0.035):
    obj.name = name
    if mat:
        obj.data.materials.append(mat)
    if obj.type == "MESH":
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        if bevel > 0:
            mod = obj.modifiers.new("PB_Soft_Edge_Bevel", "BEVEL")
            mod.width = bevel
            mod.segments = 3
            mod.limit_method = "ANGLE"
            normal = obj.modifiers.new("PB_Weighted_Corner_Normals", "WEIGHTED_NORMAL")
            normal.keep_sharp = True
        try:
            bpy.ops.object.mode_set(mode="EDIT")
            bpy.ops.mesh.select_all(action="SELECT")
            bpy.ops.uv.smart_project(island_margin=0.02)
            bpy.ops.object.mode_set(mode="OBJECT")
        except Exception:
            if bpy.context.object and bpy.context.object.mode == "EDIT":
                bpy.ops.object.mode_set(mode="OBJECT")
    return obj

def cube(name, loc, dims, mat, coll, parent=None, bevel=0.04):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.object
    obj.dimensions = dims
    finish_mesh(obj, name, mat, bevel)
    move_to_collection(obj, coll)
    if parent: obj.parent = parent
    return obj

def cylinder(name, loc, radius, depth, mat, coll, parent=None, vertices=48, bevel=0.025):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc)
    obj = bpy.context.object
    finish_mesh(obj, name, mat, bevel)
    move_to_collection(obj, coll)
    if parent: obj.parent = parent
    return obj

def sphere(name, loc, scale, mat, coll, parent=None, segments=32, rings=20):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=loc)
    obj = bpy.context.object
    obj.scale = scale
    finish_mesh(obj, name, mat, 0.0)
    for poly in obj.data.polygons: poly.use_smooth = True
    move_to_collection(obj, coll)
    if parent: obj.parent = parent
    return obj

def rod_between(name, a, b, radius, mat, coll, parent=None):
    mid = (Vector(a) + Vector(b)) * 0.5
    direction = Vector(b) - Vector(a)
    obj = cylinder(name, mid, radius, direction.length, mat, coll, parent, vertices=24, bevel=0.015)
    obj.rotation_mode = "QUATERNION"
    obj.rotation_quaternion = direction.to_track_quat("Z", "Y")
    return obj

def export_collection(coll, filepath):
    bpy.ops.object.select_all(action="DESELECT")
    selected = []
    for obj in coll.objects:
        selected.append(obj)
        selected.extend(list(obj.children_recursive))
    selected = list(dict.fromkeys(selected))
    for obj in selected:
        obj.select_set(True)
    if selected:
        bpy.context.view_layer.objects.active = selected[0]
        bpy.ops.export_scene.fbx(
            filepath=filepath, use_selection=True, apply_unit_scale=True,
            apply_scale_options="FBX_SCALE_ALL", object_types={"MESH", "EMPTY"},
            use_mesh_modifiers=True, mesh_smooth_type="FACE",
            add_leaf_bones=False, bake_anim=False
        )
    bpy.ops.object.select_all(action="DESELECT")

def build_living_room():
    coll = new_collection("LivingRoom")
    root = asset_root("LivingRoom_AssetKit", coll)
    # Sofa dimensions and colours are anchored to the cream/blue/yellow/pink reference.
    cube("Sectional_Base", (0,0,0), (3.9,0.55,1.25), MAT["cream"], coll, root, 0.16)
    cube("Sectional_Back", (0,0.75,0.52), (3.85,1.25,0.25), MAT["cream"], coll, root, 0.13)
    for x in (-1.78, 1.78):
        cube("Sectional_Arm", (x,0.43,0), (0.34,0.95,1.35), MAT["cream"], coll, root, 0.14)
    for i, x in enumerate((-1.18, 0.0, 1.18)):
        cube("Seat_Cushion_%02d" % i, (x,0.37,-0.05), (1.08,0.18,1.0),
             [MAT["yellow"], MAT["blue"], MAT["pinkpaint"]][i], coll, root, 0.12)
    cube("Coffee_Table_Top", (0,0.75,-2.0), (2.25,0.14,1.2), MAT["oak"], coll, root, 0.06)
    for x in (-0.95,0.95):
        for y in (-0.45,0.45):
            cube("Coffee_Table_Leg", (x,0.36,-2.0+y), (0.12,0.72,0.12), MAT["oak"], coll, root, 0.025)
    cube("Media_Console", (0,0.5,3.1), (3.2,0.9,0.65), MAT["oak"], coll, root, 0.06)
    cube("TV_Frame", (0,1.75,3.18), (2.6,1.55,0.12), MAT["metal"], coll, root, 0.04)
    cube("TV_Glass", (0,1.75,3.10), (2.42,1.37,0.04), MAT["blue"], coll, root, 0.01)
    # A modular bookshelf with real shelf boards and book spines.
    for z in (2.2,2.9,3.6,4.3):
        cube("Bookcase_Shelf", (4.0,z/2,3.5), (2.2,0.12,0.6), MAT["oak"], coll, root, 0.025)
    for i in range(9):
        h = 0.36 + (i%3)*0.06
        cube("Book_Spine_%02d" % i, (3.1+i*0.22,0.8+h/2,3.45), (0.16,h,0.38),
             [MAT["blue"],MAT["red"],MAT["gold"],MAT["green"]][i%4], coll, root, 0.012)
    return coll

def build_kitchen():
    coll = new_collection("Kitchen")
    root = asset_root("Kitchen_AssetKit", coll)
    for i in range(7):
        x = -3.0 + i
        cube("Teal_Base_Cabinet_%02d" % i, (x,0.9,3.2), (0.92,1.8,0.9), MAT["teal"], coll, root, 0.045)
        cube("Teal_Upper_Cabinet_%02d" % i, (x,3.15,3.2), (0.92,1.45,0.75), MAT["teal"], coll, root, 0.045)
        cylinder("Cabinet_Handle_%02d" % i, (x+0.26,0.9,2.73), 0.025, 0.24, MAT["gold"], coll, root, 16, 0.01)
    cube("Stone_Countertop", (0,1.92,3.12), (7.1,0.16,1.1), MAT["marble"], coll, root, 0.035)
    cube("Blue_Refrigerator_Body", (-4.0,1.65,-1.2), (1.75,3.3,1.75), MAT["blue"], coll, root, 0.12)
    cube("Fridge_Freezer_Seam", (-4.0,2.2,-2.09), (1.65,0.035,0.03), MAT["metal"], coll, root, 0.01)
    for y in (1.2,2.0):
        cube("Fridge_Handle", (-3.42,y,-2.1), (0.08,0.48,0.08), MAT["cream"], coll, root, 0.025)
    cube("Kitchen_Table_Top", (0,1.0,-1.2), (3.6,0.16,2.0), MAT["oak"], coll, root, 0.06)
    for x in (-1.5,1.5):
        for z in (-0.75,0.75):
            cube("Kitchen_Table_Leg", (x,0.48,-1.2+z), (0.12,0.9,0.12), MAT["oak"], coll, root, 0.025)
    for x in (-2.5,2.5):
        cube("Yellow_Dining_Chair_Seat", (x,0.9,-1.2), (0.9,0.18,0.9), MAT["yellow"], coll, root, 0.1)
        cube("Yellow_Dining_Chair_Back", (x,1.45,-0.78), (0.9,1.0,0.16), MAT["yellow"], coll, root, 0.08)
    return coll

def build_royal_palace():
    coll = new_collection("RoyalPalace")
    root = asset_root("RoyalPalace_AssetKit", coll)
    cube("Throne_Dais_Base", (0,0.25,2.0), (5.5,0.5,3.5), MAT["marble"], coll, root, 0.1)
    for step in range(3):
        cube("Throne_Dais_Step_%d" % step, (0,0.15+step*0.18,0.0+step*0.68),
             (5.1-step*0.55,0.28,0.8), MAT["red"], coll, root, 0.05)
    cube("Throne_Seat", (0,1.2,2.3), (2.35,0.42,1.6), MAT["red"], coll, root, 0.12)
    cube("Throne_Back", (0,2.55,3.0), (2.3,2.7,0.35), MAT["red"], coll, root, 0.14)
    cube("Throne_Gold_Crest", (0,4.0,3.0), (2.7,0.22,0.48), MAT["gold"], coll, root, 0.07)
    for x in (-1.38,1.38):
        cube("Throne_Arm_Rest", (x,1.75,2.3), (0.28,1.1,1.3), MAT["gold"], coll, root, 0.09)
    for x in (-3.0,3.0):
        cylinder("Lion_Display_Plinth", (x,0.55,2.0), 0.72, 1.1, MAT["marble"], coll, root)
        sphere("Lion_Body", (x,1.45,2.0), (0.82,0.7,1.05), MAT["gold"], coll, root)
        sphere("Lion_Head", (x,2.1,2.5), (0.62,0.65,0.62), MAT["gold"], coll, root)
        sphere("Lion_Mane", (x,2.08,2.37), (0.78,0.82,0.36), MAT["gold"], coll, root)
    return coll

def build_gallery_and_storage():
    gallery = new_collection("ArtGallery")
    root = asset_root("ArtGallery_AssetKit", gallery)
    for i, x in enumerate((-4.0,-1.3,1.3,4.0)):
        cylinder("Gallery_Plinth_%02d" % i, (x,0.58,0), 0.66, 1.16, MAT["marble"], gallery, root)
        sphere("Classical_Bust_%02d" % i, (x,1.75,0), (0.46,0.7,0.42), MAT["ivory"], gallery, root)
        cube("Gold_Picture_Frame_%02d" % i, (x,3.1,3.8), (1.8,2.4,0.16), MAT["gold"], gallery, root, 0.08)
        cube("Painting_Canvas_%02d" % i, (x,3.1,3.69), (1.52,2.12,0.05), [MAT["blue"],MAT["pink"],MAT["red"],MAT["green"]][i], gallery, root, 0.015)

    storage = new_collection("StorageRoom")
    sroot = asset_root("StorageRoom_AssetKit", storage)
    for side in (-1,1):
        x = side*4.2
        for level in range(4):
            cube("Rack_Shelf", (x,0.4+level*1.05,0), (0.16,0.12,6.2), MAT["metal"], storage, sroot, 0.02)
            for z in (-2.0,0.0,2.0):
                cube("Storage_Bin", (x-side*0.65,0.95+level*1.05,z), (1.1,0.75,1.6),
                     [MAT["blue"],MAT["yellow"],MAT["red"]][(level+int(z))%3], storage, sroot, 0.045)
    cube("Shipping_Crate", (0,0.65,-3.4), (2.2,1.3,1.9), MAT["oak"], storage, sroot, 0.06)
    cylinder("Paint_Bucket", (2.0,0.55,3.3), 0.42, 1.1, MAT["pinkpaint"], storage, sroot)
    return gallery, storage

def build_office():
    coll = new_collection("Office")
    root = asset_root("Office_AssetKit", coll)
    for x in (-2.6,2.6):
        cube("Office_Desk_Top", (x,1.05,0), (3.3,0.16,1.7), MAT["oak"], coll, root, 0.05)
        for dx in (-1.4,1.4):
            cube("Office_Desk_Pedestal", (x+dx,0.5,0), (0.48,0.95,1.35), MAT["cream"], coll, root, 0.04)
        cube("Monitor_Frame", (x,2.0,0.48), (1.35,0.9,0.12), MAT["metal"], coll, root, 0.035)
        cube("Monitor_Screen", (x,2.0,0.405), (1.19,0.72,0.035), MAT["blue"], coll, root, 0.015)
        cylinder("Chair_Post", (x,0.42,-2.0), 0.09, 0.75, MAT["metal"], coll, root)
        cube("Blue_Swivel_Chair_Seat", (x,0.88,-2.0), (1.15,0.24,1.1), MAT["blue"], coll, root, 0.11)
        cube("Blue_Swivel_Chair_Back", (x,1.6,-2.45), (1.12,1.25,0.2), MAT["blue"], coll, root, 0.1)
    cube("Filing_Cabinet", (-5.2,1.1,3.5), (1.55,2.2,1.3), MAT["metal"], coll, root, 0.05)
    cube("Printer", (4.8,1.1,3.5), (1.8,1.25,1.4), MAT["cream"], coll, root, 0.08)
    return coll

def build_hall_and_garden():
    hall = new_collection("GrandHall")
    root = asset_root("GrandHall_AssetKit", hall)
    for x in (-6.0,6.0):
        for z in (-4.0,4.0):
            cylinder("GrandHall_Column", (x,3.0,z), 0.42, 6.0, MAT["ivory"], hall, root)
            cylinder("Column_Capital", (x,5.9,z), 0.62, 0.28, MAT["gold"], hall, root)
    cylinder("Fountain_Basin_Outer", (0,0.35,0), 2.4, 0.55, MAT["ivory"], hall, root)
    cylinder("Fountain_Water", (0,0.66,0), 2.05, 0.08, MAT["blue"], hall, root)
    cylinder("Fountain_Stem", (0,1.35,0), 0.48, 1.35, MAT["marble"], hall, root)
    sphere("Fountain_Top_Bowl", (0,2.08,0), (1.25,0.28,1.25), MAT["ivory"], hall, root)

    garden = new_collection("Garden")
    groot = asset_root("Garden_AssetKit", garden)
    cylinder("Garden_Fountain_Basin", (0,0.28,0), 1.95, 0.48, MAT["ivory"], garden, groot)
    cylinder("Garden_Water", (0,0.54,0), 1.63, 0.07, MAT["blue"], garden, groot)
    cylinder("Blossom_Tree_Trunk", (-4.2,2.2,3.2), 0.42, 4.4, MAT["oak"], garden, groot)
    for i in range(7):
        angle = math.tau*i/7
        sphere("Blossom_Cluster_%02d" % i,
               (-4.2+math.cos(angle)*1.35,4.3+(i%2)*0.45,3.2+math.sin(angle)*1.35),
               (1.15,0.9,1.1), MAT["pink"], garden, groot)
    for x in (-4.8,4.8):
        for z in (-3.6,3.6):
            cube("Pergola_Post", (x,2.4,z), (0.24,4.8,0.24), MAT["oak"], garden, groot, 0.035)
    for i in range(7):
        cube("Pergola_Slat", (-4.5+i*1.5,4.9,0), (0.18,0.18,8.0), MAT["gold"], garden, groot, 0.025)
    return hall, garden


def build_dining_room():
    coll = new_collection("DiningRoom")
    root = asset_root("DiningRoom_AssetKit", coll)
    cube("Dining_Table_Top", (0,1.15,0), (5.2,0.18,2.5), MAT["oak"], coll, root, 0.09)
    for x in (-2.25,2.25):
        for z in (-0.95,0.95):
            cube("Dining_Table_Leg", (x,0.55,z), (0.18,1.1,0.18), MAT["gold"], coll, root, 0.035)
    for x in (-3.3,-1.1,1.1,3.3):
        for z in (-2.0,2.0):
            cube("Dining_Chair_Seat", (x,0.92,z), (0.92,0.22,0.92), MAT["cream"], coll, root, 0.12)
            cube("Dining_Chair_Back", (x,1.55,z+(0.38 if z < 0 else -0.38)), (0.92,1.15,0.18), MAT["cream"], coll, root, 0.12)
    cube("Dining_Sideboard", (0,0.9,4.2), (4.6,1.8,0.95), MAT["oak"], coll, root, 0.07)
    for x in (-1.4,0,1.4):
        sphere("Dining_Vase", (x,2.0,4.2), (0.28,0.45,0.28), MAT["gold"], coll, root)
    for x in (-1.8,1.8):
        cylinder("Chandelier_Pendant", (x,3.4,0), 0.38, 0.35, MAT["gold"], coll, root)
    return coll

def build_bedroom():
    coll = new_collection("Bedroom")
    root = asset_root("Bedroom_AssetKit", coll)
    cube("Bed_Frame", (0,0.58,0), (4.0,0.9,5.6), MAT["oak"], coll, root, 0.12)
    cube("Mattress", (0,1.12,0), (3.9,0.42,5.45), MAT["cream"], coll, root, 0.18)
    cube("Blue_Duvet", (0,1.42,-0.65), (3.85,0.16,3.6), MAT["blue"], coll, root, 0.14)
    cube("Headboard", (0,2.0,2.8), (4.3,2.0,0.28), MAT["pink"], coll, root, 0.15)
    for x in (-1.0,1.0):
        cube("Bed_Pillow", (x,1.48,1.85), (1.25,0.28,0.85), MAT["cream"], coll, root, 0.18)
    for x in (-2.55,2.55):
        cube("Bedside_Table", (x,0.65,2.2), (1.0,1.3,1.0), MAT["oak"], coll, root, 0.08)
        cylinder("Bedside_Lamp_Base", (x,1.45,2.2), 0.22, 0.16, MAT["gold"], coll, root)
        sphere("Bedside_Lamp_Shade", (x,1.82,2.2), (0.42,0.38,0.42), MAT["yellow"], coll, root)
    cube("Wardrobe", (-5.2,2.0,-2.8), (2.2,4.0,1.5), MAT["oak"], coll, root, 0.08)
    for x in (-5.6,-4.8):
        cylinder("Wardrobe_Handle", (x,2.0,-3.62), 0.035, 0.48, MAT["gold"], coll, root, 16)
    return coll

def build_bathroom():
    coll = new_collection("Bathroom")
    root = asset_root("Bathroom_AssetKit", coll)
    cube("Freestanding_Bath_Base", (2.8,0.58,1.4), (3.7,1.1,2.0), MAT["ivory"], coll, root, 0.32)
    cube("Bath_Water", (2.8,1.1,1.4), (3.15,0.06,1.45), MAT["blue"], coll, root, 0.12)
    cube("Vanity_Cabinet", (-3.4,0.8,3.4), (3.6,1.6,1.25), MAT["oak"], coll, root, 0.08)
    cube("Vanity_Counter", (-3.4,1.65,3.4), (3.8,0.16,1.4), MAT["marble"], coll, root, 0.06)
    sphere("Wash_Basin", (-3.4,1.77,3.25), (0.75,0.16,0.46), MAT["ivory"], coll, root)
    cube("Bathroom_Mirror_Frame", (-3.4,2.9,4.15), (2.5,2.15,0.14), MAT["gold"], coll, root, 0.07)
    cube("Bathroom_Mirror", (-3.4,2.9,4.06), (2.25,1.9,0.05), MAT["glass"], coll, root, 0.02)
    cylinder("Toilet_Base", (4.8,0.42,-3.5), 0.42, 0.84, MAT["ivory"], coll, root)
    sphere("Toilet_Bowl", (4.8,0.82,-3.5), (0.55,0.26,0.8), MAT["ivory"], coll, root)
    cube("Shower_Glass_Panel", (-0.3,2.0,-4.3), (0.12,4.0,4.2), MAT["glass"], coll, root, 0.025)
    return coll

def build_library():
    coll = new_collection("Library")
    root = asset_root("Library_AssetKit", coll)
    for x in (-5.0,-1.8,1.8,5.0):
        cube("Library_Bookcase_Back", (x,2.3,3.7), (2.5,4.6,0.22), MAT["oak"], coll, root, 0.045)
        for y in (0.55,1.45,2.35,3.25,4.15):
            cube("Library_Shelf", (x,y,3.48), (2.45,0.12,0.52), MAT["gold"], coll, root, 0.025)
        for i in range(8):
            height = 0.55 + (i % 3) * 0.12
            cube("Library_Book", (x-0.98+i*0.27,0.62+height/2,3.42),
                 (0.2,height,0.35), [MAT["blue"],MAT["red"],MAT["green"],MAT["gold"]][i%4], coll, root, 0.015)
    cube("Reading_Table", (0,1.1,-1.2), (4.2,0.18,2.0), MAT["oak"], coll, root, 0.07)
    for x in (-2.1,2.1):
        for z in (-0.8,0.8):
            cube("Reading_Table_Leg", (x,0.52,-1.2+z), (0.14,1.05,0.14), MAT["oak"], coll, root, 0.025)
    rod_between("Library_Ladder_Left", (5.9,0.1,2.2), (5.9,3.3,3.2), 0.055, MAT["gold"], coll, root)
    rod_between("Library_Ladder_Right", (6.45,0.1,2.2), (6.45,3.3,3.2), 0.055, MAT["gold"], coll, root)
    return coll

def build_classroom():
    coll = new_collection("Classroom")
    root = asset_root("Classroom_AssetKit", coll)
    for row in range(2):
        for col in range(3):
            x, z = -3.6 + col*3.6, 2.2-row*4.1
            cube("Student_Desk", (x,1.0,z), (2.0,0.16,1.25), MAT["oak"], coll, root, 0.06)
            cube("Student_Chair_Seat", (x,0.65,z-1.25), (1.0,0.18,0.9), MAT["blue"], coll, root, 0.07)
            cube("Student_Chair_Back", (x,1.25,z-1.65), (1.0,1.0,0.16), MAT["blue"], coll, root, 0.07)
    cube("Classroom_Chalkboard_Frame", (0,3.0,4.5), (7.8,2.6,0.22), MAT["oak"], coll, root, 0.06)
    cube("Classroom_Chalkboard", (0,3.0,4.36), (7.3,2.15,0.05), MAT["green"], coll, root, 0.015)
    sphere("Classroom_Globe", (-5.0,1.9,3.1), (0.55,0.55,0.55), MAT["blue"], coll, root)
    return coll

def build_arcade():
    coll = new_collection("Arcade")
    root = asset_root("Arcade_AssetKit", coll)
    for i, x in enumerate((-5.0,-1.7,1.7,5.0)):
        cube("Arcade_Cabinet_%02d" % i, (x,1.7,3.5), (1.65,3.4,1.3),
             [MAT["blue"],MAT["pink"],MAT["red"],MAT["yellow"]][i], coll, root, 0.12)
        cube("Arcade_Screen_Frame", (x,2.25,2.82), (1.28,1.08,0.14), MAT["gold"], coll, root, 0.04)
        cube("Arcade_Screen", (x,2.25,2.72), (1.12,0.9,0.05), MAT["blue"], coll, root, 0.02)
        cylinder("Arcade_Joystick", (x-0.38,1.2,2.7), 0.1, 0.25, MAT["red"], coll, root)
        sphere("Arcade_Button", (x+0.32,1.25,2.7), (0.12,0.12,0.12), MAT["yellow"], coll, root)
    cube("Pool_Table_Body", (0,0.9,-2.5), (5.3,0.8,2.8), MAT["oak"], coll, root, 0.1)
    cube("Pool_Table_Felt", (0,1.34,-2.5), (4.95,0.08,2.45), MAT["green"], coll, root, 0.04)
    for i in range(5):
        sphere("Pool_Ball_%02d" % i, (-1.4+i*0.65,1.48,-2.5), (0.12,0.12,0.12),
               [MAT["yellow"],MAT["red"],MAT["blue"],MAT["cream"],MAT["pink"]][i], coll, root)
    return coll

def build_cinema():
    coll = new_collection("Cinema")
    root = asset_root("Cinema_AssetKit", coll)
    cube("Cinema_Screen_Frame", (0,3.2,5.2), (9.0,5.0,0.25), MAT["gold"], coll, root, 0.08)
    cube("Cinema_Screen", (0,3.2,5.05), (8.55,4.55,0.06), MAT["blue"], coll, root, 0.025)
    for row in range(3):
        for col in range(4):
            x, z = -3.9+col*2.6, -1.0-row*2.2
            cube("Cinema_Seat_Base", (x,0.85,z), (2.0,0.75,1.55), MAT["red"], coll, root, 0.16)
            cube("Cinema_Seat_Back", (x,1.65,z-0.58), (2.0,1.35,0.38), MAT["red"], coll, root, 0.16)
            for arm in (-1,1):
                cube("Cinema_Armrest", (x+arm*0.95,1.15,z), (0.18,0.78,1.35), MAT["gold"], coll, root, 0.05)
    return coll

def build_gym():
    coll = new_collection("Gym")
    root = asset_root("Gym_AssetKit", coll)
    cube("Treadmill_Deck", (-4.0,0.7,1.2), (3.4,0.32,1.3), MAT["metal"], coll, root, 0.12)
    cube("Treadmill_Belt", (-4.0,0.9,1.2), (3.1,0.05,1.05), MAT["blue"], coll, root, 0.04)
    for x in (-5.3,-2.7):
        rod_between("Treadmill_Handrail", (x,0.9,1.8), (x,2.3,2.4), 0.06, MAT["metal"], coll, root)
    cube("Weight_Rack", (4.3,1.0,2.8), (2.4,2.0,0.3), MAT["metal"], coll, root, 0.04)
    for i in range(5):
        cylinder("Weight_Plate_%02d" % i, (3.4+i*0.45,1.55,2.8), 0.28, 0.18, MAT["blue"], coll, root)
    sphere("Exercise_Ball", (3.5,0.9,-2.8), (0.9,0.9,0.9), MAT["pink"], coll, root)
    cube("Yoga_Mat", (-2.0,0.08,-3.0), (3.0,0.12,1.3), MAT["blue"], coll, root, 0.06)
    return coll

def build_music_studio():
    coll = new_collection("MusicStudio")
    root = asset_root("MusicStudio_AssetKit", coll)
    cube("Grand_Piano_Body", (-3.4,0.95,2.2), (3.8,1.55,1.5), MAT["oak"], coll, root, 0.12)
    cube("Piano_Keyboard", (-3.4,1.78,1.35), (2.9,0.08,0.3), MAT["cream"], coll, root, 0.025)
    for i in range(6):
        cube("Piano_Black_Key_%02d" % i, (-4.65+i*0.48,1.85,1.25), (0.18,0.07,0.16), MAT["metal"], coll, root, 0.01)
    cylinder("Bass_Drum", (2.0,1.0,2.0), 0.82, 0.65, MAT["red"], coll, root)
    for i in range(3):
        cylinder("Tom_Drum_%02d" % i, (0.9+i*1.1,1.8,2.0), 0.42, 0.48, MAT["metal"], coll, root)
    for i in range(2):
        rod_between("Cymbal_Stand", (3.7+i*0.9,0.0,1.7), (3.7+i*0.9,2.2,1.7), 0.035, MAT["gold"], coll, root)
        sphere("Cymbal", (3.7+i*0.9,2.2,1.7), (0.72,0.06,0.72), MAT["gold"], coll, root)
    for x in (-5.4,5.4):
        cube("Amplifier", (x,1.2,-4.0), (1.6,2.4,1.0), MAT["metal"], coll, root, 0.06)
        cylinder("Amplifier_Speaker", (x,1.2,-4.55), 0.52, 0.12, MAT["blue"], coll, root)
    return coll

def build_laboratory():
    coll = new_collection("Laboratory")
    root = asset_root("Laboratory_AssetKit", coll)
    for x in (-4.2,0,4.2):
        cube("Lab_Bench", (x,1.05,3.4), (3.5,0.18,1.5), MAT["metal"], coll, root, 0.05)
        for i in range(3):
            cylinder("Lab_Flask_%d_%d" % (int(x),i), (x-0.8+i*0.8,1.75,3.4),
                     0.24+0.07*(i%2), 0.9, [MAT["blue"],MAT["green"],MAT["pink"]][i], coll, root, 32, 0.025)
    cube("Lab_Cabinet", (-5.2,1.8,-3.5), (2.2,3.6,1.4), MAT["cream"], coll, root, 0.08)
    sphere("Microscope_Base", (3.7,1.25,-3.5), (0.7,0.22,0.5), MAT["metal"], coll, root)
    rod_between("Microscope_Arm", (3.7,1.3,-3.5), (4.2,2.5,-3.5), 0.075, MAT["metal"], coll, root)
    return coll

def build_garage():
    coll = new_collection("Garage")
    root = asset_root("Garage_AssetKit", coll)
    cube("Car_Lower_Body", (0,0.85,0), (6.2,1.0,3.0), MAT["red"], coll, root, 0.42)
    cube("Car_Cabin", (0,1.85,0.35), (3.8,1.2,2.6), MAT["blue"], coll, root, 0.38)
    for x in (-2.0,2.0):
        for z in (-1.55,1.55):
            cylinder("Car_Tire", (x,0.62,z), 0.58, 0.32, MAT["metal"], coll, root, 48, 0.06)
            cylinder("Car_Hub", (x,0.62,z*1.04), 0.24, 0.34, MAT["gold"], coll, root, 32, 0.03)
    cube("Garage_Workbench", (-5.0,1.1,4.0), (3.0,2.2,1.3), MAT["oak"], coll, root, 0.08)
    cube("Garage_ToolChest", (5.0,1.3,4.0), (2.0,2.6,1.3), MAT["red"], coll, root, 0.07)
    return coll

def build_kids_room():
    coll = new_collection("KidsRoom")
    root = asset_root("KidsRoom_AssetKit", coll)
    for y in (1.0,3.0):
        cube("Bunk_Bed_Mattress", (-3.5,y,0.5), (3.3,0.35,4.8), MAT["blue"] if y < 2 else MAT["pink"], coll, root, 0.15)
        cube("Bunk_Bed_Rail", (-3.5,y+0.55,2.8), (3.4,0.3,0.18), MAT["oak"], coll, root, 0.06)
    for x in (-5.0,-2.0):
        rod_between("Bunk_Bed_Post", (x,0.0,2.4), (x,4.0,2.4), 0.09, MAT["oak"], coll, root)
    cube("Toy_Chest", (4.2,0.65,-2.8), (2.6,1.3,1.8), MAT["yellow"], coll, root, 0.14)
    sphere("Teddy_Bear_Body", (3.5,0.85,2.8), (0.72,0.85,0.55), MAT["oak"], coll, root)
    sphere("Teddy_Bear_Head", (3.5,1.72,2.8), (0.58,0.58,0.5), MAT["oak"], coll, root)
    for x in (3.0,4.0):
        sphere("Teddy_Ear", (x,2.15,2.8), (0.22,0.26,0.18), MAT["oak"], coll, root)
    return coll

def build_laundry():
    coll = new_collection("LaundryRoom")
    root = asset_root("LaundryRoom_AssetKit", coll)
    for x in (-4.2,0,4.2):
        cube("Washer_Dryer_Body", (x,1.35,3.0), (2.7,2.7,1.8), MAT["cream"], coll, root, 0.13)
        cylinder("Washer_Door_Rim", (x,1.35,2.05), 0.78, 0.18, MAT["metal"], coll, root, 48, 0.04)
        cylinder("Washer_Door_Glass", (x,1.35,1.94), 0.62, 0.06, MAT["blue"], coll, root, 48, 0.025)
    for x in (-4.2,0,4.2):
        cylinder("Laundry_Basket", (x,0.55,-3.6), 0.58, 1.1, MAT["blue"], coll, root, 32, 0.04)
    cube("Folding_Table", (0,1.05,0), (5.0,0.18,1.8), MAT["oak"], coll, root, 0.06)
    return coll

def build_workshop():
    coll = new_collection("Workshop")
    root = asset_root("Workshop_AssetKit", coll)
    cube("Workshop_Bench_Top", (0,1.2,4.0), (8.0,0.22,1.5), MAT["oak"], coll, root, 0.06)
    cube("Workshop_Pegboard", (0,3.0,4.8), (8.0,3.0,0.16), MAT["gold"], coll, root, 0.025)
    for i in range(8):
        x = -3.3+i*0.94
        rod_between("Pegboard_Tool_Handle", (x,2.5,4.65), (x,3.3,4.65), 0.045, MAT["metal"], coll, root)
    cube("Tool_Chest", (-4.7,0.9,-3.5), (2.0,1.8,1.5), MAT["red"], coll, root, 0.07)
    cube("Timber_Stack", (3.8,0.7,-3.5), (3.0,1.4,1.7), MAT["oak"], coll, root, 0.05)
    return coll

def build_attic():
    coll = new_collection("Attic")
    root = asset_root("Attic_AssetKit", coll)
    for x in (-4.2,0,4.2):
        cube("Attic_Storage_Trunk", (x,0.8,1.2), (2.6,1.6,1.8), MAT["oak"], coll, root, 0.12)
        cube("Attic_Trunk_Lid", (x,1.7,1.2), (2.65,0.18,1.85), MAT["gold"], coll, root, 0.08)
    cube("Covered_Armchair", (-3.4,1.15,-3.2), (2.0,2.3,2.0), MAT["cream"], coll, root, 0.38)
    sphere("Old_Globe", (4.7,1.3,-3.4), (0.85,0.85,0.85), MAT["blue"], coll, root)
    rod_between("Attic_Rafter_Left", (-7,0,5.5), (0,5.5,5.5), 0.12, MAT["oak"], coll, root)
    rod_between("Attic_Rafter_Right", (0,5.5,5.5), (7,0,5.5), 0.12, MAT["oak"], coll, root)
    return coll

def build_basement():
    coll = new_collection("Basement")
    root = asset_root("Basement_AssetKit", coll)
    for x in (-5.4,-1.8,1.8,5.4):
        rod_between("Basement_Utility_Pipe", (x,0,4.2), (x,4.5,4.2), 0.12, MAT["metal"], coll, root)
        for y in (1.0,3.2):
            cylinder("Pipe_Coupler", (x,y,4.2), 0.18, 0.28, MAT["gold"], coll, root)
    cylinder("Basement_Water_Tank", (4.5,2.0,-3.6), 1.0, 4.0, MAT["metal"], coll, root)
    cube("Electrical_Control_Cabinet", (-4.5,1.8,-3.7), (2.2,3.6,1.2), MAT["teal"], coll, root, 0.08)
    cube("Basement_Utility_Shelf", (0,1.4,-4.5), (4.8,0.18,1.2), MAT["oak"], coll, root, 0.05)
    return coll

def build_rooftop():
    coll = new_collection("Rooftop")
    root = asset_root("Rooftop_AssetKit", coll)
    for x in (-6.0,6.0):
        for z in (-4.5,4.5):
            cube("Rooftop_Pergola_Post", (x,2.3,z), (0.22,4.6,0.22), MAT["oak"], coll, root, 0.04)
    for i in range(9):
        cube("Rooftop_Pergola_Slat", (-5.2+i*1.3,4.65,0), (0.16,0.18,9.5), MAT["gold"], coll, root, 0.025)
    cube("Rooftop_Outdoor_Sofa", (0,0.65,-3.2), (4.8,1.3,1.5), MAT["cream"], coll, root, 0.2)
    for x in (-6.2,6.2):
        cylinder("Rooftop_Planter", (x,0.45,-3.0), 0.65, 0.9, MAT["ivory"], coll, root)
        sphere("Rooftop_Topiary", (x,1.6,-3.0), (0.75,1.1,0.75), MAT["green"], coll, root)
    cylinder("Rooftop_Telescope_Tripod", (4.9,1.0,3.2), 0.16, 2.0, MAT["gold"], coll, root)
    cylinder("Rooftop_Telescope_Tube", (4.9,2.1,3.2), 0.24, 1.65, MAT["blue"], coll, root)
    return coll



def poly_curve_mesh(name, points, radius, mat, coll, parent=None):
    curve = bpy.data.curves.new(name + "_Curve", "CURVE")
    curve.dimensions = "3D"
    curve.resolution_u = 16
    curve.bevel_depth = radius
    curve.bevel_resolution = 4
    spline = curve.splines.new("POLY")
    spline.points.add(len(points) - 1)
    for index, point in enumerate(points):
        spline.points[index].co = (point[0], point[1], point[2], 1.0)
    obj = bpy.data.objects.new(name, curve)
    coll.objects.link(obj)
    if parent:
        obj.parent = parent
    obj.data.materials.append(mat)
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.convert(target="MESH")
    result = bpy.context.object
    result.name = name
    result.select_set(False)
    return result

def build_grand_hall_architecture():
    coll = new_collection("GrandHallArchitecture")
    root = asset_root("GrandHallArchitecture_AssetKit", coll)

    # Tall pink-stone columns and capital details follow the atrium reference.
    for x in (-7.0, -3.5, 3.5, 7.0):
        for z in (-5.8, 5.8):
            cylinder("Atrium_Column", (x,3.0,z), 0.40, 6.0, MAT["pink"], coll, root, 48, 0.055)
            cylinder("Atrium_Column_Base", (x,0.18,z), 0.58, 0.36, MAT["ivory"], coll, root, 48, 0.045)
            cylinder("Atrium_Column_Capital", (x,5.92,z), 0.64, 0.34, MAT["ivory"], coll, root, 48, 0.045)

    # Rounded semicircular arch bands, converted to mesh for FBX export.
    for z in (-5.8, 5.8):
        for center_x in (-5.25, 0.0, 5.25):
            points = []
            radius = 2.0
            spring = 4.85
            for i in range(33):
                angle = math.pi * i / 32.0
                points.append((center_x + radius * math.cos(angle),
                               spring + radius * math.sin(angle), z))
            poly_curve_mesh("Atrium_Rounded_Arch", points, 0.16, MAT["ivory"], coll, root)

    for x in (-7.0, 7.0):
        for center_z in (-3.8, 0.0, 3.8):
            points = []
            radius = 1.7
            spring = 4.85
            for i in range(33):
                angle = math.pi * i / 32.0
                points.append((x, spring + radius * math.sin(angle),
                               center_z + radius * math.cos(angle)))
            poly_curve_mesh("Side_Gallery_Arch", points, 0.15, MAT["ivory"], coll, root)

    # Continuous mezzanine galleries and elegant railings around the open atrium.
    for x in (-7.2, 7.2):
        cube("Mezzanine_Balcony_Floor", (x,5.55,0), (1.45,0.32,11.2), MAT["marble"], coll, root, 0.08)
        for z in (-5.3, -2.65, 0.0, 2.65, 5.3):
            cube("Balcony_Baluster", (x + (0.68 if x < 0 else -0.68),6.15,z),
                 (0.12,1.15,0.12), MAT["ivory"], coll, root, 0.035)
        cube("Balcony_Top_Rail", (x + (0.68 if x < 0 else -0.68),6.78,0),
             (0.2,0.16,11.5), MAT["gold"], coll, root, 0.06)
    for z in (-5.6,5.6):
        cube("Mezzanine_Balcony_Floor", (0,5.55,z), (13.2,0.32,1.15), MAT["marble"], coll, root, 0.08)
        for x in (-6.2,-3.1,0,3.1,6.2):
            cube("Balcony_Baluster", (x,6.15,z + (0.52 if z < 0 else -0.52)),
                 (0.12,1.15,0.12), MAT["ivory"], coll, root, 0.035)
        cube("Balcony_Top_Rail", (0,6.78,z + (0.52 if z < 0 else -0.52)),
             (13.4,0.16,0.2), MAT["gold"], coll, root, 0.06)

    # Two red-carpet grand staircases with gold nosing and handrails.
    for side in (-1,1):
        for step in range(18):
            x = side * (3.65 + step * 0.22)
            y = 0.17 + step * 0.30
            z = 4.9 - step * 0.52
            cube("Grand_Stair_Red_Tread", (x,y,z), (3.15,0.28,0.68), MAT["red"], coll, root, 0.035)
            cube("Grand_Stair_Gold_Nosing", (x,y+0.15,z+0.29), (3.15,0.055,0.08), MAT["gold"], coll, root, 0.018)
        # Curved visual handrail sweep; a matching inner rail is added too.
        rail_points = [(side*(3.1+i*0.25), 0.8+i*0.3, 5.15-i*0.52) for i in range(18)]
        poly_curve_mesh("Grand_Stair_Handrail", rail_points, 0.075, MAT["gold"], coll, root)
        cube("Grand_Stair_Landing", (side*7.35,5.45,-4.0), (3.0,0.35,2.4), MAT["marble"], coll, root, 0.08)

    # Elliptical glass dome shell and individual curved ribs.
    vertices, faces = [], []
    rings, segments = 12, 64
    base_y, rx, ry, rz = 7.0, 8.2, 4.2, 6.4
    for ring in range(rings + 1):
        theta = (math.pi * 0.5) * ring / rings
        for segment in range(segments):
            phi = math.tau * segment / segments
            vertices.append((rx * math.sin(theta) * math.cos(phi),
                             base_y + ry * math.cos(theta),
                             rz * math.sin(theta) * math.sin(phi)))
    for ring in range(rings):
        for segment in range(segments):
            a = ring * segments + segment
            b = ring * segments + (segment + 1) % segments
            c = (ring + 1) * segments + (segment + 1) % segments
            d = (ring + 1) * segments + segment
            faces.append((a,b,c,d))
    mesh = bpy.data.meshes.new("GrandHall_GlassDome_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.materials.append(MAT["glass"])
    mesh.update()
    dome = bpy.data.objects.new("GrandHall_Glass_Dome", mesh)
    coll.objects.link(dome)
    dome.parent = root
    for polygon in mesh.polygons:
        polygon.use_smooth = True

    for rib in range(16):
        phi = math.tau * rib / 16.0
        points = []
        for step in range(25):
            theta = (math.pi * 0.5) * step / 24.0
            points.append((rx * math.sin(theta) * math.cos(phi) * 1.005,
                           base_y + ry * math.cos(theta) * 1.005,
                           rz * math.sin(theta) * math.sin(phi) * 1.005))
        poly_curve_mesh("Dome_Gold_Rib", points, 0.045, MAT["gold"], coll, root)

    # Central chandelier with eight curved arms and warm bulb housings.
    cylinder("Grand_Chandelier_Central_Spindle", (0,8.0,0), 0.18, 2.1, MAT["gold"], coll, root)
    sphere("Grand_Chandelier_Hub", (0,7.0,0), (0.75,0.45,0.75), MAT["gold"], coll, root)
    for i in range(8):
        angle = math.tau * i / 8.0
        end = (2.0*math.cos(angle), 6.7, 2.0*math.sin(angle))
        poly_curve_mesh("Chandelier_Arm",
                        [(0,7.0,0), (end[0]*0.45,6.85,end[2]*0.45), end],
                        0.075, MAT["gold"], coll, root)
        sphere("Chandelier_Warm_Bulb", (end[0],6.45,end[2]),
               (0.25,0.36,0.25), MAT["yellow"], coll, root)

    # Reference banners and red carpet runner complete the key composition.
    for i, x in enumerate((-6.0,-2.0,2.0,6.0)):
        cube("GrandHall_Colour_Banner_%02d" % i, (x,4.6,6.0),
             (1.15,2.2,0.12), [MAT["blue"],MAT["pink"],MAT["red"],MAT["yellow"]][i], coll, root, 0.045)
    cube("GrandHall_Red_Carpet_Runner", (0,0.045,-2.0), (4.1,0.08,10.0), MAT["red"], coll, root, 0.05)
    return coll

def build_player_mannequin():
    coll = new_collection("PlayerCharacter")
    root = asset_root("PlayerMannequin_AssetKit", coll)
    mannequin = material("PB_Mannequin_Warm_Wood", (0.62,0.36,0.18), 0.05, 0.34)
    # Faceless, neutral human proportions at exactly 1.75m nominal height.
    sphere("Mannequin_Head", (0,1.59,0), (0.19,0.22,0.19), mannequin, coll, root)
    cylinder("Mannequin_Neck", (0,1.34,0), 0.09, 0.18, mannequin, coll, root, 32, 0.025)
    cube("Mannequin_Chest", (0,1.08,0), (0.43,0.48,0.25), mannequin, coll, root, 0.11)
    cube("Mannequin_Pelvis", (0,0.79,0), (0.35,0.25,0.23), mannequin, coll, root, 0.08)
    for side in (-1,1):
        hip = (side*0.12,0.72,0)
        knee = (side*0.14,0.39,0.015)
        ankle = (side*0.14,0.09,0.02)
        rod_between("Mannequin_Thigh", hip, knee, 0.095, mannequin, coll, root)
        sphere("Mannequin_Knee", knee, (0.10,0.10,0.10), mannequin, coll, root)
        rod_between("Mannequin_Shin", knee, ankle, 0.075, mannequin, coll, root)
        cube("Mannequin_Foot", (side*0.14,0.055,0.12), (0.16,0.11,0.32), mannequin, coll, root, 0.055)
        shoulder = (side*0.27,1.21,0)
        elbow = (side*0.36,0.91,0.015)
        wrist = (side*0.39,0.67,0.02)
        sphere("Mannequin_Shoulder", shoulder, (0.12,0.12,0.12), mannequin, coll, root)
        rod_between("Mannequin_UpperArm", shoulder, elbow, 0.075, mannequin, coll, root)
        sphere("Mannequin_Elbow", elbow, (0.08,0.08,0.08), mannequin, coll, root)
        rod_between("Mannequin_Forearm", elbow, wrist, 0.06, mannequin, coll, root)
        sphere("Mannequin_Hand", (side*0.4,0.61,0.02), (0.075,0.11,0.06), mannequin, coll, root)
    # Keep a visible root and forward marker so the rigging artist has a stable origin.
    cube("Mannequin_Footprint_Origin", (0,0.012,-0.2), (0.34,0.02,0.18), MAT["gold"], coll, root, 0.01)
    return coll

def build_colour_blaster():
    coll = new_collection("ColourBlaster")
    root = asset_root("ColourBlaster_AssetKit", coll)
    cube("Blaster_Main_Housing", (0,1.25,0), (0.82,0.5,0.36), MAT["yellow"], coll, root, 0.16)
    barrel = cylinder("Blaster_Cyan_Barrel", (0.54,1.34,0), 0.17, 0.78, MAT["blue"], coll, root, 48, 0.04)
    barrel.rotation_euler[1] = math.pi/2
    nozzle = cylinder("Blaster_Pink_Nozzle", (0.96,1.34,0), 0.19, 0.16, MAT["pinkpaint"], coll, root, 48, 0.04)
    nozzle.rotation_euler[1] = math.pi/2
    cube("Blaster_Grip", (0.05,0.87,-0.02), (0.25,0.55,0.26), MAT["pinkpaint"], coll, root, 0.08)
    for side, colour in ((-1,MAT["blue"]),(1,MAT["pinkpaint"])):
        cylinder("Blaster_Paint_Tank", (side*0.27,1.68,0), 0.18, 0.62, colour, coll, root, 40, 0.035)
        cylinder("Blaster_Tank_Cap", (side*0.27,2.0,0), 0.21, 0.09, MAT["gold"], coll, root, 40, 0.02)
    sphere("Blaster_Trigger", (0.1,1.0,-0.18), (0.12,0.10,0.07), MAT["gold"], coll, root)
    cube("Blaster_Paint_Splash_Emblem", (0.0,1.25,-0.19), (0.35,0.24,0.035), MAT["pinkpaint"], coll, root, 0.06)
    return coll


def main():
    clear_scene()
    build_living_room()
    build_kitchen()
    build_royal_palace()
    build_gallery_and_storage()
    build_office()
    build_hall_and_garden()
    build_grand_hall_architecture()
    build_player_mannequin()
    build_colour_blaster()
    build_dining_room()
    build_bedroom()
    build_bathroom()
    build_library()
    build_classroom()
    build_arcade()
    build_cinema()
    build_gym()
    build_music_studio()
    build_laboratory()
    build_garage()
    build_kids_room()
    build_laundry()
    build_workshop()
    build_attic()
    build_basement()
    build_rooftop()

    for coll in list(bpy.data.collections):
        if coll.name.startswith("PB_") and coll.objects:
            export_collection(coll, os.path.join(FBX_OUT, coll.name + "_v001.fbx"))

    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "PaintBlend_ReferenceAssetKit.blend"))
    print("Paint & Blend asset kit generated in:", OUT)

if __name__ == "__main__":
    main()
