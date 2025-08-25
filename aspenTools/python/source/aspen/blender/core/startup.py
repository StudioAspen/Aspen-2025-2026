from aspen.blender.common.export_manager import export_manager

modules = [
    export_manager
]

def register():
    for mod in modules:
        mod.register()

def unregister():
    for mod in modules:
        mod.unregister()