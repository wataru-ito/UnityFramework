Shader "Framework/VFX/Fader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Thickness ("Thickness", Range(0,1)) = 0
	}
	
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature WHITE
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float _Thickness;
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				#if WHITE
				return col + (1 - col) * _Thickness;
				#else
				return col * _Thickness;
				#endif
			}
			ENDCG
		}
	}
}
