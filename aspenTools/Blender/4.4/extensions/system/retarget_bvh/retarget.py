# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

#
#   M_b = global bone matrix, relative world (PoseBone.matrix)
#   L_b = local bone matrix, relative parent and rest (PoseBone.matrix_local)
#   R_b = bone rest matrix, relative armature (Bone.matrix_local)
#   T_b = global T-pose marix, relative world
#
#   M_b = M_p R_p^-1 R_b L_b
#   M_b = A_b M'_b
#   T_b = A_b T'_b
#   A_b = T_b T'^-1_b
#   B_b = R^-1_b R_p
#
#   L_b = R^-1_b R_p M^-1_p A_b M'_b
#   L_b = B_b M^-1_p A_b M'_b
#


import bpy
import mathutils
import time
import os
from collections import OrderedDict
from mathutils import *
from bpy.props import *
from bpy_extras.io_utils import ImportHelper, orientation_helper

from .utils import *
from .target import Target
from .simplify import Simplifier, TimeScaler
from .load import BvhFile, MultiFile, BvhLoader, BvhRenamer, FrameRange
from .load import activateObject

#-------------------------------------------------------------
#   Limbs bend positive
#-------------------------------------------------------------

class Bender:
    useElbows : BoolProperty(
        name="Elbows",
        description="Keep elbow bending positive",
        default=True)

    useKnees : BoolProperty(
        name="Knees",
        description="Keep knee bending positive",
        default=True)

    useBendPositive : BoolProperty(
        name="Bend Positive",
        description="Ensure that elbow and knee bending is positive",
        default=True)

    def draw(self, context):
        self.layout.prop(self, "useElbows")
        self.layout.prop(self, "useKnees")

    def limbsBendPositive(self, rig, frames):
        limbs = {}
        if self.useElbows:
            pb = getTrgBone("forearm.L", rig, force=True)
            self.minimizeFCurve(pb, rig, frames)
            pb = getTrgBone("forearm.R", rig, force=True)
            self.minimizeFCurve(pb, rig, frames)
        if self.useKnees:
            pb = getTrgBone("shin.L", rig, force=True)
            self.minimizeFCurve(pb, rig, frames)
            pb = getTrgBone("shin.R", rig, force=True)
            self.minimizeFCurve(pb, rig, frames)


    def minimizeFCurve(self, pb, rig, frames, index=None):
        if pb is None:
            return
        if index is None:
            index = (1 if pb.rotation_mode == 'QUATERNION' else 0)
        fcu = self.findBoneFCurve(pb, rig, index)
        if fcu is None:
            return
        y0 = fcu.evaluate(0)
        t0 = frames[0]
        t1 = frames[-1]
        for kp in fcu.keyframe_points:
            t = kp.co[0]
            if t >= t0 and t <= t1:
                y = kp.co[1]
                if y < y0:
                    kp.co[1] = y0


    def findBoneFCurve(self, pb, rig, index, mode='rotation'):
        def findFCurve(path, index, fcurves):
            for fcu in fcurves:
                if (fcu.data_path == path and
                    fcu.array_index == index):
                    return fcu
            print('F-curve "%s" not found.' % path)
            return None

        if mode == 'rotation':
            if pb.rotation_mode == 'QUATERNION':
                mode = "rotation_quaternion"
            else:
                mode = "rotation_euler"
        path = 'pose.bones["%s"].%s' % (pb.name, mode)

        fcurves = getRnaFcurves(rig)
        if fcurves:
            return findFCurve(path, index, fcurves)


class MCP_OT_LimbsBendPositive(HidePropsOperator, IsArmature, Bender, FrameRange, Target):
    bl_idname = "mcp.limbs_bend_positive"
    bl_label = "Bend Limbs Positive"
    bl_description = "Ensure that limbs' X rotation is positive."
    bl_options = {'UNDO'}

    def draw(self, context):
        Bender.draw(self, context)
        FrameRange.draw(self, context)

    def prequel(self, context):
        rig = context.object
        HidePropsOperator.prequel(self, context)
        return (rig, getRigLayers(rig))

    def run(self, context):
        from .loop import getActiveFrames
        scn = context.scene
        rig = context.object
        self.findTarget(context, rig)
        startFrame,endFrame = self.getStartEndFrame()
        frames = getActiveFrames(rig, startFrame, endFrame)
        self.limbsBendPositive(rig, frames)
        print("Limbs bent positive")

    def sequel(self, context, data):
        rig,layers = data
        setRigLayers(rig, layers)
        return HidePropsOperator.sequel(self, context, data)


class CAnimation:

    def __init__(self, srcRig, trgRig, info, context):
        self.srcRig = srcRig
        self.trgRig = trgRig
        self.scene = context.scene
        self.boneAnims = OrderedDict()
        if not BS().useLimits:
            self.clearLimits(trgRig)

        scn = context.scene
        for (trgName, srcName) in info.bones:
            if (trgName in trgRig.pose.bones.keys() and
                srcName in srcRig.pose.bones.keys()):
                trgBone = trgRig.pose.bones[trgName]
                srcBone = srcRig.pose.bones[srcName]
            else:
                #print("  -", trgName, srcName)
                continue
            parent = self.getTargetParent(trgName, trgBone)
            self.boneAnims[trgName] = CBoneAnim(srcBone, trgBone, parent, self, context)


    def clearLimits(self, rig):
        for pb in rig.pose.bones:
            for cns in pb.constraints:
                if cns.type.startswith("LIMIT"):
                    cns.influence = 0.0


    def getTargetParent(self, trgName, trgBone):
        parName = mcpRna(trgBone).Parent
        while (parName and parName not in self.boneAnims.keys()):
            print("Skipping", parName)
            parBone = self.trgRig.pose.bones[parName]
            parName = mcpRna(parBone).Parent
        if parName:
            return self.boneAnims[parName]
        else:
            return None


    def printResult(self, scn, frame):
        setFrame(scn, frame)
        for name in ["LeftHip"]:
            banim = self.boneAnims[name]
            banim.printResult(frame)


    def putInTPoses(self, context):
        from .t_pose import putInTPose, putInRestPose
        scn = context.scene
        setFrame(scn, 0)
        putInRestPose(context, self.srcRig, True)
        putInTPose(context, self.srcRig, mcpRna(scn).SourceTPose)
        putInRestPose(context, self.trgRig, True)
        putInTPose(context, self.trgRig, mcpRna(scn).TargetTPose)
        for banim in self.boneAnims.values():
            banim.getTPoseMatrix()


    def retarget(self, frames, context, offset, nFrames):
        objects = hideObjects(context, self.srcRig)
        scn = context.scene
        try:
            for n,frame in enumerate(frames):
                setFrame(scn, frame)
                showProgress(n+offset, frames[n], nFrames)
                for banim in self.boneAnims.values():
                    banim.retarget(frame)
        finally:
            unhideObjects(objects)


class CBoneAnim:

    def __init__(self, srcBone, trgBone, parent, anim, context):
        self.name = srcBone.name
        self.srcMatrices = {}
        self.trgMatrices = {}
        self.srcMatrix = None
        self.trgMatrix = None
        self.srcBone = srcBone
        self.trgBone = trgBone
        self.parent = parent
        self.offset = None
        self.order,self.locks = getLocks(trgBone, context)
        self.aMatrix = None
        if self.parent:
            self.bMatrix = trgBone.bone.matrix_local.inverted() @ self.parent.trgBone.bone.matrix_local
        else:
            self.bMatrix = trgBone.bone.matrix_local.inverted()


    def __repr__(self):
        return (
            "<CBoneAnim %s" % self.name +
            "  src %s" % self.srcBone.name +
            "  trg %s\n" % self.trgBone.name +
            "  A %s\n" % self.aMatrix +
            "  B %s\n" % self.bMatrix)


    def printResult(self, frame):
        print(
            "Retarget %s => %s\n" % (self.srcBone.name, self.trgBone.name) +
            "S %s\n" % self.srcBone.matrix +
            "T %s\n" % self.trgBone.matrix +
            "R %s\n" % self.trgBone.matrix @ self.srcBone.matrix.inverted()
            )


    def insertKeyFrame(self, mat, frame):
        pb = self.trgBone
        insertRotation(pb, mat, frame)
        if not self.parent:
            insertLocation(pb, mat, frame, self.offset)


    def getTPoseMatrix(self):
        trgrot = self.trgBone.matrix.decompose()[1]
        trgmat = trgrot.to_matrix().to_4x4()
        srcrot = self.srcBone.matrix.decompose()[1]
        srcmat = srcrot.to_matrix().to_4x4()
        self.aMatrix = srcmat.inverted() @ trgmat


    def retarget(self, frame):
        self.srcMatrix = self.srcBone.matrix.copy()
        self.trgMatrix = self.srcMatrix @ self.aMatrix
        self.trgMatrix.col[3] = self.srcMatrix.col[3]
        if self.parent:
            mat1 = self.parent.trgMatrix.inverted() @ self.trgMatrix
        else:
            mat1 = self.trgMatrix
        mat2 = self.bMatrix @ mat1
        mat3 = correctMatrixForLocks(mat2, self.order, self.locks, self.trgBone)
        self.insertKeyFrame(mat3, frame)

        self.srcMatrices[frame] = self.srcMatrix
        mat1 = self.bMatrix.inverted() @ mat3
        if self.parent:
            self.trgMatrix = self.parent.trgMatrix @ mat1
        else:
            self.trgMatrix = mat1
        self.trgMatrices[frame] = self.trgMatrix

        return

        if self.name == "upper_arm.L":
            print()
            print(self)
            print("S ", self.srcMatrix)
            print("T ", self.trgMatrix)
            print(self.parent.name)
            print("TP", self.parent.trgMatrix)
            print("M1", mat1)
            print("M2", mat2)
            print("MB2", self.trgBone.matrix)


def getLocks(pb, context):
    locks = []
    order = 'XYZ'
    if BS().useUnlock:
        for cns in pb.constraints:
            if cns.type == 'LIMIT_ROTATION':
                if pb.lock_rotation[0]:
                    cns.use_limit_x = 0
                if pb.lock_rotation[2]:
                    cns.use_limit_z = 0
        pb.lock_rotation[0] = pb.lock_rotation[2] = False

    if pb.lock_rotation[1]:
        locks.append(1)
        order = 'YZX'
        if pb.lock_rotation[0]:
            order = 'YXZ'
            locks.append(0)
        if pb.lock_rotation[2]:
            locks.append(2)
    elif pb.lock_rotation[2]:
        locks.append(2)
        order = 'ZYX'
        if pb.lock_rotation[0]:
            order = 'ZXY'
            locks.append(0)
    elif pb.lock_rotation[0]:
        locks.append(0)
        order = 'XYZ'

    if pb.rotation_mode != 'QUATERNION':
        order = pb.rotation_mode

    return order,locks


def correctMatrixForLocks(mat, order, locks, pb):
    head = Vector(mat.col[3])

    if locks:
        euler = mat.to_3x3().to_euler(order)
        for n in locks:
            euler[n] = 0
        mat = euler.to_matrix().to_4x4()

    if not BS().useLimits:
        mat.col[3] = head
        return mat

    for cns in pb.constraints:
        if (cns.type == 'LIMIT_ROTATION' and
            cns.owner_space == 'LOCAL' and
            not cns.mute and
            cns.influence > 0.5):
            euler = mat.to_3x3().to_euler(order)
            if cns.use_limit_x:
                euler.x = min(cns.max_x, max(cns.min_x, euler.x))
            if cns.use_limit_y:
                euler.y = min(cns.max_y, max(cns.min_y, euler.y))
            if cns.use_limit_z:
                euler.z = min(cns.max_z, max(cns.min_z, euler.z))
            mat = euler.to_matrix().to_4x4()

    mat.col[3] = head
    return mat


def hideObjects(context, rig):
    if bpy.app.version >= (2,80,0):
        return None
    objects = []
    for ob in context.view_layer.objects:
        if ob != rig:
            objects.append((ob, list(ob.layers)))
            ob.layers = 20*[False]
    return objects


def unhideObjects(objects):
    if bpy.app.version >= (2,80,0):
        return
    for (ob,layers) in objects:
        ob.layers = layers


class Retargeter:
    useCenterAnimation : BoolProperty(
        name = "Center Animation",
        description = "Move frame 0 to origin",
        default = True)

    def draw(self, context):
        self.layout.prop(self, "useCenterAnimation")

    def prequel(self, context):
        data = changeTargetData(context.object, context.scene)
        return (time.perf_counter(), data)

    def sequel(self, context, stuff):
        time1,data = stuff
        restoreTargetData(data)
        time2 = time.perf_counter()
        print("Retargeting finished in %.3f s" % (time2-time1))


    def retargetAnimation(self, context, srcRig, trgRig):
        from .source import setSourceArmature
        from .target import findTargetArmature
        from .t_pose import setRigToFK
        from .loop import getActiveFrames
        from .mute import Muter, Unmuter

        startProgress("Retargeting %s => %s" % (srcRig.name, trgRig.name))
        if srcRig.type != 'ARMATURE':
            return None,0
        scn = context.scene
        startFrame,endFrame = self.getStartEndFrame()
        frames = getActiveFrames(srcRig, startFrame, endFrame)
        nFrames = len(frames)

        muter = Muter()
        muter.setFrames(frames)
        muted = muter.muteUnmute(context, srcRig)

        setActiveObject(context, trgRig)
        if trgRig.animation_data:
            trgRig.animation_data.action = None
        setRigToFK(trgRig)

        if frames:
            setCurrentFrame(scn, frames[0])
        else:
            raise MocapError("No frames found.")
        oldData = changeTargetData(trgRig, scn)

        setSourceArmature(srcRig, scn)
        print("Retarget %s --> %s" % (srcRig.name, trgRig.name))

        info = findTargetArmature(context, trgRig, self.useAutoTarget)
        anim = CAnimation(srcRig, trgRig, info, context)
        anim.putInTPoses(context)

        frameBlock = frames[0:100]
        index = 0
        try:
            while frameBlock:
                anim.retarget(frameBlock, context, index, nFrames)
                index += 100
                frameBlock = frames[index:index+100]

            setCurrentFrame(scn, frames[0])
        finally:
            restoreTargetData(oldData)

        #anim.printResult(scn, 1)
        act = trgRig.animation_data.action
        if self.useCenterAnimation:
            from .loop import centerAnimation
            centerAnimation(context, trgRig, act)
        act.name = trgRig.name[:4] + srcRig.name[2:]
        act.use_fake_user = False
        setInterpolation(trgRig)

        if muted:
            unmuter = Unmuter()
            unmuter.setFrames(frames)
            unmuter.muteUnmute(context, srcRig)

        endProgress("Retargeted %s --> %s" % (srcRig.name, trgRig.name))
        return act,nFrames

#
#   changeTargetData(rig, scn):
#   restoreTargetData(data):
#

def changeTargetData(rig, scn):
    def setValue(rig, prop, value):
        try:
            if hasattr(rig, prop):
                setattr(rig, prop, value)
            elif prop in rig.keys():
                rig[prop] = value
            elif prop in rig.data.keys():
                rig.data[prop] = value
        except TypeError:
            pass

    layers = getRigLayers(rig)
    enableAllRigLayers(rig)
    keepLimits = False

    def isMhxRig(rig):
        return ("MhaArmIk_L" in rig.keys())

    if isMhxRig(rig):
        for prop in ["MhaArmIk_L", "MhaArmIk_R", "MhaLegIk_L", "MhaLegIk_R", "MhaTongueIk"]:
            setValue(rig, prop, 0.0)
        for prop in ["MhaTongueIk", "MhaFingerIk_L", "MhaFingerIk_R"]:
            setValue(rig, prop, False)
        if not keepLimits:
            for prop in ["MhaForearmFollow_L", "MhaForearmFollow_R"]:
                setValue(rig, prop, False)
    return rig,layers


def restoreTargetData(data):
    rig,layers = data
    setRigLayers(rig, layers)
    return

    for (key,value) in props:
        rig[key] = value

    for b in norotBones:
        b.use_inherit_rotation = True

    for lock in locks:
        (pb, constraints) = lock
        for (cns, mute) in constraints:
            cns.mute = mute

#-------------------------------------------------------------
#   Buttons
#-------------------------------------------------------------

def getOtherRig(context, rig):
    for ob in context.scene.objects:
        if ob.select_get() and ob != rig and ob.type == 'ARMATURE':
            if Vector(ob.rotation_euler) != Vector((0,0,0)):
                print("Removing object rotation from source rig %s" % ob.name)
            return ob
    return None

#-------------------------------------------------------------
#   Retarget Renamed to Active
#-------------------------------------------------------------

class MCP_OT_RetargetRenamedToActive(HidePropsOperator, IsArmature, FrameRange, Target, Retargeter):
    bl_idname = "mcp.retarget_renamed_to_active"
    bl_label = "Retarget Renamed To Active"
    bl_description = "Retarget animation from the renamed source armature (selected) to the target (active) armature."
    bl_options = {'UNDO'}

    def draw(self, context):
        FrameRange.draw(self, context)
        Target.draw(self, context)
        Retargeter.draw(self, context)

    def run(self, context):
        from .load import checkObjectProblems
        checkObjectProblems(context)
        trgRig = context.object
        srcRig = getOtherRig(context, trgRig)
        if srcRig is None:
            raise MocapError("No source armature found")
        self.retargetAnimation(context, srcRig, trgRig)
        activateObject(context, trgRig)

    def invoke(self, context, event):
        BD.ensureInited(context.scene)
        return HidePropsOperator.invoke(self, context, event)

#-------------------------------------------------------------
#   Retarget Selected to Active
#-------------------------------------------------------------

class MCP_OT_RetargetSelectedToActive(BvhPropsOperator, IsArmature, FrameRange, BvhRenamer, Retargeter):
    bl_idname = "mcp.retarget_selected_to_active"
    bl_label = "Retarget Selected To Active"
    bl_description = "Retarget animation to the active (target) armature from the other selected (source) armatures"
    bl_options = {'UNDO'}

    def draw(self, context):
        FrameRange.draw(self, context)
        BvhRenamer.draw(self, context)
        Retargeter.draw(self, context)

    def run(self, context):
        from .load import checkObjectProblems, deleteObjects
        checkObjectProblems(context)
        trgRig = context.object
        srcRig = getOtherRig(context, trgRig)
        activateObject(context, srcRig, True)
        bpy.ops.object.duplicate()
        tmpRig = context.object
        context.view_layer.objects.active = trgRig
        try:
            self.renameAndRescaleBvh(context, tmpRig, trgRig)
            bpy.ops.object.mode_set(mode='OBJECT')
            self.retargetAnimation(context, tmpRig, trgRig)
        finally:
            deleteObjects(context, [tmpRig])
            trgRig.select_set(True)
            context.view_layer.objects.active = trgRig


    def invoke(self, context, event):
        BD.ensureInited(context.scene)
        return HidePropsOperator.invoke(self, context, event)


@orientation_helper(axis_forward='-Z', axis_up='Y')
class MCP_OT_LoadAndRetarget(HideOperator, IsArmature, MultiFile, BvhFile, BvhLoader, BvhRenamer, Retargeter, TimeScaler, Simplifier, Bender):
    bl_idname = "mcp.load_and_retarget"
    bl_label = "Load And Retarget"
    bl_description = "Load animation from bvh file to the active armature"
    bl_options = {'UNDO'}

    useNLA : BoolProperty(
        name = "Create NLA Strips",
        description = "Create a NLA strip for each loaded action",
        default = False)

    def draw(self, context):
        BvhLoader.draw(self, context)
        BvhRenamer.draw(self, context)
        Retargeter.draw(self, context)
        self.layout.prop(self, "useBendPositive")
        TimeScaler.draw(self, context)
        Simplifier.draw(self, context)
        self.layout.prop(self, "useNLA")


    def run(self, context):
        from .load import checkObjectProblems
        checkObjectProblems(context)
        rig = context.object
        infos = []
        for filepath in self.getFilePaths():
            print("---------------")
            info = self.retarget(context, filepath)
            infos.append(info)
        print("---------------")
        if self.useNLA:
            for act,size in infos:
                track = rig.animation_data.nla_tracks.new()
                track.name = act.name
                track.is_solo = True
                track.strips.new(act.name, 1, act)
            rig.animation_data.action = None
        activateObject(context, rig)
        raise MocapMessage("BVH file(s) retargeted")


    def retarget(self, context, filepath):
        from .load import deleteSourceRig

        print("\n---------------\nLoad and retarget %s" % filepath)
        trgRig = context.object
        srcRig = self.readMocapFile(context, filepath)
        info = (None, 0)
        try:
            self.renameAndRescaleBvh(context, srcRig, trgRig)
            info = self.retargetAnimation(context, srcRig, trgRig)
            scn = context.scene
            if self.useBendPositive:
                self.useKnees = self.useElbows = True
                self.limbsBendPositive(trgRig, (0,1e6))
            if self.useSimplify:
                self.simplifyFCurves(context, trgRig)
            if self.useTimeScale:
                self.timescaleFCurves(trgRig)
        finally:
            deleteSourceRig(context, srcRig, 'Y_')
        return info


    def invoke(self, context, event):
        BD.ensureInited(context.scene)
        context.window_manager.fileselect_add(self)
        return {'RUNNING_MODAL'}

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_RetargetRenamedToActive,
    MCP_OT_RetargetSelectedToActive,
    MCP_OT_LoadAndRetarget,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
