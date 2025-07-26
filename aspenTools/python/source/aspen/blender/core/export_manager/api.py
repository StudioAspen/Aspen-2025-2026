import bpy

def export_fbx(file_path: str):
    """Export selection as FBX at the specified file path.

    Args:
        file_path (str): The file path to export to.
    """
    bpy.ops.export_scene.fbx(
        filepath=file_path,
        apply_unit_scale=True,
        apply_scale_options='FBX_SCALE_UNITS',
        use_space_transform=True,
        use_selection=True,
        path_mode='COPY',
        embed_textures=True
    )