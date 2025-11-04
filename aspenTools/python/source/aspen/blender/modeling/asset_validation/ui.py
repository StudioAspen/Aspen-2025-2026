import os
import bpy

from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
from . import api
from aspen.blender.common.results_log import ui


class AssetValidationHelpWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'help_window.ui'),
            self
        )

        self.move(0, 0)

def test_all_objects_in_collection(context):
    bad_collection = False
    for obj in bpy.data.objects:
        # scene.collection is the default collection. therefore, if obj belongs to this, it isn't in a user collection.
        if context.scene.collection in obj.users_collection:
            bad_collection = True
            ui.print_log(f"{obj.name} found in default collection!", "warning")

    if bad_collection:
        ui.print_log("All meshes/objects should be placed in a collection.", "info")
    else:
        ui.print_log("All meshes were found in collections, collection test passed.", "info")

def test_all_objects_vertex_count(context):
    # Code is from depsgraph example in python documentation.
    # To see original code + explanation, see https://docs.blender.org/api/current/bpy.types.Depsgraph.html
    # TO-DO THIS DOESN'T WORK YET I JUST TYPED IT
    for obj in bpy.data.objects:
        if obj is None or obj.type != 'MESH':
            continue

        depsgraph = context.evaluated_depsgraph_get()
        object_eval = obj.evaluated_get(depsgraph)
        mesh_eval = object_eval.data
        vertex_count = len(mesh_eval.vertices)
        if vertex_count > 750:
            ui.print_log(f"{obj.name} has more than 750 evaluated vertices.", "warning")
        elif vertex_count > 500:
            ui.print_log(f"Warning: {obj.name} vertex count has more than 500 evaluated vertices; This is fine but consider reducing.", "warning")
        

