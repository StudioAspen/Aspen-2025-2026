import os
import sys
import subprocess

import bpy

ASPEN_TOOLS_ROOT = os.path.dirname(os.path.dirname(
    os.path.dirname(os.path.dirname(os.path.dirname(__file__)))))
PYTHON_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'source')
VENV_PATH = os.path.join(ASPEN_TOOLS_ROOT, 'python', 'aspenVenv', 'Lib', 'site-packages')
BLENDER_PATH = os.path.join(PYTHON_PATH, 'aspen', 'blender')

def register():
    subprocess.Popen(os.path.join(ASPEN_TOOLS_ROOT, 'python', 'uv', 'venv.bat'))

    if PYTHON_PATH not in sys.path:
        sys.path.append(PYTHON_PATH)
        sys.path.append(VENV_PATH)

    from aspen.core.telemetry import loggers

    _logger = loggers.get_blender_logger()

    user_registration_success = prompt_register_user()

    telemetry_success = register_telemetry()

    if user_registration_success and telemetry_success:
        # Get tracer
        from aspen.core.telemetry.trace import get_blender_tracer
        _tracer = get_blender_tracer()

        # Trace Blender Init
        with _tracer.start_as_current_span('blender-init') as span:
            # Set user for trace
            from aspen.core.users import api as users
            span.set_attribute('user', users.get_users()[users.get_machine_id()][users.USERNAME_KEY])

            # Trace BQT init
            with _tracer.start_as_current_span('bqt-init'):
                import bqt
                bqt.register()

            # Trace blender auto load
            with _tracer.start_as_current_span('blender-autoload-init'):
                from aspen import blender_autoload as autoload
                autoload.init()
                autoload.register()
    else:

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


def prompt_register_user() -> bool:
    """Prompt the user to register a new user. If cancelled, exit the application.

    Returns:
        bool: Returns true if user registration was successful.
    """
    # Prompt user to initialize user if not registered
    from aspen.core.users import api as users
    from PySide6 import QtWidgets

    try:
        machine_id = users.get_machine_id()
        if users.get_machine_id() in users.get_users():
            return True
        else:
            # Register user if not registered yet
            app = QtWidgets.QApplication(sys.argv)
            username, success = QtWidgets.QInputDialog.getText(None, 'Set Username', 'FirstnameLastinitial (Ex: MikyleM)')
            if success:
                users.add_user(machine_id)
                users.set_username(machine_id, username)

                return True
            else:
                app.quit()
                sys.exit(0)
    except Exception as e:
        return False


def register_telemetry() -> bool:
    """ Try to register telemetry

    Returns:
        bool: Returns true of registration of telemetry was successful.
    """

    try:
        # Register telemetry and except hook
        from aspen.core.telemetry import init as telemetry
        from aspen.core.excepthook import blender_excepthook

        telemetry.initialize('blender')
        sys.excepthook = blender_excepthook

        return True

    except Exception as e:
        # If failed to register telemetry, return false
        _logger.error(f'Failed to register telemetry- {e}')

        return False