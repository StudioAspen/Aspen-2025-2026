import os
from distutils.command.build_scripts import first_line_re

import bpy

import aspen
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

import aspen.sitecustomize as sitecustomize

from aspen.blender.rigging.publish_rig import api
from . import (ASSET_TYPE_ENUM_ITEMS, EXPORT_TYPE_ENUM_ITEMS,
               EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_RIG, EXPORT_TYPE_ENUM_ANIMATION)

from aspen.core.telemetry.loggers import get_blender_logger
from aspen.core.telemetry import trace as aspen_trace
from opentelemetry import trace
_logger = get_blender_logger()
TestPath = aspen.blender.rigging.publish_rig

ASSET_TYPE = ['Characters', 'Actors']


class PublishRigMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        #for loading UI
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'publishRig.ui'),
            self
        )

        # Get the publish rig tool

        # Set up asset types
        self.asset_types = [asset_type[0] for asset_type in ASSET_TYPE_ENUM_ITEMS]
        self.asset_type_combo_box.addItems(['Characters', 'Actors'])
        self.asset_type_combo_box.currentIndexChanged.connect(self._asset_type_combo_box_changed) # links the combo box to the uhhhh

        # Set up the Publish button
        self.publish_selection_button.clicked.connect(self._on_publish_selection_button_clicked)

    def _on_publish_selection_button_clicked(self):
        api.publish_rig(); # HOPEFULLY it saves the blend file



    def _asset_type_combo_box_changed(self, i: int):
        print(ASSET_TYPE[i])