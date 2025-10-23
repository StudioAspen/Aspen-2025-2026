# api.py
from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.blender.core import flags

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

        widget = LogEntry(text, color)

        item = QListWidgetItem()
        item.setSizeHint(widget.sizeHint())

        self.addItem(item)
        self.setItemWidget(item, widget)
        self.scrollToBottom()


    def display_test_logs(self):
        self.add_log("Welcome! Any output from Aspen tools will be displayed here.", flags.INFO_REPORT_FLAG)
        self.add_log("Run successful.", flags.FINISHED_REPORT_FLAG)
        self.add_log("Cancelled operation.", flags.CANCELLED_REPORT_FLAG)
        self.add_log("Error from a tool has occurred.", flags.ERROR_REPORT_FLAG)

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