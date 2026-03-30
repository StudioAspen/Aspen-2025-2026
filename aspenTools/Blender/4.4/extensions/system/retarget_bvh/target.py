# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from bpy.props import *
from bpy_extras.io_utils import ExportHelper
import math
import os

from .utils import *
from .armature import CArmature
from .source import CRigInfo

#----------------------------------------------------------
#   Target classes
#----------------------------------------------------------

class CTargetInfo(CArmature, CRigInfo):
    verboseString = "Read target file"

    def __init__(self, scn, name):
        CArmature.__init__(self, scn)
        CRigInfo.__init__(self, scn, name)


class Target:
    useAutoTarget : BoolProperty(
        name = "Auto Target",
        description = "Find target rig automatically",
        default = True)

    def draw(self, context):
        self.layout.prop(self, "useAutoTarget")
        if not self.useAutoTarget:
            scn = context.scene
            self.layout.prop(mcpRna(scn), "TargetRig")
            self.layout.prop(mcpRna(scn), "TargetTPose")

    def findTarget(self, context, rig):
        return findTargetArmature(context, rig, self.useAutoTarget)

#
#   findTargetArmature(context, rig, auto):
#

def findTargetArmature(context, rig, auto):
    from .t_pose import autoTPose

    scn = context.scene
    BD.ensureTargetInited(scn)

    if auto:
        mcpRna(scn).TargetRig, mcpRna(scn).TargetTPose = guessArmatureFromList(rig, scn, BD.targetInfos)

    if mcpRna(scn).TargetRig == "Automatic":
        info = CTargetInfo(scn, "Automatic")
        tposed = info.identifyRig(context, rig, mcpRna(scn).TargetTPose)
        if not tposed:
            autoTPose(context, rig)
        BD.targetInfos["Automatic"] = info
        info.display("Target")
    else:
        info = BD.targetInfos[mcpRna(scn).TargetRig]
        info.addManualBones(rig)
        tinfo = BD.tposeInfos.get(mcpRna(scn).TargetTPose)
        if tinfo:
            tinfo.addTPose(rig)
        else:
            mcpRna(scn).TargetTPose = "Default"

    mcpRna(rig).Armature = info.name
    print("Using target armature %s." % mcpRna(rig).Armature)
    return info


def guessArmatureFromList(rig, scn, infos):

    def matchAllBones(rig, info, scn):
        if not hasAllBones(info.fingerprint, rig):
            if BS().verbose:
                print(info.name, ": Fingerprint failed")
            return False
        if hasSomeBones(info.illegal, rig):
            if BS().verbose:
                print(info.name, ": Illegal bone")
            return False
        for bname,mhx in info.bones:
            pb = getCanonicalBone(rig, bname)
            if pb or canonicalName(bname) in info.optional:
                pass
            else:
                if BS().verbose:
                    print(info.name, ": Missing bone:", bname)
                return False
        return True

    print("Identifying rig")
    for name,info in infos.items():
        if name == "Automatic":
            continue
        elif matchAllBones(rig, info, scn):
            if info.t_pose_file:
                return name, info.t_pose_file
            else:
                return name, "Default"
    else:
        return "Automatic", "Default"


#-------------------------------------------------------------
#    Target initialization
#-------------------------------------------------------------

class MCP_OT_IdentifyTargetRig(BvhOperator, IsArmature):
    bl_idname = "mcp.identify_target_rig"
    bl_label = "Identify Target Rig"
    bl_description = "Identify the target rig type of the active armature."
    bl_options = {'UNDO'}

    def prequel(self, context):
        from .retarget import changeTargetData
        return changeTargetData(context.object, context.scene)

    def run(self, context):
        scn = context.scene
        mcpRna(scn).TargetRig = "Automatic"
        findTargetArmature(context, context.object, True)
        print("Identified rig %s" % mcpRna(scn).TargetRig)

    def sequel(self, context, data):
        from .retarget import restoreTargetData
        restoreTargetData(data)

#----------------------------------------------------------
#   List Rig
#----------------------------------------------------------

from .source import ListRig

class MCP_OT_ListTargetRig(BvhOperator, ListRig):
    bl_idname = "mcp.list_target_rig"
    bl_label = "List Target Rig"
    bl_description = "List the bone associations of the active target rig"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        return mcpRna(context.scene).TargetRig

    def getInfos(self, context):
        scn = context.scene
        info = BD.targetInfos.get(mcpRna(scn).TargetRig)
        tinfo = BD.tposeInfos.get(mcpRna(scn).TargetTPose)
        return info, tinfo


class MCP_OT_VerifyTargetRig(BvhOperator):
    bl_idname = "mcp.verify_target_rig"
    bl_label = "Verify Target Rig"
    bl_description = "Verify the target rig type of the active armature"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        ob = context.object
        return (mcpRna(context.scene).TargetRig and ob and ob.type == 'ARMATURE')

    def run(self, context):
        rigtype = mcpRna(context.scene).TargetRig
        info = BD.targetInfos[rigtype]
        info.testRig(rigtype, context.object, context.scene)
        raise MocapMessage("Target armature %s verified" % rigtype)

#-------------------------------------------------------------
#   Normalize Target Rig
#-------------------------------------------------------------

class MCP_OT_NormalizeTargetRig(BvhOperator, IsArmature):
    bl_idname = "mcp.normalize_target_rig"
    bl_label = "Normalize Target Rig"
    bl_description = "Change rotation mode to quaternion,\nand mute limit constraints"
    bl_options = {'UNDO'}

    def run(self, context):
        rig = context.object
        for pb in rig.pose.bones:
            pb.rotation_mode = 'QUATERNION'
            for cns in pb.constraints:
                if cns.type.startswith("LIMIT"):
                    cns.mute = True

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_IdentifyTargetRig,
    MCP_OT_ListTargetRig,
    MCP_OT_VerifyTargetRig,
    MCP_OT_NormalizeTargetRig,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
