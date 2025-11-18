import os
import logging

from PySide6.QtWidgets import QListWidget, QListWidgetItem, QWidget, QLabel, QHBoxLayout, QFrame
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

MAX_LOGS = 100

LOG_LEVEL_COLOR_DICT = {
    logging.INFO: "#64bd72",  # Green
    logging.DEBUG: "#e0e0e0",  # Light-Grey
    logging.WARNING: "#ffb300",  # Orange
    logging.ERROR: "#e53935"  # Red
}

class QHLine(QFrame):
    # I guess there's no Qt class for a horizontal line, so you can define one using QFrame.
    def __init__(self, parent=None):
        super(QHLine, self).__init__(parent)
        self.setFrameShape(QFrame.Shape.HLine)
        self.setFrameShadow(QFrame.Shadow.Sunken)

class LogEntry(QWidget):
    def __init__(self, text: str, color: str):
        super().__init__()

        layout = QHBoxLayout(self)

        log_level_box = QFrame()
        log_level_box.setFixedSize(10, 10)
        log_level_box.setStyleSheet(f"background-color: {color}; border-radius: 2px;")

        log_text_label = QLabel(text)
        log_text_label.setWordWrap(True)

        layout.addWidget(log_level_box)
        layout.addWidget(log_text_label)

class ConsoleArea(QListWidget):

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setSpacing(2)

    def add_widget(self, widget):
        """ This """
        list_item = QListWidgetItem(self)
        list_item.setSizeHint(widget.sizeHint())
        self.setItemWidget(list_item, widget)

        if self.count() > MAX_LOGS:
            self.takeItem(0)

        self.scrollToBottom()

    def add_log(self, text: str, log_level: int = logging.DEBUG):
        """ This adds a single LogEntry widget to this object. Will delete old entries if max_logs is reached.

        Args:
            text (str): Text to display
            log_level (int): Determines the color of a box displayed with the message.
        """
        color = LOG_LEVEL_COLOR_DICT.get(log_level)
        log = LogEntry(text, color)
        self.add_widget(log)

    def add_hline(self):
        hline = QHLine(self)
        self.add_widget(hline)

class ResultLogMainWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'results_log_window.ui'),
            self,
            ConsoleArea
        )

        # consoleList is the name of the ConsoleArea widget defined in the .ui file, not defined here so will have warning.
        self.move(0, 0)
        self.consoleList.add_log("Welcome to the Aspen result log! Results of any tool operations will be printed here.", logging.DEBUG)

class ResultLogHandler(logging.Handler):
    def __init__(self, window: ResultLogMainWindow, level: int = logging.NOTSET):
        logging.Handler.__init__(self, level)
        self.window = window

    def emit(self, record: logging.LogRecord):
        """ Overrides base class to call ResultLogMainWindow's print_log()
        Args:
            record (logging.LogRecord): Holds information about a given log.
        """
        msg = self.format(record)
        self.window.consoleList.add_log(record.msg, record.levelno)

# Declared it here so that we could have a persistent window in a Blender session.
g_ResultLogMainWindow = ResultLogMainWindow()

aspenLogger = logging.getLogger("aspen")

handler = ResultLogHandler(g_ResultLogMainWindow)
handler.setLevel(logging.INFO)
handler.set_name("ResultLogHandler")

aspenLogger.addHandler(handler)

def show_result_log_window():
    """ This makes the window show itself. """
    g_ResultLogMainWindow.show()

def print_hline():
    """ This makes the window show itself. """
    g_ResultLogMainWindow.consoleList.add_hline()

def _test_print_log_levels(self):
    """ This function tests calling each of the log_levels to ensure the colors display correctly. """

    g_ResultLogMainWindow.consoleList.add_log("Welcome! Any output from Aspen tools will be displayed here.", logging.DEBUG)
    g_ResultLogMainWindow.consoleList.add_log("Run successful.", logging.INFO)
    g_ResultLogMainWindow.consoleList.add_log("Cancelled operation.", logging.WARNING)
    g_ResultLogMainWindow.consoleList.add_log("Error from a tool has occurred.", logging.ERROR)

def _test_mass_print(self):
    """ This functions tests whether the Window will handle deleting old logs when capacity is reached. """
    for i in range(105):
        g_ResultLogMainWindow.consoleList.add_log(f"Log {i}")

