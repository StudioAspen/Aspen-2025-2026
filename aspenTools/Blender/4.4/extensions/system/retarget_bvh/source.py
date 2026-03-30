# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
import os
from collections import OrderedDict
from math import pi
from mathutils import *
from bpy.props import *

from .armature import CArmature
from .utils import *

#----------------------------------------------------------
#   Source classes
#----------------------------------------------------------

class CRigInfo:
    def __init__(self, scn, name):
        self.name = name
        self.filepath = "None"
        self.bones = []
        self.boneNames = {}
        self.parents = {}
        self.optional = []
        self.leaves = {}
        self.fingerprint = []
        self.illegal = []
        self.t_pose = {}
        self.t_pose_file = None


    def readFile(self, filepath):
        from .io_json import loadJson
        if BS().verbose:
            print(self.verboseString, filepath)
        self.filepath = filepath
        struct = loadJson(filepath)
        if "name" in struct.keys():
            self.name = struct["name"]
        else:
            self.name = os.path.splitext(os.path.basename(filepath))[0]
        if "bones" in struct.keys():
            self.bones = [(bname, nameOrNone(value)) for bname,value in struct["bones"].items()]
            self.boneNames = dict([(canonicalName(bname), value) for bname,value in self.bones])
        if "parents" in struct.keys():
            self.parents = struct["parents"]
        if "optional" in struct.keys():
            self.optional = [canonicalName(bname) for bname in struct["optional"]]
        if "fingerprint" in struct.keys():
            self.fingerprint = struct["fingerprint"]
        if "illegal" in struct.keys():
            self.illegal = struct["illegal"]
        if "t-pose" in struct.keys():
            self.t_pose = struct["t-pose"]
        if "t-pose-file" in struct.keys():
            self.t_pose_file = struct["t-pose-file"]

        mhxbones = dict([(mhxname,bname) for bname,mhxname in self.bones])
        if BS().ignoreLeafBones:
            for (bname, mhxname) in self.bones:
                if ".03." in mhxname:
                    self.optional.append(canonicalName(bname))
                if ".02." in mhxname:
                    leaf = mhxname.replace(".02.", ".03.")
                    self.leaves[mhxname] = (leaf, mhxbones.get(leaf))


    def identifyRig(self, context, rig, tpose):
        from .t_pose import putInRightPose
        tposed = putInRightPose(context, rig, tpose)
        self.findArmature(rig)
        self.addAutoBones(rig)
        return tposed


    def getHip(self):
        for bname,mhx in self.boneNames.items():
            if mhx == "hips":
                return bname
        return None


    def clearMcpBones(self, rig):
        for pb in rig.pose.bones:
            mcpRna(pb).Bone = ""
            mcpRna(pb).Parent = ""


    def addAutoBones(self, rig):
        self.bones = []
        for pb in rig.pose.bones:
            if mcpRna(pb).Bone:
                self.bones.append( (pb.name, mcpRna(pb).Bone) )
        self.addParents(rig)
        mcpRna(rig).TPoseDefined = False


    def addManualBones(self, rig):
        for pb in rig.pose.bones:
            mcpRna(pb).Bone = ""
        for bname,mhx in self.bones:
            pb = getCanonicalBone(rig, bname)
            if pb:
                mcpRna(pb).Bone = mhx
            else:
                print("  Missing:", bname)
        mcpRna(rig).TPoseDefined = False
        self.addParents(rig)


    def addTPose(self, rig):
        for bname in self.t_pose.keys():
            pb = getCanonicalBone(rig, bname)
            if pb:
                rotmode = ('XYZ' if pb.rotation_mode in ('QUATERNION', 'AXIS_ANGLE') else pb.rotation_mode)
                euler = Euler(Vector(self.t_pose[bname])*D, rotmode)
                mcpRna(pb).Quat = euler.to_quaternion()
        mcpRna(rig).TPoseDefined = True


    def getParent(self, rig, bname, pname):
        par = getCanonicalBone(rig, pname)
        if par:
            return par.name
        elif pname in self.parents.keys():
            return self.getParent(rig, pname, self.parents[pname])
        else:
            return ""


    def addParents(self, rig):
        for pb in rig.pose.bones:
            if mcpRna(pb).Bone:
                mcpRna(pb).Parent = ""
                par = pb.parent
                while par:
                    if mcpRna(par).Bone:
                        mcpRna(pb).Parent = par.name
                        break
                    par = par.parent
        for bname,pname in self.parents.items():
            pb = getCanonicalBone(rig, bname)
            if pb:
                pname = self.getParent(rig, bname, pname)
                mcpRna(pb).Parent = pname

        if BS().verbose:
            print("Parents")
            for pb in rig.pose.bones:
                if mcpRna(pb).Bone:
                    print("  ", pb.name, mcpRna(pb).Parent)


    def testRig(self, name, rig, scn):
        from .armature import validBone
        if not self.bones:
            raise MocapError("Cannot verify after rig identification failed")
        print("Testing %s" % name)
        bname = hasSomeBones(self.illegal, rig)
        if bname:
            raise MocapError(
                    "Armature %s does not\n" % rig.name +
                    "match armature %s.\n" % name +
                    "Has illegal bone %s     " % bname)

        pbones = dict([(canonicalName(pb.name), pb) for pb in rig.pose.bones])
        for (bname, mhxname) in self.bones:
            if canonicalName(bname) in self.optional:
                continue
            pb = pbones.get(canonicalName(bname))
            if pb is None or not validBone(pb, mute=True):
                print("  Did not find bone %s (%s)" % (bname, mhxname))
                print("Bones:")
                for pair in self.bones:
                    print("  %s : %s" % pair)
                raise MocapError(
                    "Armature %s does not\n" % rig.name +
                    "match armature %s.\n" % name +
                    "Did not find bone %s     " % bname)


class CSourceInfo(CArmature, CRigInfo):
    verboseString = "Read source file"

    def __init__(self, scn, name):
        CArmature.__init__(self, scn)
        CRigInfo.__init__(self, scn, name)

#
#   findSourceArmature(context, rig, auto):
#

def findSourceArmature(context, rig, auto):
    from .t_pose import autoTPose, putInRestPose
    scn = context.scene

    BD.ensureSourceInited(scn)
    if auto:
        from .target import guessArmatureFromList
        mcpRna(scn).SourceRig, mcpRna(scn).SourceTPose = guessArmatureFromList(rig, scn, BD.sourceInfos)

    if mcpRna(scn).SourceRig == "Automatic":
        info = CSourceInfo(scn, "Automatic")
        tposed = info.identifyRig(context, rig, mcpRna(scn).SourceTPose)
        if not tposed:
            autoTPose(context, rig)
            mcpRna(scn).SourceTPose = "Default"
        BD.sourceInfos["Automatic"] = info
        BD.activeSrcInfo = info
        info.display("Source")
    else:
        info = BD.sourceInfos[mcpRna(scn).SourceRig]
        BD.activeSrcInfo = info
        info.addManualBones(rig)
        tinfo = BD.tposeInfos.get(mcpRna(scn).SourceTPose)
        if tinfo:
            tinfo.addTPose(rig)
        else:
            mcpRna(scn).SourceTPose = "Default"

    mcpRna(rig).Armature = BD.activeSrcInfo.name
    print("Using source armature %s." % mcpRna(rig).Armature)

#
#    setSourceArmature(rig, scn)
#

def setSourceArmature(rig, scn):
    name = mcpRna(rig).Armature
    if name:
        mcpRna(scn).SourceRig = name
    else:
        raise MocapError("No source armature set")
    BD.activeSrcInfo = BD.sourceInfos[name]
    print("Set source armature to %s" % name)


#----------------------------------------------------------
#   Class
#----------------------------------------------------------

class Source:
    useAutoSource : BoolProperty(
        name = "Auto Source",
        description = "Find source rig automatically",
        default = True)

    def draw(self, context):
        self.layout.prop(self, "useAutoSource")
        if not self.useAutoSource:
            scn = context.scene
            self.layout.prop(mcpRna(scn), "SourceRig")
            self.layout.prop(mcpRna(scn), "SourceTPose")

    def findSource(self, context, rig):
        return findSourceArmature(context, rig, self.useAutoSource)

#----------------------------------------------------------
#   Source initialization
#----------------------------------------------------------

class MCP_OT_InitKnownRigs(bpy.types.Operator):
    bl_idname = "mcp.init_known_rigs"
    bl_label = "Init Known Rigs"
    bl_description = "(Re)load all json files in the known_rigs directory."
    bl_options = {'UNDO'}

    def execute(self, context):
        BD.initSources(context.scene)
        BD.initTargets(context.scene)
        return{'FINISHED'}

#----------------------------------------------------------
#   List Rig
#
#   (mhx bone, text)
#----------------------------------------------------------

class ListRig:
    def draw(self, context):
        info,tinfo = self.getInfos(context)
        mcpbones = dict([(mcpname, bname) for bname,mcpname in info.bones])
        if not mcpbones:
            return
        bonelist = []
        for mcpname,longname in BD.BoneNames:
            if mcpname is None:
                column = []
                bonelist.append(column)
            else:
                bname = mcpbones.get(mcpname, "")
                column.append((longname, bname))
        nrows = max([len(column) for column in bonelist])
        for column in bonelist:
            while len(column) < nrows:
                column.append(None)
        box = self.layout.box()
        for m in range(nrows):
            row = box.row()
            for column in bonelist:
                if column[m]:
                    string = "%-20s: %20s" % column[m]
                else:
                    string = ""
                row.label(text=string)


    def invoke(self, context, event):
        clearErrorMessage()
        wm = context.window_manager
        return wm.invoke_props_dialog(self, width=1100)


class MCP_OT_ListSourceRig(BvhOperator, ListRig):
    bl_idname = "mcp.list_source_rig"
    bl_label = "List Source Rig"
    bl_description = "List the bone associations of the active source rig"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        return mcpRna(context.scene).SourceRig

    def getInfos(self, context):
        scn = context.scene
        info = BD.sourceInfos.get(mcpRna(scn).SourceRig)
        tinfo = BD.tposeInfos.get(mcpRna(scn).SourceTPose)
        return info, tinfo

    def run(self, context):
        pass


class MCP_OT_VerifySourceRig(BvhOperator):
    bl_idname = "mcp.verify_source_rig"
    bl_label = "Verify Source Rig"
    bl_description = "Verify the source rig type of the active armature"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        ob = context.object
        return (mcpRna(context.scene).SourceRig and ob and ob.type == 'ARMATURE')

    def run(self, context):
        rigtype = mcpRna(context.scene).SourceRig
        info = BD.sourceInfos[rigtype]
        info.testRig(rigtype, context.object, context.scene)
        raise MocapMessage("Source armature %s verified" % rigtype)


class MCP_OT_IdentifySourceRig(BvhOperator):
    bl_idname = "mcp.identify_source_rig"
    bl_label = "Identify Source Rig"
    bl_description = "Identify the source rig type of the active armature"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and ob.type == 'ARMATURE')

    def run(self, context):
        from .target import guessArmatureFromList
        scn = context.scene
        rig = context.object
        mcpRna(scn).SourceRig,mcpRna(scn).SourceTPose = guessArmatureFromList(rig, scn, BD.sourceInfos)
        info = BD.sourceInfos[mcpRna(scn).SourceRig]
        if mcpRna(scn).SourceRig == "Automatic":
            info.identifyRig(context, rig, mcpRna(scn).SourceTPose)
            info.addAutoBones(rig)
        else:
            info.addManualBones(rig)
            tinfo = BD.tposeInfos.get(info.t_pose_file)
            if tinfo:
                mcpRna(scn).SourceTPose = tinfo.name
                tinfo.addTPose(rig)
        print("Identified rig %s" % mcpRna(scn).SourceRig)

#-------------------------------------------------------------
#   Normalize Source Rig
#-------------------------------------------------------------

class MCP_OT_NormalizeSourceRig(BvhOperator, IsArmature):
    bl_idname = "mcp.normalize_source_rig"
    bl_label = "Normalize Source Rig"
    bl_description = "Remove locations for bones with parent,\nand remove object transformations"
    bl_options = {'UNDO'}

    def run(self, context):
        normalizeSourceRig(context.object)


def normalizeSourceRig(rig):
    roots = set([bone.name for bone in rig.data.bones if bone.parent is None])
    roots.add("hips")
    fcurves = getRnaFcurves(rig)
    for fcu in list(fcurves):
        if fcu.data_path in ("location", "rotation_euler", "scale"):
            fcurves.remove(fcu)
        else:
            bname,channel = getBoneChannel(fcu)
            if (bname not in roots and
                channel not in ["rotation_euler", "rotation_quaternion"]):
                fcurves.remove(fcu)
    rig.location = rig.rotation_euler = (0,0,0)
    rig.scale = (1,1,1)
    for pb in rig.pose.bones:
        if pb.name not in roots:
            pb.location = (0,0,0)
            pb.scale = (1,1,1)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_InitKnownRigs,
    MCP_OT_ListSourceRig,
    MCP_OT_VerifySourceRig,
    MCP_OT_IdentifySourceRig,
    MCP_OT_NormalizeSourceRig,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
