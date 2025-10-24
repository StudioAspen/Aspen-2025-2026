import bpy

from aspen.blender.common.results_log import ui as result_log_ui
from aspen.core.telemetry import trace
from ..asset_validation.ui import AssetValidationHelpWindow
from aspen.blender.core import flags

class MODELINGPIPELINE_OT_asset_validation_start(bpy.types.Operator):
    bl_idname = 'modeling_pipeline.asset_validation_start'
    bl_label = 'Start Asset Validation'
    bl_description = 'Check Aspen Requirements for the current Asset.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        result_log_ui.print_log("Yo asset suck", "error")
        result_log_ui.show_result_log_window()
        return {"FINISHED"}

class MODELINGPIPELINE_OT_asset_validation_help(bpy.types.Operator):
    bl_idname = 'modeling_pipeline.asset_validation_help'
    bl_label = 'Open Asset Validation Help'
    bl_description = 'Open a window for help with the Asset Validation tool. Contains the requirements and who to contact for issues.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        AssetValidationHelpWindow().show()
        return {"FINISHED"}
