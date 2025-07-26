import os
import sys
#import bpy

ASPEN_TOOLS_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.dirname(os.path.dirname(__file__)))))

PYTHON_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'source')

def register():
    if PYTHON_PATH not in sys.path:
        sys.path.append(PYTHON_PATH)

    from aspen.blender.core import startup
    startup.register()

def unregister():
    from aspen.blender.core import startup
    startup.unregister()

if __name__ == "__main__":
    register()
