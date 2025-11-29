import bpy

from aspen.blender.core import flags
from aspen.core.telemetry import trace

from . import ASSET_VALIDATION_BL_IDNAME, HELP_ASSET_VALIDATION_BL_IDNAME
from ..asset_validation.ui import AssetValidationHelpWindow, AssetValidationSelectionWindow

class MODELINGPIPELINE_OT_asset_validation(bpy.types.Operator):
    bl_idname = ASSET_VALIDATION_BL_IDNAME
    bl_label = 'Open Asset Validation'
    bl_description = 'Select Asset Type before beginning tests.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context: bpy.types.Context):
        """ Display window to select type for Asset Validation """
        AssetValidationSelectionWindow().show()
        return flags.FINISHED_REPORT_FLAG

class MODELINGPIPELINE_OT_asset_validation_help(bpy.types.Operator):
    bl_idname = HELP_ASSET_VALIDATION_BL_IDNAME
    bl_label = 'Open Asset Validation Help'
    bl_description = 'Open a window for help with the Asset Validation tool. Contains the requirements and who to contact for issues.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context: bpy.types.Context):
        """" Show the help window """
        AssetValidationHelpWindow().show()
        return flags.FINISHED_REPORT_FLAG
