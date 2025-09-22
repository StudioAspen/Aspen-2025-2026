import os

import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

from . import EXPORT_TYPE_ENUM_ITEMS, ASSET_TYPE_ENUM_ITEMS


class ExportManagerMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        # Load UI
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'main_window.ui'),
            self
        )

        # Get export manager
        export_manager = bpy.context.scene.export_manager

        # Set up asset name
        self.asset_name_line_edit.setText(export_manager.asset_name)
        self.asset_name_line_edit.textChanged.connect(self._on_asset_name_line_edit_changed)

        # Set up export type
        self.export_types = [export_type[0] for export_type in EXPORT_TYPE_ENUM_ITEMS]
        self.export_type_combo_box.addItems(self.export_types)
        self.export_type_combo_box.setCurrentIndex(self.export_types.index(bpy.context.scene.export_manager.export_type))
        self.export_type_combo_box.currentIndexChanged.connect(self._on_export_type_combo_box_changed)

        # Set up asset type
        self.asset_types = [asset_type[0] for asset_type in ASSET_TYPE_ENUM_ITEMS]
        self.asset_type_combo_box.addItems(self.asset_types)
        self.asset_type_combo_box.setCurrentIndex(self.asset_types.index(bpy.context.scene.export_manager.asset_type))
        self.asset_type_combo_box.currentIndexChanged.connect(self._on_asset_type_combo_box_changed)

    def _on_asset_name_line_edit_changed(self, text: str):
        """ Set the export manager's asset name if the line edit is changed.

        Args:
            text (str): The line edit text.
        """
        bpy.context.scene.export_manager.asset_name = text

    def _on_export_type_combo_box_changed(self, index: int):
        """ Set the export manager's export type if the combo box is changed.

        Args:
            index (int): The index of the combo box
        """
        bpy.context.scene.export_manager.export_type = self.export_types[index]

    def _on_asset_type_combo_box_changed(self, index: int):
        """ Set the export manager's asset type if the combo box is changed.

        Args:
            index (int): The index of the combo box
        """
        bpy.context.scene.export_manager.asset_type = self.asset_types[index]