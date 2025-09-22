import os

import bpy

from  aspen.blender.common.export_manager import api
from aspen import sitecustomize
from aspen.blender.core import flags

from . import (EXPORT_OP_BL_IDNAME, EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_ANIMATION, EXPORT_TYPE_ENUM_RIG,)

class EXPORTMANAGER_OT_export(bpy.types.Operator):
    """An operator used to export selection directly into the unity project."""
    bl_idname = EXPORT_OP_BL_IDNAME
    bl_label = 'Export Selection'
    bl_description = 'Exports the selected objects directly into the Unity Project'
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        """Try to export selection with the specified settings."""
        # Check if the current .blend file is saved
        if not bpy.data.filepath:
            self.report(flags.ERROR_REPORT_FLAG, "File must be saved in order to export a model.")

        # Check if valid export name
        if not context.scene.export_manager.export_name:
            self.report(flags.ERROR_REPORT_FLAG, 'No export name specified.')
            return flags.CANCELLED_REPORT_FLAG

        # Get export settings
        export_manager = context.scene.export_manager
        export_name = export_manager.export_name
        export_type = export_manager.export_type
        asset_type = f'{export_manager.asset_type.lower()}s'

        # Set the export directory based on export and asset type
        export_dir = ''
        if export_type == EXPORT_TYPE_ENUM_MODEL or export_type == EXPORT_TYPE_ENUM_RIG:
            export_dir = os.path.join(sitecustomize.UNITY_PROJECT_ASSETS_DIR, 'Art', 'models', asset_type, export_name)
        elif export_type == EXPORT_TYPE_ENUM_ANIMATION:
            blend_dir = os.path.basename(os.path.dirname(bpy.data.filepath))
            export_dir = os.path.join(sitecustomize.UNITY_PROJECT_ASSETS_DIR, 'Art', 'animations', asset_type, blend_dir)
        else:
            # Cancel if unknown export type
            self.report(flags.ERROR_REPORT_FLAG, f'Unknown export type: {export_type}')
            return flags.CANCELLED_REPORT_FLAG

        os.makedirs(export_dir, exist_ok=True)
        export_path = os.path.join(export_dir, f'{export_name}.fbx')

        # Export at export path
        if export_type == EXPORT_TYPE_ENUM_MODEL or export_type == EXPORT_TYPE_ENUM_RIG:
            api.export_model_fbx(export_path)
        elif export_type == EXPORT_TYPE_ENUM_ANIMATION:
            api.export_animation_fbx(export_path)
        else:
            # Cancel if unknown export type
            self.report(flags.ERROR_REPORT_FLAG, f'Unknown export type: {export_type}')
            return flags.CANCELLED_REPORT_FLAG

        self.report(flags.INFO_REPORT_FLAG, f'Export Success: {export_path}')

        return flags.FINISHED_REPORT_FLAG