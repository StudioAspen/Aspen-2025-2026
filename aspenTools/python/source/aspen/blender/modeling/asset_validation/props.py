
import bpy

from aspen.blender.common.export_manager import ASSET_TYPE_ENUM_ITEMS

def register():
    bpy.types.Scene.asset_validation_type = bpy.props.EnumProperty(items=ASSET_TYPE_ENUM_ITEMS, name = 'Asset Type')

def unregister():
    del bpy.types.Scene.asset_validatation_type