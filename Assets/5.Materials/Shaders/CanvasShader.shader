Shader "Unlit/CanvasShader"
{
   Properties
    {
        _Tiling("Tiling", Vector) = (10, 10, 0, 0)
        _OutColor("OutColor", Color) = (0, 0, 0, 1)
        _InColor("InColor", Color) = (1, 1, 1, 0)
        [NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
        _Scroll("Scroll", Float) = 1
        _Alpha("Alpha", Float) = 1
        [HideInInspector]_BUILTIN_Surface("Float", Float) = 1
        [HideInInspector]_BUILTIN_Blend("Float", Float) = 0
        [HideInInspector]_BUILTIN_AlphaClip("Float", Float) = 1
        [HideInInspector]_BUILTIN_SrcBlend("Float", Float) = 1
        [HideInInspector]_BUILTIN_DstBlend("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZWrite("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZWriteControl("Float", Float) = 0
        [HideInInspector]_BUILTIN_ZTest("Float", Float) = 4
        [HideInInspector]_BUILTIN_CullMode("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueOffset("Float", Float) = 0
        [HideInInspector]_BUILTIN_QueueControl("Float", Float) = -1
    }
    SubShader
    {
        Tags
        {
            // RenderPipeline: <None>
            "RenderType"="Transparent"
            "BuiltInMaterialType" = "Unlit"
            "Queue"="Transparent"
            // DisableBatching: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="BuiltInUnlitSubTarget"
        }
        Pass
        {
            Pass
        {
            Name "Pass"
         
        
        // Render State
        Cull [_BUILTIN_CullMode]
        Blend [_BUILTIN_SrcBlend] [_BUILTIN_DstBlend]
        ZTest [_BUILTIN_ZTest]
        ZWrite [_BUILTIN_ZWrite]
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 3.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile_fwdbase
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature_local_fragment _ _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHAPREMULTIPLY_ON
        #pragma shader_feature_local_fragment _ _BUILTIN_AlphaClip
        #pragma shader_feature_local_fragment _ _BUILTIN_ALPHATEST_ON
        // GraphKeywords: <None>
        
        // Defines
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define BUILTIN_TARGET_API 1
        #ifdef _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #define _SURFACE_TYPE_TRANSPARENT _BUILTIN_SURFACE_TYPE_TRANSPARENT
        #endif
        #ifdef _BUILTIN_ALPHATEST_ON
        #define _ALPHATEST_ON _BUILTIN_ALPHATEST_ON
        #endif
        #ifdef _BUILTIN_AlphaClip
        #define _AlphaClip _BUILTIN_AlphaClip
        #endif
        #ifdef _BUILTIN_ALPHAPREMULTIPLY_ON
        #define _ALPHAPREMULTIPLY_ON _BUILTIN_ALPHAPREMULTIPLY_ON
        #endif
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Shim/Shims.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/LegacySurfaceVertex.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/ShaderLibrary/ShaderGraphFunctions.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float2 _Tiling;
        float4 _OutColor;
        float4 _InColor;
        float4 _MainTex_TexelSize;
        float _Scroll;
        float _Alpha;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // Graph Functions
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Fraction_float2(float2 In, out float2 Out)
        {
            Out = frac(In);
        }
        
        void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
        {
            //rotation matrix
            Rotation = Rotation * (3.1415926f/180.0f);
            UV -= Center;
            float s = sin(Rotation);
            float c = cos(Rotation);
        
            //center rotation matrix
            float2x2 rMatrix = float2x2(c, -s, s, c);
            rMatrix *= 0.5;
            rMatrix += 0.5;
            rMatrix = rMatrix*2 - 1;
        
            //multiply the UVs by the rotation matrix
            UV.xy = mul(UV.xy, rMatrix);
            UV += Center;
        
            Out = UV;
        }
        
        void Unity_Preview_float(float In, out float Out)
        {
            Out = In;
        }
        
        void Unity_Posterize_float(float In, float Steps, out float Out)
        {
            Out = floor(In / (1 / Steps)) * (1 / Steps);
        }
        
        void Unity_OneMinus_float(float In, out float Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Subtract_float(float A, float B, out float Out)
        {
            Out = A - B;
        }
        
        void Unity_Rectangle_Fastest_float(float2 UV, float Width, float Height, out float Out)
        {
            float2 d = abs(UV * 2 - 1) - float2(Width, Height);
        #if defined(SHADER_STAGE_RAY_TRACING)
            d = saturate((1 - saturate(d * 1e7)));
        #else
            d = saturate(1 - d / fwidth(d));
        #endif
            Out = min(d.x, d.y);
        }
        
        void Unity_Lerp_float4(float4 A, float4 B, float4 T, out float4 Out)
        {
            Out = lerp(A, B, T);
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float AlphaClipThreshold;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float2 _Property_0cbee4150bd34cf3858842ac1215216d_Out_0_Vector2 = _Tiling;
            float2 _TilingAndOffset_73b2bc365e994a65abaf085d12e456f7_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, _Property_0cbee4150bd34cf3858842ac1215216d_Out_0_Vector2, float2 (0, 0), _TilingAndOffset_73b2bc365e994a65abaf085d12e456f7_Out_3_Vector2);
            float2 _Fraction_ea22db93a0e6474abee5ac7d92a4d401_Out_1_Vector2;
            Unity_Fraction_float2(_TilingAndOffset_73b2bc365e994a65abaf085d12e456f7_Out_3_Vector2, _Fraction_ea22db93a0e6474abee5ac7d92a4d401_Out_1_Vector2);
            float2 _Rotate_02b300fd974f455c9bc036f638b1d166_Out_3_Vector2;
            Unity_Rotate_Degrees_float(_Fraction_ea22db93a0e6474abee5ac7d92a4d401_Out_1_Vector2, float2 (0.5, 0.5), float(45), _Rotate_02b300fd974f455c9bc036f638b1d166_Out_3_Vector2);
            float _Property_51331be6c6c94fb29c2940a9d647da69_Out_0_Float = _Scroll;
            float4 _UV_83bc296b14a84329ae9de96c6bf9e539_Out_0_Vector4 = IN.uv0;
            float _Split_210286145d72401a9f52522f4623259f_R_1_Float = _UV_83bc296b14a84329ae9de96c6bf9e539_Out_0_Vector4[0];
            float _Split_210286145d72401a9f52522f4623259f_G_2_Float = _UV_83bc296b14a84329ae9de96c6bf9e539_Out_0_Vector4[1];
            float _Split_210286145d72401a9f52522f4623259f_B_3_Float = _UV_83bc296b14a84329ae9de96c6bf9e539_Out_0_Vector4[2];
            float _Split_210286145d72401a9f52522f4623259f_A_4_Float = _UV_83bc296b14a84329ae9de96c6bf9e539_Out_0_Vector4[3];
            float _Preview_736ee415a0d64224806e56fe04d6e084_Out_1_Float;
            Unity_Preview_float(_Split_210286145d72401a9f52522f4623259f_R_1_Float, _Preview_736ee415a0d64224806e56fe04d6e084_Out_1_Float);
            float2 _Property_edd854413e7945c5ad355d05f197a961_Out_0_Vector2 = _Tiling;
            float _Split_bb98b0dc227a4cd489453dc8ca97be60_R_1_Float = _Property_edd854413e7945c5ad355d05f197a961_Out_0_Vector2[0];
            float _Split_bb98b0dc227a4cd489453dc8ca97be60_G_2_Float = _Property_edd854413e7945c5ad355d05f197a961_Out_0_Vector2[1];
            float _Split_bb98b0dc227a4cd489453dc8ca97be60_B_3_Float = 0;
            float _Split_bb98b0dc227a4cd489453dc8ca97be60_A_4_Float = 0;
            float _Float_16cb0f36144a492dbda723b79cc59f88_Out_0_Float = _Split_bb98b0dc227a4cd489453dc8ca97be60_R_1_Float;
            float _Posterize_780a9d8c433e404cb2dda6e470bde302_Out_2_Float;
            Unity_Posterize_float(_Preview_736ee415a0d64224806e56fe04d6e084_Out_1_Float, _Float_16cb0f36144a492dbda723b79cc59f88_Out_0_Float, _Posterize_780a9d8c433e404cb2dda6e470bde302_Out_2_Float);
            float _OneMinus_892b422929e448439400a54e7f8b4572_Out_1_Float;
            Unity_OneMinus_float(_Posterize_780a9d8c433e404cb2dda6e470bde302_Out_2_Float, _OneMinus_892b422929e448439400a54e7f8b4572_Out_1_Float);
            float _Multiply_cdfcdd9712024c7ca01697f8e4bb26a0_Out_2_Float;
            Unity_Multiply_float_float(_OneMinus_892b422929e448439400a54e7f8b4572_Out_1_Float, 0.5, _Multiply_cdfcdd9712024c7ca01697f8e4bb26a0_Out_2_Float);
            float _Add_41b456da2b304ce098854d1b41cd0144_Out_2_Float;
            Unity_Add_float(_Posterize_780a9d8c433e404cb2dda6e470bde302_Out_2_Float, _Multiply_cdfcdd9712024c7ca01697f8e4bb26a0_Out_2_Float, _Add_41b456da2b304ce098854d1b41cd0144_Out_2_Float);
            float _Multiply_b4bc8222abf3421a8a234f376dc69aee_Out_2_Float;
            Unity_Multiply_float_float(_Property_51331be6c6c94fb29c2940a9d647da69_Out_0_Float, _Add_41b456da2b304ce098854d1b41cd0144_Out_2_Float, _Multiply_b4bc8222abf3421a8a234f376dc69aee_Out_2_Float);
            float _Subtract_91252813ccc846699d10d2b332d1b344_Out_2_Float;
            Unity_Subtract_float(_Multiply_b4bc8222abf3421a8a234f376dc69aee_Out_2_Float, float(0.3), _Subtract_91252813ccc846699d10d2b332d1b344_Out_2_Float);
            float _Rectangle_904f791cd13d41c1b1326f7539465d7b_Out_3_Float;
            Unity_Rectangle_Fastest_float(_Rotate_02b300fd974f455c9bc036f638b1d166_Out_3_Vector2, _Subtract_91252813ccc846699d10d2b332d1b344_Out_2_Float, _Subtract_91252813ccc846699d10d2b332d1b344_Out_2_Float, _Rectangle_904f791cd13d41c1b1326f7539465d7b_Out_3_Float);
            float4 _Property_a0dda58de2df444086317e36a638599b_Out_0_Vector4 = _InColor;
            float4 _Property_11bcb62d8a4947519f3e291305304644_Out_0_Vector4 = _OutColor;
            float4 _Lerp_ddb6b9d470494eda8d0b2b56c4db2a7b_Out_3_Vector4;
            Unity_Lerp_float4((_Rectangle_904f791cd13d41c1b1326f7539465d7b_Out_3_Float.xxxx), _Property_a0dda58de2df444086317e36a638599b_Out_0_Vector4, _Property_11bcb62d8a4947519f3e291305304644_Out_0_Vector4, _Lerp_ddb6b9d470494eda8d0b2b56c4db2a7b_Out_3_Vector4);
            UnityTexture2D _Property_37f969d11aa04d8c9304585231876943_Out_0_Texture2D = UnityBuildTexture2DStructNoScale(_MainTex);
            float4 _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4 = SAMPLE_TEXTURE2D(_Property_37f969d11aa04d8c9304585231876943_Out_0_Texture2D.tex, _Property_37f969d11aa04d8c9304585231876943_Out_0_Texture2D.samplerstate, _Property_37f969d11aa04d8c9304585231876943_Out_0_Texture2D.GetTransformedUV(IN.uv0.xy) );
            float _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_R_4_Float = _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4.r;
            float _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_G_5_Float = _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4.g;
            float _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_B_6_Float = _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4.b;
            float _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_A_7_Float = _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4.a;
            float4 _Multiply_322edaa3e5c84086a9dfa211ba69aae9_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Lerp_ddb6b9d470494eda8d0b2b56c4db2a7b_Out_3_Vector4, _SampleTexture2D_6c550537e4eb4a62bcb180cd730064f7_RGBA_0_Vector4, _Multiply_322edaa3e5c84086a9dfa211ba69aae9_Out_2_Vector4);
            surface.BaseColor = (_Multiply_322edaa3e5c84086a9dfa211ba69aae9_Out_2_Vector4.xyz);
            surface.Alpha = float(1);
            surface.AlphaClipThreshold = (_Multiply_322edaa3e5c84086a9dfa211ba69aae9_Out_2_Vector4).x;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        void BuildAppDataFull(Attributes attributes, VertexDescription vertexDescription, inout appdata_full result)
        {
            result.vertex     = float4(attributes.positionOS, 1);
            result.tangent    = attributes.tangentOS;
            result.normal     = attributes.normalOS;
            result.texcoord   = attributes.uv0;
            result.vertex     = float4(vertexDescription.Position, 1);
            result.normal     = vertexDescription.Normal;
            result.tangent    = float4(vertexDescription.Tangent, 0);
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
        }
        
        void VaryingsToSurfaceVertex(Varyings varyings, inout v2f_surf result)
        {
            result.pos = varyings.positionCS;
            // World Tangent isn't an available input on v2f_surf
        
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogCoord = varyings.fogFactorAndVertexLight.x;
                COPY_TO_LIGHT_COORDS(result, varyings.fogFactorAndVertexLight.yzw);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(varyings, result);
        }
        
        void SurfaceVertexToVaryings(v2f_surf surfVertex, inout Varyings result)
        {
            result.positionCS = surfVertex.pos;
            // viewDirectionWS is never filled out in the legacy pass' function. Always use the value computed by SRP
            // World Tangent isn't an available input on v2f_surf
        
            #if UNITY_ANY_INSTANCING_ENABLED
            #endif
            #if UNITY_SHOULD_SAMPLE_SH
            #if !defined(LIGHTMAP_ON)
            #endif
            #endif
            #if defined(LIGHTMAP_ON)
            #endif
            #ifdef VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
                result.fogFactorAndVertexLight.x = surfVertex.fogCoord;
                COPY_FROM_LIGHT_COORDS(result.fogFactorAndVertexLight.yzw, surfVertex);
            #endif
        
            DEFAULT_UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(surfVertex, result);
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.shadergraph/Editor/Generation/Targets/BuiltIn/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        ENDHLSL
        }
    }
}
