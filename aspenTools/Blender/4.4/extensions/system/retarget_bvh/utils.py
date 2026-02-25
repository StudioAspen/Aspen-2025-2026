# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from bpy.props import *
import math
from mathutils import *
from .bsettings import BD, BS, mcpRna

D = math.pi/180
R = 180/math.pi

#-------------------------------------------------------------
#   Blender 5
#-------------------------------------------------------------

if bpy.app.version < (5,0,0):
    def P2B(pb):
        return pb.bone
else:
    def P2B(pb):
        return pb

#-------------------------------------------------------------
#   Action slots
#-------------------------------------------------------------

if bpy.app.version < (4,4,0):

    def getActionBag(act, id_type='OBJECT'):
        return act

    def getActionFcurves(act, id_type='OBJECT'):
        if act:
            return act.fcurves
        else:
            return []

    def getRnaFcurves(rna, id_type='OBJECT'):
        if rna.animation_data and rna.animation_data.action:
            return rna.animation_data.action.fcurves
        else:
            return []

    def setNewAction(rna, aname):
        if rna.animation_data is None:
            rna.animation_data_create()
        act = bpy.data.actions.new(name=aname)
        rna.animation_data.action = act
        return act

else:

    def getActionBag(act, id_type='OBJECT'):
        if act and act.layers:
            strip = act.layers[0].strips[0]
            for slot in act.slots:
                if slot.target_id_type == id_type:
                    return strip.channelbag(slot, ensure=True)

    def getActionFcurves(act, id_type='OBJECT'):
        bag = getActionBag(act, id_type)
        if bag:
            return bag.fcurves
        else:
            return []

    def getRnaFcurves(rna):
        if rna.animation_data and rna.animation_data.action:
            return getActionFcurves(rna.animation_data.action, rna.id_type)
        else:
            return []

    def setNewAction(rna, aname):
        if rna.animation_data is None:
            rna.animation_data_create()
        act = bpy.data.actions.new(name=aname)
        rna.animation_data.action = act
        if rna.id_type == 'OBJECT':
            path = "location"
        elif rna.id_type == 'KEY':
            path = 'key_blocks[0].value'
        rna.keyframe_insert(path)
        rna.keyframe_delete(path)
        return act

#-------------------------------------------------------------
#   Bone layers
#-------------------------------------------------------------

if bpy.app.version < (4,0,0):
    def inVisibleLayer(bone, rig):
        for vis1,vis2 in zip(rig.data.layers, bone.layers):
            if vis1 and vis2:
                return True
        return False

    def getRigLayers(rig):
        return list(rig.data.layers)

    def setRigLayers(rig, layers):
        rig.data.layers = layers

    def enableAllRigLayers(rig):
        rig.data.layers = 32*[True]

    def enableRigLayer(rig, layer, cname, value):
        rig.data.layers[layer] = value

else:
    def inVisibleLayer(bone, rig):
        for coll in rig.data.collections:
            if coll.is_visible and bone.name in coll.bones:
                return True
        return (len(rig.data.collections) == 0)

    def getRigLayers(rig):
        return [(coll,coll.is_visible) for coll in rig.data.collections]

    def setRigLayers(rig, layers):
        for coll,vis in layers:
            coll.is_visible = vis

    def enableAllRigLayers(rig):
        for coll in rig.data.collections:
            coll.is_visible = True

    def enableRigLayer(rig, layer, cname, value):
        coll = rig.data.collections.get(cname)
        if coll:
            coll.is_visible = value

#-------------------------------------------------------------
#
#-------------------------------------------------------------

def rotationMatrix(mat):
    return mat.to_quaternion().to_matrix().to_4x4()


def evalFcurve(fcu, frame):
    if fcu is None:
        return 0.0
    else:
        return fcu.evaluate(frame)


def setActiveObject(context, ob):
    vly = context.view_layer
    vly.objects.active = ob
    vly.update()


def updateScene(context):
    deps = context.evaluated_depsgraph_get()
    deps.update()


def updateObject(context, ob):
    dg = context.evaluated_depsgraph_get()
    return ob.evaluated_get(dg)


def setFrame(scn, frame):
    try:
        scn.frame_set(frame)
    except TypeError:
        scn.frame_set(int(frame))

def setCurrentFrame(scn, frame):
    try:
        scn.frame_current = frame
    except TypeError:
        scn.frame_current = int(frame)

#
#  quadDict():
#

def quadDict():
    return {
        0: {},
        1: {},
        2: {},
        3: {},
    }

MhxLayers = 8*[True] + 8*[False] + 8*[True] + 8*[False]
RigifyLayers = 27*[True] + 5*[False]

#
#   Identify rig type
#

def hasAllBones(blist, rig):
    for bname in blist:
        if not getCanonicalBone(rig, bname):
            return False
    return True

def hasSomeBones(blist, rig):
    for bname in blist:
        pb = getCanonicalBone(rig, bname)
        if pb:
            return pb.name

def isMhxRig(rig):
    return (rig.type == 'ARMATURE' and hasAllBones(["foot.rev.L"], rig))

def isMakeHuman(rig):
    return hasAllBones(["risorius03.R"], rig)

def isMhx7Rig(rig):
    return hasAllBones(["FootRev_L"], rig)

def isRigify(rig):
    return hasAllBones(["MCH-spine.flex"], rig)

def isRigify2(rig):
    return hasAllBones(["MCH-forearm_ik.L"], rig)

#
#   nameOrNone(string):
#

def nameOrNone(string):
    if string == "None":
        return None
    else:
        return string


def canonicalName(string):
    return string.lower().replace(' ','_').replace('-','_')


def getCanonicalBone(rig, bname):
    pb = rig.pose.bones.get(bname)
    if pb:
        return pb
    return rig.pose.bones.get(bname.replace(" ", "_"))

#
#   getRoll(bone):
#

def getRoll(bone):
    return getRollMat(bone.matrix_local)


def getRollMat(mat):
    quat = mat.to_3x3().to_quaternion()
    if abs(quat.w) < 1e-4:
        roll = pi
    else:
        roll = -2*math.atan(quat.y/quat.w)
    return roll


#
#   getTrgBone(b):
#

def getTrgBone(bname, rig, force=False):
    for pb in rig.pose.bones:
        if mcpRna(pb).Bone == bname:
            return pb
    if force:
        raise MocapError("No %s bone found" % bname)
    return None

#
#   isRotation(mode):
#   isLocation(mode):
#

def isRotation(mode):
    return (mode[0:3] == 'rot')

def isLocation(mode):
    return (mode[0:3] == 'loc')

#
#    Insert location and rotation
#

def insertLocation(pb, mat, frame=None, offset=None):
    if frame is None:
        frame = bpy.context.scene.frame_current
    pb.location = mat.to_translation()
    if offset:
        pb.location -= offset
    pb.keyframe_insert("location", group=pb.name)


def insertRotation(pb, mat, frame=None):
    if frame is None:
        frame = bpy.context.scene.frame_current
    if pb.rotation_mode == 'QUATERNION':
        pb.rotation_quaternion = mat.to_quaternion()
        pb.keyframe_insert("rotation_quaternion", frame=frame, group=pb.name)
    elif pb.rotation_mode == "AXIS_ANGLE":
        axis, angle = mat.to_quaternion().to_axis_angle()
        pb.rotation_axis_angle = (angle, *axis)
        pb.keyframe_insert("rotation_axis_angle", frame=frame, group=pb.name)
    else:
        pb.rotation_euler = mat.to_euler(pb.rotation_mode)
        pb.keyframe_insert("rotation_euler", frame=frame, group=pb.name)

#-------------------------------------------------------------
#   setInterpolation
#-------------------------------------------------------------

def setInterpolation(rig):
    fcurves = getRnaFcurves(rig)
    for fcu in fcurves:
        for pt in fcu.keyframe_points:
            pt.interpolation = 'LINEAR'
        fcu.extrapolation = 'CONSTANT'

#-------------------------------------------------------------
#   Utils
#-------------------------------------------------------------

def getBoneChannel(fcu):
    words = fcu.data_path.split('"')
    if words[0] == "pose.bones[":
        return words[1], words[-1].split(".")[-1]
    else:
        return None, None

#-------------------------------------------------------------
#   Progress
#-------------------------------------------------------------

def startProgress(string):
    print(string + " (0%)")
    wm = bpy.context.window_manager
    wm.progress_begin(0, 100)

def endProgress(string):
    print(string + " (100%)")
    wm = bpy.context.window_manager
    wm.progress_end()

def showProgress(n, frame, nFrames, step=20):
    pct = (100.0*n)/nFrames
    if n % step == 0:
        print("%d (%.1f " % (int(frame), pct) + "%)")
    wm = bpy.context.window_manager
    wm.progress_update(int(pct))

#-------------------------------------------------------------
#   Error handling
#-------------------------------------------------------------

def clearErrorMessage():
    global theMessage, theErrorLines
    theMessage = ""
    theErrorLines = []

clearErrorMessage()

def getErrorMessage():
    """getErrorMessage()

    Returns:
    The error message from previous operator invokation if it raised
    an error, or the empty string if the operator exited without errors.
    """
    global theMessage
    return theMessage


def getSilentMode():
    global theSilentMode
    return theSilentMode

def setSilentMode(value):
    """setSilentMode(value)

    In silent mode, operators fail silently if they encounters an error.
    This is useful for scripting.

    value: True turns silent mode on, False turns it off.
    """
    global theSilentMode
    theSilentMode = value

setSilentMode(False)


class MocapError(Exception):
    def __init__(self, value):
        global theErrorLines, theMessage
        theMessage = value
        theErrorLines = theMessage.split("\n")
        print("*** BVH Retargeter Error ***")
        for line in theErrorLines:
            print(line)

    def __str__(self):
        return repr(theMessage)


class MocapMessage(Exception):
    def __init__(self, value):
        global theErrorLines, theMessage
        theMessage = value
        theErrorLines = theMessage.split("\n")
        print(theMessage)


class MocapPopup(bpy.types.Operator):
    def execute(self, context):
        return {'RUNNING_MODAL'}

    def invoke(self, context, event):
        wm = context.window_manager
        wm.progress_end()
        return wm.invoke_props_dialog(self)

    def draw(self, context):
        global theErrorLines
        for line in theErrorLines:
            self.layout.label(text=line)


class ErrorOperator(MocapPopup):
    bl_idname = "mcp.error"
    bl_label = "BVH Retargeter Error"


class MessageOperator(MocapPopup):
    bl_idname = "mcp.message"
    bl_label = "BVH Retargeter"

#-------------------------------------------------------------
#   Poll
#-------------------------------------------------------------

class IsMesh:
    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.type == 'MESH')


class IsArmature:
    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.type == 'ARMATURE')


class IsMeshArmature:
    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.type in ('MESH', 'ARMATURE'))


class IsMhx:
    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.type == 'ARMATURE' and isMhxRig(ob))


class HasAnimation:
    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.animation_data and ob.animation_data.action)

#-------------------------------------------------------------
#   Execute
#-------------------------------------------------------------

class BvhOperator(bpy.types.Operator):
    def execute(self, context):
        clearErrorMessage()
        data = self.prequel(context)
        try:
            self.run(context)
        except MocapError:
            if getSilentMode():
                print(theMessage)
            else:
                bpy.ops.mcp.error('INVOKE_DEFAULT')
        except MocapMessage:
            if getSilentMode():
                print(theMessage)
            else:
                bpy.ops.mcp.message('INVOKE_DEFAULT')
        except KeyboardInterrupt:
            global theErrorLines
            theErrorLines = ["Keyboard interrupt"]
            bpy.ops.mcp.error('INVOKE_DEFAULT')
        finally:
            self.sequel(context, data)
        return{'FINISHED'}

    def prequel(self, context):
        try:
            self.mode = context.object.mode
            bpy.ops.object.mode_set(mode='OBJECT')
        except (RuntimeError, AttributeError):
            self.mode = None

    def sequel(self, context, data):
        if self.mode:
            try:
                bpy.ops.object.mode_set(mode=self.mode)
            except (RuntimeError, AttributeError):
                pass


class BvhPropsOperator(BvhOperator):
    def invoke(self, context, event):
        clearErrorMessage()
        wm = context.window_manager
        return wm.invoke_props_dialog(self)

#-------------------------------------------------------------
#   HideOperator class
#-------------------------------------------------------------

class HideOperator(BvhOperator):
    def prequel(self, context):
        BvhOperator.prequel(self, context)
        rig = context.object
        self.layerColls = []
        if rig:
            self.hideLayerColls(rig, context.view_layer.layer_collection)
            rig.hide_viewport = False
            rig.hide_select = False

    def hideLayerColls(self, rig, layer):
        self.layerColls.append((layer, layer.exclude))
        if rig.name not in layer.collection.all_objects:
            layer.exclude = True
        for child in layer.children:
            self.hideLayerColls(rig, child)

    def sequel(self, context, _data):
        BvhOperator.prequel(self, context)
        for layer,exclude in self.layerColls:
            layer.exclude = exclude


class HidePropsOperator(HideOperator):
    def invoke(self, context, event):
        clearErrorMessage()
        wm = context.window_manager
        return wm.invoke_props_dialog(self)
