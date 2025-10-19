import os

import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

import aspen.sitecustomize as sitecustomize

from aspen.blender.common.export_manager import api
from . import (ASSET_TYPE_ENUM_ITEMS, EXPORT_TYPE_ENUM_ITEMS,
               EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_RIG, EXPORT_TYPE_ENUM_ANIMATION)

from aspen.core.telemetry.loggers import get_blender_logger
from aspen.core.telemetry import trace as aspen_trace
from opentelemetry import trace
_logger = get_blender_logger()

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

        # Set up export selection button
        self.export_selection_button.clicked.connect(self._on_export_selection_button_clicked)

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

    @aspen_trace.trace_blender_function()
    def _on_export_selection_button_clicked(self):
        """ Call the export operator.

        In order to properly export, context uses selected_objects, which is a part of the area 'VIEW_3D'.
        Since this is called from an external window, the context must be temporarily overridden.
        """

        # Look for 'VIEW_3D' area to temp override
        found_window = None
        found_area = None
        for window in bpy.context.window_manager.windows:
            for area in window.screen.areas:
                if area.type == 'VIEW_3D':
                    found_window = window
                    found_area = area
                    break

        if found_area:
            with bpy.context.temp_override(window=found_window, area=found_area):
                # Check if the current .blend file is saved
                if not bpy.data.filepath:
                    raise Exception('File must be saved in order to export a model.')

                context = bpy.context
                # Check if valid export name
                if not context.scene.export_manager.asset_name:
                    _logger.warning('Asset name is not valid.')

                # Get export settings
                export_manager = context.scene.export_manager
                asset_name = export_manager.asset_name.strip()
                export_type = export_manager.export_type
                asset_type = f'{export_manager.asset_type.lower()}s'

                # Set the export directory based on export and asset type
                if export_type == EXPORT_TYPE_ENUM_MODEL or export_type == EXPORT_TYPE_ENUM_RIG:
                    export_dir = os.path.join(sitecustomize.UNITY_PROJECT_ASSETS_DIR, 'Art', 'models', asset_type,
                                              asset_name)
                elif export_type == EXPORT_TYPE_ENUM_ANIMATION:
                    blend_dir = os.path.basename(os.path.dirname(bpy.data.filepath))
                    export_dir = os.path.join(sitecustomize.UNITY_PROJECT_ASSETS_DIR, 'Art', 'animations',
                                              asset_type, blend_dir)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {export_type}')

                os.makedirs(export_dir, exist_ok=True)
                export_path = os.path.join(export_dir, f'{asset_name}.fbx')

                # Export at export path
                if export_type == EXPORT_TYPE_ENUM_MODEL or export_type == EXPORT_TYPE_ENUM_RIG:
                    api.export_model_fbx(export_path)
                elif export_type == EXPORT_TYPE_ENUM_ANIMATION:
                    api.export_animation_fbx(export_path)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {export_type}')

                _logger.info(f'Export Success: {export_path}')
                trace.get_current_span().set_attribute("export_path", str(export_path))