using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JelleKUL.MeshMapping
{
    /// <summary>
    /// Manages The Image mapper shader's connection
    /// </summary>
    public class MeshMappingController : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("The material To map the HDRI's to")]
        private Material mappingMaterial;
        [SerializeField]
        [Tooltip("The Image list containing all the HDRI's to map")]
        private LocalisedImageListScriptableObject imageList;

        private ComputeBuffer buffer;

        private void OnDrawGizmos()
        {
            SetBuffer();
        }

        private void OnDrawGizmosSelected()
        {
            SetImageBuffer();
        }

        /// <summary>
        /// Send the created texture array to the material Buffer
        /// </summary>
        void SetImageBuffer()
        {
            mappingMaterial.SetTexture("_TextureArray", SetTextureArray());
        }

        /// <summary>
        /// send the Position and roation for each HDRI in 
        /// </summary>
        void SetBuffer()
        {
            if (!(mappingMaterial && imageList)) return;

            HDRI[] hDRIs = new HDRI[imageList.images.Count];
            for (int i = 0; i < hDRIs.Length; i++)
            {
                hDRIs[i] = new HDRI()
                {
                    pos = imageList.images[i].position,
                    rot = imageList.images[i].GetRotationEuler(),
                    fov = imageList.images[i].fov
                };
            }

            buffer = new ComputeBuffer(hDRIs.Length, hDRIs[0].GetSize(), ComputeBufferType.Default);

            buffer.SetData(hDRIs);
            mappingMaterial.SetBuffer("HDRIS", buffer);
            mappingMaterial.SetInt("numHDRI", hDRIs.Length);

            //buffer.Release();

        }

        /// <summary>
        /// Creates a texturearray from the imagelist scriptableobject
        /// </summary>
        /// <returns> the array </returns>
        public Texture2DArray SetTextureArray()
        {
            if (!(mappingMaterial && imageList)) return null;

            Texture2D[] textures = new Texture2D[imageList.images.Count];
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i] = imageList.images[i].image;
            }
            Texture2DArray array = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, false);
            for (int i = 0; i < textures.Length; i++)
                array.SetPixels(textures[i].GetPixels(), i);

            array.Apply();
            return array;
        }

        void OnDestroy()
        {
            buffer.Release();
        }

        /// <summary>
        /// Custom data structure to send to the GPU buffer
        /// </summary>
        struct HDRI
        {
            //public Matrix4x4 transformMatrix;
            //public Vector4[] pixels;
            public Vector3 pos;
            public Vector3 rot;
            public float fov; // the horizontal fov of the image

            public int GetSize()
            {
                int bufferSize = 0;
                //bufferSize += System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector4)) * pixels.Length;
                //bufferSize += System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
                bufferSize += System.Runtime.InteropServices.Marshal.SizeOf(typeof(Vector3)) * 2;
                bufferSize += System.Runtime.InteropServices.Marshal.SizeOf(typeof(float));
                return bufferSize;
            }
        }

    }
}