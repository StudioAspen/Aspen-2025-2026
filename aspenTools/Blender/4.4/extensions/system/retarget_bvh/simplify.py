# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from math import pi
from .utils import *

#
#    Simplifier
#

class FCurvesGetter:

    useVisible : BoolProperty(
        name="Only Visible F-Curves",
        description="Only visible F-curves",
        default=False)

    useBones : BoolProperty(
        name="Bone F-Curves",
        description = "Include bone F-curves",
        default = True)

    useKeys : BoolProperty(
        name = "Key F-Curves",
        description = "Include key F-curves",
        default = True)

    useSelected : BoolProperty(
        name="Only Selected Bones",
        description="Only F-curves for selected bones",
        default=False)

    useMarkers : BoolProperty(
        name="Only Between Markers",
        description="Only between markers",
        default=False)

    def draw(self, context):
        self.layout.prop(self, "useVisible")
        self.layout.prop(self, "useBones")
        if self.useBones:
            self.layout.prop(self, "useSelected")
        self.layout.prop(self, "useKeys")
        self.layout.prop(self, "useMarkers")


    def getActionFCurves(self, fcurves, rig, scn):
        from .loop import getMarkedTime

        if self.useVisible:
            fculist = [fcu for fcu in fcurves if not fcu.hide]
        else:
            fculist = list(fcurves)

        bonecurves = [fcu for fcu in fculist if fcu.data_path.split('"')[0] == "pose.bones["]
        keycurves = [fcu for fcu in fculist if fcu.data_path[0:2] == '["']
        fculist = []
        if self.useBones:
            if self.useSelected:
                for fcu in bonecurves:
                    bname = fcu.data_path.split('"')[1]
                    pb = rig.pose.bones[bname]
                    if P2B(pb).select:
                        fculist.append(fcu)
            fculist = bonecurves
        if self.useKeys:
            fculist = fculist + keycurves

        if self.useMarkers:
            (minTime, maxTime) = getMarkedTime(scn)
            if minTime == None:
                raise MocapError("Need two selected markers")
                return ([], 0, 0)
        else:
            (minTime, maxTime) = (-10000,10000)
        return (fculist, minTime, maxTime)


class Simplifier(FCurvesGetter):

    useSimplify : BoolProperty(
        name="Simplify F-Curves",
        description="Simplify F-curves",
        default=False)

    maxErrLoc : FloatProperty(
        name="Max Loc Error",
        description="Max error for location FCurves when doing simplification",
        min=0.001,
        default=0.01)

    maxErrRot : FloatProperty(
        name="Max Rot Error",
        description="Max error for rotation (degrees) FCurves when doing simplification",
        min=0.001,
        default=0.1)

    maxErrScale : FloatProperty(
        name="Max Scale Error",
        description="Max error for scale FCurves when doing simplification",
        min=0.001,
        default=0.01)

    maxErrKey : FloatProperty(
        name="Max Key Error",
        description="Max error for key FCurves when doing simplification",
        min=1e-4,
        default=0.01)

    def draw(self, context):
        self.layout.prop(self, "useSimplify")
        if self.useSimplify:
            self.drawSimplify(context)

    def drawSimplify(self, context):
        FCurvesGetter.draw(self, context)
        self.layout.prop(self, "maxErrLoc")
        self.layout.prop(self, "maxErrRot")
        self.layout.prop(self, "maxErrScale")
        self.layout.prop(self, "maxErrKey")


    def simplifyFCurves(self, context, rig):
        scn = context.scene
        fcurves = getRnaFcurves(rig)
        if not fcurves:
            return
        (fculist, minTime, maxTime) = self.getActionFCurves(fcurves, rig, scn)
        if not fculist:
            return
        for fcu in fculist:
            self.simplifyFCurve(fcu, rig.animation_data.action, minTime, maxTime)
        setInterpolation(rig)
        print("F-curves simplified")


    def splitFCurvePoints(self, fcu, minTime, maxTime):
        if minTime == 'All':
            points = fcu.keyframe_points
            before = []
            after = []
        else:
            points = []
            before = []
            after = []
            for pt in fcu.keyframe_points:
                t = pt.co[0]
                if t < minTime:
                    before.append(pt.co)
                elif t > maxTime:
                    after.append(pt.co)
                else:
                    points.append(pt)
        return (points, before, after)


    def simplifyFCurve(self, fcu, act, minTime, maxTime):
        words = fcu.data_path.split(".")
        if words[-1] == "location":
            maxErr = self.maxErrLoc
        elif words[-1] == "rotation_quaternion":
            maxErr = self.maxErrRot * 1.0/180
        elif words[-1] == "rotation_euler":
            maxErr = self.maxErrRot * pi/180
        elif words[-1] == "scale":
            maxErr = self.maxErrScale
        elif words[-1][0:2] == '["':
            maxErr = self.maxErrKey
        else:
            print("Unknown FCurve type %s" % words[-1])
            return

        (points, before, after) = self.splitFCurvePoints(fcu, minTime, maxTime)

        nPoints = len(points)
        nBefore = len(before)
        nAfter = len(after)
        if nPoints <= 2:
            return
        keeps = []
        new = [0, nPoints-1]
        while new:
            keeps += new
            keeps.sort()
            new = self.iterateFCurves(points, keeps, maxErr)
        newVerts = []
        for n in keeps:
            newVerts.append(points[n].co.copy())
        nNewPoints = len(newVerts)

        oldOffset = nBefore+nPoints
        newOffset = nBefore+nNewPoints
        for n in range(nAfter):
            fcu.keyframe_points[n+newOffset].co = fcu.keyframe_points[n+oldOffset].co.copy()
        n = nBefore+nPoints+nAfter
        n1 = nBefore+nNewPoints+nAfter
        while n > n1:
            n -= 1
            kp = fcu.keyframe_points[n]
            fcu.keyframe_points.remove(kp)
        for n in range(nNewPoints):
            fcu.keyframe_points[n+nBefore].co = newVerts[n]


    def iterateFCurves(self, points, keeps, maxErr):
        new = []
        for edge in range(len(keeps)-1):
            n0 = keeps[edge]
            n1 = keeps[edge+1]
            (x0, y0) = points[n0].co
            (x1, y1) = points[n1].co
            if x1 > x0:
                dxdn = (x1-x0)/(n1-n0)
                dydx = (y1-y0)/(x1-x0)
                err = 0
                for n in range(n0+1, n1):
                    (x, y) = points[n].co
                    xn = n0 + dxdn*(n-n0)
                    yn = y0 + dydx*(xn-x0)
                    if abs(y-yn) > err:
                        err = abs(y-yn)
                        worst = n
                if err > maxErr:
                    new.append(worst)
        return new

#
#   TimeScaler
#

class TimeScaler:
    useTimeScale : BoolProperty(
        name="Time-Scale F-Curves",
        description="Scale F-curves in time after loading",
        default=False)

    factor : FloatProperty(
        name="Time-Scale Factor",
        description="Factor for rescaling time",
        min=0.01, max=100, default=1.0)

    def draw(self, context):
        self.layout.prop(self, "useTimeScale")
        if self.useTimeScale:
            self.layout.prop(self, "factor")


    def timescaleFCurves(self, rig):
        fcurves = getRnaFcurves(rig)
        for fcu in fcurves:
            self.timescaleFCurve(fcu)
        print("F-curves time-scaled")

    def timescaleFCurve(self, fcu):
        n = len(fcu.keyframe_points)
        if n < 2:
            return
        (t0,v0) = fcu.keyframe_points[0].co
        (tn,vn) = fcu.keyframe_points[n-1].co
        limitData = getFCurveLimits(fcu)
        (mode, upper, lower, diff) = limitData

        tm = t0
        vm = v0
        inserts = []
        for pk in fcu.keyframe_points:
            (tk,vk) = pk.co
            tn = self.factor*(tk-t0) + t0
            if upper:
                if (vk > upper) and (vm < lower):
                    inserts.append((tm, vm, tn, vk))
                elif (vm > upper) and (vk < lower):
                    inserts.append((tm, vm, tn,vk))
            pk.co = (tn,vk)
            tm = tn
            vm = vk

        addFCurveInserts(fcu, inserts, limitData)

#
#   getFCurveLimits(fcu):
#

def getFCurveLimits(fcu):
    words = fcu.data_path.split('.')
    mode = words[-1]
    if mode == 'rotation_euler':
        upper = 0.8*pi
        lower = -0.8*pi
        diff = pi
    elif mode == 'rotation_quaternion':
        upper = 0.8
        lower = -0.8
        diff = 2
    else:
        upper = 0
        lower = 0
        diff = 0
    #print(words[1], mode, upper, lower)
    return (mode, upper, lower, diff)

#
#   addFCurveInserts(fcu, inserts, limitData):
#

def addFCurveInserts(fcu, inserts, limitData):
    (mode, upper, lower, diff) = limitData
    for (tm,vm,tn,vn) in inserts:
        tp = int((tm+tn)/2 - 0.1)
        tq = tp + 1
        vp = (vm+vn)/2
        if vm > upper:
            vp += diff/2
            vq = vp - diff
        elif vm < lower:
            vp -= diff/2
            vq = vp + diff
        if tp > tm:
            fcu.keyframe_points.insert(frame=tp, value=vp)
        if tq < tn:
            fcu.keyframe_points.insert(frame=tq, value=vq)
    return


#-------------------------------------------------------------
#   class MCP_OT_SimplifyFCurves(BvhOperator):
#-------------------------------------------------------------

class MCP_OT_SimplifyFCurves(BvhPropsOperator, IsArmature, Simplifier):
    bl_idname = "mcp.simplify_fcurves"
    bl_label = "Simplify F-Curves"
    bl_description = "Simplify F-curves"
    bl_options = {'UNDO'}

    def draw(self, context):
        self.drawSimplify(context)

    def run(self, context):
        self.useSimplify = True
        self.simplifyFCurves(context, context.object)


class MCP_OT_TimescaleFCurves(BvhPropsOperator, IsArmature, TimeScaler):
    bl_idname = "mcp.timescale_fcurves"
    bl_label = "Time-Scale F-Curves"
    bl_description = "Scale F-curves in time"
    bl_options = {'UNDO'}

    def draw(self, context):
        self.layout.prop(self, "factor")

    def run(self, context):
        self.useTimeScale = True
        self.timescaleFCurves(context.object)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_SimplifyFCurves,
    MCP_OT_TimescaleFCurves,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
