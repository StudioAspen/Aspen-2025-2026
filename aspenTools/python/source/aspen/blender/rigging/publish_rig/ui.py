import os

import bpy

# from aspen.blender.rigging.publish_rig.api import publish_rig
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

import aspen.sitecustomize as sitecustomize

from aspen.blender.rigging.publish_rig import api
# from . import (ASSET_TYPE_ENUM_ITEMS, EXPORT_TYPE_ENUM_ITEMS,
#                EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_RIG, EXPORT_TYPE_ENUM_ANIMATION)

from aspen.core.telemetry.loggers import get_blender_logger
from aspen.core.telemetry import trace as aspen_trace
from opentelemetry import trace
_logger = get_blender_logger()

class RigPublishMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        #for loading UI
        self.publish_rig_button = None
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'publishRig.ui'),
            self
        )

        # Get the publish rig tool
        publish_rig = api.publish_rig = f'{os.path.dirname(bpy.data.filepath)}'


        # Set up the Publish button
        self.publish_rig_button.clicked.connect(self._TEST_on_button_clicked)

    def _TEST_on_button_clicked(self):
        api.publish_rig(); #HOPEFULLY it saves the blend file


# )
