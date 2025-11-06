# api.py
import os

from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
import logging

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
        """ This adds a single LogEntry widget to this object. Will delete old entries if max_logs is reached.

        Args:
            text (str): Text to display
            flag (str, optional): Determines the color of a box displayed with the message.
        """
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

        # This is a box that matches the flag passed in to a color.
        box = QFrame()
        box.setFixedSize(10, 10)
        box.setStyleSheet(f"background-color: {color}; border-radius: 2px;")

        # This is the text to be displayed.
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
        self.consoleList.add_log("Welcome to the Aspen result log! Results of any tool operations will be printed here.", "info")

    def print_log(self, text, flag="info"):
        """ This adds and prints a single log to the window.

        Args:
            text (str): Text to display
            flag (str, optional): Determines the color of a box displayed with the message.
        """
        self.consoleList.add_log(text, flag)

    def test_print_flags(self):
        """ This function tests calling each of the flags to ensure the colors display correctly. """

        self.consoleList.add_log("Welcome! Any output from Aspen tools will be displayed here.", "info")
        self.consoleList.add_log("Run successful.", "finished")
        self.consoleList.add_log("Cancelled operation.", "cancelled")
        self.consoleList.add_log("Error from a tool has occurred.", "error")

    def test_mass_print(self):
        """ This functions tests whether the Window will handle deleting old logs when capacity is reached. """
        for i in range(105):
            self.consoleList.add_log(f"Log {i}")

# Derived class of logging module's handler so that we can override emit() and connect it to our functions.
class ResultLogHandler(logging.Handler):
    def __init__(self, window: ResultLogMainWindow, level=logging.NOTSET):
        logging.Handler.__init__(self, level)
        self.window = window

    #TODO Match the log level in the LogRecord to a flag.
    def emit(self, record):
        msg = self.format(record)
        self.window.print_log(msg)


