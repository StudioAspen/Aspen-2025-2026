import bpy
from aspen.blender.rigging.bone_customization_menu.ui import BoneCustomizationMainWindow
from aspen.blender.core import flags

from . import BONE_CUSTOMIZATION_BL_IDNAME

from aspen.core.telemetry import trace

class RIGGINGPIPELINE_OT_bonecustomizationmenu(bpy.types.Operator):
    """Bone Customization Menu."""
    bl_idname = BONE_CUSTOMIZATION_BL_IDNAME
    bl_label = 'Bone Customization Menu'
    bl_description = 'A tool that lets rig artists set bone control shapes and their rotations in one menu.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        """Launch the Bone Customization Menu."""

        BoneCustomizationMainWindow().show()

        return flags.FINISHED_REPORT_FLAG