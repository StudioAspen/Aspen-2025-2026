// Made with Amplify Shader Editor v1.9.8.1
// Available at the Unity Asset Store - http://u3d.as/y3X
Shader "MoreMountains/MMAdvancedToon_URP"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[Header(Albedo)]_MainTex("MainTex", 2D) = "white" {}
		_Tint("Tint", Color) = (1,1,1,1)
		[Header(Normal Map)]_Normal("Normal", 2D) = "bump" {}
		[Header(Ramp Texture)][Toggle(_USERAMPTEXTURE_ON)] _UseRampTexture("UseRampTexture", Float) = 0
		_RampTexture("RampTexture", 2D) = "white" {}
		[Header(Generated Ramp)]_RampDark("RampDark", Color) = (0.3490566,0.3490566,0.3490566,0)
		_RampLight("RampLight", Color) = (1,1,1,0)
		_StepWidth("StepWidth", Range( 0.05 , 1)) = 0.25
		[IntRange]_StepAmount("StepAmount", Range( 0 , 16)) = 2
		_RampOffset("RampOffset", Range( 0 , 1)) = 0.5
		[Header(Vertex Colors)][Toggle(_USEVERTEXCOLORS_ON)] _UseVertexColors("UseVertexColors", Float) = 0
		[Header(Shadow)]_ShadowColor("ShadowColor", Color) = (1,0,0.115766,1)
		_LightColor("LightColor", Color) = (1,1,1,1)
		_ShadowBlur("ShadowBlur", Range( 0.01 , 1)) = 1
		_ShadowStrength("ShadowStrength", Range( 0 , 1)) = 1
		_ShadowSize("ShadowSize", Range( 0.01 , 1)) = 0.5
		[KeywordEnum(Multiply,Replace,Lighten,HardMix)] _ShadowMixMode("ShadowMixMode", Float) = 0
		[Header(Specular)][Toggle(_USESPECULAR_ON)] _UseSpecular("UseSpecular", Float) = 0
		_SpecularSize("SpecularSize", Range( 0 , 1)) = 0.4
		_SpecularFalloff("SpecularFalloff", Range( 0 , 2)) = 1
		[HDR]_SpecularColor("SpecularColor", Color) = (2,2,2,1)
		_SpecularPower("SpecularPower", Float) = 1
		_SpecularForceUnderShadow("SpecularForceUnderShadow", Float) = 0
		[Header(Rim Light)][Toggle(_USERIMLIGHT_ON)] _UseRimLight("UseRimLight", Float) = 0
		_RimColor("RimColor", Color) = (0,0.7342432,1,1)
		_RimPower("RimPower", Range( 0 , 1)) = 0.6547081
		_RimAmount("RimAmount", Range( 0 , 1)) = 0.7
		[Toggle(_HIDERIMUNDERSHADOW_ON)] _HideRimUnderShadow("HideRimUnderShadow", Float) = 0
		[Toggle(_SHARPRIMLIGHT_ON)] _SharpRimLight("SharpRimLight", Float) = 1
		[Header(Emission)]_EmissionTexture("EmissionTexture", 2D) = "white" {}
		[HDR]_EmissionColor("EmissionColor", Color) = (2,2,2,1)
		_EmissionForce("EmissionForce", Float) = 0
		[Header(Animation)]_Framerate("Framerate", Float) = 5
		[Header(VertexOffset)][Toggle(_USEVERTEXOFFSET_ON)] _UseVertexOffset("UseVertexOffset", Float) = 0
		_VertexOffsetNoiseTexture("VertexOffsetNoiseTexture", 2D) = "white" {}
		_VertexOffsetFrequency("VertexOffsetFrequency", Float) = 2
		_VertexOffsetMagnitude("VertexOffsetMagnitude", Float) = 0.05
		_VertexOffsetX("VertexOffsetX", Float) = 0.5
		_VertexOffsetY("VertexOffsetY", Float) = 0.5
		_VertexOffsetZ("VertexOffsetZ", Float) = 0.5
		[Header(Outline)]_OutlineColor("OutlineColor", Color) = (0.5451996,1,0,1)
		_OutlineWidth("OutlineWidth", Float) = 0
		_OutlineAlpha("OutlineAlpha", Range( 0 , 1)) = 0
		[Header(SecondaryTexture)]_SecondaryTexture("SecondaryTexture", 2D) = "white" {}
		_SecondaryTextureStrength("SecondaryTextureStrength", Float) = 0
		_SecondaryTextureSize("SecondaryTextureSize", Float) = 1
		_SecondaryTextureSpeedFactor("SecondaryTextureSpeedFactor", Float) = 0
		[Header(ToneMapping)]_Desaturation("Desaturation", Range( 0 , 1)) = 0
		_Contrast("Contrast", Range( -1 , 0.99)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleUI] _AddPrecomputedVelocity("Add Precomputed Velocity", Float) = 1
		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0



		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off



		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		#pragma only_renderers d3d11 metal // ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL


		Pass
		{
			Name "ExtraPrePass"


			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite On
			ZTest LEqual
			Offset 0,0
			ColorMask RGBA



			HLSLPROGRAM

			#define ASE_VERSION 19801
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_VERT_POSITION
			#define ASE_NEEDS_VERT_NORMAL
			#pragma shader_feature_local _USEVERTEXCOLORS_ON
			#pragma shader_feature_local _USERAMPTEXTURE_ON
			#pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
			#pragma shader_feature_local _USESPECULAR_ON
			#pragma shader_feature_local _SHARPRIMLIGHT_ON
			#pragma shader_feature_local _USERIMLIGHT_ON
			#pragma shader_feature_local _USEVERTEXOFFSET_ON


			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD3;
				#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _LightColor;
			float4 _ShadowColor;
			float4 _Tint;
			float4 _MainTex_ST;
			float4 _EmissionColor;
			float4 _Normal_ST;
			float4 _RimColor;
			float4 _EmissionTexture_ST;
			float4 _SpecularColor;
			float4 _OutlineColor;
			float4 _RampDark;
			float4 _RampLight;
			float _SpecularFalloff;
			float _OutlineWidth;
			float _OutlineAlpha;
			float _Framerate;
			float _EmissionForce;
			float _StepAmount;
			float _VertexOffsetMagnitude;
			float _Contrast;
			float _VertexOffsetX;
			float _VertexOffsetZ;
			float _ShadowSize;
			float _ShadowBlur;
			float _ShadowStrength;
			float _StepWidth;
			float _SpecularSize;
			float _SecondaryTextureSize;
			float _VertexOffsetY;
			float _SpecularForceUnderShadow;
			float _RampOffset;
			float _SpecularPower;
			float _SecondaryTextureStrength;
			float _RimAmount;
			float _RimPower;
			float _VertexOffsetFrequency;
			float _SecondaryTextureSpeedFactor;
			float _Desaturation;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Normal;
			sampler2D _MainTex;
			sampler2D _RampTexture;
			sampler2D _EmissionTexture;
			sampler2D _VertexOffsetNoiseTexture;



			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float4 temp_cast_0 = (0.0).xxxx;
				half steppedTime293 = ( round( ( ( _TimeParameters.x ) * _Framerate ) ) / _Framerate );
				float3 temp_output_281_0 = ( input.positionOS.xyz * _VertexOffsetFrequency );
				half2 vertexOffsetXUV302 = ( steppedTime293 + (temp_output_281_0).xy );
				half2 vertexOffsetYUV303 = ( ( steppedTime293 * 2.0 ) + (temp_output_281_0).yz );
				half2 vertexOffsetZUV304 = ( ( steppedTime293 * 4.0 ) + (temp_output_281_0).xz );
				float4 appendResult308 = (float4(( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetXUV302, 0, 0.0) ).r - _VertexOffsetX ) , ( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetYUV303, 0, 0.0) ).r - _VertexOffsetY ) , ( tex2Dlod( _VertexOffsetNoiseTexture, float4( vertexOffsetZUV304, 0, 0.0) ).r - _VertexOffsetZ ) , 0.0));
				#ifdef _USEVERTEXOFFSET_ON
				float4 staticSwitch350 = ( _VertexOffsetMagnitude * appendResult308 );
				#else
				float4 staticSwitch350 = temp_cast_0;
				#endif
				float3 vertexOffset311 = (staticSwitch350).xyz;
				half3 outlineOffset488 = ( input.normalOS * _OutlineWidth );


				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = ( vertexOffset311 + outlineOffset488 );

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					output.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						output.fogFactorAndVertexLight.x = ComputeFogFactor(vertexInput.positionCS.z);
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				output.positionCS = vertexInput.positionCS;
				output.clipPosV = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;

				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;

				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				float3 WorldPosition = input.positionWS;
				float3 WorldViewDirection = GetWorldSpaceNormalizeViewDir( WorldPosition );
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = input.clipPosV;
				float4 ScreenPos = ComputeScreenPos( input.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = input.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float4 temp_output_2_0_g1 = _OutlineColor;


				float3 Color = (temp_output_2_0_g1).rgb;
				float Alpha = (temp_output_2_0_g1).a;
				float AlphaClipThreshold = _OutlineAlpha;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						Color.rgb = MixFogColor(Color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						Color.rgb = MixFog(Color.rgb, inputData.fogCoord);
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}


		Pass
		{

			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA



			HLSLPROGRAM

			#pragma multi_compile_fragment _ALPHATEST_ON
			#pragma shader_feature_local _RECEIVE_SHADOWS_OFF
			#define ASE_VERSION 19801
			#define ASE_SRP_VERSION 170003


			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
			#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
			#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SCREEN_POSITION
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma shader_feature_local _USEVERTEXCOLORS_ON
			#pragma shader_feature_local _USERAMPTEXTURE_ON
			#pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
			#pragma shader_feature_local _USESPECULAR_ON
			#pragma shader_feature_local _SHARPRIMLIGHT_ON
			#pragma shader_feature_local _USERIMLIGHT_ON
			#pragma shader_feature_local _USEVERTEXOFFSET_ON
			#pragma shader_feature_local _SHADOWMIXMODE_MULTIPLY _SHADOWMIXMODE_REPLACE _SHADOWMIXMODE_LIGHTEN _SHADOWMIXMODE_HARDMIX
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
			#pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _FORWARD_PLUS
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _FORWARD_PLUS
			#pragma multi_compile _ _LIGHT_LAYERS


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					half4 fogFactorAndVertexLight : TEXCOORD2;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD3;
				#endif
				#if defined(LIGHTMAP_ON)
					float4 lightmapUVOrVertexSH : TEXCOORD4;
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					float2 dynamicLightmapUV : TEXCOORD5;
				#endif
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_texcoord10 : TEXCOORD10;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _LightColor;
			float4 _ShadowColor;
			float4 _Tint;
			float4 _MainTex_ST;
			float4 _EmissionColor;
			float4 _Normal_ST;
			float4 _RimColor;
			float4 _EmissionTexture_ST;
			float4 _SpecularColor;
			float4 _OutlineColor;
			float4 _RampDark;
			float4 _RampLight;
			float _SpecularFalloff;
			float _OutlineWidth;
			float _OutlineAlpha;
			float _Framerate;
			float _EmissionForce;
			float _StepAmount;
			float _VertexOffsetMagnitude;
			float _Contrast;
			float _VertexOffsetX;
			float _VertexOffsetZ;
			float _ShadowSize;
			float _ShadowBlur;
			float _ShadowStrength;
			float _StepWidth;
			float _SpecularSize;
			float _SecondaryTextureSize;
			float _VertexOffsetY;
			float _SpecularForceUnderShadow;
			float _RampOffset;
			float _SpecularPower;
			float _SecondaryTextureStrength;
			float _RimAmount;
			float _RimPower;
			float _VertexOffsetFrequency;
			float _SecondaryTextureSpeedFactor;
			float _Desaturation;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Normal;
			sampler2D _MainTex;
			sampler2D _RampTexture;
			sampler2D _EmissionTexture;
			sampler2D _SecondaryTexture;


			half4 CalculateShadowMask216_g7(  )
			{
				#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
				half4 shadowMask = inputData.shadowMask;
				#elif !defined (LIGHTMAP_ON)
				half4 shadowMask = unity_ProbesOcclusion;
				#else
				half4 shadowMask = half4(1, 1, 1, 1);
				#endif
				return shadowMask;
			}

			float3 AdditionalLightsLambertMask17x( float3 WorldPosition, float2 ScreenUV, float3 WorldNormal, float4 ShadowMask )
			{
				float3 Color = 0;
				#if defined(_ADDITIONAL_LIGHTS)
					#define SUM_LIGHTLAMBERT(Light)\
						half3 AttLightColor = Light.color * ( Light.distanceAttenuation * Light.shadowAttenuation );\
						Color += LightingLambert( AttLightColor, Light.direction, WorldNormal );
					InputData inputData = (InputData)0;
					inputData.normalizedScreenSpaceUV = ScreenUV;
					inputData.positionWS = WorldPosition;
					uint meshRenderingLayers = GetMeshRenderingLayer();
					uint pixelLightCount = GetAdditionalLightsCount();
					#if USE_FORWARD_PLUS
					[loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
					{
						FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					}
					#endif

					LIGHT_LOOP_BEGIN( pixelLightCount )
						Light light = GetAdditionalLight(lightIndex, WorldPosition, ShadowMask);
						#ifdef _LIGHT_LAYERS
						if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
						#endif
						{
							SUM_LIGHTLAMBERT( light );
						}
					LIGHT_LOOP_END
				#endif
				return Color;
			}


			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				float2 uv_Normal = input.texcoord.xy * _Normal_ST.xy + _Normal_ST.zw;
				float3 normal83 = UnpackNormalScale( tex2Dlod( _Normal, float4( uv_Normal, 0, 0.0) ), 1.0f );
				float3 ase_tangentWS = TransformObjectToWorldDir( input.ase_tangent.xyz );
				float3 ase_normalWS = TransformObjectToWorldNormal( input.normalOS );
				float ase_tangentSign = input.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_bitangentWS = cross( ase_normalWS, ase_tangentWS ) * ase_tangentSign;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal33 = normal83;
				float3 worldNormal33 = normalize( float3( dot( tanToWorld0, tanNormal33 ), dot( tanToWorld1, tanNormal33 ), dot( tanToWorld2, tanNormal33 ) ) );
				float3 vertexToFrag80 = worldNormal33;
				output.ase_texcoord7.xyz = vertexToFrag80;
				output.ase_texcoord8.xyz = ase_tangentWS;
				output.ase_texcoord9.xyz = ase_normalWS;
				output.ase_texcoord10.xyz = ase_bitangentWS;

				output.ase_texcoord6.xy = input.texcoord.xy;
				output.ase_color = input.ase_color;

				//setting value to unused interpolator channels and avoid initialization warnings
				output.ase_texcoord6.zw = 0;
				output.ase_texcoord7.w = 0;
				output.ase_texcoord8.w = 0;
				output.ase_texcoord9.w = 0;
				output.ase_texcoord10.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				#if defined(LIGHTMAP_ON)
					OUTPUT_LIGHTMAP_UV(input.texcoord1, unity_LightmapST, output.lightmapUVOrVertexSH.xy);
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
					output.dynamicLightmapUV.xy = input.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif

				#if defined(ASE_FOG) || defined(_ADDITIONAL_LIGHTS_VERTEX)
					output.fogFactorAndVertexLight = 0;
					#if defined(ASE_FOG) && !defined(_FOG_FRAGMENT)
						output.fogFactorAndVertexLight.x = ComputeFogFactor(vertexInput.positionCS.z);
					#endif
					#ifdef _ADDITIONAL_LIGHTS_VERTEX
						half3 vertexLight = VertexLighting( vertexInput.positionWS, normalInput.normalWS );
						output.fogFactorAndVertexLight.yzw = vertexLight;
					#endif
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					output.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				output.positionCS = vertexInput.positionCS;
				output.clipPosV = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;
				output.ase_tangent = input.ase_tangent;
				output.ase_color = input.ase_color;
				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				output.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				output.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag ( PackedVaryings input
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						#ifdef _WRITE_RENDERING_LAYERS
						, out float4 outRenderingLayers : SV_Target1
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				float3 WorldPosition = input.positionWS;
				float3 WorldViewDirection = GetWorldSpaceNormalizeViewDir( WorldPosition );
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = input.clipPosV;
				float4 ScreenPos = ComputeScreenPos( input.clipPosV );

				float2 NormalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = input.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float2 uv_EmissionTexture = input.ase_texcoord6.xy * _EmissionTexture_ST.xy + _EmissionTexture_ST.zw;
				float4 computedEmission182 = ( ( tex2D( _EmissionTexture, uv_EmissionTexture ) * _EmissionColor ) * _EmissionForce );
				float3 vertexToFrag80 = input.ase_texcoord7.xyz;
				float3 normalizeResult81 = normalize( vertexToFrag80 );
				float dotResult34 = dot( normalizeResult81 , SafeNormalize( _MainLightPosition.xyz ) );
				float NdotL31 = dotResult34;
				float4 lerpResult277 = lerp( _RampDark , _RampLight , saturate( (( floor( ( NdotL31 / _StepWidth ) ) / _StepAmount )*0.5 + _RampOffset) ));
				float2 temp_cast_0 = (saturate( (NdotL31*0.5 + 0.5) )).xx;
				#ifdef _USERAMPTEXTURE_ON
				float4 staticSwitch3 = tex2D( _RampTexture, temp_cast_0 );
				#else
				float4 staticSwitch3 = lerpResult277;
				#endif
				float4 ramp51 = staticSwitch3;
				float ase_lightIntensity = max( max( _MainLightColor.r, _MainLightColor.g ), _MainLightColor.b ) + 1e-7;
				float4 ase_lightColor = float4( _MainLightColor.rgb / ase_lightIntensity, ase_lightIntensity );
				float3 worldPosValue184_g7 = WorldPosition;
				float3 WorldPosition173_g7 = worldPosValue184_g7;
				float4 ase_positionSSNorm = ScreenPos / ScreenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 ScreenUV183_g7 = (ase_positionSSNorm).xy;
				float2 ScreenUV173_g7 = ScreenUV183_g7;
				float3 ase_tangentWS = input.ase_texcoord8.xyz;
				float3 ase_normalWS = input.ase_texcoord9.xyz;
				float3 ase_bitangentWS = input.ase_texcoord10.xyz;
				float3 tanToWorld0 = float3( ase_tangentWS.x, ase_bitangentWS.x, ase_normalWS.x );
				float3 tanToWorld1 = float3( ase_tangentWS.y, ase_bitangentWS.y, ase_normalWS.y );
				float3 tanToWorld2 = float3( ase_tangentWS.z, ase_bitangentWS.z, ase_normalWS.z );
				float3 tanNormal12_g7 = float3(0,0,1);
				float3 worldNormal12_g7 = normalize( float3( dot( tanToWorld0, tanNormal12_g7 ), dot( tanToWorld1, tanNormal12_g7 ), dot( tanToWorld2, tanNormal12_g7 ) ) );
				float3 worldNormalValue185_g7 = worldNormal12_g7;
				float3 WorldNormal173_g7 = worldNormalValue185_g7;
				half4 localCalculateShadowMask216_g7 = CalculateShadowMask216_g7();
				float4 shadowMaskValue182_g7 = localCalculateShadowMask216_g7;
				float4 ShadowMask173_g7 = shadowMaskValue182_g7;
				float3 localAdditionalLightsLambertMask17x173_g7 = AdditionalLightsLambertMask17x( WorldPosition173_g7 , ScreenUV173_g7 , WorldNormal173_g7 , ShadowMask173_g7 );
				float2 texCoord382 = input.ase_texcoord6.xy * float2( 1,1 ) + float2( 0,0 );
				half steppedTime293 = ( round( ( ( _TimeParameters.x ) * _Framerate ) ) / _Framerate );
				float2 uv_MainTex = input.ase_texcoord6.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 temp_cast_3 = (1.0).xxxx;
				#ifdef _USEVERTEXCOLORS_ON
				float4 staticSwitch7 = input.ase_color;
				#else
				float4 staticSwitch7 = temp_cast_3;
				#endif
				float4 blendOpSrc460 = ( tex2D( _SecondaryTexture, ( ( texCoord382 * _SecondaryTextureSize ) + ( steppedTime293 * _SecondaryTextureSpeedFactor ) ) ) * _SecondaryTextureStrength );
				float4 blendOpDest460 = ( ( tex2D( _MainTex, uv_MainTex ) * _Tint ) * staticSwitch7 );
				float4 albedo11 = ( saturate( ( blendOpDest460 - blendOpSrc460 ) ));
				float4 temp_output_73_0 = ( ( ( ramp51 * float4( ase_lightColor.rgb , 0.0 ) ) + float4( localAdditionalLightsLambertMask17x173_g7 , 0.0 ) ) * albedo11 );
				float temp_output_120_0 = ( 1.0 - _SpecularSize );
				float dotResult106 = dot( WorldViewDirection , ase_normalWS );
				float2 uv_Normal = input.ase_texcoord6.xy * _Normal_ST.xy + _Normal_ST.zw;
				float3 normal83 = UnpackNormalScale( tex2D( _Normal, uv_Normal ), 1.0f );
				float3 tanNormal99 = normal83;
				float3 worldNormal99 = float3( dot( tanToWorld0, tanNormal99 ), dot( tanToWorld1, tanNormal99 ), dot( tanToWorld2, tanNormal99 ) );
				float dotResult110 = dot( WorldViewDirection , -reflect( SafeNormalize( _MainLightPosition.xyz ) , worldNormal99 ) );
				float specular113 = ( pow( dotResult106 , _SpecularFalloff ) * dotResult110 );
				float specularDelta116 = fwidth( specular113 );
				float smoothstepResult121 = smoothstep( temp_output_120_0 , ( temp_output_120_0 + specularDelta116 ) , specular113);
				float temp_output_2_0_g2 = _ShadowStrength;
				float temp_output_3_0_g2 = ( 1.0 - temp_output_2_0_g2 );
				float3 appendResult7_g2 = (float3(temp_output_3_0_g2 , temp_output_3_0_g2 , temp_output_3_0_g2));
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoords );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float clampResult189 = clamp( ase_lightAtten , 0.0 , 1.0 );
				float lerpResult409 = lerp( clampResult189 , step( _ShadowSize , clampResult189 ) , _ShadowBlur);
				float temp_output_191_0 = pow( lerpResult409 , _ShadowBlur );
				float4 lerpResult194 = lerp( float4( ( ( _ShadowColor.rgb * temp_output_2_0_g2 ) + appendResult7_g2 ) , 0.0 ) , _LightColor , temp_output_191_0);
				float4 shadow195 = lerpResult194;
				float4 temp_cast_8 = (_SpecularForceUnderShadow).xxxx;
				float4 temp_output_274_0 = round( pow( max( shadow195 , float4( 0.9528302,0.9528302,0.9528302,0 ) ) , temp_cast_8 ) );
				float4 specularIntensity124 = ( ( _SpecularPower * smoothstepResult121 ) * temp_output_274_0 );
				float4 computedSpecular133 = ( temp_output_274_0 * ( specular113 * _SpecularColor * saturate( specularIntensity124 ) ) );
				#ifdef _USESPECULAR_ON
				float4 staticSwitch137 = ( ( temp_output_73_0 * ( 1.0 - specularIntensity124 ) ) + computedSpecular133 );
				#else
				float4 staticSwitch137 = temp_output_73_0;
				#endif
				float4 litColor422 = staticSwitch137;
				float shadowArea411 = temp_output_191_0;
				float4 blendOpSrc410 = litColor422;
				float4 blendOpDest410 = shadow195;
				float4 blendOpSrc430 = litColor422;
				float4 blendOpDest430 = shadow195;
				#if defined( _SHADOWMIXMODE_MULTIPLY )
				float4 staticSwitch420 = ( litColor422 * shadow195 );
				#elif defined( _SHADOWMIXMODE_REPLACE )
				float4 staticSwitch420 = ( ( litColor422 * shadowArea411 ) + ( shadow195 * ( 1.0 - shadowArea411 ) ) );
				#elif defined( _SHADOWMIXMODE_LIGHTEN )
				float4 staticSwitch420 = ( saturate( max( blendOpSrc410, blendOpDest410 ) ));
				#elif defined( _SHADOWMIXMODE_HARDMIX )
				float4 staticSwitch420 = ( saturate(  round( 0.5 * ( blendOpSrc430 + blendOpDest430 ) ) ));
				#else
				float4 staticSwitch420 = ( litColor422 * shadow195 );
				#endif
				float4 shadowMix435 = staticSwitch420;
				float4 temp_cast_9 = (0.0).xxxx;
				float rimAmount169 = _RimAmount;
				float3 tanNormal87 = normal83;
				float3 worldNormal87 = float3( dot( tanToWorld0, tanNormal87 ), dot( tanToWorld1, tanNormal87 ), dot( tanToWorld2, tanNormal87 ) );
				float dotResult89 = dot( worldNormal87 , WorldViewDirection );
				float NdotV90 = dotResult89;
				#ifdef _HIDERIMUNDERSHADOW_ON
				float staticSwitch166 = NdotL31;
				#else
				float staticSwitch166 = 1.0;
				#endif
				float temp_output_148_0 = ( ( 1.0 - NdotV90 ) * pow( staticSwitch166 , _RimPower ) );
				float smoothstepResult150 = smoothstep( ( rimAmount169 - 0.01 ) , ( 0.01 + rimAmount169 ) , temp_output_148_0);
				#ifdef _SHARPRIMLIGHT_ON
				float staticSwitch168 = smoothstepResult150;
				#else
				float staticSwitch168 = ( rimAmount169 * temp_output_148_0 );
				#endif
				#ifdef _USERIMLIGHT_ON
				float4 staticSwitch164 = ( staticSwitch168 * _RimColor );
				#else
				float4 staticSwitch164 = temp_cast_9;
				#endif
				float4 rimLight157 = staticSwitch164;
				float4 preToneMapping438 = ( shadowMix435 + rimLight157 );
				float grayscale442 = Luminance(preToneMapping438.rgb);
				float4 temp_cast_11 = (grayscale442).xxxx;
				float4 lerpResult444 = lerp( preToneMapping438 , temp_cast_11 , _Desaturation);
				float4 temp_cast_12 = (_Contrast).xxxx;
				float4 postToneMapping439 = (float4( 0,0,0,0 ) + (lerpResult444 - temp_cast_12) * (float4( 1,1,1,0 ) - float4( 0,0,0,0 )) / (float4( 1,1,1,0 ) - temp_cast_12));
				float4 lightCol68 = postToneMapping439;
				float4 temp_output_485_0 = ( computedEmission182 + lightCol68 );

				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = temp_output_485_0.rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData = (InputData)0;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;

				#ifdef ASE_FOG
					inputData.fogCoord = InitializeInputDataFog(float4(inputData.positionWS, 1.0), input.fogFactorAndVertexLight.x);
				#endif
				#ifdef _ADDITIONAL_LIGHTS_VERTEX
					inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
				#endif

				inputData.normalizedScreenSpaceUV = NormalizedScreenSpaceUV;

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(input.positionCS, Color);
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						Color.rgb = MixFogColor(Color.rgb, half3(0,0,0), inputData.fogCoord);
					#else
						Color.rgb = MixFog(Color.rgb, inputData.fogCoord);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}


		Pass
		{

			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM

			#pragma multi_compile _ALPHATEST_ON
			#define ASE_VERSION 19801
			#define ASE_SRP_VERSION 170003


			#pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_SHADOWCASTER

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#pragma shader_feature_local _USEVERTEXCOLORS_ON
			#pragma shader_feature_local _USERAMPTEXTURE_ON
			#pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
			#pragma shader_feature_local _USESPECULAR_ON
			#pragma shader_feature_local _SHARPRIMLIGHT_ON
			#pragma shader_feature_local _USERIMLIGHT_ON
			#pragma shader_feature_local _USEVERTEXOFFSET_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _LightColor;
			float4 _ShadowColor;
			float4 _Tint;
			float4 _MainTex_ST;
			float4 _EmissionColor;
			float4 _Normal_ST;
			float4 _RimColor;
			float4 _EmissionTexture_ST;
			float4 _SpecularColor;
			float4 _OutlineColor;
			float4 _RampDark;
			float4 _RampLight;
			float _SpecularFalloff;
			float _OutlineWidth;
			float _OutlineAlpha;
			float _Framerate;
			float _EmissionForce;
			float _StepAmount;
			float _VertexOffsetMagnitude;
			float _Contrast;
			float _VertexOffsetX;
			float _VertexOffsetZ;
			float _ShadowSize;
			float _ShadowBlur;
			float _ShadowStrength;
			float _StepWidth;
			float _SpecularSize;
			float _SecondaryTextureSize;
			float _VertexOffsetY;
			float _SpecularForceUnderShadow;
			float _RampOffset;
			float _SpecularPower;
			float _SecondaryTextureStrength;
			float _RimAmount;
			float _RimPower;
			float _VertexOffsetFrequency;
			float _SecondaryTextureSpeedFactor;
			float _Desaturation;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Normal;
			sampler2D _MainTex;
			sampler2D _RampTexture;
			sampler2D _EmissionTexture;



			float3 _LightDirection;
			float3 _LightPosition;

			PackedVaryings VertexFunction( Attributes input )
			{
				PackedVaryings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( output );



				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				float3 positionWS = TransformObjectToWorld( input.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					output.positionWS = positionWS;
				#endif

				float3 normalWS = TransformObjectToWorldDir(input.normalOS);

				#if _CASTING_PUNCTUAL_LIGHT_SHADOW
					float3 lightDirectionWS = normalize(_LightPosition - positionWS);
				#else
					float3 lightDirectionWS = _LightDirection;
				#endif

				float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));

				//code for UNITY_REVERSED_Z is moved into Shadows.hlsl from 6000.0.22 and or higher
				positionCS = ApplyShadowClamping(positionCS);

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					output.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				output.positionCS = positionCS;
				output.clipPosV = positionCS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;

				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;

				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( input );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = input.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = input.clipPosV;
				float4 ScreenPos = ComputeScreenPos( input.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = input.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif



				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}


		Pass
		{

			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM

			#pragma multi_compile _ALPHATEST_ON
			#define ASE_VERSION 19801
			#define ASE_SRP_VERSION 170003


			#pragma vertex vert
			#pragma fragment frag

			#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#pragma shader_feature_local _USEVERTEXCOLORS_ON
			#pragma shader_feature_local _USERAMPTEXTURE_ON
			#pragma shader_feature_local _HIDERIMUNDERSHADOW_ON
			#pragma shader_feature_local _USESPECULAR_ON
			#pragma shader_feature_local _SHARPRIMLIGHT_ON
			#pragma shader_feature_local _USERIMLIGHT_ON
			#pragma shader_feature_local _USEVERTEXOFFSET_ON


			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE) && (SHADER_TARGET >= 45)
				#define ASE_SV_DEPTH SV_DepthLessEqual
				#define ASE_SV_POSITION_QUALIFIERS linear noperspective centroid
			#else
				#define ASE_SV_DEPTH SV_Depth
				#define ASE_SV_POSITION_QUALIFIERS
			#endif

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct PackedVaryings
			{
				ASE_SV_POSITION_QUALIFIERS float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _LightColor;
			float4 _ShadowColor;
			float4 _Tint;
			float4 _MainTex_ST;
			float4 _EmissionColor;
			float4 _Normal_ST;
			float4 _RimColor;
			float4 _EmissionTexture_ST;
			float4 _SpecularColor;
			float4 _OutlineColor;
			float4 _RampDark;
			float4 _RampLight;
			float _SpecularFalloff;
			float _OutlineWidth;
			float _OutlineAlpha;
			float _Framerate;
			float _EmissionForce;
			float _StepAmount;
			float _VertexOffsetMagnitude;
			float _Contrast;
			float _VertexOffsetX;
			float _VertexOffsetZ;
			float _ShadowSize;
			float _ShadowBlur;
			float _ShadowStrength;
			float _StepWidth;
			float _SpecularSize;
			float _SecondaryTextureSize;
			float _VertexOffsetY;
			float _SpecularForceUnderShadow;
			float _RampOffset;
			float _SpecularPower;
			float _SecondaryTextureStrength;
			float _RimAmount;
			float _RimPower;
			float _VertexOffsetFrequency;
			float _SecondaryTextureSpeedFactor;
			float _Desaturation;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _Normal;
			sampler2D _MainTex;
			sampler2D _RampTexture;
			sampler2D _EmissionTexture;



			PackedVaryings VertexFunction( Attributes input  )
			{
				PackedVaryings output = (PackedVaryings)0;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);



				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = input.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					input.positionOS.xyz = vertexValue;
				#else
					input.positionOS.xyz += vertexValue;
				#endif

				input.normalOS = input.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( input.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					output.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					output.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				output.positionCS = vertexInput.positionCS;
				output.clipPosV = vertexInput.positionCS;
				return output;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 positionOS : INTERNALTESSPOS;
				float3 normalOS : NORMAL;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( Attributes input )
			{
				VertexControl output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				output.positionOS = input.positionOS;
				output.normalOS = input.normalOS;

				return output;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> input)
			{
				TessellationFactors output;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(input[0].positionOS, input[1].positionOS, input[2].positionOS, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				output.edge[0] = tf.x; output.edge[1] = tf.y; output.edge[2] = tf.z; output.inside = tf.w;
				return output;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			PackedVaryings DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				Attributes output = (Attributes) 0;
				output.positionOS = patch[0].positionOS * bary.x + patch[1].positionOS * bary.y + patch[2].positionOS * bary.z;
				output.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;

				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = output.positionOS.xyz - patch[i].normalOS * (dot(output.positionOS.xyz, patch[i].normalOS) - dot(patch[i].positionOS.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				output.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * output.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], output);
				return VertexFunction(output);
			}
			#else
			PackedVaryings vert ( Attributes input )
			{
				return VertexFunction( input );
			}
			#endif

			half4 frag(PackedVaryings input
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( input );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = input.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float4 ClipPos = input.clipPosV;
				float4 ScreenPos = ComputeScreenPos( input.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = input.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif



				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef ASE_DEPTH_WRITE_ON
					float DepthValue = input.positionCS.z;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#if defined(LOD_FADE_CROSSFADE)
					LODFadeCrossFade( input.positionCS );
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}


	}

	CustomEditor "UnityEditor.ShaderGraphUnlitGUI"
	FallBack "Hidden/Shader Graph/FallbackError"

	Fallback Off
}
/*ASEBEGIN
Version=19801
Node;AmplifyShaderEditor.CommentaryNode;84;-6398.291,-2500.077;Inherit;False;810.3552;580.1461;Normal Map;2;82;83;Normal Map;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;135;-4270.331,273.337;Inherit;False;2352.279;2466.376;Specular;39;224;133;131;119;126;129;127;124;121;122;123;117;120;116;118;115;114;113;112;108;110;107;109;106;104;111;101;105;99;100;98;244;245;247;248;269;271;272;274;Specular;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;35;-6394.275,-1621.117;Inherit;False;1336.096;443.0846;NdotL;7;85;31;34;81;32;80;33;NdotL;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;210;-4316.516,-1149.049;Inherit;False;3210.925;1296.744;Shadow;16;191;190;189;188;195;194;204;192;198;193;407;408;409;411;498;499;Shadow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;378;-6382.085,-400.4365;Inherit;False;1421.198;341.5015;Comment;6;288;289;290;291;292;293;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-4303.229,-2444.127;Inherit;False;2993.004;1198.177;Ramp;20;51;3;4;76;37;36;38;277;50;276;275;78;93;79;49;94;41;40;39;6;Ramp;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;15;-4365.522,-4292.567;Inherit;False;2898.835;1634.152;Albedo;19;11;14;7;10;12;2;13;1;379;380;382;383;384;381;387;388;389;457;460;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;175;-630.5005,-3179.538;Inherit;False;4182.399;1269.731;Custom Lighting;20;70;72;139;65;69;134;141;140;73;159;438;158;437;137;138;422;68;440;502;504;Custom Lighting;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;91;-6389.196,-936.1287;Inherit;False;1133.981;413.4298;NdotV;5;86;87;88;89;90;NdotV;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;173;-4246.42,4273.515;Inherit;False;3617.067;1146.109;Rim Light;23;157;164;165;156;168;142;172;150;155;171;148;152;147;170;151;146;163;166;144;169;149;167;143;Rim Light;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;436;-637.6216,-1760.471;Inherit;False;1728.671;1740.177;Shadow Mix;16;412;419;426;417;427;249;418;421;414;415;410;430;420;250;425;435;Shadow Mix;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;456;-624.9999,117.4403;Inherit;False;1569.145;562.9836;Tone Mapping;7;441;444;442;443;452;448;439;Tone Mapping;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;332;-4249.92,5639.075;Inherit;False;3131.241;1853.44;Vertex Offset;37;311;362;350;359;310;308;309;315;331;329;314;327;326;323;328;330;313;322;325;324;304;302;303;299;297;295;294;301;286;285;287;300;298;296;281;280;279;Vertex Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;376;1728,-16;Inherit;False;1278.943;484.4639;Outline;4;364;356;354;377;Outline;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;174;2280.463,-1682.002;Inherit;False;1258.342;857.4734;Final;6;71;184;474;492;496;497;Final;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;187;-4270.963,3064.128;Inherit;False;2278.231;843.0205;Emission;6;182;180;178;179;176;177;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;82;-6342.859,-2240.926;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;True;1;Header(Normal Map);False;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;83;-5943.662,-2244.336;Float;False;normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-4220.331,1041.668;Inherit;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;99;-3991.64,1032.872;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ReflectOpNode;101;-3720.432,980.0969;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;105;-4040.847,367.337;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;111;-4045.882,527.1082;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;85;-6355.366,-1550.021;Inherit;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;104;-3497.604,980.0958;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-3805.461,615.0662;Float;False;Property;_SpecularFalloff;SpecularFalloff;19;0;Create;True;0;0;0;True;0;False;1;1;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;106;-3685.25,453.8093;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;33;-6122.435,-1556.394;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.VertexToFragmentNode;80;-5906.466,-1539.523;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;189;-3206.193,-689.9624;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;110;-3286.501,890.6708;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;408;-3023.774,-802.4628;Float;False;Property;_ShadowSize;ShadowSize;15;0;Create;True;0;0;0;True;0;False;0.5;0.281;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;108;-3496.139,481.6629;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;81;-5646.468,-1541.523;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;32;-6102.436,-1366.394;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;190;-2990.67,-102.864;Float;False;Property;_ShadowBlur;ShadowBlur;13;0;Create;True;0;0;0;True;0;False;1;0.538;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;112;-3053.41,683.9678;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;407;-2811.39,-640.3524;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;288;-6332.085,-173.935;Float;False;Property;_Framerate;Framerate;32;0;Create;True;0;0;0;False;1;Header(Animation);False;5;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;289;-6312.839,-350.4365;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;34;-5502.437,-1442.394;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;193;-2490.299,-1029.671;Float;False;Property;_ShadowColor;ShadowColor;11;0;Create;True;0;0;0;False;1;Header(Shadow);False;1,0,0.115766,1;0.3867923,0.3867923,0.3867923,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;409;-2603.832,-376.1035;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;-2813.017,615.7037;Float;True;specular;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-2513.879,-815.6242;Float;False;Property;_ShadowStrength;ShadowStrength;14;0;Create;True;0;0;0;True;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;290;-6021.639,-288.0374;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;204;-2095.522,-941.0401;Inherit;True;Lerp White To;-1;;2;047d7c189c36a62438973bad9d37b1c2;0;2;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;192;-2257.108,-683.5875;Float;False;Property;_LightColor;LightColor;12;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.PowerNode;191;-2214.937,-386.631;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-5327.133,-1423.386;Float;False;NdotL;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;114;-3063.968,898.2288;Inherit;False;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RoundOpNode;291;-5831.839,-272.4373;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-4121.739,-2254.808;Float;False;Property;_StepWidth;StepWidth;7;0;Create;True;0;0;0;True;0;False;0.25;0.25;0.05;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;194;-1831.83,-694.3508;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;-4003.614,-2355.187;Inherit;False;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;292;-5675.839,-264.6375;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;40;-3758.223,-2310.115;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;116;-2643.596,884.6198;Float;False;specularDelta;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;118;-4092.129,1294.172;Float;False;Property;_SpecularSize;SpecularSize;18;0;Create;True;0;0;0;True;0;False;0.4;0.4;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;293;-5281.887,-262.0375;Half;False;steppedTime;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;41;-3594.712,-2315.081;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;94;-3705.001,-2058.321;Float;False;Property;_StepAmount;StepAmount;8;1;[IntRange];Create;True;0;0;0;False;0;False;2;4;0;16;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;120;-3772.225,1306.375;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;-3565.063,1433.395;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;379;-4204.608,-3137.401;Float;False;Property;_SecondaryTextureSize;SecondaryTextureSize;45;0;Create;True;0;0;0;True;0;False;1;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;269;-3759.183,1735.286;Inherit;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0.9528302,0.9528302,0.9528302,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;36;-3301.19,-1534.746;Inherit;False;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;272;-3669.064,2027.191;Float;False;Property;_SpecularForceUnderShadow;SpecularForceUnderShadow;22;0;Create;True;0;0;0;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-4200.861,-2858.142;Float;False;Property;_SecondaryTextureSpeedFactor;SecondaryTextureSpeedFactor;46;0;Create;True;0;0;0;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;382;-4218.378,-3285.435;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;38;-3361.386,-1414.372;Float;False;Constant;_RampScaleAndOffset;RampScaleAndOffset;8;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-4235.591,-2985.924;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;79;-3545.247,-1836.601;Float;False;Property;_RampOffset;RampOffset;9;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;93;-3567.75,-1934.395;Float;False;Constant;_RampScale;RampScale;7;0;Create;True;0;0;0;True;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;123;-3654.278,1211.111;Inherit;False;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;49;-3337.018,-2218.369;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;383;-3877.554,-3233.795;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;388;-3890.861,-2978.142;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;78;-3229.247,-1969.252;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;271;-3353.143,1784.866;Inherit;True;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;21.47;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;121;-3355.371,1338.375;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;37;-3021.036,-1476.643;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;244;-3270.683,1178.104;Float;False;Property;_SpecularPower;SpecularPower;21;0;Create;True;0;0;0;True;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;76;-2732.488,-1511.635;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;245;-3013.739,1316.68;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;275;-2920.151,-2366.576;Float;False;Property;_RampDark;RampDark;5;0;Create;True;0;0;0;True;1;Header(Generated Ramp);False;0.3490566,0.3490566,0.3490566,0;0.3490564,0.3490564,0.3490564,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;50;-2872.888,-1959.341;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;2;-4195.963,-4028.957;Float;False;Property;_Tint;Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.VertexColorNode;13;-4315.522,-3674.198;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;12;-4328.115,-3815.262;Float;False;Constant;_NoVertexColor;NoVertexColor;8;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-4217.489,-4242.567;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;True;1;Header(Albedo);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RoundOpNode;274;-3033.939,1801.531;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;276;-2889.599,-2180.924;Float;False;Property;_RampLight;RampLight;6;0;Create;True;0;0;0;True;0;False;1,1,1,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;389;-3684.861,-3054.142;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;4;-2482.249,-1603.606;Inherit;True;Property;_RampTexture;RampTexture;4;0;Create;True;0;0;0;True;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-3858.242,-4062.196;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;277;-2329.182,-2006.194;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;381;-3506.364,-3460.195;Inherit;True;Property;_SecondaryTexture;SecondaryTexture;43;0;Create;True;0;0;0;False;1;Header(SecondaryTexture);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;380;-3502.459,-3222.047;Float;False;Property;_SecondaryTextureStrength;SecondaryTextureStrength;44;0;Create;True;0;0;0;True;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;7;-4068.787,-3745.562;Float;False;Property;_UseVertexColors;UseVertexColors;10;0;Create;True;0;0;0;True;1;Header(Vertex Colors);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;247;-2795.206,1632.378;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-2255.071,1262.254;Float;True;specularIntensity;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;3;-1964.97,-1941.765;Float;True;Property;_UseRampTexture;UseRampTexture;3;0;Create;True;0;0;0;True;1;Header(Ramp Texture);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-3658.77,-3889.004;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;457;-3169.096,-3442.503;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;460;-3006.056,-3726.161;Inherit;True;Subtract;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;127;-4124.793,2452.915;Inherit;False;124;specularIntensity;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;86;-6339.196,-878.3267;Inherit;False;83;normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;119;-3987.218,2244.336;Float;False;Property;_SpecularColor;SpecularColor;20;1;[HDR];Create;True;0;0;0;True;0;False;2,2,2,1;2,2,2,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;129;-3829.799,2458.054;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;139;-185.6951,-2438.733;Inherit;True;124;specularIntensity;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;87;-6061.468,-886.1287;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;88;-6061.468,-706.6989;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-3619.327,2223.57;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;89;-5771.26,-811.2357;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;140;92.18002,-2438.526;Inherit;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;342.0722,-2535.595;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-4002.27,4458.294;Float;False;Property;_RimAmount;RimAmount;26;0;Create;True;0;0;0;True;0;False;0.7;0.7;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;167;-4093.783,4677.084;Float;True;Constant;_DontHideRimUnderShadow;DontHideRimUnderShadow;20;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-5498.219,-809.6757;Float;False;NdotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;320.2142,-2215.196;Inherit;True;133;computedSpecular;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;149;-4076.015,4938.533;Inherit;True;31;NdotL;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;144;-3817.804,5113.262;Float;False;Property;_RimPower;RimPower;25;0;Create;True;0;0;0;True;0;False;0.6547081;0.6547081;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;651.1277,-2478.866;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;169;-3654.976,4457.298;Float;False;rimAmount;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-3382.022,4583.278;Inherit;False;90;NdotV;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;170;-3190.997,5000.548;Inherit;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-2933.597,5005.695;Float;False;Constant;_RimAmountAdjuster;RimAmountAdjuster;17;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;137;899.6168,-2648.938;Float;True;Property;_UseSpecular;UseSpecular;17;0;Create;True;0;0;0;True;1;Header(Specular);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;-1839.482,-323.2064;Float;False;shadowArea;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;146;-3097.397,4572.993;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;147;-3410.963,4913;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-2787.997,4639.093;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;152;-2664.497,5065.494;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;171;-2586.136,4469.81;Inherit;False;169;rimAmount;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;155;-2657.997,4913.394;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;412;-587.6216,-1075.267;Inherit;True;411;shadowArea;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;150;-2405.541,4849.111;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;426;-523.2374,-1408.464;Inherit;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;172;-2379.181,4559.983;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;417;-282.1374,-907.5627;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;419;-307.2184,-1147.121;Inherit;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;427;-163.255,-525.0162;Inherit;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;418;-17.89666,-1020.36;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;421;-195.0661,-410.7025;Inherit;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;414;-318.0788,-1385.818;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;249;42.56085,-1574.666;Inherit;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;142;-1992.58,5045.396;Float;False;Property;_RimColor;RimColor;24;0;Create;True;0;0;0;True;0;False;0,0.7342432,1,1;0,0.7342432,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;415;221.7419,-1117.513;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;156;-1699.836,4854.964;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;165;-1640.464,4699.232;Float;False;Constant;_DefaultRimLight;DefaultRimLight;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;430;221.9494,-278.2941;Inherit;True;HardMix;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;367.6071,-1629.438;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendOpsNode;410;226.3874,-590.3668;Inherit;True;Lighten;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;164;-1408.503,4788.544;Float;False;Property;_UseRimLight;UseRimLight;23;0;Create;True;0;0;0;True;1;Header(Rim Light);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;420;677.1989,-1358.637;Float;False;Property;_ShadowMixMode;ShadowMixMode;16;0;Create;True;0;0;0;False;0;False;0;0;0;True;;KeywordEnum;4;Multiply;Replace;Lighten;HardMix;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;435;848.0494,-1509.184;Float;False;shadowMix;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;157;-1113.162,4847.988;Float;False;rimLight;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;159;1624.691,-2350.295;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;438;1920.378,-2351.102;Float;False;preToneMapping;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCGrayscale;442;-312.9865,295.9985;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;443;-406.1073,400.7591;Float;False;Property;_Desaturation;Desaturation;47;0;Create;True;0;0;0;False;1;Header(ToneMapping);False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;448;-77.80162,544.4241;Float;False;Property;_Contrast;Contrast;48;0;Create;True;0;0;0;True;0;False;0;0;-1;0.99;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;444;-49.92125,251.7669;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;452;312.1986,427.4243;Inherit;True;5;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;3;COLOR;0,0,0,0;False;4;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;439;676.1446,423.965;Float;False;postToneMapping;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;68;2654.833,-2380.81;Float;False;lightCol;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;279;-3584.773,6111.28;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;280;-3656.927,6318.275;Float;False;Property;_VertexOffsetFrequency;VertexOffsetFrequency;35;0;Create;True;0;0;0;True;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;296;-2970.528,6241.574;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;281;-3277.327,6185.674;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;298;-2967.928,6470.375;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;300;-2666.328,6214.274;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;301;-2658.528,6424.874;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;286;-2941.927,6344.274;Inherit;False;False;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;287;-2934.128,6575.674;Inherit;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;294;-3022.528,6016.673;Inherit;False;293;steppedTime;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;285;-2939.327,6110.274;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;299;-2502.528,6509.375;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;297;-2520.728,6298.774;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;295;-2648.128,6066.074;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;303;-2352.263,6301.432;Half;False;vertexOffsetYUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;302;-2421.174,6069.311;Half;False;vertexOffsetXUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;304;-2326.875,6497.284;Half;False;vertexOffsetZUV;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;322;-4199.92,6551.378;Float;True;Property;_VertexOffsetNoiseTexture;VertexOffsetNoiseTexture;34;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;324;-4183.92,7066.577;Inherit;False;303;vertexOffsetYUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;313;-4194.428,6829.375;Inherit;False;302;vertexOffsetXUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;325;-4183.92,7306.577;Inherit;False;304;vertexOffsetZUV;1;0;OBJECT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;327;-3844.72,7229.777;Inherit;True;Property;_TextureSample2;Texture Sample 2;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;323;-3831.92,6660.178;Inherit;True;Property;_TextureSample0;Texture Sample 0;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;314;-3463.585,6942.798;Float;False;Property;_VertexOffsetX;VertexOffsetX;37;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-3459.805,7144.349;Float;False;Property;_VertexOffsetY;VertexOffsetY;38;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;326;-3847.92,6989.777;Inherit;True;Property;_TextureSample1;Texture Sample 1;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleSubtractOpNode;315;-3225.48,6894.424;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;329;-3231.234,7091.208;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;309;-2899.191,6772.206;Float;False;Property;_VertexOffsetMagnitude;VertexOffsetMagnitude;36;0;Create;True;0;0;0;False;0;False;0.05;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;308;-2810.996,6918.278;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;310;-2543.653,6865.912;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;359;-2546.756,6765.939;Float;False;Constant;_NoVertexOffset;NoVertexOffset;41;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;377;1792,256;Float;False;Property;_OutlineAlpha;OutlineAlpha;42;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;362;-1924.862,6853.405;Inherit;False;True;True;True;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;177;-4093.892,3523.297;Float;False;Property;_EmissionColor;EmissionColor;30;1;[HDR];Create;True;0;0;0;True;0;False;2,2,2,1;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;176;-4114.888,3231.99;Inherit;True;Property;_EmissionTexture;EmissionTexture;29;0;Create;True;0;0;0;True;1;Header(Emission);False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;179;-3756.952,3615.031;Float;False;Property;_EmissionForce;EmissionForce;31;0;Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-3650.371,3452.438;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;180;-3390.29,3522.278;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;248;-2792.295,1910.664;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;485;2983.227,-1299.069;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;366;2832,-608;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;330;-3456.605,7329.949;Float;False;Property;_VertexOffsetZ;VertexOffsetZ;39;0;Create;True;0;0;0;True;0;False;0.5;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;331;-3228.035,7276.808;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;364;2720,144;Float;False;outline;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;354;1792,48;Float;False;Property;_OutlineColor;OutlineColor;40;0;Create;True;0;0;0;True;1;Header(Outline);False;0.5451996,1,0,1;0.5451995,1,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.NormalVertexDataNode;486;1856,-368;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;355;1856,-208;Float;False;Property;_OutlineWidth;OutlineWidth;41;0;Create;True;0;0;0;True;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;487;2160,-304;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;2576,-656;Inherit;False;311;vertexOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;311;-1647.014,6860.691;Float;False;vertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;488;2368,-288;Half;False;outlineOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;365;2592,-544;Inherit;False;488;outlineOffset;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;490;2432,-768;Inherit;False;Alpha Split;-1;;1;07dab7960105b86429ac8eebd729ed6d;0;1;2;COLOR;0,0,0,0;False;2;FLOAT3;0;FLOAT;6
Node;AmplifyShaderEditor.OutlineNode;356;2144,192;Inherit;False;0;True;Masked;0;0;Front;True;True;True;True;0;False;;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;100;-4018.027,864.2838;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;117;-3797.932,1519.586;Inherit;False;116;specularDelta;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FWidthOpNode;115;-2826.564,896.7169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;224;-4149.815,1702.134;Inherit;True;195;shadow;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;195;-1417.53,-680.6537;Float;False;shadow;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;166;-3768.122,4805.354;Float;True;Property;_HideRimUnderShadow;HideRimUnderShadow;27;0;Create;True;0;0;0;True;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;350;-2275.896,6843.649;Float;False;Property;_UseVertexOffset;UseVertexOffset;33;0;Create;True;0;0;0;True;1;Header(VertexOffset);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StaticSwitch;168;-2116.708,4754.35;Float;False;Property;_SharpRimLight;SharpRimLight;28;0;Create;True;0;0;0;True;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;109;-3541.583,745.5388;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;126;-4000.241,1986.725;Inherit;True;113;specular;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;495;3120.523,-1137.124;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;497;2704,-912;Inherit;False;Constant;_Float0;Float 0;49;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;496;2864,-1024;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;184;2705.276,-1381.75;Inherit;False;182;computedEmission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;182;-2832,3520;Float;False;computedEmission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2653.021,-3745.462;Float;False;albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightAttenuation;188;-3792,-816;Inherit;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;498;-3792,-528;Inherit;False;SRP Additional Light;-1;;3;6c86746ad131a0a408ca599df5f40861;3,6,1,9,0,23,0;6;2;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;15;FLOAT3;0,0,0;False;14;FLOAT3;0,0,0;False;18;FLOAT;0.5;False;32;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCGrayscale;499;-3520,-576;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;133;-2474.236,2214.922;Float;True;computedSpecular;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;494;2176.747,-1005.653;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FunctionNode;492;2448,-1056;Inherit;False;SRP Additional Light;-1;;1;6c86746ad131a0a408ca599df5f40861;3,6,1,9,0,23,0;6;2;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;15;FLOAT3;0,0,0;False;14;FLOAT3;1,1,1;False;18;FLOAT;0.1;False;32;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;71;2672,-1200;Inherit;False;68;lightCol;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;440;2314.613,-2368.219;Inherit;False;439;postToneMapping;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;441;-574.9998,167.4404;Inherit;False;438;preToneMapping;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;158;1280,-2272;Inherit;False;157;rimLight;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;437;1280,-2400;Inherit;False;435;shadowMix;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;422;1228.756,-2683.629;Float;False;litColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;425;51.66533,-1710.471;Inherit;False;422;litColor;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;0,-2816;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;72;-288,-2672;Inherit;False;11;albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;69;-640,-2896;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;65;-640,-3088;Inherit;False;51;ramp;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-1587.945,-1918.789;Float;False;ramp;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-384,-3008;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;502;-128,-2880;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;504;-464,-2784;Inherit;False;SRP Additional Light;-1;;7;6c86746ad131a0a408ca599df5f40861;3,6,1,9,0,23,0;6;2;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;15;FLOAT3;0,0,0;False;14;FLOAT3;0,0,0;False;18;FLOAT;0.5;False;32;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;475;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;476;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;477;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;478;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;479;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;480;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;481;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;482;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;483;3152,-1296;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;MotionVectors;0;10;MotionVectors;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;False;False;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=MotionVectors;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;473;3104,-768;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;True;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;True;True;1;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;1;False;;True;3;False;;True;False;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;474;3280,-1168;Float;False;True;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;MoreMountains/MMAdvancedToon;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;9;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;True;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=TransparentCutout=RenderType;Queue=AlphaTest=Queue=0;UniversalMaterialType=Unlit;True;5;True;2;d3d11;metal;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;27;Surface;0;0;  Blend;0;0;Two Sided;1;0;Alpha Clipping;1;638726019334724320;  Use Shadow Threshold;0;0;Forward Only;0;0;Cast Shadows;1;0;Receive Shadows;1;0;Motion Vectors;0;638726016367634060;  Add Precomputed Velocity;0;0;GPU Instancing;0;638726016299875490;LOD CrossFade;0;638726016292037580;Built-in Fog;0;638726016282175540;Meta Pass;0;0;Extra Pre Pass;1;638726016425747790;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;11;True;True;True;True;False;False;False;False;False;False;False;False;;False;0
WireConnection;83;0;82;0
WireConnection;99;0;98;0
WireConnection;101;0;100;0
WireConnection;101;1;99;0
WireConnection;104;0;101;0
WireConnection;106;0;105;0
WireConnection;106;1;111;0
WireConnection;33;0;85;0
WireConnection;80;0;33;0
WireConnection;189;0;188;0
WireConnection;110;0;109;0
WireConnection;110;1;104;0
WireConnection;108;0;106;0
WireConnection;108;1;107;0
WireConnection;81;0;80;0
WireConnection;112;0;108;0
WireConnection;112;1;110;0
WireConnection;407;0;408;0
WireConnection;407;1;189;0
WireConnection;34;0;81;0
WireConnection;34;1;32;0
WireConnection;409;0;189;0
WireConnection;409;1;407;0
WireConnection;409;2;190;0
WireConnection;113;0;112;0
WireConnection;290;0;289;2
WireConnection;290;1;288;0
WireConnection;204;1;193;0
WireConnection;204;2;198;0
WireConnection;191;0;409;0
WireConnection;191;1;190;0
WireConnection;31;0;34;0
WireConnection;291;0;290;0
WireConnection;194;0;204;0
WireConnection;194;1;192;0
WireConnection;194;2;191;0
WireConnection;292;0;291;0
WireConnection;292;1;288;0
WireConnection;40;0;39;0
WireConnection;40;1;6;0
WireConnection;116;0;115;0
WireConnection;293;0;292;0
WireConnection;41;0;40;0
WireConnection;120;0;118;0
WireConnection;122;0;120;0
WireConnection;122;1;117;0
WireConnection;269;0;224;0
WireConnection;49;0;41;0
WireConnection;49;1;94;0
WireConnection;383;0;382;0
WireConnection;383;1;379;0
WireConnection;388;0;384;0
WireConnection;388;1;387;0
WireConnection;78;0;49;0
WireConnection;78;1;93;0
WireConnection;78;2;79;0
WireConnection;271;0;269;0
WireConnection;271;1;272;0
WireConnection;121;0;123;0
WireConnection;121;1;120;0
WireConnection;121;2;122;0
WireConnection;37;0;36;0
WireConnection;37;1;38;0
WireConnection;37;2;38;0
WireConnection;76;0;37;0
WireConnection;245;0;244;0
WireConnection;245;1;121;0
WireConnection;50;0;78;0
WireConnection;274;0;271;0
WireConnection;389;0;383;0
WireConnection;389;1;388;0
WireConnection;4;1;76;0
WireConnection;10;0;1;0
WireConnection;10;1;2;0
WireConnection;277;0;275;0
WireConnection;277;1;276;0
WireConnection;277;2;50;0
WireConnection;381;1;389;0
WireConnection;7;1;12;0
WireConnection;7;0;13;0
WireConnection;247;0;245;0
WireConnection;247;1;274;0
WireConnection;124;0;247;0
WireConnection;3;1;277;0
WireConnection;3;0;4;0
WireConnection;14;0;10;0
WireConnection;14;1;7;0
WireConnection;457;0;381;0
WireConnection;457;1;380;0
WireConnection;460;0;457;0
WireConnection;460;1;14;0
WireConnection;129;0;127;0
WireConnection;87;0;86;0
WireConnection;131;0;126;0
WireConnection;131;1;119;0
WireConnection;131;2;129;0
WireConnection;89;0;87;0
WireConnection;89;1;88;0
WireConnection;140;0;139;0
WireConnection;141;0;73;0
WireConnection;141;1;140;0
WireConnection;90;0;89;0
WireConnection;138;0;141;0
WireConnection;138;1;134;0
WireConnection;169;0;143;0
WireConnection;137;1;73;0
WireConnection;137;0;138;0
WireConnection;411;0;191;0
WireConnection;146;0;163;0
WireConnection;147;0;166;0
WireConnection;147;1;144;0
WireConnection;148;0;146;0
WireConnection;148;1;147;0
WireConnection;152;0;151;0
WireConnection;152;1;170;0
WireConnection;155;0;170;0
WireConnection;155;1;151;0
WireConnection;150;0;148;0
WireConnection;150;1;155;0
WireConnection;150;2;152;0
WireConnection;172;0;171;0
WireConnection;172;1;148;0
WireConnection;417;0;412;0
WireConnection;418;0;419;0
WireConnection;418;1;417;0
WireConnection;414;0;426;0
WireConnection;414;1;412;0
WireConnection;415;0;414;0
WireConnection;415;1;418;0
WireConnection;156;0;168;0
WireConnection;156;1;142;0
WireConnection;430;0;427;0
WireConnection;430;1;421;0
WireConnection;250;0;425;0
WireConnection;250;1;249;0
WireConnection;410;0;427;0
WireConnection;410;1;421;0
WireConnection;164;1;165;0
WireConnection;164;0;156;0
WireConnection;420;1;250;0
WireConnection;420;0;415;0
WireConnection;420;2;410;0
WireConnection;420;3;430;0
WireConnection;435;0;420;0
WireConnection;157;0;164;0
WireConnection;159;0;437;0
WireConnection;159;1;158;0
WireConnection;438;0;159;0
WireConnection;442;0;441;0
WireConnection;444;0;441;0
WireConnection;444;1;442;0
WireConnection;444;2;443;0
WireConnection;452;0;444;0
WireConnection;452;1;448;0
WireConnection;439;0;452;0
WireConnection;68;0;440;0
WireConnection;281;0;279;0
WireConnection;281;1;280;0
WireConnection;300;0;296;0
WireConnection;301;0;298;0
WireConnection;286;0;281;0
WireConnection;287;0;281;0
WireConnection;285;0;281;0
WireConnection;299;0;301;0
WireConnection;299;1;287;0
WireConnection;297;0;300;0
WireConnection;297;1;286;0
WireConnection;295;0;294;0
WireConnection;295;1;285;0
WireConnection;303;0;297;0
WireConnection;302;0;295;0
WireConnection;304;0;299;0
WireConnection;327;0;322;0
WireConnection;327;1;325;0
WireConnection;323;0;322;0
WireConnection;323;1;313;0
WireConnection;326;0;322;0
WireConnection;326;1;324;0
WireConnection;315;0;323;1
WireConnection;315;1;314;0
WireConnection;329;0;326;1
WireConnection;329;1;328;0
WireConnection;308;0;315;0
WireConnection;308;1;329;0
WireConnection;308;2;331;0
WireConnection;310;0;309;0
WireConnection;310;1;308;0
WireConnection;362;0;350;0
WireConnection;178;0;176;0
WireConnection;178;1;177;0
WireConnection;180;0;178;0
WireConnection;180;1;179;0
WireConnection;248;0;274;0
WireConnection;248;1;131;0
WireConnection;485;0;184;0
WireConnection;485;1;71;0
WireConnection;366;0;312;0
WireConnection;366;1;365;0
WireConnection;331;0;327;1
WireConnection;331;1;330;0
WireConnection;364;0;356;0
WireConnection;487;0;486;0
WireConnection;487;1;355;0
WireConnection;311;0;362;0
WireConnection;488;0;487;0
WireConnection;490;2;354;0
WireConnection;356;0;354;0
WireConnection;356;2;377;0
WireConnection;115;0;114;0
WireConnection;195;0;194;0
WireConnection;166;1;167;0
WireConnection;166;0;149;0
WireConnection;350;1;359;0
WireConnection;350;0;310;0
WireConnection;168;1;172;0
WireConnection;168;0;150;0
WireConnection;495;0;485;0
WireConnection;495;1;496;0
WireConnection;496;0;492;0
WireConnection;496;1;497;0
WireConnection;182;0;180;0
WireConnection;11;0;460;0
WireConnection;499;0;498;0
WireConnection;133;0;248;0
WireConnection;422;0;137;0
WireConnection;73;0;502;0
WireConnection;73;1;72;0
WireConnection;51;0;3;0
WireConnection;70;0;65;0
WireConnection;70;1;69;1
WireConnection;502;0;70;0
WireConnection;502;1;504;0
WireConnection;473;0;490;0
WireConnection;473;1;490;6
WireConnection;473;2;377;0
WireConnection;473;3;366;0
WireConnection;474;2;485;0
ASEEND*/
//CHKSM=8AF806D9B2164DB9F7B2F161762C80D4926BCC5E
