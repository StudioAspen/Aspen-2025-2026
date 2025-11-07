import os
import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
import logging
from .api import is_default_name

class AssetValidationHelpWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'help_window.ui'),
            self
        )

        self.move(0, 0)

# Manually retrieve logger reference in each function or should each module should hold a global logger reference?
logger = logging.getLogger("aspen")

def test_objects_in_collection(context):
    """ This tests whether all objects in the scene have been placed into custom collections.
    Args:
        context (bpy.context): The Blender context.
    """
    bad_collection = False
    for obj in bpy.data.objects:
        if context.scene.collection in obj.users_collection: # scene.collection is the default collection.
            bad_collection = True
            logger.warning(f"{obj.name} found in default collection!")

    if bad_collection:
        logger.warning("All meshes/objects should be placed in a collection.")
    else:
        logger.info("All meshes were found in collections, collection test passed.")

def test_asset_vertex_count(context):
    """ This tests if the asset meets target vertex counts.
    Args:
        context (bpy.context): The Blender context.
    """
    # Code is from depsgraph example in python documentation. See https://docs.blender.org/api/current/bpy.types.Depsgraph.html
    vertex_count = 0
    for obj in bpy.data.objects:
        if obj is None or obj.type != 'MESH':
            continue

        depsgraph = context.evaluated_depsgraph_get()
        object_eval = obj.evaluated_get(depsgraph)
        mesh_eval = object_eval.data
        vertex_count += len(mesh_eval.vertices)

    if vertex_count > 750:
        logger.error(f"Vertex count of {vertex_count} exceeds 750.")
    elif vertex_count > 500:
        logger.warning(f"Vertex count of {vertex_count} is acceptable but exceeds 500, reduce if possible.")
    elif vertex_count == 0:
        logger.warning("Hey I think there's nothing in the scene :P")
    else:
        logger.info(f"Vertex test passed with {vertex_count} vertices.")

def test_normals_facing_outwards(context):
    # TODO This seems a bit more complex. Unfortunately, though Blender can highlight inward/outward normals
    # TODO for you, it seems to be a graphical pass and they don't expose that information to the API.
    # Possible: Dot (face - obj.center, normal) or some graph algorithm?
    logger.debug("Hi nothing happened.")


def test_object_default_names():
    """ This checks all objects to see if they have the default Blender names. """
    default_named_objects = []

    for obj in bpy.data.objects:
        if obj.type == 'MESH' and is_default_name(obj.name):
                default_named_objects.append(obj.name)

    if len(default_named_objects) > 0:
        list_str = ""
        for name in default_named_objects:
            list_str += name + "\n"
        logger.warning("Objects with default names found, rename please:\n" + list_str)
    else:
        logger.info("Object name test passed.")

