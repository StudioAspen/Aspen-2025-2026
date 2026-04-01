import re
import logging
import bpy
from aspen.blender.common.export_manager import ASSET_TYPE_ENUM_ITEMS

# Regex list of default mesh name patterns Blender uses. Maybe add more?
default_mesh_name_patterns = [
    r"^Cube(\.\d+)?$",
    r"^Circle(\.\d+)?$",
    r"^Plane(\.\d+)?$",
    r"^Sphere(\.\d+)?$",
    r"^Icosphere(\.\d+)?$",
    r"^Cone(\.\d+)?$",
    r"^Cylinder(\.\d+)?$",
    r"^Torus(\.\d+)?$",
    r"^Suzanne(\.\d+)?$",
]

asset_type_vertex_requirements = { # [0] is acceptable number but prefer to reduce, [1] is vertex maximum.
    'CHARACTER': (2300, 5000),
    'ACTOR': (500, 750),
    'PROP': (500, 750),
}

def is_not_valid_object_for_test(obj: bpy.types.Object):
    """ This function helps check if the object is valid for testing.
    I'm only going to test visible meshes.
    Args:
        obj (bpy.types.Object): Object to test.
        """
    return obj is None or obj.type != 'MESH' or not obj.visible_get()

def is_default_blender_mesh_name(name: str):
    """Check if a string matches a Blender default object pattern.
    Args:
        name (str): String to check.
    """
    return any(re.match(pattern, name) for pattern in default_mesh_name_patterns)


logger = logging.getLogger("aspen")

def check_objects_in_collection(context: bpy.types.Context)-> bool:
    """ This tests whether all objects in the scene have been placed into custom collections.
    Args:
        context (bpy.context): The Blender context.
    """
    bad_collection = False
    for obj in bpy.data.objects:
        if is_not_valid_object_for_test(obj):
            continue

        if context.scene.collection in obj.users_collection: # scene.collection is the default collection.
            bad_collection = True
            logger.warning(f"{obj.name} found in default collection!")

    if bad_collection:
        logger.warning("All meshes/objects should be placed in a collection.")
    else:
        logger.info("All meshes were found in collections, collection test passed.")

    return not bad_collection

def check_asset_vertex_count(context: bpy.types.Context):
    """ This tests if the asset meets target vertex counts.
    Args:
        context (bpy.context): The Blender context.
    """
    # Code is from depsgraph example in python documentation. See https://docs.blender.org/api/current/bpy.types.Depsgraph.html
    vertex_count = 0
    for obj in bpy.data.objects:
        if is_not_valid_object_for_test(obj):
            continue

        depsgraph = context.evaluated_depsgraph_get()
        object_eval = obj.evaluated_get(depsgraph)
        mesh_eval = object_eval.data
        vertex_count += len(mesh_eval.vertices)

    vert_warn = asset_type_vertex_requirements[bpy.context.scene.asset_validation_type][0]
    vert_max = asset_type_vertex_requirements[bpy.context.scene.asset_validation_type][1]

    if vertex_count > vert_max:
        logger.error(f"Vertex count of {vertex_count} exceeds {vert_max} maximum for {bpy.context.scene.asset_validation_type}.")
    elif vertex_count > vert_warn:
        logger.warning(f"Vertex count of {vertex_count} is acceptable but exceeds {vert_warn} for {bpy.context.scene.asset_validation_type}, reduce if possible.")
    elif vertex_count == 0:
        logger.warning("Hey I think there's nothing in the scene :P")
    else:
        logger.info(f"Vertex test passed with {vertex_count} vertices for {bpy.context.scene.asset_validation_type} asset type.")
        return True
    return False

def check_object_default_names():
    """ This checks all objects to see if they have the default Blender names. """
    default_named_objects = []

    for obj in bpy.data.objects:
        if is_not_valid_object_for_test(obj):
            continue

        if obj.type == 'MESH' and is_default_blender_mesh_name(obj.name):
                default_named_objects.append(obj.name)

    if len(default_named_objects) > 0:
        list_str = ""
        for name in default_named_objects:
            list_str += name + "\n"
        logger.warning("Objects with default names found, rename please:\n" + list_str)
        return False
    else:
        logger.info("Object name test passed.")
        return True


