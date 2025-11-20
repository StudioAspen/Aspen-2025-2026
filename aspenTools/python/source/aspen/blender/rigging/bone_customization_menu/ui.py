import os

from aspen.core.qt.singleton_main_window import SingletonMainWindow
from aspen.core.qt import ui_loader

class BoneCustomizationMainWindow(SingletonMainWindow):

    def __init__(self, parent=None):
        super().__init__(parent=parent)

        #Load UI, so computer knows which file to load
        ui_loader.load_ui(
            os.path.join(os.path.dirname(__file__), 'main_window.ui'), self
        )
        #buttons and input fields
        self.bone_selection.setText('bone go here')
        self.shape_selection_combobox.addItems(['Circle', 'Sphere', 'Square', 'Cube', 'Rectangle', 'Box'])
        self.rotation_x.setText('0')
        self.rotation_y.setText('0')

BoneCustomizationMainWindow().show()
