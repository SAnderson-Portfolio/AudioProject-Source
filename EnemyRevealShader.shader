Shader "Custom/EnemyRevealShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

		_Radius("Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
		Cull off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		static const float _SoundLimit = 36;

        sampler2D _MainTex;
        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

		float3 _PlayerPosition;
		float _Radius;
		
		float _CurrentRadius[_SoundLimit];
		float3 _SoundLocations[_SoundLimit];
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			bool toDiscard = false;
			for (int i = 0; i < _SoundLimit; i++)
			{
				if (distance(_SoundLocations[i], IN.worldPos) > _CurrentRadius[i])
					toDiscard = true;
				else
				{
					toDiscard = false;
					break;
				}

			}
			if (toDiscard)
				discard;

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
            o.Albedo = c.rgb;
			for (int j = 0; j < _SoundLimit; j++)
			{
				if (distance(_SoundLocations[j], IN.worldPos) <= _CurrentRadius[j] && distance(_SoundLocations[j], IN.worldPos) > (_CurrentRadius[j] - 0.05f))
				{
					o.Albedo = fixed3(1, 1, 1);
				}
			}
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

			
        }
        ENDCG
    }
    FallBack "Diffuse"
}
