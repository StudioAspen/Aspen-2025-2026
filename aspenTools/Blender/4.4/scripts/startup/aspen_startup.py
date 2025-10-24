import os
import subprocess

import sys
import bpy

ASPEN_TOOLS_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.dirname(os.path.dirname(__file__)))))
PYTHON_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'source')
VENV_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'aspenVenv', 'Lib', 'site-packages')
BLENDER_PATH = os.path.join(PYTHON_PATH, 'aspen', 'blender')

def register():

    if PYTHON_PATH not in sys.path:
        sys.path.append(PYTHON_PATH)
        sys.path.append(VENV_PATH)

    """
    from aspen.core.telemetry import init as telemetry
    from aspen.core.excepthook import blender_excepthook

    telemetry.initialize('blender')
    sys.excepthook = blender_excepthook

    from aspen.core.telemetry.trace import get_blender_tracer
    _tracer = get_blender_tracer()
    
    # Trace Blender Init
    with _tracer.start_as_current_span('blender-init'):
        # Trace BQT init
        with _tracer.start_as_current_span('bqt-init'):
            import bqt
            bqt.register()

        # Trace blender auto load
        with _tracer.start_as_current_span('blender-autoload-init'):
            from aspen import blender_autoload as autoload
            autoload.init()
            autoload.register()
    """

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