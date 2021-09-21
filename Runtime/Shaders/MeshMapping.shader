// The meshmapping shader to map different images to a mesh
Shader "Custom/MeshMapping"
{
    Properties
    {
        _TextureArray ("TextureArray", 2DArray) = "" {}
        _MaxDepth ("Max depth", Float) = 8.0
        _DepthError ("Depth Error", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
            #pragma exclude_renderers d3d11 gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // The properties defined
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);
            float _MaxDepth;
            float _DepthError;

            struct HDRI
            {
                float3 pos;
                float3 rot;
                float fov;
            };

            // the structured buffer send from the CPU
            StructuredBuffer<HDRI> HDRIS;
            int numHDRI;

            struct appdata // per vertex data to send to the vertex shader from the object
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                //float4 color : COLOR;
                //float2 uv : TEXCOORD0;
                
            };
            // the data to send from the vertex shader to the fragment shader aka. interpolators:
            // The data the is returned is interpolated between the neighbouring vertices
            struct v2f 
            {
                float2 uv : TEXCOORD0; // can contain wathever we write to it, we can decide the name of it
                float4 vertex : SV_POSITION; // clip space position of vertex
                float3 normal : NORMAL; // the normal of the vertex
                float3 worldPos : TEXCOORD1; // the world position of the vertex
            };

            // calculate the offset of the hdri to offset the positions
            float3 HDRIOffset(float3 pos, int index){
                
                pos -= HDRIS[index].pos;

                // 3 inverse rotations, one for each plane
                float3 rot = HDRIS[index].rot;
                pos.xz = mul(pos.xz, float2x2(cos(rot.y), sin(rot.y), -sin(rot.y), cos(rot.y)));
                pos.yz = mul(pos.yz, float2x2(cos(rot.x), -sin(rot.x), sin(rot.x), cos(rot.x)));
                pos.xy = mul(pos.xy, float2x2(cos(rot.z), -sin(rot.z), sin(rot.z), cos(rot.z)));

                return pos;
            }

            // unity's coordinate system differs from the generic systems, the y & z axis are switched
            // Convert the world coordinates to spherical uv coordinates
            float3 WorldToSphere(float3 pos){

                float r = sqrt(pow(pos.x, 2) + pow(pos.z, 2) + pow(pos.y, 2)); // the distance to the point
                float theta = atan2(sqrt(pow(pos.x, 2) + pow(pos.z, 2)), pos.y); //the up and down angle
                float phi = atan2(pos.z, pos.x); //the left to right angle
                return (float3(-phi/6.28-0.25, -theta/3.14, r)); // add a certain offset to phi to make the center of the hdri face forward (z)
            }

            // remap a value
            float remap(float value, float low1, float high1, float low2, float high2){
                return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
            }

            // Returns a value from 1 to 0 depending on the distance from an HDRI with the _MaxDepth as 0
            float GetDistanceConfidence(float3 pos, int index){
                return  1 - (distance(pos, HDRIS[index].pos) / _MaxDepth);
            }

            // Returns a value from 0 to 1 depending on the normal direction of the point from an HDRI
            float GetNormalConfidence(float3 pos, float3 normal, int index){
                float dotProduct = dot( normal, normalize(pos - HDRIS[index].pos) );
                return remap(dotProduct, -1, 1, 1, 0);
            }

            //returns a value either 0 or 1, 0 being occluded, 1 being visible
            float GetOcclusionConfidence(float3 sphereCoords, int index){
                float dist = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(sphereCoords.xy, index)).w;
                return abs(dist - remap(sphereCoords.z, 0,_MaxDepth, 1, 0)) < _DepthError;
                
            }

            // uses the distance, normal and occlusion to determine the best HDRI to use for the image
            int GetBestHDRI(float3 pos, float3 normal){
                float bestValue = 0;
                int bestIndex = 0;
                for(int i = 0; i < numHDRI; i++){
                    float newValue = GetDistanceConfidence(pos, i) * GetNormalConfidence(pos, normal, i) * GetOcclusionConfidence(WorldToSphere(HDRIOffset(pos,i)), i);

                    if(newValue > bestValue){
                        bestValue = newValue;
                        bestIndex = i;
                    }
                }
                return bestIndex;
            }

            // the actual vertex shader returning the interpolated data in a v2f struct
            // using the appdata as an imput wich can contain a bunch of stuff
            v2f vert (appdata v)
            {
                v2f o; // o for output
                o.vertex = UnityObjectToClipPos(v.vertex); // converts object space to clip space
                o.normal =  UnityObjectToWorldNormal(v.normal); // converts the normal to world direction
                o.worldPos =  mul(unity_ObjectToWorld, v.vertex); // convert the vertex to world position
                
                return o;
            }

            // the fragment shader, used to write to the frame buffer
            fixed4 frag (v2f i) : SV_Target // target is the frame buffer for forward rendering
            {
                //calculate the best hdri for each fragment
                int index = GetBestHDRI(i.worldPos, i.normal);
                // convert the position to spherical coordinates
                float3 sphereCoords = WorldToSphere(HDRIOffset(i.worldPos, index));
                // calculate the end color by sampling the array with the calculated index and spherical position
                fixed4 col = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(sphereCoords.xy, index));

                return col;
            }
            
            ENDCG
        }
    }
}
