import bpy
from . import PUBLISH_RIG_BL_IDNAME

class ASPENRIGGING_PT_panel(bpy.types.Panel):
    bl_label = 'Aspen Rigging'
    bl_idname = 'ASPENRIGGING_PT_panel'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Aspen'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):

        """Draws the Rigging panel."""
        layout = self.layout

        layout.operator(PUBLISH_RIG_BL_IDNAME, icon='EXPORT')