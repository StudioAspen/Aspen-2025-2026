# Props = Properties
import bpy

from . import PUBLISH_TYPE_ENUM_CHARACTER, ASSET_TYPE_ENUM_ITEMS, PUBLISH_TYPE_ENUM_ITEMS


class PublishRigToolSettings(bpy.types.PropertyGroup):
    asset_name: bpy.props.StringProperty(name='Publish Name')
    publish_type: bpy.props.EnumProperty(
        name='Publish Type',
        items=PUBLISH_TYPE_ENUM_ITEMS
    )
    asset_type: bpy.props.EnumProperty(
        name='Asset Type',
        items=ASSET_TYPE_ENUM_ITEMS
    )


def register():
    bpy.types.Scene.publish_rig = bpy.props.PointerProperty(type=PublishRigToolSettings)
    bpy.utils.register_class(PublishRigToolSettings)


def unregister():
    del bpy.types.Scene.publish_rig # this should delete before unregistering the class
    bpy.utils.unregister_class(PublishRigToolSettings)

