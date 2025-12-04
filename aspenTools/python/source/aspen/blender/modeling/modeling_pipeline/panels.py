
import bpy
from . import ASSET_VALIDATION_BL_IDNAME, HELP_ASSET_VALIDATION_BL_IDNAME

class ASPENMODELING_PT_panel(bpy.types.Panel):
    bl_label = 'Aspen Modeling'
    bl_idname = 'ASPENMODELING_PT_panel'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Aspen'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        header, body = self.layout.panel("ASSET_VALIDATION_PANEL", default_closed=False)
        header.label(text="Asset Validation")

        if not body:
            return
        asset_validation_row = body.row(align=True)
        asset_validation_split = asset_validation_row.split(factor=0.90)
        asset_validation_split.operator(ASSET_VALIDATION_BL_IDNAME)
        asset_validation_split.operator(HELP_ASSET_VALIDATION_BL_IDNAME, text="", icon='QUESTION')

        label_row = body.row(align=True)
        label_row.label(text="Shortcuts")
        shortcut_row = body.row(align=True)
        shortcut_row.prop(context.space_data.overlay, "show_face_orientation", text="Face Orientation")

