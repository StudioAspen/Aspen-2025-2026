# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from .utils import *

#-------------------------------------------------------------
#   Mute Constraints
#-------------------------------------------------------------

class MuteUnmuter:
    def __init__(self):
        self.auto = True
        self.frames = []


    def setFrames(self, frames):
        self.frames = [int(frame) for frame in frames]


    def setup(self, rig, scn):
        self.auto = scn.tool_settings.use_keyframe_insert_auto
        first = last = scn.frame_current
        fcurves = getRnaFcurves(rig)
        self.auto = True
        for fcu in fcurves:
            times = [int(kp.co[0]) for kp in fcu.keyframe_points]
            if times:
                times.sort()
                if times[0] < first:
                     first = times[0]
                if times[-1] > last:
                     last = times[-1]
        self.frames = range(first, last+1)


    def getConstraints(self, rig):
        pbones = []
        iks = []
        limits = []
        for pb in rig.pose.bones:
            for cns in pb.constraints:
                if cns.mute and self.skipmute:
                    pass
                elif cns.type == 'IK':
                    iks.append(cns)
                    n = cns.chain_count
                    while n >= 0:
                        pbones.append(pb)
                        pb = pb.parent
                        n -= 1
                elif cns.type == 'LIMIT_ROTATION':
                    limits.append(cns)
        return pbones, iks, limits


    def getMatrices(self, rig):
        gmats = {}
        for pb in rig.pose.bones:
            gmats[pb.name] = pb.matrix.copy()
        return gmats


    def insertKeys(self, pbones, frame):
        for pb in pbones:
            pb.keyframe_insert("location", frame=frame, group=pb.name)
            if pb.rotation_mode == 'QUATERNION':
                pb.keyframe_insert("rotation_quaternion", frame=frame, group=pb.name)
            else:
                pb.keyframe_insert("rotation_euler", frame=frame, group=pb.name)
            pb.keyframe_insert("scale", frame=frame, group=pb.name)


    def muteUnmute(self, context, rig):
        scn = context.scene
        pbones, iks, limits = self.getConstraints(rig)
        if not iks:
            print("%s has no IK constraints" % rig.name)
            return False
        print("%s IK constraints for %s" % (self.doing, rig.name))
        if self.auto:
            gmatss = []
            for frame in self.frames:
                scn.frame_current = frame
                self.restoreBones(pbones, iks)
                updateScene(context)
                gmats = self.getMatrices(rig)
                gmatss.append((frame, gmats))
            self.muteConstraints(iks)
            self.muteConstraints(limits)
            for frame,gmats in gmatss:
                self.snapBones(pbones, gmats)
                self.insertKeys(pbones, frame)
            self.removeFcurves(rig, pbones)
        else:
            gmats = self.getMatrices(rig)
            self.muteConstraints(iks)
            self.muteConstraints(limits)
            self.snapBones(pbones, gmats)
        return True

#-------------------------------------------------------------
#   Mute Constraints
#-------------------------------------------------------------

class Muter(MuteUnmuter):
    doing = "Mute"
    skipmute = True

    def snapBones(self, pbones, gmats):
        n = 0
        for pb in pbones:
            M1 = gmats[pb.name]
            R1 = pb.bone.matrix_local
            if pb.parent:
                M0 = gmats[pb.parent.name]
                R0 = pb.parent.bone.matrix_local
                pb.matrix_basis = R1.inverted() @ R0 @ M0.inverted() @ M1
            else:
                pb.matrix_basis = R1.inverted() @ M1

    def muteConstraints(self, constraints):
        for cns in constraints:
            cns.mute = True

    def restoreBones(self, pbones, constraints):
        for cns in constraints:
            cns.mute = False
        for pb in pbones:
            pb.matrix_basis = Matrix()

    def removeFcurves(self, rig, pbones):
        pass


class MCP_OT_MuteIkConstraints(Muter, BvhOperator, IsArmature):
    bl_idname = "mcp.mute_ik_constraints"
    bl_label = "Mute IK Constraints"
    bl_description = "Mute all IK and limit rotation constraints"
    bl_options = {'UNDO'}

    def run(self, context):
        rig = context.object
        self.setup(rig, context.scene)
        self.muteUnmute(context, rig)

#-------------------------------------------------------------
#   Unmute Constraints
#-------------------------------------------------------------

class Unmuter(MuteUnmuter):
    doing = "Unmute"
    skipmute = False

    def snapBones(self, pbones, mats):
        for pb in pbones:
            pb.matrix_basis = Matrix()

    def muteConstraints(self, constraints):
        for cns in constraints:
            cns.mute = False

    def restoreBones(self, pbones, constraints):
        for cns in constraints:
            cns.mute = True

    def removeFcurves(self, rig, pbones):
        fcurves = getRnaFcurves(rig)
        bnames = [pb.name for pb in pbones]
        for fcu in list(fcurves):
            bname,channel = getBoneChannel(fcu)
            if bname in bnames:
                fcurves.remove(fcu)


class MCP_OT_UnmuteIkConstraints(Unmuter, BvhOperator, IsArmature):
    bl_idname = "mcp.unmute_ik_constraints"
    bl_label = "Unmute IK Constraints"
    bl_description = "Unmute all IK and limit rotation constraints"
    bl_options = {'UNDO'}

    def run(self, context):
        rig = context.object
        self.setup(rig, context.scene)
        self.muteUnmute(context, rig)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_MuteIkConstraints,
    MCP_OT_UnmuteIkConstraints,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
