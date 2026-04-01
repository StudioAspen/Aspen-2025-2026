# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from bpy.props import EnumProperty, StringProperty
from .utils import *

#
#   ActionGroup
#

class ActionGroup(bpy.types.PropertyGroup):
    name : StringProperty()
    select : BoolProperty()
    fake : BoolProperty()
    users : IntProperty()


class ActionList:
    filter : StringProperty(
        name="Filter",
        description="Filter action names with this",
        default="")

    actions : CollectionProperty(type = ActionGroup)

    def draw(self, context):
        split = self.layout.split(factor = 0.5)
        split.label(text="Action")
        split.label(text="Select")
        split.label(text="Users")
        for act in self.actions:
            if self.filter in act.name:
                split = self.layout.split(factor = 0.6)
                split.label(text=act.name)
                split.prop(act, "select", text="")
                split.label(text = str(act.users))
        self.layout.separator()
        self.layout.prop(self, "filter")


    def invoke(self, context, event):
        animdata = context.object.animation_data
        self.actions.clear()
        for act in bpy.data.actions:
            item = self.actions.add()
            item.name = act.name
            item.select = self.selected(animdata, act)
            item.fake = act.use_fake_user
            item.users = act.users

        return BvhPropsOperator.invoke(self, context, event)


    def getActions(self, context):
        acts = []
        for agrp in self.actions:
            if agrp.name in bpy.data.actions.keys():
                act = bpy.data.actions[agrp.name]
                acts.append((act, agrp.select))
        return acts

#
#   Buttons:
#

class MCP_OT_DeleteAction(BvhOperator, IsArmature, ActionList):
    bl_idname = "mcp.delete_action"
    bl_label = "Delete Actions"
    bl_description = "Delete the action selected in the action list"
    bl_options = {'UNDO'}

    def selected(self, animdata, act):
        return False

    def run(self, context):
        self.failed = []
        for act,select in self.getActions(context):
            if select:
                self.deleteAction(act)
        if self.failed:
            msg = ("Could not delete all actions.\n%s" % [act.name for act in self.failed])
            raise MocapError(msg)

    def deleteAction(self, act):
        act.use_fake_user = False
        if act.users == 0:
            bpy.data.actions.remove(act)
        else:
            self.failed.append(act)


def deleteAction(act):
    act.use_fake_user = False
    if act.users == 0:
        bpy.data.actions.remove(act)
    else:
        print("Action %s has %d users" % (act.name, act.users))


class MCP_OT_DeleteAllActions(BvhPropsOperator):
    bl_idname = "mcp.delete_all_actions"
    bl_label = "Delete All Actions"
    bl_description = "Delete all action"
    bl_options = {'UNDO'}

    def draw(self, context):
        self.layout.label(text="Really delete all actions?")

    def run(self, context):
        for act in bpy.data.actions:
            deleteAction(act)


class MCP_OT_SetCurrentAction(BvhOperator, IsArmature, ActionList):
    bl_idname = "mcp.set_current_action"
    bl_label = "Set Current Action"
    bl_description = "Set the action selected in the action list as the current action"
    bl_options = {'UNDO'}

    def selected(self, animdata, act):
        return (animdata and animdata.action == act)

    def run(self, context):
        for act,select in self.getActions(context):
            if select:
                context.object.animation_data.action = act
                print("Action set to %s" % act)


class MCP_OT_SetAllFakeUser(BvhPropsOperator, IsArmature):
    bl_idname = "mcp.set_all_fake_user"
    bl_label = "Set All Fake Users"
    bl_description = "Add or remove fake users from all actions"
    bl_options = {'UNDO'}

    fake : BoolProperty(
        name = "Add Fake User",
        description = "Add or remove fake user",
        default = False)

    def draw(self, context):
        self.layout.prop(self, "fake")

    def run(self, context):
        for act in bpy.data.actions:
            act.use_fake_user = self.fake


class MCP_OT_SetFakeUser(BvhOperator, IsArmature, ActionList):
    bl_idname = "mcp.set_fake_user"
    bl_label = "Set Fake User"
    bl_description = "Make selected actions fake and others unfake"
    bl_options = {'UNDO'}

    def selected(self, animdata, act):
        return act.use_fake_user

    def run(self, context):
        for act,select in self.getActions(context):
            act.use_fake_user = select

#-------------------------------------------------------------
#   Remove keyframes from frame 0
#-------------------------------------------------------------

class MCP_OT_RemoveFrameZero(BvhOperator, IsArmature):
    bl_idname = "mcp.remove_frame_zero"
    bl_label = "Remove Frame Zero"
    bl_description = "Remove all keys from frame 0"
    bl_options = {'UNDO'}

    def run(self, context):
        rig = context.object
        fcurves = getRnaFcurves(rig)
        for fcu in fcurves:
            kps = [kp for kp in fcu.keyframe_points if kp.co[0] == 0.0]
            for kp in kps:
                fcu.keyframe_points.remove(kp, fast=True)

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    ActionGroup,

    MCP_OT_DeleteAction,
    MCP_OT_DeleteAllActions,
    MCP_OT_SetCurrentAction,
    MCP_OT_SetFakeUser,
    MCP_OT_SetAllFakeUser,
    MCP_OT_RemoveFrameZero,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
