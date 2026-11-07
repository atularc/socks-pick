Shader "Hidden/SockPairs/ReferenceBackdrop"
{
    Properties { _MainTex ("Screenshot", 2D) = "white" {} }
    CGINCLUDE
    #include "UnityCG.cginc"
    sampler2D _MainTex; float4 _MainTex_TexelSize;
    fixed4 blur(float2 uv,float2 d)
    {
        fixed3 rgb=tex2D(_MainTex,uv).rgb*.227027;
        rgb+=(tex2D(_MainTex,uv+d*1.384615).rgb+tex2D(_MainTex,uv-d*1.384615).rgb)*.316216;
        rgb+=(tex2D(_MainTex,uv+d*3.230769).rgb+tex2D(_MainTex,uv-d*3.230769).rgb)*.070270;
        return fixed4(rgb,1);
    }
    fixed4 horizontal(v2f_img i):SV_Target{return blur(i.uv,float2(_MainTex_TexelSize.x*1.4,0));}
    fixed4 vertical(v2f_img i):SV_Target{return blur(i.uv,float2(0,_MainTex_TexelSize.y*1.4));}
    ENDCG
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment horizontal
            ENDCG }
        Pass { CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment vertical
            ENDCG }
    }
}
