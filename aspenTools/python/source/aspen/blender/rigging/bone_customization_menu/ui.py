import os

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

class BoneCustomizationMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        #Load UI, so computer knows which file to load
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'main_window.ui')
        )
        #buttons and input fields
        self.asset_name_bone_selection.hasSelectedText()
        self.asset_name_shape_selection_combobox
        self.asset_name_rotation_x.setText('0')
        self.asset_name_rotation_y.setText('0')

BoneCustomizationMainWindow().show()
