import os

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
class ExportManagerMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        self.show()

        # Load UI
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'main_window.ui'),
            self
        )