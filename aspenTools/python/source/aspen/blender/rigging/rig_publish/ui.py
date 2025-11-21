import os

import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

import aspen.sitecustomize as sitecustomize

from aspen.blender.common.export_manager import api
# from . import (ASSET_TYPE_ENUM_ITEMS, EXPORT_TYPE_ENUM_ITEMS,
               # EXPORT_TYPE_ENUM_MODEL, EXPORT_TYPE_ENUM_RIG, EXPORT_TYPE_ENUM_ANIMATION)

from aspen.core.telemetry.loggers import get_blender_logger
from aspen.core.telemetry import trace as aspen_trace
from opentelemetry import trace
_logger = get_blender_logger()

class RigPublishMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        #for loading UI
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'publishRig.ui'),
            self
        )


# )
