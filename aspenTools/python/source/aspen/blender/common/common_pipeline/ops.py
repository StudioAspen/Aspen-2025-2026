
import bpy
from aspen.blender.common.export_manager.ui import ExportManagerMainWindow
from aspen.blender.core import flags

from . import EXPORT_MANAGER_IDNAME

class COMMONPIPELINE_OT_exportmanager(bpy.types.Operator):
    """An operator used to export directly into the unity project."""
    bl_idname = EXPORT_MANAGER_IDNAME
    bl_label = 'Export Manager'
    bl_description = 'A tool that helps export assets directly into the Unity Project'
    bl_options = {'REGISTER'}

    def execute(self, context):
        """Launch the Export Manager"""

        ExportManagerMainWindow().show()

        return flags.FINISHED_REPORT_FLAG
