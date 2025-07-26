import bpy

from .panels import EXPORTMANAGER_PT_panel
from .operators import EXPORTMANAGER_OT_export
from .props import ExportManagerSettings

classes = [
    EXPORTMANAGER_PT_panel,
    EXPORTMANAGER_OT_export,
    ExportManagerSettings
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)

    # Create export manager settings
    bpy.types.Scene.export_manager = bpy.props.PointerProperty(type=ExportManagerSettings)

def unregister():
    # Delete export manager settings
    del bpy.types.Scene.export_manager

    for cls in classes:
        bpy.utils.unregister_class(cls)