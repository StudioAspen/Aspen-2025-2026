import os

from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
import logging

MAX_LOGS = 100

class ConsoleArea(QListWidget):
    color_map = {
<<<<<<< HEAD
        "info" : "#3a82f7",
        "finished" : "#4caf50",
        "cancelled" : "#ffb300",
        "error" : "#e53935",
        "warning" : "#ff9900"
=======
        # the enums in logging correspond to integers.
        logging.INFO: "#64bd72",  # Green
        logging.DEBUG : "#e0e0e0", # Light-Grey
        logging.WARNING : "#ffb300", # Orange
        logging.ERROR : "#e53935" # Red
>>>>>>> jonathan_results_log
    }

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setSpacing(2)

    def add_log(self, text, log_level=logging.DEBUG):
        """ This adds a single LogEntry widget to this object. Will delete old entries if max_logs is reached.

        Args:
            text (str): Text to display
            log_level (int): Determines the color of a box displayed with the message.
        """
        color = self.color_map.get(log_level)

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

        # This is a box that matches the log_level passed in to a color.
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
        self.consoleList.add_log("Welcome to the Aspen result log! Results of any tool operations will be printed here.", logging.DEBUG)

    def print_log(self, text, log_level = logging.DEBUG):
        """ This adds and prints a single log to the window.

        Args:
            text (str): Text to display
            log_level (int): Determines the color of a box displayed with the message.
        """
        self.consoleList.add_log(text, log_level)

    def test_print_log_levels(self):
        """ This function tests calling each of the log_levels to ensure the colors display correctly. """

        self.consoleList.add_log("Welcome! Any output from Aspen tools will be displayed here.", logging.DEBUG)
        self.consoleList.add_log("Run successful.", logging.INFO)
        self.consoleList.add_log("Cancelled operation.", logging.WARNING)
        self.consoleList.add_log("Error from a tool has occurred.", logging.ERROR)

    def test_mass_print(self):
        """ This functions tests whether the Window will handle deleting old logs when capacity is reached. """
        for i in range(105):
            self.consoleList.add_log(f"Log {i}")

# Derived class of logging module's handler so that we can override emit() and connect it to our functions.
class ResultLogHandler(logging.Handler):
    def __init__(self, window: ResultLogMainWindow, level=logging.NOTSET):
        logging.Handler.__init__(self, level)
        self.window = window

    def emit(self, record):
        """ Overrides base class to call ResultLogMainWindow's print_log()
        Args:
            record (logging.LogRecord): Holds information about a given log.
        """
        msg = self.format(record)
        self.window.print_log(msg, record.levelno)


