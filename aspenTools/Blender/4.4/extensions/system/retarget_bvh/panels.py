# SPDX-FileCopyrightText: 2019-2025, Thomas Larsson
#
# SPDX-License-Identifier: GPL-2.0-or-later

import bpy
from bpy.props import BoolProperty
from .utils import *
from .buildnumber import BUILD

#----------------------------------------------------------
#   Panels
#----------------------------------------------------------

class MCP_PT_Base:
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "BVH"
    bl_options = {'DEFAULT_CLOSED'}

#-------------------------------------------------------------
#   Main panel
#-------------------------------------------------------------

class MCP_PT_Main(MCP_PT_Base, bpy.types.Panel):
    bl_label = "Retarget BVH (version 5.0.0.%04d)" % BUILD
    bl_options = set()

    def draw(self, context):
        self.layout.operator("mcp.load_and_retarget")
        self.layout.separator()
        self.layout.operator("mcp.load_bvh")
        self.layout.operator("mcp.retarget_selected_to_active")
        self.layout.separator()
        self.layout.operator("mcp.mute_ik_constraints")
        self.layout.operator("mcp.unmute_ik_constraints")


class MCP_PT_Debug(MCP_PT_Base, bpy.types.Panel):
    bl_parent_id = "MCP_PT_Main"
    bl_label = "Debug"

    def draw(self, context):
        self.layout.operator("mcp.normalize_source_rig")
        self.layout.operator("mcp.normalize_target_rig")
        self.layout.separator()
        self.layout.operator("mcp.rename_active_to_selected")
        self.layout.operator("mcp.load_and_rename_bvh")
        self.layout.operator("mcp.retarget_renamed_to_active")

#-------------------------------------------------------------
#   Edit panel
#-------------------------------------------------------------

class MCP_PT_Edit(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "Edit Actions"

    def draw(self, context):
        self.layout.operator("mcp.remove_frame_zero")
        self.layout.operator("mcp.clear_bones")
        self.layout.operator("mcp.fix_genesis38")
        self.layout.operator("mcp.center_animation")
        #layout.operator("mcp.limbs_bend_positive")
        self.layout.operator("mcp.fixate_bone")
        self.layout.operator("mcp.transfer_to_peers")
        self.layout.operator("mcp.simplify_fcurves")
        self.layout.operator("mcp.timescale_fcurves")

        self.layout.separator()
        self.layout.operator("mcp.loop_fcurves")
        self.layout.operator("mcp.repeat_fcurves")
        self.layout.operator("mcp.stitch_actions")

#-------------------------------------------------------------
#    Y-axis panel
#-------------------------------------------------------------

class MCP_PT_YAxis(MCP_PT_Base, bpy.types.Panel):
    bl_label = "Y Axis"

    def draw(self, context):
        from mathutils import Vector
        bones = []
        n = 0
        for rig in context.view_layer.objects:
            if rig.select_get() and rig.type == 'ARMATURE':
                for pb in rig.pose.bones:
                    if P2B(pb).select:
                        bones.append( (mcpRna(pb).Bone, rig.name, n, rig, pb) )
                        n += 1

        bones.sort()
        for _,_,_,rig,pb in bones:
            quat = rig.matrix_world.to_quaternion()
            mat = quat.to_matrix().to_4x4() @ pb.matrix
            yaxis = Vector(mat.col[1][0:3])*100
            box = self.layout.box()
            box.label(text = "%s : %s" % (rig.name, pb.name))
            row = box.row()
            for n in range(3):
                row.label(text = "%.3f" % yaxis[n])

#-------------------------------------------------------------
#    Source rigs panel
#-------------------------------------------------------------

class MCP_PT_SourceRigs(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "Source Armature"

    def draw(self, context):
        scn = context.scene
        if not BD.sourceInfos:
            self.layout.operator("mcp.init_known_rigs")
            return
        self.layout.operator("mcp.init_known_rigs", text="Reinit Known Rigs")
        self.layout.prop(mcpRna(scn), "SourceRig")
        self.layout.prop(mcpRna(scn), "SourceTPose")
        self.layout.separator()
        self.layout.operator("mcp.identify_source_rig")
        self.layout.operator("mcp.verify_source_rig")
        self.layout.operator("mcp.list_source_rig")
        self.layout.operator("mcp.put_in_src_t_pose")

#-------------------------------------------------------------
#    Target rigs panel
#-------------------------------------------------------------

class MCP_PT_TargetRigs(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "Target Armature"

    def draw(self, context):
        rig = context.object
        scn = context.scene
        if not BD.targetInfos:
            self.layout.operator("mcp.init_known_rigs")
            return
        self.layout.operator("mcp.init_known_rigs", text="Reinit Known Rigs")
        self.layout.separator()
        self.layout.prop(mcpRna(scn), "TargetRig")
        self.layout.prop(mcpRna(scn), "TargetTPose")
        self.layout.prop(mcpRna(rig), "ReverseHip")
        self.layout.separator()
        self.layout.operator("mcp.identify_target_rig")
        self.layout.operator("mcp.verify_target_rig")
        self.layout.operator("mcp.list_target_rig")
        self.layout.operator("mcp.put_in_trg_t_pose")

#-------------------------------------------------------------
#   T-pose panel
#-------------------------------------------------------------

class MCP_PT_TPose(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "T-Pose"

    def draw(self, context):
        scn = context.scene
        self.layout.prop(mcpRna(scn), "SourceTPose", text="Source T-Pose")
        self.layout.prop(mcpRna(scn), "TargetTPose", text="Target T-Pose")
        self.layout.operator("mcp.put_in_src_t_pose")
        self.layout.operator("mcp.put_in_trg_t_pose")
        self.layout.separator()
        #self.layout.operator("mcp.define_t_pose")
        #self.layout.operator("mcp.undefine_t_pose")
        self.layout.operator("mcp.load_t_pose")
        self.layout.operator("mcp.save_t_pose")
        self.layout.operator("mcp.rest_current_pose")

#-------------------------------------------------------------
#   Action panel
#-------------------------------------------------------------

class MCP_PT_Actions(MCP_PT_Base, bpy.types.Panel, IsArmature):
    bl_label = "Actions"

    def draw(self, context):
        self.layout.operator("mcp.set_current_action")
        self.layout.operator("mcp.set_fake_user")
        self.layout.operator("mcp.set_all_fake_user")
        self.layout.operator("mcp.delete_action")
        self.layout.operator("mcp.delete_all_actions")
        self.layout.operator("mcp.delete_hash")

#----------------------------------------------------------
#   Initialize
#----------------------------------------------------------

classes = [
    MCP_PT_Main,
    MCP_PT_Debug,
    MCP_PT_Edit,
    MCP_PT_YAxis,
    MCP_PT_SourceRigs,
    MCP_PT_TargetRigs,
    MCP_PT_TPose,
    #MCP_PT_Actions,

    ErrorOperator,
    MessageOperator
]

def register():
    for cls in classes:
        bpy.utils.register_class(cls)


def unregister():
    for cls in classes:
        bpy.utils.unregister_class(cls)