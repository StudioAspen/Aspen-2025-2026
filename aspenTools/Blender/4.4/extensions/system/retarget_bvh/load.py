# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy, os, mathutils, math, time
from bpy_extras.io_utils import ImportHelper, orientation_helper, axis_conversion
from math import sin, cos
from mathutils import *
from bpy.props import *

from .utils import *
from .source import Source
from .target import Target
from .simplify import TimeScaler

import numpy as np

class BvhFile:
    filename_ext = ".bvh"
    filter_glob : StringProperty(default="*.bvh;*.fbx;*.glb;*.gltf", options={'HIDDEN'})
    filepath : StringProperty(name="File Path", description="Filepath used for importing the BVH file", maxlen=1024, default="")


class MultiFile(ImportHelper):
    files : CollectionProperty(
        name = "File Path",
        type = bpy.types.OperatorFileListElement)

    directory : StringProperty(
        subtype='DIR_PATH')

    def getFilePaths(self):
        if self.files:
            filepaths = []
            for file_elem in self.files:
                filepath = os.path.join(self.directory, file_elem.name)
                filepaths.append(filepath)
            return filepaths
        else:
            return [self.filepath]

#-------------------------------------------------------------
#    class CNode:
#-------------------------------------------------------------

class CNode:
    def __init__(self, words, parent):
        name = words[1]
        for word in words[2:]:
            name += ' '+word

        self.name = name
        self.parent = parent
        self.children = []
        self.head = Vector((0,0,0))
        self.offset = Vector((0,0,0))
        if parent:
            parent.children.append(self)
        self.channels = []
        self.matrix = None
        self.inverse = None
        return

    def __repr__(self):
        return "<CNode %s, %d>" % (self.name, len(self.children))

    def display(self, pad):
        vec = self.offset
        if vec.length < Epsilon:
            c = '*'
        else:
            c = ' '
        print("%s%s%10s (%8.3f %8.3f %8.3f)" % (c, pad, self.name, vec[0], vec[1], vec[2]))
        for child in self.children:
            child.display(pad+"  ")
        return


    def build(self, amt, orig, parent):
        self.head = orig + self.offset
        if not self.children:
            return self.head

        zero = (self.offset.length < Epsilon)
        eb = amt.edit_bones.new(self.name)
        if parent:
            eb.parent = parent
        eb.head = self.head
        tails = Vector((0,0,0))
        for child in self.children:
            tails += child.build(amt, self.head, eb)
        tail = tails/len(self.children)
        if (tail-self.head).length == 0:
            print("Zero-length bone: %s" % eb.name)
            vec = self.head - parent.head
            tail = self.head + vec*0.1
        eb.tail = tail
        (loc, rot, scale) = eb.matrix.decompose()
        self.matrix = rot.to_matrix()
        self.inverse = self.matrix.copy()
        self.inverse.invert()
        if zero:
            return eb.tail
        else:
            return eb.head

#
#    readMocapFile(context, filepath):
#    Custom importer
#

Location = 1
Rotation = 2
Hierarchy = 1
Motion = 2
Frames = 3

Epsilon = 1e-5

class FrameRange:
    useAllFrames : BoolProperty(
        name = "All Frames",
        description = "Import all frames in file",
        default = True)

    startFrame : IntProperty(
        name = "Start Frame",
        description = "Starting frame for the animation",
        default = 1)

    endFrame : IntProperty(
        name = "Last Frame",
        description = "Last frame for the animation",
        default = 250)

    def draw(self, context):
        self.layout.prop(self, "useAllFrames")
        if not self.useAllFrames:
            self.layout.prop(self, "startFrame")
            self.layout.prop(self, "endFrame")

    def getStartEndFrame(self):
        if self.useAllFrames:
            return -9999, 9999
        else:
            return self.startFrame, self.endFrame



def getOrientation(scn, context):
    BD.ensureDataInited()
    enums = [(key, key, key) for key in BD.orientation.keys()]
    return [("Manual", "Manual", "Manual")] + enums


class BvhLoader(FrameRange):
    useDeleteFbx = True

    orientation : EnumProperty(
        items = getOrientation,
        name = "Orientation",
        description = "Global orientation presets.\nSome applications can produce different orientations,\nin that case use Manual")

    scale : FloatProperty(
        name="Scale",
        description="Scale the BVH by this value",
        min=0.0001, max=1000000.0,
        soft_min=0.001, soft_max=100.0,
        precision = 3,
        default=1.0)

    ssFactor : IntProperty(
        name="Subsample Factor",
        description="Sample only every n:th frame",
        min=1, default=1)

    useDefaultSS : BoolProperty(
        name="Use default subsample",
        description = "Subsample based on difference in frame rates between BVH file and Blender",
        default=True)

    def draw(self, context):
        self.layout.prop(self, "orientation")
        if self.orientation == "Manual":
            self.layout.prop(self, "axis_forward")
            self.layout.prop(self, "axis_up")
        FrameRange.draw(self, context)
        self.layout.prop(self, "useDefaultSS")
        if not self.useDefaultSS:
            self.layout.prop(self, "ssFactor")


    def getOrientation(self):
        if self.orientation == "Manual":
            return self.axis_forward, self.axis_up
        else:
            return BD.orientation[self.orientation]


    def readMocapFile(self, context, filepath):
        filepath = os.path.realpath(os.path.expanduser(filepath))
        ext = os.path.splitext(filepath)[-1].lower()
        startProgress( "Loading BVH/FBX/GLTF file %s" % filepath)
        time1 = time.perf_counter()
        if ext == ".fbx":
            filetype = "FBX"
            rig, act, imported_objects = self.loadFbxFile(context, filepath)
            try:
                self.truncAction(act)
                if BS().useNativeFbx:
                    imported_objects = imported_objects - set([rig])
                    act = None
                else:
                    bvhpath = self.saveFbx2Bvh(context, act, filepath)
                    rig = self.loadBvhFile(context, bvhpath)
            finally:
                if act:
                    bpy.data.actions.remove(act)
                if self.useDeleteFbx:
                    print("Deleting temporary FBX objects")
                    deleteObjects(context, list(imported_objects))
        elif ext in ('.glb','.gltf'):
            filetype = "GLTF"
            rig, act, imported_objects = self.loadGltfFile(context, filepath)
            try:
                self.truncAction(act)
                if BS().useNativeFbx:
                    imported_objects = imported_objects - set([rig])
                    act = None
                else:
                    gltfpath = self.saveGltf2Bvh(context, act, filepath)
                    rig = self.loadBvhFile(context, gltfpath)
            finally:
                if act:
                    bpy.data.actions.remove(act)
                if self.useDeleteFbx:
                    print("Deleting temporary GLTF objects")
                    deleteObjects(context, list(imported_objects))

        elif ext == ".bvh":
            filetype = "BVH"
            rig = self.loadBvhFile(context, filepath)
        else:
            raise MocapError("Not a BVH, FBX or GLTF file: " + filepath)
        if not rig:
            raise MocapError("%s file \n%s\n is corrupt: No rig defined" % (filetype, filepath))
        setInterpolation(rig)
        time2 = time.perf_counter()
        endProgress("%s file %s loaded in %.3f s" % (filetype, filepath, time2-time1))
        renameBvhRig(rig, filepath)
        mcpRna(rig).IsSourceRig = True
        return rig


    def loadFbxFile(self, context, filepath):
        scn = context.scene
        existing_objects = set(scn.objects)
        axis_forward, axis_up = self.getOrientation()

        try:
            bpy.ops.import_scene.fbx(
                filepath = filepath,
                global_scale = 1.0,
                axis_forward = axis_forward,
                axis_up = axis_up,
                automatic_bone_orientation=True,
                ignore_leaf_bones=False
                )
        except AttributeError:
            raise MocapError("Blender's built-in FBX importer must be enabled")
        rig = context.object
        self.transferObjectToRoots(context, rig)
        fcurves = getRnaFcurves(rig)
        if fcurves:
            self.fixFbxRig(rig, fcurves)
            imported_objects = set(scn.objects) - existing_objects
            print("Temporary FBX objects imported: %s" % imported_objects)
            act = rig.animation_data.action
            return rig, act, imported_objects
        else:
            raise MocapError("No action found in FBX file %s" % filepath)


    def transferObjectToRoots(self, context, rig):

        def removeScale(rig, useLocation):
            bag = getActionBag(rig.animation_data.action)
            for fcu in list(bag.fcurves):
                if fcu.data_path.endswith("scale"):
                    bag.fcurves.remove(fcu)
            scale = list(rig.scale)
            rig.scale = (1,1,1)
            for pb in rig.pose.bones:
                pb.scale = (1,1,1)
            if not useLocation:
                return
            for fcu in bag.fcurves:
                if fcu.data_path == "location":
                    fac = 1/scale[fcu.array_index]
                    for kp in fcu.keyframe_points:
                        kp.co[1] *= fac

        removeScale(rig, True)
        fcurves = getRnaFcurves(rig)
        obfcus = [fcu for fcu in fcurves
                  if fcu.data_path in ["location", "rotation_euler", "scale"]]
        if obfcus:
            activateObject(context, rig)
            bpy.ops.object.duplicate()
            rig2 = context.object
            act = rig2.animation_data.action
            rig.animation_data.action = None
            context.view_layer.objects.active = rig2
            rig.select_set(True)
            rig.matrix_world = Matrix()
            for pb in rig.pose.bones:
                pb.matrix_basis = Matrix()
                cns = pb.constraints.new('COPY_TRANSFORMS')
                cns.target = rig2
                cns.subtarget = pb.name
                P2B(pb).select = True
            bpy.ops.nla.bake(frame_start=int(act.frame_range[0]),
                             frame_end=int(act.frame_range[1]),
                             only_selected=False,
                             visual_keying=True,
                             clear_constraints=True,
                             clean_curves=False,
                             bake_types={'POSE'})
            context.view_layer.objects.active = rig
            removeScale(rig, False)


    def loadGltfFile(self, context, filepath):
        scn = context.scene
        print(f"Scene: {scn}")
        existing_objects = set(scn.objects)
        axis_forward, axis_up = self.getOrientation()
        try:
            bpy.ops.import_scene.gltf(
                filepath = filepath
                )
        except AttributeError:
            raise MocapError("Blender's built-in GLTF importer must be enabled")

        # The GLTF import appears to result in multiple objects, we need the armature
        for obj in bpy.context.selected_editable_objects:
            if obj.type == 'ARMATURE':
                rig = obj
                bpy.ops.object.select_all(action='DESELECT')
                rig.select_set(True)
                bpy.context.view_layer.objects.active = rig
                break
        self.transferObjectToRoots(context, rig)

        print(f"Rig / context.object: {rig}")
        fcurves = getRnaFcurves(rig)
        if fcurves:
            # self.fixFbxRig(rig, fcurves)
            imported_objects = set(scn.objects) - existing_objects
            print("Temporary GLTF objects imported: %s" % imported_objects)
            act = rig.animation_data.action
            return rig, act, imported_objects
        else:
            raise MocapError("No action found in GLTF file %s" % filepath)


    def saveFbx2Bvh(self, context, act, filepath):
        bvh_path = "%s.bvh" % os.path.splitext(os.path.realpath(filepath))[0]
        try:
            bpy.ops.export_anim.bvh(
               filepath=bvh_path,
               frame_start=int(act.frame_range[0]),
               frame_end=int(act.frame_range[1]),
               root_transform_only=False,
               global_scale = 0.01
               )
        except AttributeError:
            raise MocapError("Blender's builtin BVH exporter must be enabled")
        return bvh_path


    def saveGltf2Bvh(self, context, act, filepath):
        gltf_path = "%s.bvh" % os.path.splitext(os.path.realpath(filepath))[0]
        try:
            bpy.ops.export_anim.bvh(
               filepath=gltf_path,
               frame_start=int(act.frame_range[0]),
               frame_end=int(act.frame_range[1]),
               root_transform_only=False,
               global_scale = 1
               )
        except AttributeError as ae:
            raise MocapError(f"Blender's builtin BVH exporter must be enabled:\n{ae}")
        return gltf_path


    def fixFbxRig(self, rig, fcurves):
        for fcu in list(fcurves):
            words = fcu.data_path.split('"')
            if words[0] == "pose.bones[" and words[1].endswith("_end"):
                fcurves.remove(fcu)
        bpy.ops.object.mode_set(mode='EDIT')
        for eb in list(rig.data.edit_bones):
            if eb.name.endswith("_end"):
                par = eb.parent
                rig.data.edit_bones.remove(eb)
        bpy.ops.object.mode_set(mode='OBJECT')


    def loadBvhFile(self, context, filepath):
        if BS().useBlenderBvh:
            rig, act = self.loadBlenderBvhFile(context, filepath)
            self.truncAction(act)
            return rig
        else:
            return self.loadPluginBvhFile(context, filepath)


    def loadBlenderBvhFile(self, context, filepath):
        axis_forward, axis_up = self.getOrientation()
        try:
            bpy.ops.import_anim.bvh(
                filepath = filepath,
                global_scale = self.scale,
                frame_start = 1,
                rotate_mode = 'XYZ',
                use_fps_scale = False,
                update_scene_fps = False,
                update_scene_duration = False,
                axis_forward = axis_forward,
                axis_up = axis_up,
            )
        except AttributeError as ae:
            raise MocapError(f"[loadBlenderBvhFile] Blender's builtin BVH importer must be enabled:\n{ae}")
        rig = context.object
        act = rig.animation_data.action
        return rig, act


    def loadBlenderGltfFile(self, context, filepath):
        axis_forward, axis_up = self.getOrientation()
        try:
            bpy.ops.import_anim.gltf(
                filepath = filepath,
                global_scale = self.scale,
                frame_start = 1,
                rotate_mode = 'XYZ',
                use_fps_scale = False,
                update_scene_fps = False,
                update_scene_duration = False,
                axis_forward = axis_forward,
                axis_up = axis_up,
            )
        except AttributeError as ae:
            raise MocapError(f"[loadBlenderGltfFile] Blender's builtin BVH importer must be enabled:\n{ae}")
        rig = context.object
        act = rig.animation_data.action
        return rig, act


    def truncAction(self, act):
        if not self.useAllFrames:
            from .loop import truncFCurves
            fcurves = getActionFcurves(act)
            truncFCurves(fcurves, self.startFrame, self.endFrame)


    def loadPluginBvhFile(self, context, filepath):
        frameno = 1
        axis_forward, axis_up = self.getOrientation()
        flipMatrix = axis_conversion(
            from_forward=axis_forward,
            from_up=axis_up,
        )
        startFrame,endFrame = self.getStartEndFrame()
        ssFactor = self.ssFactor
        level = 0
        nErrors = 0
        scn = context.scene
        coll = scn.collection
        rig = None
        fp = open(filepath, "r")
        print( "Reading skeleton" )
        lineNo = 0
        for line in fp:
            words= line.split()
            lineNo += 1
            if len(words) == 0:
                continue
            key = words[0].upper()
            if key == 'HIERARCHY':
                status = Hierarchy
                ended = False
            elif key == 'MOTION':
                if level != 0:
                    raise MocapError("Tokenizer out of kilter %d" % level)
                amt = bpy.data.armatures.new("BvhAmt")
                rig = bpy.data.objects.new("BvhRig", amt)
                coll.objects.link(rig)
                setActiveObject(context, rig)
                updateScene(context)
                bpy.ops.object.mode_set(mode='EDIT')
                bpy.ops.object.mode_set(mode='EDIT')
                root.build(amt, Vector((0,0,0)), None)
                #root.display('')
                bpy.ops.object.mode_set(mode='OBJECT')
                status = Motion
                print("Reading motion")
            elif status == Hierarchy:
                if key == 'ROOT':
                    node = CNode(words, None)
                    root = node
                    nodes = [root]
                elif key == 'JOINT':
                    node = CNode(words, node)
                    nodes.append(node)
                    ended = False
                elif key == 'OFFSET':
                    (x,y,z) = (float(words[1]), float(words[2]), float(words[3]))
                    node.offset = self.scale * flipMatrix @ Vector((x,y,z))
                elif key == 'END':
                    node = CNode(words, node)
                    ended = True
                elif key == 'CHANNELS':
                    oldmode = None
                    for word in words[2:]:
                        (index, mode, sign) = channelYup(word)
                        if mode != oldmode:
                            indices = []
                            node.channels.append((mode, indices))
                            oldmode = mode
                        indices.append((index, sign))
                elif key == '{':
                    level += 1
                elif key == '}':
                    if not ended:
                        node = CNode(["End", "Site"], node)
                        node.offset = self.scale * flipMatrix @ Vector((0,1,0))
                        node = node.parent
                        ended = True
                    level -= 1
                    node = node.parent
                else:
                    raise MocapError("Did not expect %s" % words[0])
            elif status == Motion:
                if key == 'FRAMES:':
                    nFrames = int(words[1])
                elif key == 'FRAME' and words[1].upper() == 'TIME:':
                    frameTime = float(words[2])
                    frameFactor = int(1.0/(scn.render.fps*frameTime) + 0.49)
                    if self.useDefaultSS:
                        ssFactor = frameFactor if frameFactor > 0 else 1
                    startFrame *= ssFactor
                    endFrame *= ssFactor
                    status = Frames
                    frame = 0
                    frameno = 1

                    bpy.ops.object.mode_set(mode='POSE')
                    pbones = rig.pose.bones
                    for pb in pbones:
                        pb.rotation_mode = 'QUATERNION'
            elif status == Frames:
                if (frame >= startFrame and
                    frame <= endFrame and
                    frame % ssFactor == 0 and
                    frame < nFrames):
                    self.addFrame(words, frameno, nodes, pbones, flipMatrix)
                    showProgress(frameno, frame, nFrames, step=200)
                    frameno += 1
                frame += 1
        fp.close()
        if frameno == 1:
            print("Warning: No frames in range %d -- %d." % (startFrame, endFrame))
        return rig


    def addFrame(self, words, frame, nodes, pbones, flipMatrix):
        m = 0
        first = True
        flipInv = flipMatrix.inverted()
        for node in nodes:
            bname = node.name
            if bname not in pbones.keys():
                for (mode, indices) in node.channels:
                    m += len(indices)
            else:
                pb = pbones[bname]
                for (mode, indices) in node.channels:
                    if mode == Location:
                        vec = Vector((0,0,0))
                        for (index, sign) in indices:
                            vec[index] = sign*float(words[m])
                            m += 1
                        if first:
                            pb.location = node.inverse @ (self.scale * flipMatrix @ vec) - node.head
                            pb.keyframe_insert('location', frame=frame, group=bname)
                            if len(node.children) > 1:
                                first = False
                    elif mode == Rotation:
                        mats = []
                        for (axis, sign) in indices:
                            angle = sign*float(words[m])*D
                            mats.append(Matrix.Rotation(angle, 3, axis))
                            m += 1
                        mat = (node.inverse @ flipMatrix) @ mats[0] @ mats[1] @ mats[2] @ (flipInv @ node.matrix)
                        insertRotation(pb, mat, frame)

#
#    channelYup(word):
#    channelZup(word):
#

def channelYup(word):
    if word == 'Xrotation':
        return ('X', Rotation, +1)
    elif word == 'Yrotation':
        return ('Y', Rotation, +1)
    elif word == 'Zrotation':
        return ('Z', Rotation, +1)
    elif word == 'Xposition':
        return (0, Location, +1)
    elif word == 'Yposition':
        return (1, Location, +1)
    elif word == 'Zposition':
        return (2, Location, +1)

def channelZup(word):
    if word == 'Xrotation':
        return ('X', Rotation, +1)
    elif word == 'Yrotation':
        return ('Z', Rotation, +1)
    elif word == 'Zrotation':
        return ('Y', Rotation, -1)
    elif word == 'Xposition':
        return (0, Location, +1)
    elif word == 'Yposition':
        return (2, Location, +1)
    elif word == 'Zposition':
        return (1, Location, -1)

#-------------------------------------------------------------
#   end BVH importer
#-------------------------------------------------------------


#-------------------------------------------------------------
#    class CEditBone():
#-------------------------------------------------------------


class CEditBone():
    def __init__(self, bone):
        self.name = bone.name
        self.head = bone.head.copy()
        self.tail = bone.tail.copy()
        self.roll = bone.roll
        if bone.parent:
            self.parent = bone.parent.name
        else:
            self.parent = None
        if self.parent:
            self.use_connect = bone.use_connect
        else:
            self.use_connect = False
        #self.matrix = bone.matrix.copy().rotation_part()
        (loc, rot, scale) = bone.matrix.decompose()
        self.matrix = rot.to_matrix()
        self.inverse = self.matrix.copy()
        self.inverse.invert()

    def __repr__(self):
        return ("%s p %s\n  h %s\n  t %s\n" % (self.name, self.parent, self.head, self.tail))

#
#    renameBones(srcRig, context):
#

def renameBones(srcRig, context):
    srcBones = []
    trgBones = {}

    setActiveObject(context, srcRig)
    bpy.ops.object.mode_set(mode='EDIT')
    bpy.ops.object.mode_set(mode='EDIT')
    #print("Ren", bpy.context.object, srcRig.mode)
    ebones = srcRig.data.edit_bones
    for bone in ebones:
        srcBones.append( CEditBone(bone) )

    setbones = []
    renamed = {}
    if srcRig.animation_data:
        bag = getActionBag(srcRig.animation_data.action)
    else:
        bag = None
    for srcBone in srcBones:
        srcName = srcBone.name
        trgName = BD.activeSrcInfo.boneNames.get(canonicalName(srcName))
        if isinstance(trgName, tuple):
            print("BUG. Target name is tuple:", trgName)
            trgName = trgName[0]
        eb = ebones[srcName]
        if trgName:
            if bag and srcName in bag.groups.keys():
                grp = bag.groups[srcName]
                grp.name = trgName
            eb.name = trgName
            trgBones[trgName] = CEditBone(eb)
            setbones.append((eb, trgName))
        else:
            eb.name = '_' + srcName
        renamed[srcName] = eb.name

    for (eb, name) in setbones:
        eb.name = name
    #createExtraBones(ebones, trgBones)
    bpy.ops.object.mode_set(mode='OBJECT')
    for pb in srcRig.pose.bones:
        if mcpRna(pb).Parent and mcpRna(pb).Parent in renamed.keys():
            mcpRna(pb).Parent = renamed[mcpRna(pb).Parent]

#
#    renameBvhRig(srcRig, filepath):
#

def renameBvhRig(srcRig, filepath):
    base = os.path.basename(filepath)
    (filename, ext) = os.path.splitext(base)
    print("File", filename, len(filename))
    if len(filename) > 12:
        words = filename.split('_')
        if len(words) == 1:
            words = filename.split('-')
        name = 'Y_'
        if len(words) > 1:
            words = words[1:]
        for word in words:
            name += word
    else:
        name = 'Y_' + filename
    print("Name", name, srcRig)

    srcRig.name = name
    adata = srcRig.animation_data
    if adata:
        adata.action.name = name
    return

#
#    deleteSourceRig(context, rig, prefix):
#

def deleteSourceRig(context, rig, prefix):
    ob = context.object
    setActiveObject(context, rig)
    bpy.ops.object.mode_set(mode='OBJECT')
    setActiveObject(context, ob)
    deleteObjects(context, [rig])
    if bpy.data.actions:
        for act in bpy.data.actions:
            if act.name[0:2] == prefix:
                act.use_fake_user = False
                if act.users == 0:
                    bpy.data.actions.remove(act)

#-------------------------------------------------------------
#   Activate object
#-------------------------------------------------------------

def activateObject(context, ob, strict=False):
    if ob:
        try:
            context.view_layer.objects.active = ob
            bpy.ops.object.mode_set(mode='OBJECT')
            bpy.ops.object.select_all(action='DESELECT')
            ob.select_set(True)
            return True
        except:
            pass
    if strict:
        raise MocapError("Could not activate %s" % ob)
    return False

#-------------------------------------------------------------
#   Delete objects
#-------------------------------------------------------------

def deleteObjects(context, objects):
    try:
        bpy.ops.object.mode_set(mode='OBJECT')
        bpy.ops.object.select_all(action='DESELECT')
    except RuntimeError:
        return
    for ob in objects:
        if ob:
            dtype = ob.type
            if ob.data:
                data = ob.data
                users = ob.data.users
            else:
                users = 0
            for coll in bpy.data.collections:
                if ob in coll.objects.values():
                    coll.objects.unlink(ob)
            bpy.data.objects.remove(ob)
            if users == 1:
                if dtype == 'MESH':
                    bpy.data.meshes.remove(data)
                elif dtype == 'ARMATURE':
                    bpy.data.armatures.remove(data)
                elif dtype == 'CURVES':
                    bpy.data.curves.remove(data)

#----------------------------------------------------------
#   Renamer
#----------------------------------------------------------

class BvhRenamer(Source, Target):
    useAutoScale : BoolProperty(
        name="Auto Scale",
        description="Rescale skeleton to match target",
        default=True)

    useNormalizeSource : BoolProperty(
        name = "Normalize Source Rig",
        description = "Remove translations",
        default = True)

    def draw(self, context):
        Source.draw(self, context)
        Target.draw(self, context)
        self.layout.prop(self, "useAutoScale")
        self.layout.prop(self, "useNormalizeSource")
        if not self.useAutoScale:
            self.layout.prop(self, "scale")


    def rescaleRig(self, trgRig, srcRig):
        if not self.useAutoScale:
            return

        def getThighLength(rig):
            thigh = getTrgBone("thigh.L", rig, force=True)
            shin = getTrgBone("shin.L", rig)
            if thigh and shin:
                return (thigh.head - shin.head).length
            else:
                print("Thigh and shin not found")
                return None

        trgScale = getThighLength(trgRig)
        srcScale = getThighLength(srcRig)
        if trgScale is None or srcScale is None:
            return
        scale = trgScale/srcScale
        print("Rescale %s with factor %f" % (srcRig.name, scale))
        self.scale = scale

        bpy.ops.object.mode_set(mode='EDIT')
        ebones = srcRig.data.edit_bones
        for eb in ebones:
            eb.use_connect = False
        for eb in ebones:
            eb.head *= scale
            eb.tail *= scale
        bpy.ops.object.mode_set(mode='OBJECT')
        fcurves = getRnaFcurves(srcRig)
        for fcu in fcurves:
            words = fcu.data_path.split('.')
            if words[-1] == 'location':
                for kp in fcu.keyframe_points:
                    kp.co[1] *= scale


    def renameAndRescaleBvh(self, context, srcRig, trgRig):
        if mcpRna(srcRig).Renamed:
            raise MocapError("%s already renamed and rescaled." % srcRig.name)

        from .t_pose import putInTPose
        from .source import normalizeSourceRig

        scn = context.scene
        scn.frame_current = 0
        setActiveObject(context, srcRig)
        #(srcRig, srcBones, action) =  renameBvhRig(rig, filepath)
        self.findTarget(context, trgRig)
        self.findSource(context, srcRig)
        renameBones(srcRig, context)
        if self.useNormalizeSource:
            normalizeSourceRig(srcRig)
        putInTPose(context, srcRig, mcpRna(scn).SourceTPose)
        setInterpolation(srcRig)
        self.rescaleRig(trgRig, srcRig)
        mcpRna(srcRig).Renamed = True

#----------------------------------------------------------
#   Object Problems
#----------------------------------------------------------

def checkObjectProblems(context):
    problems = ""
    epsilon = 1e-2
    rig = context.object
    rig.hide_viewport = False

    eu = rig.rotation_euler
    if abs(eu.x) + abs(eu.y) + abs(eu.z) > epsilon:
        problems += "object rotation\n"

    vec = rig.scale - Vector((1,1,1))
    if vec.length > epsilon:
        problems += "object scaling\n"

    if problems:
        msg = ("BVH Retargeter cannot use this rig because it has:\n" +
               problems +
               "Apply object transformations before using BVH Retargeter")
        raise MocapError(msg)

#-------------------------------------------------------------
#   class MCP_OT_LoadBvh(BvhOperator, MultiFile, BvhFile):
#-------------------------------------------------------------

@orientation_helper(axis_forward='-Z', axis_up='Y')
class MCP_OT_LoadBvh(HideOperator, MultiFile, BvhFile, BvhLoader):
    bl_idname = "mcp.load_bvh"
    bl_label = "Load BVH/FBX/GLTF File"
    bl_description = "Load an armature from a bvh, fbx or gltf file"
    bl_options = {'UNDO'}

    useDeleteFbx : BoolProperty(
        name = "Delete FBX Objects",
        description = "Delete imported FBX/GLTF objects",
        default = True)

    def draw(self, context):
        self.layout.prop(self, "scale")
        BvhLoader.draw(self, context)
        self.layout.prop(self, "useDeleteFbx")

    def run(self, context):
        for filepath in self.getFilePaths():
            rig = self.readMocapFile(context, filepath)
            activateObject(context, rig)
        print("BVH file(s) loaded")

    def invoke(self, context, event):
        context.window_manager.fileselect_add(self)
        return {'RUNNING_MODAL'}

#
#   class MCP_OT_RenameActiveToSelected(BvhOperator):
#

class MCP_OT_RenameActiveToSelected(BvhPropsOperator, IsArmature, TimeScaler, BvhRenamer):
    bl_idname = "mcp.rename_active_to_selected"
    bl_label = "Rename Selected From Active"
    bl_description = "Rename bones of selected (source) armatures and scale it to fit the active (target) armature"
    bl_options = {'UNDO'}

    def draw(self, context):
        BvhRenamer.draw(self, context)
        TimeScaler.draw(self, context)

    def run(self, context):
        scn = context.scene
        trgRig = context.object
        for srcRig in context.selected_objects:
            if srcRig != trgRig and srcRig.type == 'ARMATURE':
                self.renameAndRescaleBvh(context, srcRig, trgRig)
                if self.useTimeScale:
                    self.timescaleFCurves(srcRig)
                bpy.ops.object.mode_set(mode='OBJECT')
                print("%s renamed" % srcRig.name)
        activateObject(context, trgRig)

    def invoke(self, context, event):
        BD.ensureInited(context.scene)
        return BvhPropsOperator.invoke(self, context, event)

#
#   class MCP_OT_LoadAndRenameBvh(HideOperator, ImportHelper, BvhFile):
#

@orientation_helper(axis_forward='-Z', axis_up='Y')
class MCP_OT_LoadAndRenameBvh(HideOperator, IsArmature, ImportHelper, BvhFile, BvhLoader, BvhRenamer, TimeScaler):
    bl_idname = "mcp.load_and_rename_bvh"
    bl_label = "Load And Rename BVH File"
    bl_description = "Load armature from bvh file and rename bones"
    bl_options = {'UNDO'}

    def draw(self, context):
        BvhLoader.draw(self, context)
        BvhRenamer.draw(self, context)
        TimeScaler.draw(self, context)

    def prequel(self, context):
        from .retarget import changeTargetData
        return changeTargetData(context.object, context.scene)

    def run(self, context):
        checkObjectProblems(context)
        trgRig = context.object
        srcRig = self.readMocapFile(context, self.properties.filepath)
        self.renameAndRescaleBvh(context, srcRig, trgRig)
        if self.useTimeScale:
            self.timescaleFCurves(srcRig)
        bpy.ops.object.mode_set(mode='OBJECT')
        srcRig.select_set(True)
        trgRig.select_set(True)
        activateObject(context, trgRig)
        print("%s loaded and renamed" % srcRig.name)

    def sequel(self, context, data):
        from .retarget import restoreTargetData
        restoreTargetData(data)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_OT_LoadBvh,
    MCP_OT_RenameActiveToSelected,
    MCP_OT_LoadAndRenameBvh,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
