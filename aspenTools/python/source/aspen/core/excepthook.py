import sys
from aspen.core.telemetry.loggers import get_blender_logger

def blender_excepthook(exc_type, exc_value, exc_traceback):
    """Custom hook for catching excetions in blender.

    Logs exceptions for telemetry.
    """
    if issubclass(exc_type, KeyboardInterrupt):
        sys.__excepthook__(exc_type, exc_value, exc_traceback)
        return

    # Log to file
    get_blender_logger().error(f'{exc_type.__name__}: {exc_value}', exc_info=(exc_type, exc_value, exc_traceback))

    # Call original
    sys.__excepthook__(exc_type, exc_value, exc_traceback)