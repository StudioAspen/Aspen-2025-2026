import os

import bpy

from aspen.core.telemetry import trace
_tracer = trace.get_blender_tracer()

@trace.trace_blender_function()
def save_textures():
    """Save textures in blend file."""
    for image in bpy.data.images:
        # If a FILE, just save
        if image.source == 'FILE':
            image.save()

        # If generated in blender, save it to blend file directly as a PNG
        elif image.source == 'GENERATED':
            image.filepath_raw = f'{os.path.dirname(bpy.data.filepath)}/{image.name}.png'
            image.file_format = 'PNG'
            image.save()

@trace.trace_blender_function()
def export_model_fbx(file_path: str):
    """Export selection as FBX at the specified file path.

    Args:
        file_path (str): The file path to export to.
    """
    # Save all textures in the scene otherwise they won't be embedded into FBX
    save_textures()

    # Export model as FBX
    bpy.ops.export_scene.fbx(
        filepath=file_path,
        use_custom_props=True,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',
        use_space_transform=False,
        use_selection=True,
        path_mode='COPY',
        embed_textures=True,
        axis_forward='Y',
        axis_up='Z'
    )

@trace.trace_blender_function()
def export_animation_fbx(file_path: str):
    """Export selection as FBX at the specified file path.

    Args:
        file_path (str): The file path to export to.
    """

    # Export animation as FBX
    bpy.ops.export_scene.fbx(
        filepath=file_path,
        use_custom_props=True,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',
        use_space_transform=False,
        use_selection=True,
        path_mode='COPY',
        embed_textures=False,
        axis_forward='Y',
        axis_up='Z'
    )