import os

import bpy

import aspen.blender.common.export_manager.api as api
import aspen.core.site as site
import aspen.blender.core.flags as flags

EXPORT_OP_BL_IDNAME = 'export_manager.export'
ADD_EXPORT_TARGET_OP_BL_IDNAME = 'export_manager.add_export_target'
EXPORT_PANEL_BL_IDNAME = 'EXPORTMANAGER_PT_panel'

EXPORT_TYPE_ENUM_MODEL = 'MODEL'
EXPORT_TYPE_ENUM_ANIMATION = 'ANIMATION'
EXPORT_TYPE_ENUM_RIG = 'RIG'
EXPORT_TYPE_ENUM_ITEMS = [
    (EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_MODEL.title(), ''),
    (EXPORT_TYPE_ENUM_ANIMATION, EXPORT_TYPE_ENUM_ANIMATION.title(), ''),
    (EXPORT_TYPE_ENUM_RIG, EXPORT_TYPE_ENUM_RIG.title(), '')
]

ASSET_TYPE_ENUM_ITEMS = [
    ('CHARACTER', 'Character', ''),
    ('PROP', 'Prop', 'Rigged actors'),
    ('ACTOR', 'Actor', '')
]

class EXPORTMANAGER_PT_panel(bpy.types.Panel):
    """Panel used for export manager tool in Blender"""
    bl_label = "Export Manager"
    bl_idname = EXPORT_PANEL_BL_IDNAME
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "Export Manager"

    def draw(self, context):
        layout = self.layout

        export_manager = context.scene.export_manager

        layout.prop(export_manager, "export_name")
        layout.prop(export_manager, "export_type")
        layout.prop(export_manager, 'asset_type')

        layout.operator(EXPORT_OP_BL_IDNAME, icon='EXPORT')


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
            export_dir = os.path.join(site.UNITY_ASSETS_ROOT, 'Art', 'models', asset_type, export_name)
        elif export_type == EXPORT_TYPE_ENUM_ANIMATION:
            blend_dir = os.path.basename(os.path.dirname(bpy.data.filepath))
            export_dir = os.path.join(site.UNITY_ASSETS_ROOT, 'Art', 'animations', asset_type, blend_dir)
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


class ExportManagerSettings(bpy.types.PropertyGroup):
    export_name: bpy.props.StringProperty(name='Export Name')
    export_type: bpy.props.EnumProperty(
        name='Export Type',
        items=EXPORT_TYPE_ENUM_ITEMS
    )
    asset_type: bpy.props.EnumProperty(
        name='Asset Type',
        items=ASSET_TYPE_ENUM_ITEMS
    )

classes = [
    EXPORTMANAGER_PT_panel, EXPORTMANAGER_OT_export, ExportManagerSettings
]

def register():
    """Register tool"""
    for cls in classes:
        bpy.utils.register_class(cls)

    bpy.types.Scene.export_manager = bpy.props.PointerProperty(type=ExportManagerSettings)

def unregister():
    """Unregister tool"""
    del bpy.types.Scene.export_manager

    for cls in classes:
        bpy.utils.unregister_class(cls)