import os
import bpy

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader
from aspen.blender.common.export_manager import ASSET_TYPE_ENUM_ITEMS
from aspen.blender.common.results_log.ui import ResultLogMainWindow
from . import api

class AssetValidationHelpWindow(SingletonMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent=parent)

        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'help_window.ui'),
            self
        )

        self.move(0, 0)


class AssetValidationSelectionWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        # Load UI
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'selection_window.ui'),
            self
        )

        # Set up asset type
        self.asset_types = [asset_type[0] for asset_type in ASSET_TYPE_ENUM_ITEMS]
        self.asset_type_combo_box.addItems(self.asset_types)
        self.asset_type_combo_box.setCurrentIndex(self.asset_types.index(bpy.context.scene.asset_validation_type))
        self.asset_type_combo_box.currentIndexChanged.connect(self._on_asset_type_combo_box_changed)

        # Set up export selection button
        self.start_validation_button.clicked.connect(self._on_start_validation_button_clicked)


    def _on_asset_type_combo_box_changed(self, index: int):
        """ Set the asset validation asset type when combo box changes.
        Shamelessly 'inspired' by Mikyle's code.

        Args:
            index (int): The index of the combo box
        """
        bpy.context.scene.asset_validation_type = self.asset_types[index]

    def _on_start_validation_button_clicked(self):
        """ Begins the asset validation process when the start button is pressed. """
        window = ResultLogMainWindow()

        context = bpy.context
        api.check_objects_in_collection(context)
        api.check_asset_vertex_count(context)
        api.check_object_default_names()

        window.show()

        # Testing if we removed old handlers correctly
        # aspenLogger = logging.getLogger('aspen')
        # print("printing handlers: ")
        # for h in aspenLogger.handlers:
        #     print(f"Handler: {h.name}")
        #