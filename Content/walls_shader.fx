#define BlendReflectf(base, blend) 		((blend == 1.0) ? blend : min(base * base / (1.0 - blend), 1.0))
#define BlendDarkenf(base, blend)		min(blend, base)
#define Blend(base, blend, funcf) 		float3(funcf(base.r, blend.r), funcf(base.g, blend.g), funcf(base.b, blend.b))
#define BlendReflect(base, blend) 		Blend(base, blend, BlendReflectf)
#define BlendDarken(base, blend)		Blend(base, blend, BlendDarkenf)

sampler s0;

int u_wallBlendingMode;
int u_wallTopBlendingMode;
texture u_wallOverlayTexture;
texture u_wallTopOverlayTexture;
float2 u_wallOverlayTextureSize;
float2 u_wallTopOverlayTextureSize;
float3 u_wallColor;
float3 u_wallTopColor;
float2 u_screenSize;

sampler wallOverlay_sampler : register(s1)
{
    Texture = (u_wallOverlayTexture);
    magfilter = POINT;
    minfilter = POINT;
};

sampler wallTopOverlay_sampler : register(s2)
{
    Texture = (u_wallTopOverlayTexture);
    magfilter = POINT;
    minfilter = POINT;
};

bool CompareColors(float3 a, float3 b)
{
    float COMPARISON_THRESHOLD = 0.001f;

    return a.r < b.r + COMPARISON_THRESHOLD && a.r > b.r - COMPARISON_THRESHOLD
    && a.g < b.g + COMPARISON_THRESHOLD && a.g > b.g - COMPARISON_THRESHOLD
    && a.b < b.b + COMPARISON_THRESHOLD && a.b > b.b - COMPARISON_THRESHOLD;
}



float4 PixelShaderFunction(float2 texture_coords : TEXCOORD0) : COLOR0
{
    float4 texcolor = tex2D(s0, texture_coords);

    if (u_wallBlendingMode != 0 && CompareColors(texcolor.rgb, u_wallColor))
    {
        float2 newCoords = (texture_coords * u_screenSize) / u_wallOverlayTextureSize;
        float4 overlay = tex2D(wallOverlay_sampler, newCoords);
		
		if (u_wallBlendingMode == 1) texcolor.rgb = BlendDarken(texcolor.rgb, overlay.rgb);
		else if (u_wallBlendingMode == 2) texcolor.rgb = BlendReflect(texcolor.rgb, overlay.rgb);
    }
    else if (u_wallTopBlendingMode != 0 && CompareColors(texcolor.rgb, u_wallTopColor))
    {
        float2 newCoords = (texture_coords * u_screenSize) / u_wallTopOverlayTextureSize;
        float4 overlay = tex2D(wallTopOverlay_sampler, newCoords);
        
		if (u_wallTopBlendingMode == 1) texcolor.rgb = BlendDarken(texcolor.rgb, overlay.rgb);
		else if (u_wallTopBlendingMode == 2) texcolor.rgb = BlendReflect(texcolor.rgb, overlay.rgb);
    }

     return texcolor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}