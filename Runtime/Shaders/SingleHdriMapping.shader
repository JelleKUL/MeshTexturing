Shader "Mapping/SingleHDRI"
{
    Properties
    {
        _MainTex ("HDRI Texture", 2D) = "white" {}
        _HDRIPos ("HDRI position", Vector) = (0,0,0,0)
        _HDRIRot ("HDRI rotation", Vector) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // The properties defined
            sampler2D _MainTex;
            float4 _HDRIPos;
            float4 _HDRIRot;

            // per vertex data
            struct appdata 
            {
                float4 vertex : POSITION;
            };

            //the data to send from the vertex shader to the fragment shader aka. interpolators:
            // The data the is returned is interpolated between the neighbouring vertices
            struct v2f 
            {
                float4 vertex : SV_POSITION;
                float3 SphereCoords : TEXCOORD2;
            };
            // calculate the offset of the hdri to offset the positions
            float3 HDRIOffset(float3 pos){
                pos -= _HDRIPos;

                pos.xz = mul(pos.xz, float2x2(cos(_HDRIRot.y), sin(_HDRIRot.y), -sin(_HDRIRot.y), cos(_HDRIRot.y)));
                pos.yz = mul(pos.yz, float2x2(cos(_HDRIRot.x), -sin(_HDRIRot.x), sin(_HDRIRot.x), cos(_HDRIRot.x)));
                pos.xy = mul(pos.xy, float2x2(cos(_HDRIRot.z), -sin(_HDRIRot.z), sin(_HDRIRot.z), cos(_HDRIRot.z)));

                return pos;
            }

            // unitys coordinate system differs from the rest, the y & z axis are switched
            float3 WorldToSphere(float3 pos){
                float r = sqrt(pow(pos.x, 2) + pow(pos.z, 2) + pow(pos.y, 2));
                float theta = atan2(sqrt(pow(pos.x, 2) + pow(pos.z, 2)), pos.y); //the up and down angle
                float phi = atan2(pos.z, pos.x); //the left to right angle
                return (float3(-phi/6.28-0.25, -theta/3.14, r)); // add a certain offset to phi to make the center of the hdri face forward (z)
            }

            // the actual vertex shader returning the interpolated data in a v2f struct
            // using the appdata as an imput wich can contain a bunch of stuff
            v2f vert (appdata v)
            {
                v2f o; // o for output
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.SphereCoords = WorldToSphere(HDRIOffset(mul(unity_ObjectToWorld, v.vertex)));
                return o;
            }

            // the fragment shader, used to write to the frame buffer
            fixed4 frag (v2f i) : SV_Target // target is the frame buffer for forward rendering
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.SphereCoords.xy);
                return col;
            }
            ENDCG
        }
    }
}
