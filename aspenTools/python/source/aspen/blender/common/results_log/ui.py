import bpy
from .api import ResultLogMainWindow

# Declared it here so that we could have a persistent window in a Blender session.
g_ResultLogMainWindow = ResultLogMainWindow()

def show_result_log_window():
    """ This makes the window show itself. """
    g_ResultLogMainWindow.show()

def test_result_log_window():
    """ Test function to see if the flags are working. """
    g_ResultLogMainWindow.test_print_flags()

def print_log(text, flag="info"):
    """ Prints a log to the window. This is the main function any modules will be calling to display results on the window.

    Args:
        text (str): The text to display.
        flag (str, optional): Will determine the color of a box displayed in the log.
    """
    g_ResultLogMainWindow.print_log(text, flag)