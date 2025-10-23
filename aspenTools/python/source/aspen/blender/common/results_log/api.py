# api.py
import os

from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
from aspen.blender.core import flags

MAX_LOGS = 100
class ConsoleArea(QListWidget):
    color_map = {
        "info" : "#3a82f7",
        "finished" : "#4caf50",
        "cancelled" : "#ffb300",
        "error" : "#e53935"
    }

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setSpacing(2)

    def add_log(self, text, flag="info"):

        color = self.color_map.get(flag)

        log = LogEntry(text, color)

        list_item = QListWidgetItem(self)
        list_item.setSizeHint(log.sizeHint())
        self.setItemWidget(list_item, log)

        if self.count() > MAX_LOGS:
            self.takeItem(0)

        self.scrollToBottom()


class LogEntry(QWidget):
    def __init__(self, text, color):
        super().__init__()
        layout = QHBoxLayout(self)

        # Colored indicator box
        box = QFrame()
        box.setFixedSize(10, 10)
        box.setStyleSheet(f"background-color: {color}; border-radius: 2px;")

        # Message text
        label = QLabel(text)
        label.setWordWrap(True)

        layout.addWidget(box)
        layout.addWidget(label)


class ResultLogMainWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'results_log_window.ui'),
            self,
            ConsoleArea
        )
        self.move(0, 0)
        self.consoleList.add_log("Welcome to the Aspen result log! Find the result of any tool operations in here.", "info")

    def test_print(self):
        self.consoleList.add_log("Welcome! Any output from Aspen tools will be displayed here.", "info")
        self.consoleList.add_log("Run successful.", "finished")
        self.consoleList.add_log("Cancelled operation.", "cancelled")
        self.consoleList.add_log("Error from a tool has occurred.", "error")

    def test_mass_print(self):
        for i in range(105):
            self.consoleList.add_log(f"Log {i}")