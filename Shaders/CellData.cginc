
sampler2D _CellData;
float4 _CellData_TexelSize;
float3 _ViewerPos;  
sampler2D _LightingData;

struct vertexInput {
    float4 vertex : POSITION;
    float4 texcoord0 : TEXCOORD0;
    float4 texcoord2 : TEXCOORD2;
};

//X is lighting, y is alpha, z is the background index, w is the terrain index
float4 GetCellData (vertexInput v, int index) {


	float2 uv;
	uv.x = (v.texcoord2.z + 0.5) * _CellData_TexelSize.x - _ViewerPos.x * _CellData_TexelSize.x;
	float row = floor(uv.x);
	uv.x -= row;
	uv.y = (row + 0.5) * _CellData_TexelSize.y;
	
	
	float4 data = tex2Dlod(_CellData, float4(uv, 0, 0));
	data.w *= 255;
    data.z *= 255;   
	return data;
}

float4 GetLightingData (vertexInput v, int index) {

	float2 uv;
	uv.x = (v.texcoord2[index] + 0.5) * _CellData_TexelSize.x - _ViewerPos.x * _CellData_TexelSize.x;
	float row = floor(uv.x);
	uv.x -= row;
	uv.y = (row + 0.5) * _CellData_TexelSize.y;
	
	float4 data = tex2Dlod(_LightingData, float4(uv, 0, 0));
	///data.x *= 255;
   
	//data.w *= 255;
    //data.z *= 255;   
	return data;
}


