Shader "Custom/PositionHighlight"
{
Properties
    {
        _MainTex ("基础纹理", 2D) = "white" {}
        _HighlightColor ("高亮颜色", Color) = (1,0.5,0.5,1)
        _HighlightCenter ("高亮中心", Vector) = (0,0,0,0)
        _HighlightRadius ("影响半径", Range(0.1, 5)) = 1.0
        _ColorIntensity ("颜色强度", Range(0, 2)) = 1.0
        _EdgeSmoothness ("边缘平滑", Range(0, 1)) = 0.2       
        _DeformationDepth ("凹陷深度", Range(0, 0.3)) = 0.1
        _DeformationHardness ("形变硬度", Range(0.1, 5)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow
        #pragma target 3.5

        sampler2D _MainTex;
        float3 _HighlightCenter;
        float4 _HighlightColor;
        float _HighlightRadius;
        float _ColorIntensity;
        float _EdgeSmoothness;
        float _DeformationDepth;
        float _DeformationHardness;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // 顶点着色器：实现按压变形
        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            
            // 计算世界空间到模型空间的转换
            float3 centerOS = mul(unity_WorldToObject, float4(_HighlightCenter, 1)).xyz;
            
            // 计算顶点到中心的距离
            float distance = length(v.vertex.xyz - centerOS);
            
            // 形变衰减计算（指数衰减曲线）
            float attenuation = pow(saturate(1 - distance / _HighlightRadius), _DeformationHardness);
            
            // 沿法线方向凹陷
            float3 deformation = v.normal * _DeformationDepth * attenuation;
            v.vertex.xyz -= deformation;
        }

        // 表面着色器：处理颜色混合
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 基础纹理采样
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);
            
            // 高亮强度计算
            float distanceToCenter = distance(IN.worldPos, _HighlightCenter);
            float highlightFactor = 1 - smoothstep(
                _HighlightRadius * (1 - _EdgeSmoothness),
                _HighlightRadius,
                distanceToCenter
            );
            
            // 颜色混合
            float3 blendedColor = lerp(
                baseColor.rgb,
                _HighlightColor.rgb * _ColorIntensity,
                highlightFactor * _HighlightColor.a
            );

            // 输出参数
            o.Albedo = blendedColor;
            o.Metallic = 0;
            o.Smoothness = 0.5;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}