Shader "Custom/CardShader"
{
	Properties
	{
    	_MainTex ("Texture", 2D) = "white" { }
	}
	SubShader
	{
       Cull back
		//Tags { "RenderType"="TransparentCutout" }
		Tags { "Queue"="Transparent" }
		//LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input
		{
			float2 uv_MainTex;
			float4 color : COLOR;
		};
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * IN.color;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
	//Fallback "VertexLit"
} 