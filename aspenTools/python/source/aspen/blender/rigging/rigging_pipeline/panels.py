import bpy

class ASPENRIGGING_PT_panel(bpy.types.Panel):
    bl_label = 'Aspen Rigging'
    bl_idname = 'ASPENRIGGING_PT_panel'
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = 'Aspen'
    bl_options = {'DEFAULT_CLOSED'}

    def draw(self, context):
        layout = self.layout