#define BlendReflectf(base, blend) 		((blend == 1.0) ? blend : min(base * base / (1.0 - blend), 1.0))
#define BlendDarkenf(base, blend)		min(blend, base)
#define Blend(base, blend, funcf) 		float3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))
#define BlendReflect(base, blend) 		Blend(base, blend, BlendReflectf)
#define BlendDarken(base, blend)		Blend(base, blend, BlendDarkenf)

sampler s0;
int u_blendMode;

float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 v_color : COLOR0, float2 texture_coords : TEXCOORD0) : SV_TARGET0
{
	float4 output = v_color;
	if (u_blendMode != 0)
	{
		float4 overlay = tex2D(s0, texture_coords);
		if (u_blendMode == 1) output.rgb = BlendDarken(v_color, overlay);
		else if (u_blendMode == 2) output.rgb = BlendReflect(v_color, overlay);
	}
	return output;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}