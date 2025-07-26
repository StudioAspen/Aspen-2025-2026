import bpy

from . import config

class ExportManagerSettings(bpy.types.PropertyGroup):
    export_name: bpy.props.StringProperty(name='Export Name')
    export_type: bpy.props.EnumProperty(
        name='Export Type',
        items=config.EXPORT_TYPE_ENUM_ITEMS
    )
    asset_type: bpy.props.EnumProperty(
        name='Asset Type',
        items=config.ASSET_TYPE_ENUM_ITEMS
    )