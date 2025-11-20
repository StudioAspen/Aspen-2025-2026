
import bpy
from . import EXPORT_MANAGER_BL_IDNAME


class ASPENCOMMON_PT_panel(bpy.types.Panel):
    bl_label = 'Aspen Common'
    bl_idname = 'ASPENCOMMON_PT_panel'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Aspen'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        """ Draw the Common Pipeline panel"""

        layout = self.layout

        layout.operator(EXPORT_MANAGER_BL_IDNAME, icon='EXPORT')