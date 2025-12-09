import os

import bpy

from aspen.core.telemetry import trace
_tracer = trace.get_blender_tracer()

@trace.trace_blender_function()
# Here, Mikyle is creating a function that saves textures in the user's .blend file. - Eman
# FIRST, the function purges unused data (dunno exactly what this means, ask Mikyle.)...
# ...two conditions are set. If the texture being used was imported by the user, simply save the image as whatever format it is...
# ...If the texture being used was generated in blender, save it as a PNG in the blend file
# After being defined, the function is later used in the "export_model_fbx" function.


def save_textures():
    """Save textures in blend file."""

    # Purge unused data
    bpy.ops.outliner.orphans_purge(do_local_ids=True, do_linked_ids=True, do_recursive=True)

    # Save images
    for image in bpy.data.images:
        # If a FILE, just save
        if image.source == 'FILE':
            image.save()

        # If generated in blender, save it to blend file directly as a PNG
        elif image.source == 'GENERATED':
            image.filepath_raw = f'{os.path.dirname(bpy.data.filepath)}/{image.name}.png' #os.p.dirname(bpy.d.fpath) is the blend file's location.
                                                                                          #image.name refers to the name the user already set by the user.
                                                                                          #...plus .png added as the extension.
            image.file_format = 'PNG'
            image.save() // # #

@trace.trace_blender_function()
# This function exports the user's model to the user's specified file path. This file path is specified in...
# object properties -> custom properties tab -> + new.

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
        use_custom_props=True, # this applies the file path set by the user
        apply_unit_scale=True, # smth to do with the rig's size
        apply_scale_options='FBX_SCALE_ALL', # applies the size the user set?
        use_space_transform=False, # leaves rig (and mesh's) transforms as is.
        use_selection=True, # the function only messes with the selected rig.
        path_mode='COPY', # copies the file to the specified location.
        embed_textures=True, # attaches the textures to rig.
        axis_forward='Y', # probably determines what way the model faces in Unity.
        axis_up='Z' #...same thing here, but with what way is up.
    )

@trace.trace_blender_function()
#



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

# So how is this all linked to the UI?