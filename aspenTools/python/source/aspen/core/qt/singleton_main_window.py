# Property of Guardian's Lament
# Author: Mikyle Mosquera
# 2025Q3

import os
import sys

from PySide6 import QtWidgets, QtGui

from aspen import sitecustomize

__all__ = ['SingletonMainWindow']

class SingletonMainWindow(QtWidgets.QMainWindow):
    _instance = None  # Singleton instance

    def __new__(cls):

        # Close if window is already opened
        if cls._instance is not None:
            cls._instance.close()
            cls._instance.deleteLater()
            cls._instance = None

        # Create and return new instance
        cls._instance = super().__new__(cls)
        return cls._instance

    def __init__(self, parent=None):
        app = QtWidgets.QApplication.instance()
        if parent is None:
            parent = QtWidgets.QApplication.instance().blender_widget
        super().__init__(parent=parent)

        if getattr(self, "_initialized", False):
            return

        self._initialized = True
        self.setWindowIcon(QtGui.QIcon(os.path.join(sitecustomize.PYTHON_IMAGES_DIR, 'core', 'egd_logo.png')))

