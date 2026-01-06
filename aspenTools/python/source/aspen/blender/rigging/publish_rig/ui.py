import os
from distutils.command.build_scripts import first_line_re

import bpy

import aspen
from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

import aspen.sitecustomize as sitecustomize # This is controlling directories and is allowing Mikyle's tool to access the Unity folders. It's a gate woahh

from aspen.blender.rigging.publish_rig import api
from . import (ASSET_TYPE_ENUM_ITEMS, PUBLISH_TYPE_ENUM_ITEMS,
               PUBLISH_TYPE_ENUM_CHARACTER, PUBLISH_TYPE_ENUM_ACTOR)

from aspen.core.telemetry.loggers import get_blender_logger
from aspen.core.telemetry import trace as aspen_trace
from opentelemetry import trace

from .api import publish_rig

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

        # Set up asset types
        self.asset_types = [asset_type[0] for asset_type in ASSET_TYPE_ENUM_ITEMS]
        self.asset_type_combo_box.addItems(['Characters', 'Actors'])
        self.asset_type_combo_box.currentIndexChanged.connect(self._asset_type_combo_box_changed) # links the combo box to the uhhhh

        # Set up the Publish button
        self.publish_selection_button.clicked.connect(self._on_publish_selection_button_clicked)

        # Get publish settings
        # publish_type = publish_rig.export_type



    def _on_publish_selection_button_clicked(self):
        # Look for 'VIEW_3D' area to temp override

                # Check if the current .blend file is saved
                if not bpy.data.filepath:
                    raise Exception('File must be saved in order to export a model.')

                # Get export settings
                asset_name = publish_rig.asset_name.strip
                publish_type = publish_rig.export_type
                asset_type = f'{publish_rig.asset_type.lower()}s'

                # Set the export directory based on export and asset type
                if publish_type == PUBLISH_TYPE_ENUM_CHARACTER:
                    publish_dir = os.path.join(sitecustomize.BLENDER_PROJECT_ASSETS_DIR, 'Art', 'models', asset_type,
                                              asset_name) # "Export to a folder of the rig's name"
                elif publish_type == PUBLISH_TYPE_ENUM_ACTOR:
                    blend_dir = os.path.basename(os.path.dirname(bpy.data.filepath)) # gets the .blend file's current file path, apparently.
                    publish_dir = os.path.join(sitecustomize.BLENDER_PROJECT_ASSETS_DIR, 'Art', 'animations',
                                              asset_type, blend_dir)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {publish_type}')

                publish_path = os.path.join(publish_dir, f'{asset_name}.fbx')

                # Publish at publish path
                if publish_type == PUBLISH_TYPE_ENUM_CHARACTER:
                    api.export_model_fbx(publish_path)
                elif publish_type == PUBLISH_TYPE_ENUM_ACTOR:
                    api.export_animation_fbx(publish_path)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {publish_type}')

                # api.publish_rig() # HOPEFULLY it saves the blend file



    def _asset_type_combo_box_changed(self, i: int):
        print(ASSET_TYPE[i])