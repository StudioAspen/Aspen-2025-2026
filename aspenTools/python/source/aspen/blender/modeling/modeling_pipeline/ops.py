import bpy

from aspen.blender.common.results_log import ui as result_log_ui
from ..asset_validation import ui
from ..asset_validation.ui import AssetValidationHelpWindow
from aspen.core.telemetry import trace


class MODELINGPIPELINE_OT_asset_validation_start(bpy.types.Operator):
    bl_idname = 'modeling_pipeline.asset_validation_start'
    bl_label = 'Start Asset Validation'
    bl_description = 'Check Aspen Requirements for the current Asset.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        ui.test_objects_in_collection(context)
        ui.test_asset_vertex_count(context)
        ui.test_object_default_names()

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
