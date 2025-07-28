import os

import bpy

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
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_ALL',
        use_space_transform=False,
        use_selection=True,
        path_mode='COPY',
        embed_textures=True,
        axis_forward='Y',
        axis_up='Z'
    )

def export_animation_fbx(file_path: str):
    pass