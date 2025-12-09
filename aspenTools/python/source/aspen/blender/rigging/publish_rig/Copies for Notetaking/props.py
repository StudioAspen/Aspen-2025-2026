
import bpy

from . import EXPORT_TYPE_ENUM_ITEMS, ASSET_TYPE_ENUM_ITEMS


class ExportManagerSettings(bpy.types.PropertyGroup):
    asset_name: bpy.props.StringProperty(name='Export Name')
    export_type: bpy.props.EnumProperty(
        name='Export Type',
        items=EXPORT_TYPE_ENUM_ITEMS
    )
    asset_type: bpy.props.EnumProperty(
        name='Asset Type',
        items=ASSET_TYPE_ENUM_ITEMS
    )


def register():
    bpy.types.Scene.export_manager = bpy.props.PointerProperty(type=ExportManagerSettings)

def unregister():
    del bpy.types.Scene.export_manager