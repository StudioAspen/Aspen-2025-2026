# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from math import pi, sqrt
from mathutils import *

from .utils import *
from .simplify import FCurvesGetter
from .load import FrameRange

#
#   fCurveIdentity(fcu):
#

def fCurveIdentity(fcu):
    words = fcu.data_path.split('"')
    if len(words) < 2:
        return (None, None)
    name = words[1]
    words = fcu.data_path.split('.')
    mode = words[-1]
    return (name, mode)

#
#   Loop F-curves
#

class MCP_OT_LoopFCurves(BvhPropsOperator, IsArmature, FCurvesGetter):
    bl_idname = "mcp.loop_fcurves"
    bl_label = "Loop F-Curves"
    bl_description = "Make the beginning and end of the selected time range connect smoothly. Use before repeating."
    bl_options = {'UNDO'}

    blendRange : IntProperty(
        name="Blend Range",
        min=1,
        default=5)

    loopInPlace : BoolProperty(
        name="Loop In Place",
        description="Remove Location F-curves",
        default=False)

    deleteOutside : BoolProperty(
        name="Delete Outside Keyframes",
        description="Delete all keyframes outside the looped region",
        default = False)

    def draw(self, context):
        self.layout.prop(self, "blendRange")
        self.layout.prop(self, "loopInPlace")
        self.layout.prop(self, "deleteOutside")
        FCurvesGetter.draw(self, context)
        self.layout.separator()

    def run(self, context):
        startProgress("Loop F-curves")
        scn = context.scene
        rig = context.object
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        self.useMarkers = True
        (fculist, minTime, maxTime) = self.getActionFCurves(fcurves, rig, scn)
        if not fculist:
            return

        frames = getActiveFrames(rig, minTime, maxTime)
        nFrames = len(frames)
        self.normalizeRotCurves(scn, rig, fculist, frames)

        hasLocation = {}
        for n,fcu in enumerate(fculist):
            (name, mode) = fCurveIdentity(fcu)
            if isRotation(mode):
                self.loopFCurve(fcu, minTime, maxTime, scn)

        if self.loopInPlace:
            iknames = [pb.name for pb in self.getIkBoneList(rig)]
            ikbones = {}
            for fcu in fculist:
                (name, mode) = fCurveIdentity(fcu)
                if isLocation(mode) and name in iknames:
                    ikbones[name] = rig.pose.bones[name]

            for pb in ikbones.values():
                print("IK bone %s" % pb.name)
                setFrame(scn, minTime)
                head0 = pb.head.copy()
                setFrame(scn, maxTime)
                head1 = pb.head.copy()
                offs = (head1-head0)/(maxTime-minTime)

                restMat = pb.bone.matrix_local.to_3x3()
                restInv = restMat.inverted()

                heads = {}
                for n,frame in enumerate(frames):
                    setFrame(scn, frame)
                    showProgress(n, frame, nFrames)
                    heads[frame] = pb.head.copy()

                for n,frame in enumerate(frames):
                    showProgress(n, frame, nFrames)
                    setFrame(scn, frame)
                    head = heads[frame] - (frame-minTime)*offs
                    diff = head - pb.bone.head_local
                    pb.location = restInv @ diff
                    pb.keyframe_insert("location", group=pb.name)

        if self.deleteOutside:
            truncFCurves(fculist, minTime, maxTime)
        raise MocapMessage("F-curves looped")


    def loopFCurve(self, fcu, t0, tn, scn):
        from .simplify import getFCurveLimits
        delta = self.blendRange

        v0 = fcu.evaluate(t0)
        vn = fcu.evaluate(tn)
        fcu.keyframe_points.insert(frame=t0, value=v0)
        fcu.keyframe_points.insert(frame=tn, value=vn)
        (mode, upper, lower, diff) = getFCurveLimits(fcu)
        if mode == 'location':
            dv = vn-v0
        else:
            dv = 0.0

        newpoints = []
        for dt in range(delta):
            eps = 0.5*(1-dt/delta)

            t1 = t0+dt
            v1 = fcu.evaluate(t1)
            tm = tn+dt
            vm = fcu.evaluate(tm) - dv
            if (v1 > upper) and (vm < lower):
                vm += diff
            elif (v1 < lower) and (vm > upper):
                vm -= diff
            pt1 = (t1, (eps*vm + (1-eps)*v1))

            t1 = t0-dt
            v1 = fcu.evaluate(t1) + dv
            tm = tn-dt
            vm = fcu.evaluate(tm)
            if (v1 > upper) and (vm < lower):
                v1 -= diff
            elif (v1 < lower) and (vm > upper):
                v1 += diff
            ptm = (tm, eps*v1 + (1-eps)*vm)

            newpoints.extend([pt1,ptm])

        newpoints.sort()
        for (t,v) in newpoints:
            fcu.keyframe_points.insert(frame=t, value=v)


    def normalizeRotCurves(self, scn, rig, fcurves, frames):
        hasQuat = {}
        for fcu in fcurves:
            (name, mode) = fCurveIdentity(fcu)
            if mode == 'rotation_quaternion':
                hasQuat[name] = rig.pose.bones[name]

        nFrames = len(frames)
        for n,frame in enumerate(frames):
            setFrame(scn, frame)
            showProgress(n, frame, nFrames)
            for (name, pb) in hasQuat.items():
                pb.rotation_quaternion.normalize()
                pb.keyframe_insert("rotation_quaternion", group=name)


    def getIkBoneList(self, rig):
        hips = getTrgBone('hips', rig)
        if hips is None:
            if isMhxRig(rig):
                hips = rig.pose.bones["root"]
            elif isRigify(rig):
                hips = rig.pose.bones["hips"]
            elif isRigify2(rig):
                hips = rig.pose.bones["torso"]
            else:
                for bone in rig.data.bones:
                    if bone.parent is None:
                        hips = bone
                        break
        blist = [hips]
        for bname in ['hand.ik.L', 'hand.ik.R', 'foot.ik.L', 'foot.ik.R']:
            try:
                blist.append(rig.pose.bones[bname])
            except KeyError:
                pass
        return blist


def truncFCurves(fcurves, minTime, maxTime):
    for fcu in fcurves:
        kps = [kp for kp in fcu.keyframe_points
               if kp.co[0] < minTime or kp.co[0] > maxTime]
        kps.reverse()
        for kp in kps:
            fcu.keyframe_points.remove(kp, fast=True)

#
#   repeatFCurves(context, nRepeats):
#

class MCP_OT_RepeatFCurves(BvhPropsOperator, IsArmature, FCurvesGetter):
    bl_idname = "mcp.repeat_fcurves"
    bl_label = "Repeat Animation"
    bl_description = "Repeat the part of the animation between selected markers n times"
    bl_options = {'UNDO'}

    repeatNumber : IntProperty(
        name="Repeat Number",
        min=1,
        default=1)

    def draw(self, context):
        self.layout.prop(self, "repeatNumber")
        FCurvesGetter.draw(self, context)
        self.layout.separator()

    def run(self, context):
        startProgress("Repeat F-curves %d times" % self.repeatNumber)
        rig = context.object
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        self.useMarkers = True
        (fculist, minTime, maxTime) = self.getActionFCurves(fcurves, context.object, context.scene)
        if not fculist:
            return

        dt0 = maxTime-minTime
        for fcu in fculist:
            (name, mode) = fCurveIdentity(fcu)
            dy0 = fcu.evaluate(maxTime) - fcu.evaluate(minTime)
            points = []
            for kp in fcu.keyframe_points:
                t = kp.co[0]
                if t >= minTime and t < maxTime:
                    points.append((t, kp.co[1]))
            for n in range(1, self.repeatNumber):
                dt = n*dt0
                dy = n*dy0
                for (t,y) in points:
                    fcu.keyframe_points.insert(t+dt, y+dy, options={'FAST'})

        raise MocapMessage("F-curves repeated %d times" % self.repeatNumber)


#
#   stitchActions(context):
#

def getActionItems(self, context):
    return [(act.name, act.name, act.name) for act in bpy.data.actions]


class MCP_OT_StitchActions(BvhPropsOperator, IsArmature):
    bl_idname = "mcp.stitch_actions"
    bl_label = "Stitch Actions"
    bl_description = "Stitch two action together seamlessly"
    bl_options = {'UNDO'}

    blendRange : IntProperty(
        name="Blend Range",
        min=1,
        default=5)

    firstAction : EnumProperty(
        items = getActionItems,
        name = "First Action")

    secondAction : EnumProperty(
        items = getActionItems,
        name = "Second Action")

    firstEndFrame : IntProperty(
        name="First End Frame",
        default=1)

    secondStartFrame : IntProperty(
        name="Second Start Frame",
        default=1)

    actionTarget : EnumProperty(
        items = [('Stitch new', 'Stitch new', 'Stitch new'),
                 ('Prepend second', 'Prepend second', 'Prepend second')],
        name = "Action Target")

    outputActionName : StringProperty(
        name="Output Action Name",
        maxlen=24,
        default="Stitched")


    def run(self, context):
        from .retarget import getLocks, correctMatrixForLocks

        startProgress("Stitch actions")
        scn = context.scene
        rig = context.object
        act1 = bpy.data.actions[self.firstAction]
        act2 = bpy.data.actions[self.secondAction]
        fcurves1 = getActionFcurves(act1)
        fcurves2 = getActionFcurves(act2)
        frame1 = self.firstEndFrame
        frame2 = self.secondStartFrame
        delta = self.blendRange
        factor = 1.0/delta
        shift = frame1 - frame2 - delta

        if rig.animation_data:
            rig.animation_data.action = None

        first1,last1 = self.getActionExtent(fcurves1)
        first2,last2 = self.getActionExtent(fcurves2)
        frames1 = range(first1, frame1)
        frames2 = range(frame2, last2+1)
        frames = range(first1, last2+shift+1)
        bmats1,_ = getBaseMatrices(fcurves1, frames1, rig, True)
        bmats2,useLoc = getBaseMatrices(fcurves2, frames2, rig, True)

        deletes = []
        for bname in bmats2.keys():
            try:
                bmats1[bname]
            except KeyError:
                deletes.append(bname)
        for bname in deletes:
            del bmats2[bname]

        orders = {}
        locks = {}
        for bname in bmats2.keys():
            pb = rig.pose.bones[bname]
            orders[bname],locks[bname] = getLocks(pb, context)

        nFrames = len(frames)
        for n,frame in enumerate(frames):
            setFrame(scn, frame)
            showProgress(n, frame, nFrames)

            if frame <= frame1-delta:
                n1 = frame - first1
                for bname,mats in bmats1.items():
                    pb = rig.pose.bones[bname]
                    mat = mats[n1]
                    if useLoc[bname]:
                        insertLocation(pb, mat)
                    insertRotation(pb, mat)

            elif frame >= frame1:
                n2 = frame - frame1
                for bname,mats in bmats2.items():
                    pb = rig.pose.bones[bname]
                    mat = mats[n2]
                    if useLoc[bname]:
                        insertLocation(pb, mat)
                    insertRotation(pb, mat)

            else:
                n1 = frame - first1
                n2 = frame - frame1 + delta
                eps = factor*n2
                for bname,mats2 in bmats2.items():
                    pb = rig.pose.bones[bname]
                    mats1 = bmats1[bname]
                    mat1 = mats1[n1]
                    mat2 = mats2[n2]
                    mat = (1-eps)*mat1 + eps*mat2
                    mat = correctMatrixForLocks(mat, orders[bname], locks[bname], pb)
                    if useLoc[bname]:
                        insertLocation(pb, mat)
                    insertRotation(pb, mat)

        setInterpolation(rig)
        act = rig.animation_data.action
        act.name = self.outputActionName
        raise MocapMessage("Actions stitched")


    def getActionExtent(self, fcurves):
        first = 10000
        last = -10000
        for fcu in fcurves:
            t0 = int(fcu.keyframe_points[0].co[0])
            t1 = int(fcu.keyframe_points[-1].co[0])
            if t0 < first:
                first = t0
            if t1 > last:
                last = t1
        return first,last


#
#   shiftBoneFCurves(context, rig):
#   class MCP_OT_ShiftBoneFCurves(HideOperator):
#

def getBaseMatrices(fcurves, frames, rig, useAll):
    locFcurves = {}
    quatFcurves = {}
    eulerFcurves = {}
    for fcu in fcurves:
        (bname, mode) = fCurveIdentity(fcu)
        if bname in rig.pose.bones.keys():
            pb = rig.pose.bones[bname]
        else:
            continue
        if useAll or P2B(pb).select:
            if mode == "location":
                try:
                    fcurves = locFcurves[bname]
                except KeyError:
                    fcurves = locFcurves[bname] = [None,None,None]
            elif mode == "rotation_euler":
                try:
                    fcurves = eulerFcurves[bname]
                except KeyError:
                    fcurves = eulerFcurves[bname] = [None,None,None]
            elif mode == "rotation_quaternion":
                try:
                    fcurves = quatFcurves[bname]
                except KeyError:
                    fcurves = quatFcurves[bname] = [None,None,None,None]
            else:
                continue

            fcurves[fcu.array_index] = fcu

    basemats = {}
    useLoc = {}
    for bname,fcurves in eulerFcurves.items():
        useLoc[bname] = False
        order = rig.pose.bones[bname].rotation_mode
        fcu0,fcu1,fcu2 = fcurves
        rmats = basemats[bname] = []
        for frame in frames:
            x = evalFcurve(fcu0, frame)
            y = evalFcurve(fcu1, frame)
            z = evalFcurve(fcu2, frame)
            euler = Euler((x,y,z), order)
            rmats.append(euler.to_matrix().to_4x4())

    for bname,fcurves in quatFcurves.items():
        useLoc[bname] = False
        fcu0,fcu1,fcu2,fcu3 = fcurves
        rmats = basemats[bname] = []
        for frame in frames:
            w = evalFcurve(fcu0, frame)
            x = evalFcurve(fcu1, frame)
            y = evalFcurve(fcu2, frame)
            z = evalFcurve(fcu3, frame)
            quat = Quaternion((w,x,y,z))
            rmats.append(quat.to_matrix().to_4x4())

    for bname,fcurves in locFcurves.items():
        useLoc[bname] = True
        fcu0,fcu1,fcu2 = fcurves
        tmats = []
        for frame in frames:
            x = evalFcurve(fcu0, frame)
            y = evalFcurve(fcu1, frame)
            z = evalFcurve(fcu2, frame)
            tmats.append(Matrix.Translation((x,y,z)))
        try:
            rmats = basemats[bname]
        except KeyError:
            basemats[bname] = tmats
            rmats = None
        if rmats:
            mats = []
            for n,rmat in enumerate(rmats):
                tmat = tmats[n]
                mats.append( tmat @ rmat )
            basemats[bname] = mats

    return basemats, useLoc


def printmat(mat):
    print("   (%.4f %.4f %.4f %.4f)" % tuple(mat.to_quaternion()))


class MCP_OT_ShiftBoneFCurves(HideOperator, IsArmature):
    bl_idname = "mcp.shift_animation"
    bl_label = "Shift Animation"
    bl_description = "Shift the animation globally for selected boens"
    bl_options = {'UNDO'}

    def run(self, context):
        from .retarget import getLocks, correctMatrixForLocks

        startProgress("Shift animation")
        scn = context.scene
        rig = context.object
        frames = [scn.frame_current] + getActiveFrames(rig)
        nFrames = len(frames)
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        basemats, useLoc = getBaseMatrices(fcurves, frames, rig, False)

        deltaMat = {}
        orders = {}
        locks = {}
        for bname,bmats in basemats.items():
            pb = rig.pose.bones[bname]
            bmat = bmats[0]
            deltaMat[pb.name] = pb.matrix_basis @ bmat.inverted()
            orders[pb.name], locks[pb.name] = getLocks(pb, context)

        for n,frame in enumerate(frames[1:]):
            setFrame(scn, frame)
            showProgress(n, frame, nFrames)
            for bname,bmats in basemats.items():
                pb = rig.pose.bones[bname]
                mat = deltaMat[pb.name] @ bmats[n+1]
                mat = correctMatrixForLocks(mat, orders[bname], locks[bname], pb)
                if useLoc[bname]:
                    insertLocation(pb, mat)
                insertRotation(pb, mat)

        raise MocapMessage("Animation shifted")

#----------------------------------------------------------
#   Clear bones
#----------------------------------------------------------

class MCP_OT_ClearBones(HideOperator, IsArmature):
    bl_idname = "mcp.clear_bones"
    bl_label = "Clear Bones"
    bl_description = "For selected bones, clear pose at current frame\nand shift pose at other frames"
    bl_options = {'UNDO'}

    def run(self, context):
        from .retarget import getLocks, correctMatrixForLocks

        startProgress("Clear bones")
        scn = context.scene
        rig = context.object
        pbones = [pb for pb in rig.pose.bones if P2B(pb).select]
        invmats = dict([(pb.name, pb.matrix_basis.inverted()) for pb in pbones])
        orders = {}
        locks = {}
        children = []
        for pb in rig.pose.bones:
            orders[pb.name], locks[pb.name] = getLocks(pb, context)
            par = rig.pose.bones.get(mcpRna(pb).Parent)
            if par and par in pbones:
                children.append((pb, par, pb.bone.matrix_local, par.bone.matrix_local))
        frames = getActiveFrames(rig)
        if len(frames) == 0:
            frames = [scn.frame_current]
        nFrames = len(frames)
        for n,frame in enumerate(frames):
            setFrame(scn, frame)
            showProgress(n, frame, nFrames)
            updateScene(context)
            cmats = [(pb, pb.matrix_basis.copy(), par.matrix_basis.copy(), R1, R0)
                for pb,par,R1,R0 in children]
            for pb in pbones:
                mat = invmats[pb.name] @ pb.matrix_basis
                mat = rotationMatrix(mat)
                pb.matrix_basis = mat
                insertRotation(pb, mat)
            for pb,L1,L0,R1,R0 in cmats:
                mat = R1.inverted() @ R0 @ L0 @ R0.inverted() @ R1 @ L1
                mat = rotationMatrix(mat)
                mat = correctMatrixForLocks(mat, orders[pb.name], locks[pb.name], pb)
                pb.matrix_basis = mat
                insertRotation(pb, mat)

#----------------------------------------------------------
#   G8 Fix
#----------------------------------------------------------

class MCP_OT_FixGenesis38(HideOperator, IsArmature):
    bl_idname = "mcp.fix_genesis38"
    bl_label = "Fix Genesis 3,8"
    bl_description = "Move animation from bend to twist bones for Genesis 3 and 8"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        rig = context.object
        return (rig and
                rig.type == 'ARMATURE' and
                "lThighBend" in rig.pose.bones.keys() and
                rig.animation_data and
                rig.animation_data.action)

    def run(self, context):
        rig = context.object
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        bnames = ["Shldr", "Forearm", "Thigh"]
        bendnames = ["%s%sBend" % (prefix, bname) for prefix in ["l", "r"] for bname in bnames]
        twistnames = ["%s%sTwist" % (prefix, bname) for prefix in ["l", "r"] for bname in bnames]
        bendpaths = ['pose.bones["%s"].rotation_euler' % bend for bend in bendnames]
        twistpaths = ['pose.bones["%s"].rotation_euler' % twist for twist in twistnames]
        twistcurves = {}
        for fcu in list(fcurves):
            if fcu.data_path in twistpaths:
                if (fcu.array_index == 1 and
                    len(fcu.keyframe_points) > 1):
                    bname = fcu.data_path.split('"')[1]
                    bname = bname[:-5]
                    twistcurves[bname] = fcu
                else:
                    fcurves.remove(fcu)
        for bname in twistnames:
            pb = rig.pose.bones.get(bname)
            if pb:
                pb.rotation_euler = (0,0,0)
        for fcu in list(fcurves):
            if (fcu.data_path in bendpaths and
                fcu.array_index == 1):
                bname = fcu.data_path.split('"')[1]
                bname = bname[:-4]
                fcu1 = twistcurves.get(bname)
                if fcu1:
                    fcurves.remove(fcu)
                else:
                    fcu.data_path = fcu.data_path.replace("Bend", "Twist")
        for bname in bendnames:
            pb = rig.pose.bones.get(bname)
            if pb:
                pb.rotation_euler[1] = 0

#----------------------------------------------------------
#   Fixate Bone Location
#----------------------------------------------------------

class MCP_OT_FixateBoneFCurves(HidePropsOperator, FrameRange, IsArmature):
    bl_idname = "mcp.fixate_bone"
    bl_label = "Fixate Bone Location"
    bl_description = "Keep bone location fixed (local coordinates)"
    bl_options = {'UNDO'}

    fixX : BoolProperty(
        name="X",
        description="Fix Local X Location",
        default=True)

    fixY : BoolProperty(
        name="Y",
        description="Fix Local Y Location",
        default=True)

    fixZ : BoolProperty(
        name="Z",
        description="Fix Local Z Location",
        default=True)

    def draw(self, context):
        row = self.layout.row()
        row.prop(self, "fixX")
        row.prop(self, "fixY")
        row.prop(self, "fixZ")
        FrameRange.draw(self, context)


    def run(self, context):
        startProgress("Fixate bone locations")
        rig = context.object
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        scn = context.scene
        frame = scn.frame_current
        startFrame,endFrame = self.getStartEndFrame()

        fixArray = [False,False,False]
        if self.fixX:
            fixArray[0] = True
        if self.fixY:
            fixArray[1] = True
        if self.fixZ:
            fixArray[2] = True

        for fcu in fcurves:
            (bname, mode) = fCurveIdentity(fcu)
            pb = rig.pose.bones[bname]
            if P2B(pb).select and isLocation(mode) and fixArray[fcu.array_index]:
                value = fcu.evaluate(frame)
                for kp in fcu.keyframe_points:
                    if kp.co[0] >= startFrame and kp.co[0] <= endFrame:
                        kp.co[1] = value
        raise MocapMessage("Bone locations fixated")

#----------------------------------------------------------
#   Transfer to peers
#----------------------------------------------------------

class MCP_OT_TransferToPeers(HidePropsOperator, FrameRange, IsArmature):
    bl_idname = "mcp.transfer_to_peers"
    bl_label = "Transfer To Peers"
    bl_description = "Transfer bone location to other bones with same parent"
    bl_options = {'UNDO'}

    def run(self, context):
        startProgress("Transfer to peers")
        rig = context.object
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        startFrame,endFrame = self.getStartEndFrame()
        bpy.ops.object.mode_set(mode='POSE')
        active = context.active_pose_bone
        par = active.parent
        roots = [pb.name for pb in rig.pose.bones if pb.parent == par]
        rmat = active.bone.matrix_local
        diffs = [Vector((0,0,0)) for frame in range(startFrame, endFrame+1)]
        for fcu in fcurves:
            (bname, channel) = fCurveIdentity(fcu)
            if bname == active.name and channel == "location":
                for frame in range(startFrame, endFrame+1):
                    n = frame-startFrame
                    diffs[n][fcu.array_index] = fcu.evaluate(frame)
        for fcu in fcurves:
            (bname, channel) = fCurveIdentity(fcu)
            if bname in roots and channel == "location":
                bone = rig.data.bones[bname]
                for kp in fcu.keyframe_points:
                    frame = int(kp.co[0])
                    if frame in range(startFrame, endFrame+1):
                        n = frame-startFrame
                        vec = active.bone.matrix_local.inverted() @ diffs[n] @ bone.matrix_local
                        kp.co[1] -= vec[fcu.array_index]
        setInterpolation(rig)

#----------------------------------------------------------
#   Center animation
#----------------------------------------------------------

class MCP_OT_CenterAnimation(BvhOperator, IsArmature):
    bl_idname = "mcp.center_animation"
    bl_label = "Center Animation"
    bl_description = "Center hip at frame 0"
    bl_options = {'UNDO'}

    def run(self, context):
        rig = context.object
        act = rig.animation_data.action
        centerAnimation(context, rig, act)
        setInterpolation(rig)


def centerAnimation(context, rig, act):
    def getHipChannel(fcu, hip):
        words = fcu.data_path.split('"',2)
        if len(words) == 3 and words[1] == hip.name:
            return words[2].rsplit(".",1)[-1]
        else:
            return None

    def findTimes(fcurves, hip):
        tmin = 100000
        tmax = -100000
        for fcu in fcurves:
            channel = getHipChannel(fcu, hip)
            if channel:
                times = [kp.co[0] for kp in fcu.keyframe_points]
                t0 = int(min(times))
                t1 = int(max(times))
                if t0 < tmin:
                    tmin = t0
                if t1 > tmax:
                    tmax = t1
        return tmin,tmax

    fcurves = getActionFcurves(act)
    hip = getTrgBone("hips", rig)
    if hip is None:
        from .target import findTargetArmature
        scn = context.scene
        mcpRna(scn).TargetRig = "Automatic"
        findTargetArmature(context, rig, True)
        hip = getTrgBone("hips", rig)
    if hip is None:
        raise MocapError("No hip bone found")
    tmin,tmax = findTimes(fcurves, hip)
    if tmin > tmax:
        raise MocapError("No frames found")
    nframes = tmax-tmin+1
    locs = [Vector((0,0,0)) for n in range(nframes)]
    fcustruct = {0: None, 1: None, 2: None}
    hippath = None
    for fcu in fcurves:
        channel = getHipChannel(fcu, hip)
        if channel == "location":
            fcustruct[fcu.array_index] = fcu
            hippath = fcu.data_path
            for n in range(nframes):
                locs[n][fcu.array_index] = fcu.evaluate(tmin+n)
    if hippath == None:
        hip.location = (0.0, 0.0, 0.0)
        return

    for fcu in fcustruct.values():
        if fcu:
            fcurves.remove(fcu)
    nfcustruct = {}
    for idx in range(3):
        nfcu = fcurves.new(hippath, index=idx)
        nfcu.keyframe_points.add(count=nframes)
        nfcustruct[idx] = nfcu
    loc0 = locs[0]
    nlocs = [loc - loc0 for loc in locs]
    for n,loc in enumerate(locs):
        nloc = loc - loc0
        for idx in range(3):
            nfcu = nfcustruct[idx]
            nfcu.keyframe_points[n].co = (tmin+n, nloc[idx])

#----------------------------------------------------------
#   Get active frames
#----------------------------------------------------------

def getActiveFrames(ob, minTime=None, maxTime=None, action=None):
    if action:
        fcurves = getActionFcurves(action)
    else:
        fcurves = getRnaFcurves(ob)
    if fcurves:
        active = set()
        for fcu in fcurves:
            for kp in fcu.keyframe_points:
                active.add(int(kp.co[0]))
    else:
        return []

    frames = list(active)
    frames.sort()
    if minTime is not None:
        while frames[0] < minTime:
            frames = frames[1:]
    if maxTime is not None:
        frames.reverse()
        while frames[0] > maxTime:
            frames = frames[1:]
        frames.reverse()
    return frames


def getMarkedTime(scn):
    markers = []
    for mrk in scn.timeline_markers:
        if mrk.select:
            markers.append(mrk.frame)
    markers.sort()
    if len(markers) >= 2:
        return (markers[0], markers[-1])
    else:
        return (None, None)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_LoopFCurves,
    MCP_OT_RepeatFCurves,
    MCP_OT_StitchActions,
    MCP_OT_ShiftBoneFCurves,
    MCP_OT_ClearBones,
    MCP_OT_FixGenesis38,
    MCP_OT_FixateBoneFCurves,
    MCP_OT_TransferToPeers,
    MCP_OT_CenterAnimation,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
