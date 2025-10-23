import bpy
from .api import ResultLogMainWindow

g_ResultLogMainWindow = ResultLogMainWindow()

def show_result_log_window():
    g_ResultLogMainWindow.show()

def test_result_log_window():
    g_ResultLogMainWindow.test_print()