#  BVH and FBX Retargeter - Mocap retargeting tool
#  Copyright (c) 2019-2025, Thomas Larsson
#
#  This program is free software: you can redistribute it and/or modify
#  it under the terms of the GNU General Public License as published by
#  the Free Software Foundation, either version 2 of the License, or
#  (at your option) any later version.
#
#  This program is distributed in the hope that it will be useful,
#  but WITHOUT ANY WARRANTY; without even the implied warranty of
#  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#  GNU General Public License for more details.
#
#  You should have received a copy of the GNU General Public License
#  along with this program.  If not, see <https://www.gnu.org/licenses/>.

bl_info = {
    "name": "BVH and FBX Retargeter",
    "author": "Thomas Larsson",
    "version": (5,0,0),
    "blender": (5,0,0),
    "location": "View3D > Tools > Retarget BVH",
    "description": "Mocap retargeting tool",
    "warning": "",
    "doc_url": "https://bitbucket.org/Diffeomorphic/retarget_bvh/wiki/Home",
    "tracker_url": "https://bitbucket.org/Diffeomorphic/import_daz/issues?status=new&status=open",
    "category": "Animation"}

#----------------------------------------------------------
#   In some Blender builds numpy isn't found because
#   "site-packages" is not in sys.path
#----------------------------------------------------------

import sys
import os
try:
    import numpy
    fail = False
except ModuleNotFoundError:
    fail = True

if fail:
    missing = []
    for path in sys.path:
        if os.path.basename(path) == "python":
            packpath = os.path.join(path, "site-packages")
            if packpath not in sys.path:
                missing.append(packpath)
    print("Adding missing packages")
    for path in missing:
        print("  Add %s" % path)
        sys.path.append(path)

#----------------------------------------------------------
# To support reload properly, try to access a package var, if it's there, reload everything
#----------------------------------------------------------

Modules = ["bsettings", "utils", "io_json", "armature", "source", "target",
           "t_pose", "simplify", "load", "retarget", "action",
           "loop", "mute", "panels", "alayers"]

if "bpy" in locals():
    print("Reloading BVH Retargeter")
    import imp
    for modname in Modules:
        exec("imp.reload(%s)" % modname)
else:
    print("Loading BVH Retargeter")
    import bpy
    for modname in Modules:
        exec("from . import %s" % modname)

#----------------------------------------------------------
#   Import documented functions available for external scripting
#----------------------------------------------------------

from .utils import getErrorMessage, setSilentMode

#----------------------------------------------------------
#   Prefences
#----------------------------------------------------------

from bpy.props import BoolProperty

class BvhPreferences(bpy.types.AddonPreferences):
    bl_idname = __name__

    verbose : BoolProperty(
        name="Verbose",
        description="Verbose mode for debugging")

    useLimits : BoolProperty(
        name="Use Limits",
        description="Restrict angles to Limit Rotation constraints")

    useUnlock : BoolProperty(
        name="Unlock Rotation",
        description="Clear X and Z rotation locks")

    ignoreLeafBones : BoolProperty(
        name = "Ignore Leaf Bones",
        default = False)

    useBlenderBvh : BoolProperty(
        name = "Blender BVH",
        description = "Use Blender's built-in BVH importer")

    useNativeFbx : BoolProperty(
        name = "Native FBX",
        description = "Retarget from the native FBX animation\ninstead of converting to BVH")


    def draw(self, context):
        box = self.layout.box()
        box.prop(self, "verbose")
        box.prop(self, "useLimits")
        box.prop(self, "useUnlock")
        box.prop(self, "ignoreLeafBones")
        box = self.layout.box()
        box.prop(self, "useBlenderBvh")
        box.prop(self, "useNativeFbx")

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

def register():
    bpy.utils.register_class(BvhPreferences)
    bsettings.register()
    action.register()
    load.register()
    loop.register()
    retarget.register()
    simplify.register()
    source.register()
    t_pose.register()
    target.register()
    mute.register()
    panels.register()
    alayers.register()

    from .bsettings import BD
    addon = bpy.context.preferences.addons.get(__name__)
    if addon and addon.preferences:
        BD.prefs = addon.preferences


def unregister():
    action.unregister()
    load.unregister()
    loop.unregister()
    retarget.unregister()
    simplify.unregister()
    source.unregister()
    t_pose.unregister()
    target.unregister()
    mute.unregister()
    panels.unregister()
    alayers.unregister()
    bsettings.unregister()
    bpy.utils.unregister_class(BvhPreferences)
    global BS
    BS = None


if __name__ == "__main__":
    register()

print("BVH Retargeter loaded")

