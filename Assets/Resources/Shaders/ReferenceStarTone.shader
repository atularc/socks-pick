Shader "UI/SockPairs/ReferenceStarTone"
{
    Properties { _MainTex ("Art", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct a {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            struct v {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
            sampler2D _MainTex;
            v vert(a i){v o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color;return o;}
            fixed4 frag(v i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                // Only yellow/gold belongs to the star. Keep the original peach panel
                // and its shadows untouched, avoiding rectangular replacement patches.
                float gold=smoothstep(.035,.11,c.g-c.b)*smoothstep(.015,.045,c.g*.67-c.b);
                float value=dot(c.rgb,float3(.30,.59,.11));
                c.rgb=lerp(c.rgb,value*float3(.72,.69,.63),gold);
                return c*i.color;
            }
            ENDCG
        }
    }
}
