Shader "Custom/ChunkShader" {
	Properties {
		_BackgroundTexArray("Background Texture Array", 2DArray) = "white" {}
		
		_CellTexArray ("Terrain Texture Array", 2DArray) = "white" {}

        _Texture ("Texture", 2D) = "white" {}
	}
	
	SubShader {	         
	    Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            Zwrite Off
		    Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		    LOD 200
	    
		
		    CGPROGRAM
		    #pragma fragment frag alpha:blend 
		    #pragma vertex vert alpha:blend 
		
		    #include "CellData.cginc"
		    #include "UnityCG.cginc"

            #pragma require 2darray
           
		    UNITY_DECLARE_TEX2DARRAY(_CellTexArray);
		    UNITY_DECLARE_TEX2DARRAY(_BackgroundTexArray);

		    struct Input  {
		        float2 uv_MainTex : texcoord0;
		        float4 position : SV_POSITION;

                float4 Lighting : lighting;
			    float4 Terrain : terrain;
		    };


            sampler2D _Texture;

		    half _Glossiness;
		    half _Metallic;
		    fixed4 _Color;
           
            Input vert (vertexInput v)
            {
                Input data;
                
                data.position = UnityObjectToClipPos (v.vertex);
               
                data.Terrain = GetCellData(v, 2);
      
                data.Lighting = GetLightingData(v, 2);
                    
                data.uv_MainTex.xy = v.texcoord0.xy;
        
                return data;
            }

            fixed4 GetLighting(Input IN)
            {
                return fixed4(IN.Lighting.xxx ,255);
            }
        
            fixed4 frag (Input IN) : SV_Target
		    {	   
		        fixed4 cX;   
		    
		        if(IN.Terrain.w != 0)
		        {
		            cX = (UNITY_SAMPLE_TEX2DARRAY(_CellTexArray, float3(IN.uv_MainTex.xy, IN.Terrain.w)));
		        }
		    
		        else
		        {
		            cX = (UNITY_SAMPLE_TEX2DARRAY(_BackgroundTexArray, float3(IN.uv_MainTex.xy, 1)));
		        }
		               
                fixed4 finalColor = cX * GetLighting(IN);
	
		        finalColor.a = IN.Terrain.y;
		        		    
		        return finalColor;
		    }
		
		    ENDCG
        }
    }
}