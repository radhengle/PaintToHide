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

def main():
    clear_scene()
    build_living_room()
    build_kitchen()
    build_royal_palace()
    build_gallery_and_storage()
    build_office()
    build_hall_and_garden()

    for coll in list(bpy.data.collections):
        if coll.name.startswith("PB_") and coll.objects:
            export_collection(coll, os.path.join(FBX_OUT, coll.name + "_v001.fbx"))

    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "PaintBlend_ReferenceAssetKit.blend"))
    print("Paint & Blend asset kit generated in:", OUT)

if __name__ == "__main__":
    main()
