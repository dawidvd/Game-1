//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
texture ScreenTexture;
 
// Our sampler for the texture, which is just going to be pretty simple
sampler TextureSampler = sampler_state
{
    Texture = <ScreenTexture>;
};
 
float bluramount  = 1.0f;
float center      = 1.1f;
float stepSize    = 0.004f;
float steps       = 3.0f;


 
//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point
float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	float minOffs     = (steps-1.0) / -2.0f;
	float maxOffs     = (steps-1.0) / +2.0f;
	float amount;
    float4 blurred;
	
	amount = pow((texCoord.y * center) * 2.0 - 1.0, 2.0) * bluramount;
        
        //This is the accumulation of color from the surrounding pixels in the texture
    blurred = float4(0.0, 0.0, 0.0, 1.0);
        
        //From minimum offset to maximum offset
    for (float offsX = minOffs; offsX <= maxOffs; ++offsX) 
	{
        for (float offsY = minOffs; offsY <= maxOffs; ++offsY) 
		{

                //copy the coord so we can mess with it
            float2 temp_tcoord = texCoord.xy;

                //work out which uv we want to sample now
            temp_tcoord.x += offsX * amount * stepSize;
            temp_tcoord.y += offsY * amount * stepSize;

                //accumulate the sample 
            blurred += tex2D(TextureSampler, temp_tcoord);
        
        }
    } 
        
        //because we are doing an average, we divide by the amount (x AND y, hence steps * steps)
    blurred /= float(steps * steps);

	
	return blurred;
}
 
 float4 PixelShaderFunction2(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
	return tex2D( TextureSampler, texCoord);
}
 
//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique Plain
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}

technique Plain2
{
    pass Pass1
    {
        PixelShader = compile ps_4_0 PixelShaderFunction2();
    }
}