import trace #search up what trace does and describe here.
import os
import bpy

from aspen.core.telemetry import trace




@trace.trace_blender_function()
def publish_rig(file_path: str):
    """Publish rig as a .blend file to the specified file path."""

    # save_textures() # will need.
    bpy.ops.wm.save_as_mainfile(filepath=file_path) # saves file as a .blend file.
        # use_custom_props=True, # Custom properties will be applied to the current save



    )
