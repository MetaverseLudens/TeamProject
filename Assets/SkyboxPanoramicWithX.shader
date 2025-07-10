Shader "Custom/SkyboxPanoramicWithX"
{
    Properties
    {
        _MainTex ("Panoramic (HDR)", 2D) = "grey" {}
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Y Rotation", Range(0, 360)) = 0
        _RotationX ("X Rotation", Range(-90, 90)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Tint;
            float _Exposure;
            float _Rotation;
            float _RotationX;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Direction vector
                float3 dir = normalize(mul(unity_ObjectToWorld, v.vertex).xyz);

                // Apply Y-axis rotation (_Rotation)
                float radY = radians(_Rotation);
                float sinY = sin(radY);
                float cosY = cos(radY);

                dir = float3(
                    dir.x * cosY - dir.z * sinY,
                    dir.y,
                    dir.x * sinY + dir.z * cosY
                );

                // Apply X-axis rotation (_RotationX)
                float radX = radians(_RotationX);
                float sinX = sin(radX);
                float cosX = cos(radX);

                dir = float3(
                    dir.x,
                    dir.y * cosX - dir.z * sinX,
                    dir.y * sinX + dir.z * cosX
                );

                o.dir = dir;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);

                float2 latlong;
                latlong.x = 0.5 + atan2(dir.x, -dir.z) / (2 * UNITY_PI);
                latlong.y = acos(dir.y) / UNITY_PI;

                fixed4 tex = tex2D(_MainTex, latlong);
                tex.rgb *= _Tint.rgb * _Exposure;

                return tex;
            }
            ENDCG
        }
    }
}
