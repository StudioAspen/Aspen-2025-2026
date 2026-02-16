# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from bpy.types import Menu, Panel, UIList
from .utils import *
from .panels import MCP_PT_Base
from .load import FrameRange

#----------------------------------------------------------
#   Panel
#----------------------------------------------------------

class MCP_MT_ContextMenu(Menu):
    bl_label = "Anim Layer Specials"

    def draw(self, _context):
        pass


class MCP_UL_AnimLayers(UIList):

    def draw_item(self, context, layout, _data, slot, icon, _active_data, _active_propname, _index):
        ob = context.object
        adata = ob.animation_data
        row = layout.row()
        row.prop(slot, "name", text="", emboss=False)
        if slot.index < len(adata.nla_tracks):
            track = adata.nla_tracks[slot.index]
            row.prop(track, "mute", text="", invert_checkbox=True)
        else:
            row.label(text="", icon='LAYER_ACTIVE')


class MCP_PT_AnimLayers(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "Animation Layers"

    def draw(self, context):
        ob = context.object
        is_sortable = len(mcpRna(ob).AnimLayers) > 1
        rows = (5 if is_sortable else 3)
        row = self.layout.row()
        row.template_list("MCP_UL_AnimLayers", "", mcpRna(ob), "AnimLayers", mcpRna(ob), "AnimLayerIndex", rows=rows)

        col = row.column(align=True)
        col.operator("mcp.animlayer_add", icon='ADD', text="")
        col.operator("mcp.animlayer_remove", icon='REMOVE', text="")
        col.separator()
        col.menu("MCP_MT_ContextMenu", icon='DOWNARROW_HLT', text="")
        if is_sortable:
            col.separator()
            col.operator("mcp.animlayer_move", icon='TRIA_UP', text="").up = True
            col.operator("mcp.animlayer_move", icon='TRIA_DOWN', text="").up = False

        self.layout.operator("mcp.bake_animlayers")


class MCP_OT_AnimLayerAdd(BvhOperator, HasAnimation):
    bl_idname = "mcp.animlayer_add"
    bl_label = ""
    bl_description = "Add an animation layer"
    bl_options = {'UNDO'}

    def run(self, context):
        ob = context.object
        adata = ob.animation_data
        adata.action_blend_type = 'COMBINE'
        adata.action_extrapolation = 'HOLD'
        adata.action_influence = 1.0
        act = adata.action
        track = adata.nla_tracks.new()
        track.name = act.name
        strip = track.strips.new(act.name, 0, act)
        strip.blend_type = 'COMBINE'
        strip.extrapolation = 'HOLD'
        if len(mcpRna(ob).AnimLayers) == 0:
            pg = mcpRna(ob).AnimLayers.add()
            pg.name = act.name
            pg.action = act
            pg.index = 0
        mcpRna(ob).AnimLayerIndex = len(adata.nla_tracks)
        aname = "Anim Layer %d" % mcpRna(ob).AnimLayerIndex
        nact = setNewAction(ob, aname)
        pg = mcpRna(ob).AnimLayers.add()
        pg.name = aname
        pg.action = nact
        pg.index = len(mcpRna(ob).AnimLayers) - 1


class MCP_OT_AnimLayerRemove(BvhOperator, HasAnimation):
    bl_idname = "mcp.animlayer_remove"
    bl_label = ""
    bl_description = "Remove an animation layer"
    bl_options = {'UNDO'}

    def run(self, context):
        ob = context.object
        adata = ob.animation_data
        idx = mcpRna(ob).AnimLayerIndex
        pg = mcpRna(ob).AnimLayers[idx]
        if idx < len(adata.nla_tracks):
            track = adata.nla_tracks[idx]
            adata.nla_tracks.remove(track)
        elif idx == 0:
            pass
        elif idx == len(adata.nla_tracks):
            track = adata.nla_tracks[idx-1]
            adata.action = track.strips[0].action
            adata.nla_tracks.remove(track)
        else:
            print("BUG", idx)
        for pg in mcpRna(ob).AnimLayers:
            if pg.index > idx:
                pg.index -= 1
        mcpRna(ob).AnimLayers.remove(idx)
        mcpRna(ob).AnimLayerIndex = len(adata.nla_tracks)


class MCP_OT_AnimLayerMove(BvhOperator, HasAnimation):
    bl_idname = "mcp.animlayer_move"
    bl_label = ""
    bl_description = "Move an animation layer"
    bl_options = {'UNDO'}

    up : BoolProperty()

    def run(self, context):
        ob = context.object
        idx = mcpRna(ob).AnimLayerIndex
        if self.up:
            if idx > 0 and idx < len(mcpRna(ob).AnimLayers)-1:
                self.interchange(ob, idx-1, idx)
                mcpRna(ob).AnimLayerIndex -= 1
        else:
            if idx < len(mcpRna(ob).AnimLayers)-2:
                self.interchange(ob, idx, idx+1)
                mcpRna(ob).AnimLayerIndex += 1


    def interchange(self, ob, idx1, idx2):
        def addLayer(alayer, idx):
            pg = mcpRna(ob).AnimLayers.add()
            pg.name = alayer["name"]
            pg.action = alayer["action"]
            pg.index = idx

        nlayers = len(mcpRna(ob).AnimLayers)
        alayers = [dict(mcpRna(ob).AnimLayers[idx]) for idx in range(idx1, nlayers)]
        revlist = list(range(idx1, nlayers))
        revlist.reverse()
        for idx in revlist:
            mcpRna(ob).AnimLayers.remove(idx)
        addLayer(alayers[1], idx1)
        addLayer(alayers[0], idx2)
        for alayer in alayers[2:]:
            addLayer(alayer, alayer["index"])

        tracks = ob.animation_data.nla_tracks
        track1 = tracks[idx1]
        track2 = tracks[idx2]
        track = tracks.new(prev=track2)
        track.name = track1.name
        track.mute = track1.mute
        for strip1 in track1.strips:
            strip = track.strips.new(strip1.name, int(strip1.frame_start), strip1.action)
            strip.blend_type = 'COMBINE'
            strip.extrapolation = 'HOLD'
        tracks.remove(track1)

#----------------------------------------------------------
#   Bake animation layers
#----------------------------------------------------------

class MCP_OT_BakeAnimLayers(HidePropsOperator, FrameRange):
    bl_idname = "mcp.bake_animlayers"
    bl_label = "Bake Animation Layers"
    bl_description = "Bake all animation layers to a single action"
    bl_options = {'UNDO'}

    @classmethod
    def poll(self, context):
        ob = context.object
        return (ob and len(mcpRna(ob).AnimLayers) > 1)

    def run(self, context):
        from .loop import getActiveFrames
        rig = context.object
        tracks = rig.animation_data.nla_tracks
        strip = tracks[0].strips[0]
        scn = context.scene
        startFrame,endFrame = self.getStartEndFrame()
        frames = getActiveFrames(rig, startFrame, endFrame, strip.action)

        matss = dict([(pb.name, []) for pb in rig.pose.bones])
        for frame in frames:
            scn.frame_current = frame
            updateScene(context)
            for pb in rig.pose.bones:
                matss[pb.name].append((frame, pb.matrix_basis.copy()))

        def addMatrices(pb, bag, mats, comp, nidxs, channel, xyz=None):
            datapath = 'pose.bones["%s"].%s' % (pb.name, channel)
            fcus = [bag.fcurves.new(datapath, index=idx) for idx in range(nidxs)]
            for frame, mat in mats:
                ys = mat.decompose()[comp]
                if xyz:
                    ys = ys.to_euler(xyz)
                for y,fcu in zip(ys, fcus):
                    fcu.keyframe_points.insert(frame, y, options={'FAST'})

        pg = mcpRna(rig).AnimLayers[0]
        oact = pg.action
        act = setNewAction(rig, pg.name)
        act.use_fake_user = oact.use_fake_user
        bag = getActionBag(act)
        for pb in rig.pose.bones:
            mats = matss[pb.name]
            addMatrices(pb, bag, mats, 0, 3, "location")
            if pb.rotation_mode == 'QUATERNION':
                addMatrices(pb, bag, mats, 1, 4, "rotation_quaternion")
            else:
                addMatrices(pb, bag, mats, 1, 3, "rotation_euler", pb.rotation_mode)
            addMatrices(pb, bag, mats, 2, 3, "scale")

        mcpRna(rig).AnimLayers.clear()
        for track in list(tracks):
            tracks.remove(track)




#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_MT_ContextMenu,
    MCP_UL_AnimLayers,
    MCP_PT_AnimLayers,
    MCP_OT_AnimLayerAdd,
    MCP_OT_AnimLayerRemove,
    MCP_OT_AnimLayerMove,
    MCP_OT_BakeAnimLayers,
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)
