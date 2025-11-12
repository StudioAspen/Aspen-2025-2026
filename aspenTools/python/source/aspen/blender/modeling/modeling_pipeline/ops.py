import bpy

from aspen.blender.common.results_log import ui as result_log_ui
from aspen.blender.core import flags
from aspen.core.telemetry import trace
from . import ASSET_VALIDATION_BL_IDNAME, HELP_ASSET_VALIDATION_BL_IDNAME
from ..asset_validation import api
from ..asset_validation import ui


class MODELINGPIPELINE_OT_asset_validation(bpy.types.Operator):
    bl_idname = ASSET_VALIDATION_BL_IDNAME
    bl_label = 'Start Asset Validation'
    bl_description = 'Check Aspen Requirements for the current Asset.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context: bpy.types.Context):
        """ Execute model review tests and display the results in Result Log. """
        api.check_objects_in_collection(context)
        api.check_asset_vertex_count(context)
        api.check_object_default_names()

        result_log_ui.show_result_log_window()
        return flags.FINISHED_REPORT_FLAG

class MODELINGPIPELINE_OT_asset_validation_help(bpy.types.Operator):
    bl_idname = HELP_ASSET_VALIDATION_BL_IDNAME
    bl_label = 'Open Asset Validation Help'
    bl_description = 'Open a window for help with the Asset Validation tool. Contains the requirements and who to contact for issues.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self):
        """" Show the help window """
        ui.AssetValidationHelpWindow().show()
        return flags.FINISHED_REPORT_FLAG
