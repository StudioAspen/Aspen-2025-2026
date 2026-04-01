# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import os
import sys
import bpy
from bpy.props import *

#----------------------------------------------------------
#   Propgroups
#----------------------------------------------------------

def mcpRna(rna):
    return rna.bvh_retargeter

def getTPoses(scn, context):
    return BD.tposeEnums

def getSources(scn, context):
    return BD.sourceEnums

def getTargets(scn, context):
    return BD.targetEnums


class RetargeterSceneSettings(bpy.types.PropertyGroup):
    SourceTPose : EnumProperty(
        items = getTPoses,
        name = "TPose Source")

    TargetTPose : EnumProperty(
        items = getTPoses,
        name = "TPose Target")

    SourceRig : EnumProperty(
        items = getSources,
        name = "Source rig")

    TargetRig : EnumProperty(
        items = getTargets,
        name = "Target rig")


class McpAnimLayer(bpy.types.PropertyGroup):
    action : PointerProperty(type = bpy.types.Action)
    index : IntProperty()


class RetargeterObjectSettings(bpy.types.PropertyGroup):
    Armature : StringProperty()

    ReverseHip : BoolProperty(
        name = "Reverse Hip",
        description = "The rig has a reverse hip",
        default = False)

    AnimLayers : CollectionProperty(type = McpAnimLayer)
    AnimLayerIndex : IntProperty()
    ActionName : StringProperty()
    Renamed : BoolProperty(default = False)
    TPoseDefined : BoolProperty(default = False)
    TPoseFile : StringProperty(default = "")
    ArmatureName : StringProperty(default = "")
    ArmatureModifier : StringProperty(default = "")
    IsSourceRig : BoolProperty(default=False)


class RetargeterPoseBoneSettings(bpy.types.PropertyGroup):
    Bone : StringProperty(
        name = "Canonical Bone Name",
        description = "Canonical bone corresponding to this bone",
        default = "")

    Parent : StringProperty(
        name = "Parent",
        description = "Parent of this bone for retargeting purposes",
        default = "")

    Quat : FloatVectorProperty(size=4, default=(1,0,0,0))

#----------------------------------------------------------
#   Preferences
#----------------------------------------------------------

class BvhData:
    def __init__(self):
        # Global variables
        self.prefs = None
        self.sourceInfos = {}
        self.activeSrcInfo = None
        self.targetInfos = {}
        self.tposeInfos = {}
        self.facsTables = {}
        self.orientation = {}

        self.tposeEnums = [("Default", "Default", "Default")]
        self.sourceEnums = [("Automatic", "Automatic", "Automatic")]
        self.targetEnums = [("Automatic", "Automatic", "Automatic")]

        self.markers = []
        self.editLoc = None
        self.editRot = None


    def readJsonFiles(self, scn, cinfo, infos, subdir, name):
        keys = []
        folder = os.path.join(os.path.dirname(__file__), subdir)
        for fname in os.listdir(folder):
            filepath = os.path.join(folder, fname)
            if os.path.splitext(fname)[-1] == ".json":
                info = cinfo(scn, name)
                info.readFile(filepath)
                infos[info.name] = info
                keys.append(info.name)
        keys.sort()
        return keys


    def initData(self):
        from .io_json import loadJson
        filepath =  os.path.join(os.path.dirname(__file__), "data", "orientation.json")
        struct = loadJson(filepath)
        self.orientation = struct["orientation"]


    def initTPoses(self, scn):
        from .t_pose import CTPoseInfo
        self.tposeInfos = { "Default" : CTPoseInfo(scn, "Default") }
        keys = self.readJsonFiles(scn, CTPoseInfo, self.tposeInfos, "t_poses", "")
        self.tposeEnums = [(key,key,key) for key in ["Default"] + keys]
        mcpRna(scn).SourceTPose = 'Default'
        mcpRna(scn).TargetTPose = 'Default'
        print("T-poses initialized")


    def initSources(self, scn):
        from .source import CSourceInfo
        self.initTPoses(scn)
        self.sourceInfos = { "Automatic" : CSourceInfo(scn, "Automatic") }
        keys = self.readJsonFiles(scn, CSourceInfo, self.sourceInfos, "known_rigs", "")
        self.sourceEnums = [(key,key,key) for key in ["Automatic"] + keys]
        mcpRna(scn).SourceRig = 'Automatic'
        print("Defined SourceRig")


    def initTargets(self, scn):
        from .target import CTargetInfo
        self.initTPoses(scn)
        self.targetInfos = { "Automatic" : CTargetInfo(scn, "Automatic") }
        keys = self.readJsonFiles(scn, CTargetInfo, self.targetInfos, "known_rigs", "Manual")
        self.targetEnums = [(key,key,key) for key in ["Automatic"] + keys]
        print("Defined TargetRig")


    def ensureDataInited(self):
        if not self.orientation:
            self.initData()


    def ensureSourceInited(self, scn):
        if not self.sourceInfos:
            self.initSources(scn)


    def ensureTargetInited(self, scn):
        if not self.targetInfos:
            self.initTargets(scn)


    def ensureInited(self, scn):
        self.ensureSourceInited(scn)
        self.ensureTargetInited(scn)


    def ensureFacsInited(self):
        if self.facsTables:
            return
        from .io_json import loadJson
        folder = os.path.join(os.path.dirname(__file__), "facs")
        for fname in os.listdir(folder):
            filepath = os.path.join(folder, fname)
            if os.path.splitext(fname)[-1] == ".json":
                struct = loadJson(filepath)
                self.facsTables[struct["fingerprint"]] = struct
                print("FACS %s %s" % (struct["name"], struct["fingerprint"]))


    BoneNames = [
        (None,           None),
        ("hips",         "Root bone"),
        ("spine",        "Lower spine"),
        ("spine-1",      "Lower spine 2"),
        ("chest",        "Upper spine"),
        ("chest-1",      "Upper spine 2"),
        ("neck",         "Neck"),
        ("head",         "Head"),
        ("",             ""),
        ("shoulder.L",   "L shoulder"),
        ("upper_arm.L",  "L upper arm"),
        ("upper_arm_twist.L",  "L upper arm twist"),
        ("forearm.L",    "L forearm"),
        ("forearm_twist.L",    "L forearm twist"),
        ("hand.L",       "L hand"),
        ("",             ""),
        ("shoulder.R",   "R shoulder"),
        ("upper_arm.R",  "R upper arm"),
        ("upper_arm_twist.R",  "R upper arm twist"),
        ("forearm.R",    "R forearm"),
        ("forearm_twist.R",    "R forearm twist"),
        ("hand.R",       "R hand"),

        (None,           None),
        ("hip.L",        "L hip"),
        ("thigh.L",      "L thigh"),
        ("thigh_twist.L",      "L thigh twist"),
        ("shin.L",       "L shin"),
        ("foot.L",       "L foot"),
        ("toe.L",        "L toes"),
        ("",             ""),
        ("hip.R",        "R hip"),
        ("thigh.R",      "R thigh"),
        ("thigh_twist.R",      "R thigh twist"),
        ("shin.R",       "R shin"),
        ("foot.R",       "R foot"),
        ("toe.R",        "R toes"),

        (None,           None),
        ("f_thumb.01.L",   "L thumb 1"),
        ("f_thumb.02.L",   "L thumb 2"),
        ("f_thumb.03.L",   "L thumb 3"),
        ("f_index.01.L",   "L index 1"),
        ("f_index.02.L",   "L index 2"),
        ("f_index.03.L",   "L index 3"),
        ("f_middle.01.L",   "L middle 1"),
        ("f_middle.02.L",   "L middle 2"),
        ("f_middle.03.L",   "L middle 3"),
        ("f_ring.01.L",   "L ring 1"),
        ("f_ring.02.L",   "L ring 2"),
        ("f_ring.03.L",   "L ring 3"),
        ("f_pinky.01.L",   "L pinky 1"),
        ("f_pinky.02.L",   "L pinky 2"),
        ("f_pinky.03.L",   "L pinky 3"),

        (None,           None),
        ("f_thumb.01.R",   "R thumb 1"),
        ("f_thumb.02.R",   "R thumb 2"),
        ("f_thumb.03.R",   "R thumb 3"),
        ("f_index.01.R",   "R index 1"),
        ("f_index.02.R",   "R index 2"),
        ("f_index.03.R",   "R index 3"),
        ("f_middle.01.R",   "R middle 1"),
        ("f_middle.02.R",   "R middle 2"),
        ("f_middle.03.R",   "R middle 3"),
        ("f_ring.01.R",   "R ring 1"),
        ("f_ring.02.R",   "R ring 2"),
        ("f_ring.03.R",   "R ring 3"),
        ("f_pinky.01.R",   "R pinky 1"),
        ("f_pinky.02.R",   "R pinky 2"),
        ("f_pinky.03.R",   "R pinky 3"),
    ]

    def getMcpBones(self, rig):
        return dict([(mcpRna(pb).Bone,pb) for pb in rig.pose.bones if mcpRna(pb).Bone])


    def sortBones(self, rig):
        mcpbones = self.getMcpBones(rig)
        bnames = [bname for bname,longname in self.BoneNames if bname]
        bones = [mcpbones[bname] for bname in bnames if bname in mcpbones.keys()]
        return bones


BD = BvhData()

def BS():
    return BD.prefs

#-------------------------------------------------------------
#   Initialize
#-------------------------------------------------------------

classes = [
    McpAnimLayer,
    RetargeterPoseBoneSettings,
    RetargeterObjectSettings,
    RetargeterSceneSettings
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)

    bpy.types.PoseBone.bvh_retargeter = PointerProperty(type=RetargeterPoseBoneSettings)
    bpy.types.Object.bvh_retargeter = PointerProperty(type=RetargeterObjectSettings)
    bpy.types.Scene.bvh_retargeter = PointerProperty(type=RetargeterSceneSettings)

def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)

