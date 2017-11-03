sampler s0;

int u_palette;
texture u_lut;
sampler lut_sampler : register(s1)
{
    Texture = (u_lut);
    magfilter = POINT;
    minfilter = POINT;
};

float4 PixelShaderFunction(float2 texture_coords : TEXCOORD0) : COLOR0
{

    float4 texcolor = tex2D(s0, texture_coords);
	float a = texcolor.a;

    const int lut_height = 5;
    const float pixelH = 1.0 / lut_height;
    float2 coords = float2(0, pixelH * (u_palette + 0.5));
		
		 if (texcolor.r == 1.0) coords.x = 1;
    else if (texcolor.g == 1.0) coords.x = 2;
    else if (texcolor.b == 1.0) coords.x = 3;
    else						coords.x = 4;

    coords.x -= 0.5;
    coords.x /= 4.0;

	texcolor = tex2D(lut_sampler, coords);
	texcolor.a = a;

	return texcolor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}