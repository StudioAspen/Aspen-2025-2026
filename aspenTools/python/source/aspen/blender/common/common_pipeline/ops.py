
import bpy
from aspen.blender.common.export_manager.ui import ExportManagerMainWindow
from aspen.blender.common.results_log import ui
from aspen.blender.core import flags

from . import EXPORT_MANAGER_BL_IDNAME
from . import RESULT_LOG_BL_IDNAME

from aspen.core.telemetry import trace

import logging


class COMMONPIPELINE_OT_exportmanager(bpy.types.Operator):
    """An operator used to export directly into the unity project."""
    bl_idname = EXPORT_MANAGER_BL_IDNAME
    bl_label = 'Export Manager'
    bl_description = 'A tool that helps export assets directly into the Unity Project'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        """Launch the Export Manager"""

        ExportManagerMainWindow().show()

        return flags.FINISHED_REPORT_FLAG

class COMMONPIPELINE_OT_resultlog(bpy.types.Operator):
    bl_idname = RESULT_LOG_BL_IDNAME
    bl_label = 'Show Result Log'
    bl_description = 'Open the Result Log to display results from Aspen Tools.'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        ui.show_result_log_window()

        return flags.FINISHED_REPORT_FLAG

class COMMONPIPELINE_OT_testprint(bpy.types.Operator):
    bl_idname = 'common_pipeline.testprint'
    bl_label = 'Print a log'
    bl_description = 'Test printing a log using the python logging module'
    bl_options = {'REGISTER'}

    @trace.trace_blender_operator()
    def execute(self, context):
        logger = logging.getLogger("aspen")
        logger.info('Testing op')

        return flags.FINISHED_REPORT_FLAG
