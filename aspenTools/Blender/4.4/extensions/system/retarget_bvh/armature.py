# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
import os
from collections import OrderedDict
from math import pi
from mathutils import *
from bpy.props import *

from .utils import *


class CArmature:

    def __init__(self, scn):
        self.name = "Automatic"
        self.boneNames = OrderedDict()
        self.rig = None


    def display(self, type):
        if not BS().verbose:
            return
        print("%s Armature: %s" % (type, self.name))
        for bname,mhx in self.boneNames.items():
            print("  %14s %14s" % (bname, mhx))


    def findArmature(self, rig):
        self.rig = rig
        roots = []
        for pb in rig.pose.bones:
            if pb.parent is None:
                roots.append(pb)

        hipsChildren = roots
        nChildren = len(roots)
        first = True
        hips = None
        self.clearBones()
        while first or nChildren != 3:
            if not first and nChildren == 2 and mcpRna(rig).ReverseHip:
                break
            elif nChildren == 0:
                self.clearBones()
                raise MocapError("Hip bone must have children: %s" % hips.name)
            elif nChildren == 1:
                hips = hipsChildren[0]
                hipsChildren = self.validChildren(hips)
                nChildren = len(hipsChildren)
            elif nChildren == 2:
                counts = self.getChildCount(hipsChildren)
                #for m,n,pb in counts:
                #    print(m,n,pb.name)
                hips = counts[-1][2]
                hipsChildren = self.validChildren(hips)
                nChildren = len(hipsChildren)
            else:
                counts = self.getChildCount(hipsChildren)
                hipsChildren = [counts[-3][2], counts[-2][2], counts[-1][2]]
                nChildren = 3
            if BS().verbose and (hips is not None):
                print("  Try hips: %s, children: %d" % (hips.name, nChildren))
            first = False

        if hips is None:
            self.clearBones()
            raise MocapError("Found no candidate hip bone")

        if BS().verbose:
            print("Mapping bones automatically:")
            print("  hips:", hips.name)
        self.setBone("hips", hips)
        hiphead,hiptail,_ = getHeadTailDir(hips)

        if mcpRna(rig).ReverseHip and len(hipsChildren) == 2:
            legroot = hipsChildren[1]
            spine = hipsChildren[0]
            _,terminal = self.chainEnd(legroot)
            head,tail,vec = getHeadTailDir(terminal)
            if tail[2] > hiptail[2]:
                legroot = hipsChildren[0]
                spine = hipsChildren[1]
            hipsChildren = [spine] + self.validChildren(legroot)

        if len(hipsChildren) < 3:
            string = "Hips %s has %d children:\n" % (hips.name, len(hipsChildren))
            for child in hipsChildren:
                string += "  %s\n" % child.name
            self.clearBones()
            raise MocapError(string)

        spine = None
        spineTail = hiptail
        leftLeg = None
        leftLegTail = hiptail
        rightLeg = None
        rightLegTail = hiptail

        limbs = []
        for n,pb in enumerate(hipsChildren):
            _,terminal = self.chainEnd(pb)
            _,tail,_ = getHeadTailDir(terminal)
            limbs.append((tail[0], n, pb))
        limbs.sort()
        rightLeg = limbs[0][2]
        spine = limbs[1][2]
        leftLeg = limbs[2][2]

        if BS().verbose:
            print("  spine:", spine.name)
            print("  right leg:", rightLeg.name)
            print("  left leg:", leftLeg.name)
        self.findSpine(spine)
        self.findLeg(leftLeg, ".L")
        self.findLeg(rightLeg, ".R")


    def errLimb(self, name, pb, tail):
        if pb is None:
            return ("  No %s\n" % name)
        else:
            return ("  %s = %s, tail = %s\n" % (name, pb.name, tuple(tail)))


    def clearBones(self):
        for pb in self.rig.pose.bones:
            mcpRna(pb).Bone = ""
        self.boneNames = {}


    def setBone(self, bname, pb):
        mcpRna(pb).Bone = bname
        self.boneNames[canonicalName(pb.name)] = bname


    def findTerminal(self, pb, bnames, prefnames=None):
        if prefnames is None:
            prefnames = bnames
        self.setBone(bnames[0], pb)
        bnames = bnames[1:]
        prefnames = prefnames[1:]
        children = self.validChildren(pb)
        if bnames:
            if len(children) >= 1:
                child = children[0]
                for pb in children[1:]:
                    if prefnames[0] in canonicalName(pb.name):
                        child = pb
                        break
                return self.findTerminal(child, bnames, prefnames)
            else:
                return None
        else:
            while len(children) == 1:
                pb = children[0]
                children = self.validChildren(pb)
            return pb


    def findLeg(self, hip, suffix):
        bnames = ["hip"+suffix, "thigh"+suffix, "shin"+suffix, "foot"+suffix, "toe"+suffix]
        prefnames = ["X", "X", "X", "foot", "toe"]
        if BS().verbose:
            print("  hip%s:" % suffix, hip.name)
        try:
            thigh = self.validChildren(hip)[0]
        except IndexError:
            self.clearBones()
            raise MocapError("Hip %s has no children" % hip.name)
        shins = self.validChildren(thigh, True)
        if len(shins) == 0:
            self.clearBones()
            raise MocapError("Thigh %s has no children" % thigh.name)
        elif len(shins) > 1:
            shin = thigh
            thigh = hip
            if BS().verbose:
                print("  thigh%s:" % suffix, thigh.name)
                print("  shin%s:" % suffix, shin.name)
            bnames = bnames[1:]
            prefnames = prefnames[1:]
        else:
            shin = shins[0]
            foot = None
            if hip.length > shin.length:
                foot = shin
                shin = thigh
                thigh = hip
                bnames = bnames[1:]
                prefnames = prefnames[1:]
            else:
                feet = self.validChildren(shin)
                if feet:
                    foot = feet[0]
            if BS().verbose:
                print("  thigh%s:" % suffix, thigh.name)
                print("  shin%s:" % suffix, shin.name)
                if foot:
                    print("  foot%s:" % suffix, foot.name)

        self.findTerminal(hip, bnames, prefnames)


    def findArm(self, shoulder, suffix):
        bnames = ["shoulder"+suffix, "upper_arm"+suffix, "forearm"+suffix, "hand"+suffix]
        if BS().verbose:
            print("  shoulder%s:" % suffix, shoulder.name)
        try:
            upperarm = self.validChildren(shoulder)[0]
        except IndexError:
            self.clearBones()
            raise MocapError("Shoulder %s has no children" % shoulder.name)
        if BS().verbose:
            print("  upper_arm%s:" % suffix, upperarm.name)
        try:
            forearm = self.validChildren(upperarm, True)[0]
        except IndexError:
            self.clearBones()
            raise MocapError("Upper arm %s has no children" % upperarm.name)
        if BS().verbose:
            print("  forearm%s:" % suffix, forearm.name)
        hands = self.validChildren(forearm)
        if hands:
            hand = hands[0]
            if BS().verbose:
                print("  hand%s:" % suffix, hand.name)
            if upperarm.bone.length < hand.bone.length:
                bnames = ["shoulder"+suffix, ""] + bnames[1:]
        self.findTerminal(shoulder, bnames)


    def findHead(self, neck):
        bnames = ["neck", "head"]
        if BS().verbose:
            print("  neck:", neck.name)
        self.findTerminal(neck, bnames)


    def findSpine(self, spine1):
        n,spine2 = self.spineEnd(spine1)
        if BS().verbose:
            print("  spine:", spine1.name)
            print("  chest:", spine2.name)
        if n == 1:
            bnames = ["spine"]
        elif n == 2:
            bnames = ["spine", "chest"]
        elif n == 3:
            bnames = ["spine", "spine-1", "chest"]
        else:
            bnames = ["spine", "spine-1", "chest", "chest-1"]

        self.findTerminal(spine1, bnames)
        spine2Children = self.validChildren(spine2)
        if len(spine2Children) == 3:
            _,stail,_ = getHeadTailDir(spine2)
            limbs = []
            for pb in spine2Children:
                _,tail,_ = getHeadTailDir(pb)
                limbs.append((tail[0],pb))
            try:
                limbs.sort()
                fail = False
            except TypeError:
                fail = True
                reason = "Some of the bones have the same X coordinate\n"
            if not fail:
                self.findArm(limbs[0][1], ".R")
                self.findHead(limbs[1][1])
                self.findArm(limbs[2][1], ".L")
        else:
            fail = True
            reason = "Top of spine %s has %d children\n" % (spine2.name, len(spine2Children))
        if fail:
            string = "Could not auto-detect armature because:\n" + reason
            for child in spine2Children:
                string += "  %s\n" % child.name
            string += ("Is the source rig oriented correctly?\n" +
                       "Try to change vertical or horizonal orientation.")
            self.clearBones()
            raise MocapError(string)


    def validChildren(self, pb, mute=False):
        children = []
        for child in pb.children:
            if validBone(child, self.rig, mute):
                children.append(child)
        return children


    def getChildCount(self, children):
        counts = []
        for n,child in enumerate(children):
            counts.append((self.countChildren(child, 5), n, child))
        counts.sort()
        return counts


    def countChildren(self, pb, depth):
        if depth < 0:
            return 0
        n = 1
        for child in pb.children:
            n += self.countChildren(child, depth-1)
        return n


    def chainEnd(self, pb):
        n = 1
        while pb and (len(self.validChildren(pb)) == 1):
            n += 1
            pb = self.validChildren(pb)[0]
        return n,pb


    def spineEnd(self, pb):
        n = 1
        while pb:
            children = self.validChildren(pb)
            if len(children) == 1:
                n += 1
                pb = children[0]
            elif len(children) == 0 and len(pb.children) == 1:
                n += 1
                pb = pb.children[0]
            else:
                return n,pb
        return n,pb


def validBone(pb, rig=None, mute=False):
    if rig is not None:
        if not inVisibleLayer(pb.bone, rig):
            print("Hidden", pb.name)
            return False

    for cns in pb.constraints:
        if cns.mute or cns.influence < 0.2:
            pass
        elif cns.type[0:5] == 'LIMIT':
            #if mute:
            #    cns.mute = True
            pass
        elif cns.type in ['COPY_LOCATION', 'COPY_SCALE']:
            pass
        elif (cns.type == 'COPY_ROTATION' and
              cns.use_offset):
            pass
        elif cns.type == 'IK':
            if mute:
                pass
                #cns.mute = True
            elif cns.target is None:
                pass
            else:
                return False
        else:
            print("Not a valid constraint: %s" % cns.type)
            return False
    return True


def getHeadTailDir(pb):
    mat = pb.bone.matrix_local
    mat = pb.matrix
    head = Vector(mat.col[3][:3])
    vec = Vector(mat.col[1][:3])
    tail = head + pb.bone.length * vec
    return head, tail, vec

