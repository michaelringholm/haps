// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Sprites/CroppedY"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
	    _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        _MinY("Min Y", Float) = 0
        _MaxY("Max Y", Float) = 1
    }

    SubShader
    {
        Tags
    {
        "Queue" = "Transparent"
        "IgnoreProjector" = "True"
        "RenderType" = "Transparent"
        "PreviewType" = "Plane"
        "CanUseSpriteAtlas" = "True"
    }

    Cull Off
    Lighting Off
    ZWrite Off
    Blend One OneMinusSrcAlpha

	Pass
    {
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ PIXELSNAP_ON
#include "UnityCG.cginc"

    struct appdata_t
    {
        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex   : SV_POSITION;
        fixed4 color : COLOR;
        half2 texcoord  : TEXCOORD0;
        float2 screenPos: TEXCOORD2;
    };

    fixed4 _Color;
    float _MinY;
    float _MaxY;

    v2f vert(appdata_t IN)
    {
        v2f OUT;
        OUT.vertex = UnityObjectToClipPos(IN.vertex);
        OUT.texcoord = IN.texcoord;
        OUT.color = IN.color * _Color;
        OUT.screenPos = (float2)ComputeScreenPos(OUT.vertex);
#ifdef PIXELSNAP_ON
        OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

        return OUT;
    }

    sampler2D _MainTex;

    fixed4 frag(v2f IN) : SV_Target
    {
        fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
		c.a *= (IN.screenPos.y >= _MinY);
		c.a *= (IN.screenPos.y <= _MaxY);
		c.rgb *= c.a;
		return c;
    }
    ENDCG
    }
    }
}
