import os

import bpy

from . import config
import aspen.blender.core.export_manager.api as api
import aspen.core.site as site

class EXPORTMANAGER_OT_export(bpy.types.Operator):
    """An operator used to export selection directly into the unity project."""
    bl_idname = config.EXPORT_OP_BL_IDNAME
    bl_label = 'Export Selection'
    bl_description = 'Exports the selected objects directly into the Unity Project'
    bl_options = {'REGISTER', 'UNDO'}

    def execute(self, context):
        """Try to export selection with the specified settings."""
        # Get export settings
        export_manager = context.scene.export_manager
        export_name = export_manager.export_name
        export_type = export_manager.export_type
        asset_type = f'{export_manager.asset_type.lower()}s'

        # Check if valid export name
        if not export_name:
            self.report({'ERROR'}, 'No export name specified.')
            return {'CANCELLED'}

        # Set the export directory based on export and asset type
        export_dir = ''
        if export_type == 'MODEL':
            export_dir = os.path.join(site.UNITY_ASSETS_ROOT, 'Art', 'models', asset_type, export_name)
        elif export_type == 'ANIMATION':
            export_dir = os.path.join(site.UNITY_ASSETS_ROOT, 'Art', 'animations', asset_type, export_name)
        else:
            self.report({'ERROR'}, f'Unknown export type: {export_type}')

        os.makedirs(export_dir, exist_ok=True)
        export_path = os.path.join(export_dir, f'{export_name}.fbx')

        # Export at export path
        api.export_fbx(export_path)
        self.report({'INFO'}, f'Export Success: {export_path}')

        return {'FINISHED'}