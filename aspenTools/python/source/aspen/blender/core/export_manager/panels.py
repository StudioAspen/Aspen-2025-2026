import bpy

from . import config

class EXPORTMANAGER_PT_panel(bpy.types.Panel):
    """Panel used for export manager tool in Blender"""
    bl_label = "Export Manager"
    bl_idname = config.EXPORT_PANEL_BL_IDNAME
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Export Manager"

    def draw(self, context):
        layout = self.layout
        layout.label(text="Export Manager")

        export_manager = context.scene.export_manager

        layout.prop(export_manager, "export_name")
        layout.prop(export_manager, "export_type")
        layout.prop(export_manager, 'asset_type')

        layout.operator(config.EXPORT_OP_BL_IDNAME, icon='EXPORT')