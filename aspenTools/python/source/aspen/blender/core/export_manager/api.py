import bpy

def export_model_fbx(file_path: str):
    """Export selection as FBX at the specified file path.

    Args:
        file_path (str): The file path to export to.
    """
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