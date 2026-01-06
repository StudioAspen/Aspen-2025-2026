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

# from .api import publish_rig

_logger = get_blender_logger()

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

    def _on_asset_name_line_edit_changed(self, text: str):
        bpy.context.scene.publish_rig.asset_name = text


    def _on_asset_type_combo_box_changed(self, index: int):
        bpy.context.scene.publish_rig.publish_type = self.publish_types[index]



    def _on_publish_selection_button_clicked(self):
        # Look for 'VIEW_3D' area to temp override

                # Check if the current .blend file is saved
                if not bpy.data.filepath:
                    raise Exception('File must be saved in order to export a model.')

                # Get publish rig tool
                publish_rig = bpy.context.scene.publish_rig


                # Get export settings
                asset_name = publish_rig.asset_name.strip() # !! probably will be an issue soon. Update: yeah
                # publish_type = publish_rig
                publish_type = publish_rig.publish_type
                asset_type = f'{publish_rig.asset_type.lower()}s'

                # Set the export directory based on export and asset type
                if publish_type == PUBLISH_TYPE_ENUM_CHARACTER:
                    # publish_dir = os.path.join(sitecustomize.TECH_ART_BLENDER_ASSETS_DIR, asset_type,
                    #                           asset_name) # "Export to a folder of the rig's name"
                    publish_dir = os.path.join(sitecustomize.TECH_ART_BLENDER_ASSETS_DIR, 'characters', '[a].blend'
                                              ) # "Export to a folder of the rig's name"
                elif publish_type == PUBLISH_TYPE_ENUM_ACTOR:
                    blend_dir = os.path.basename(os.path.dirname(bpy.data.filepath)) # gets the .blend file's current file path, apparently.
                    publish_dir = os.path.join(sitecustomize.ART_ASSETS_DIR, 'Art', 'animations',
                                              asset_type, blend_dir)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {publish_type}')

                publish_path = os.path.join(publish_dir)
                # publish_path = os.path.join(publish_dir, f'{asset_name}.fbx')

                # Publish at publish path
                if publish_type == PUBLISH_TYPE_ENUM_CHARACTER:
                    api.publish_character(publish_path)
                elif publish_type == PUBLISH_TYPE_ENUM_ACTOR:
                    api.publish_character(publish_path)
                else:
                    # Cancel if unknown export type
                    raise Exception(f'Unknown export type: {publish_type}')

                # api.publish_rig() # HOPEFULLY it saves the blend file



    def _asset_type_combo_box_changed(self, i: int):
        print(ASSET_TYPE[i]) # for testing
        print(bpy.context)
        print(ASSET_TYPE_ENUM_ITEMS[i]) # works.