import os
import subprocess

import sys
import bpy

ASPEN_TOOLS_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.dirname(os.path.dirname(__file__)))))
PYTHON_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'source')
VENV_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'venv', 'Lib', 'site-packages')
BLENDER_PATH = os.path.join(PYTHON_PATH, 'aspen', 'blender')

def register():
    subprocess.Popen(os.path.join(ASPEN_TOOLS_ROOT, 'python', 'uv', 'venv.bat'))

    if PYTHON_PATH not in sys.path:
        sys.path.append(PYTHON_PATH)
        sys.path.append(VENV_PATH)

    import bqt
    bqt.register()

    from aspen import blender_autoload as autoload
    autoload.init()
    autoload.register()

def unregister():
    import bqt
    bqt.unregister()

    from aspen import blender_autoload as autoload
    autoload.unregister()

    bpy.ops.preferences.script_directory_remove(directory=BLENDER_PATH)