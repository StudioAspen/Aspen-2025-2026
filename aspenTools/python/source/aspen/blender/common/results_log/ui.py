import os
import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
from aspen.blender.core import flags

from .api import ConsoleArea


class ResultLogMainWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'results_log_window.ui'),
            self,
            ConsoleArea
        )

        # Example usage
        self.consoleList.add_log("Welcome to the Aspen result log! Find the result of any tool operations in here.", "info")