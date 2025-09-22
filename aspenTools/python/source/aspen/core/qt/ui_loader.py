# Property of Guardian's Lament
# Author: Mikyle Mosquera
# 2025Q3

from typing import Optional

from PySide6 import QtWidgets, QtGui, QtUiTools, QtCore

class UIFileLoader(QtUiTools.QUiLoader):
    """ A UI Loader that allows for loading .ui files into an existing class."""

    def __init__(self, base_instance: Optional[QtWidgets.QWidget] = None):
        """ Initialize the QUILoader with starting context.

        Args:
            base_instance: An existing widget that will be used for the UI file.
        """

        super().__init__()
        self.base_instance = base_instance

    def createWidget(self, class_name: str, parent: Optional[QtWidgets.QWidget] = None,
                     name: Optional[str] = None) -> QtWidgets.QWidget:
        """ Usually called to create each widget defined in the UI file.

        Overridden behavior:
        - Support reusing existing base_instance
        - Add support for custom widgets
        - Add aliases to all widgets to the base instance
        """
        if class_name.startswith('DESIGNER_ONLY'):
            return None

        # QUILoader usually returns top-level widget, but return base_instance of present
        if parent is None and self.base_instance:
            return self.base_instance

        widget = super().createWidget(class_name, parent, name)

        # Build aliases from base name to all the created pieces by name
        if self.base_instance:
            setattr(self.base_instance, name, widget)

        return widget


def load_ui(ui_file : str, base_instance: Optional[QtWidgets.QWidget] = None):
    """ Create a loader to load a UI file into a existing widget.

    Args:
        ui_file: The path to the .ui file to be loaded.
        base_instance: An existing widget that will be used for the UI file.
    """

    loader = UIFileLoader(base_instance=base_instance)

    widget = loader.load(ui_file)
    QtCore.QMetaObject.connectSlotsByName(widget)

    if isinstance(widget, (QtWidgets.QMainWindow, QtWidgets.QWidget)):
        if widget.parent():
            parent_geometry = widget.parent().geometry()
            widget_geometry = widget.geometry()

            relative_x = (parent_geometry.width() - widget_geometry.width()) // 2
            relative_y = (parent_geometry.height() - widget_geometry.height()) // 2

            absolute_position = widget.parent().mapToGlobal(QtCore.QPoint(relative_x, relative_y))
            widget.move(absolute_position)

    return widget