
import bpy
from aspen.blender.rigging.publish_rig.ui import PublishRigMainWindow
from aspen.blender.core import flags

from . import PUBLISH_RIG_BL_IDNAME

from aspen.core.telemetry import trace


class RIGGINGPIPELINE_OT_publishrig(bpy.types.Operator):
    """An operator used to export directly into the unity project."""
    bl_idname = PUBLISH_RIG_BL_IDNAME
    bl_label = 'Publish Rig Tool'
    bl_description = 'A tool that blows up a part of Alaska.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        """Launch the Publish Rig Tool."""

        PublishRigMainWindow().show()

        return flags.FINISHED_REPORT_FLAG
