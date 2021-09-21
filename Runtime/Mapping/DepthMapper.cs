using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace JelleKUL.MeshMapping
{
    /// <summary>
    /// Functions to create a Depth map using Raycasts to a meshCollider
    /// </summary>
    public class DepthMapper : MonoBehaviour
    {
        [Header("Mapping Parameters")]
        [SerializeField]
        [Tooltip("The physicslayers the rays will hit")]
        private LayerMask rayCastLayer;
        [SerializeField]
        [Min(0.1f)]
        [Tooltip("The max range of the raycast, after that everything wil be black")]
        private float maxRange = 8;
        [SerializeField]
        [Tooltip("The size of the image, so the number of rays to cast, higher number: longer render")]
        private Vector2Int imageSize;

        [Header("Saving Parameters")]
        [SerializeField]
        [Tooltip("save the resulting depthmap to a file, use the texturename to define the path.")]
        private bool saveToFile = false;
        [SerializeField]
        [Tooltip("The name and path of the image, .jpg gets added automatically")]
        private string textureName = "depthMap";
        [SerializeField]
        [Tooltip("Save to depthmap to the alpha channel of the provided targetHDRI, remapping the resolution")]
        private bool saveToTarget = false;
        [SerializeField]
        [Tooltip("The TargetHDRI to match the Transform to and save the depth map to.")]
        private LocalisedImageScriptableObject targetHDRI;

        [Header("Debug Parameters")]
        [SerializeField]
        [Tooltip("Provide a target renderer to display the output in the scene, works best with a plane")]
        private Renderer targetRenderer;
        [SerializeField]
        [Tooltip("Draws all the raycasts and colors them depending on the distance, is quite slow.")]
        private bool drawRays = false;

        Vector3[] rayDirections = new Vector3[0];


        // Start is called before the first frame update
        void Start()
        {
            RenderDepth();
        }

        /// <summary>
        /// Sets up the required data like a texture and the rays.
        /// </summary>
        void SetupTransform()
        {
            rayDirections = GetAllDirections(imageSize.x, imageSize.y);

            if (targetHDRI == null)
            {
                Debug.LogWarning(gameObject.name + ": Max range cannot be 0, please enter a correct value");
                return;
            }
            transform.position = targetHDRI.position;
            transform.rotation = targetHDRI.GetRotation();
        }

        /// <summary>
        /// Creates a depth texture with the given parameters.
        /// </summary>
        /// <returns>The resulting depth texture as a grayscale Texture2D</returns>
        public Texture2D RenderDepth()
        {
            Texture2D texture = new Texture2D(imageSize.x, imageSize.y);
            if (targetRenderer) targetRenderer.material.mainTexture = texture;

            SetupTransform();

            //check the depth and return the correct color to distance
            if (maxRange == 0)
            {
                Debug.LogWarning(gameObject.name + ": Max range cannot be 0, please enter a correct value");
                return null;
            }

            int counter = 0;
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    RaycastHit hit;
                    Vector3 raycastDirection = transform.TransformDirection(rayDirections[counter]);
                    if (Physics.Raycast(transform.position, raycastDirection, out hit, maxRange, rayCastLayer)) // Does the ray intersect any meshes on the raycast layer
                    {
                        Color newColor = Color.HSVToRGB(0, 0, 1 - (hit.distance / maxRange)); // create a new color that is grayscale depending on the distance.
                        texture.SetPixel(x, y, newColor); // set the pixel of the texture to that color

                        if (drawRays) Debug.DrawRay(transform.position, raycastDirection * hit.distance, newColor, 2f);
                    }
                    else //if the ray did not hit anything, set the pixel to black 
                    {
                        texture.SetPixel(x, y, Color.black);
                    }
                    counter++;
                }
            }
            texture.Apply(); //apply the texture to actually set the pixels to the texture
            Debug.Log(gameObject.name + ": Texture applied");


            if (saveToFile)
            {
                //saving the texture
                byte[] bytes = texture.EncodeToJPG();
                File.WriteAllBytes(Application.dataPath + "/" + textureName + ".jpg", bytes);
                Debug.Log(gameObject.name + ": Data saved @ " + Application.dataPath + "/" + textureName + ".jpg");
            }

            if (saveToTarget)
            {
                Color[] textureColors = targetHDRI.image.GetPixels(); //create a new array to change all the pixels at once

                counter = 0;
                for (int y = 0; y < targetHDRI.image.height; y++)
                {
                    for (int x = 0; x < targetHDRI.image.width; x++)
                    {
                        int newx = LerpInt(x, 0, targetHDRI.image.width, 0, texture.width);
                        int newy = LerpInt(y, 0, targetHDRI.image.height, 0, texture.height);

                        Color.RGBToHSV(texture.GetPixel(newx, newy), out float H, out float S, out float V); //convert the depthTexture to hsv to get the value at the new pixel
                        textureColors[counter].a = V; // set the alpha value to that new value
                        counter++;
                    }
                }
                targetHDRI.image.SetPixels(textureColors);
                targetHDRI.image.Apply(false);
            }
            return texture;
        }

        /// <summary>
        /// Helper Function to Remap values to a new range
        /// </summary>
        int LerpInt(float value, float low1, float high1, float low2, float high2)
        {
            return Mathf.FloorToInt(low2 + (value - low1) * (high2 - low2) / (high1 - low1));
        }

        /// <summary>
        /// Converts a normalised Spherical coordinate to a normalised cartesian direction
        /// </summary>
        /// <param name="phi">Horizontal angle (0,2 * Pi)</param>
        /// <param name="theta">Vertival angle (0,Pi)</param>
        /// <returns>An xyz Direction</returns>
        Vector3 GetDirection(float phi, float theta)
        {
            Vector3 cartesianVector = Vector3.zero;
            phi += Mathf.PI / 2f; // offset by 90° to center the image to the forward direction
            phi *= -1; // multiply by -1 to invert the angle (inside out)

            // the y&z axis are switched compared to [this](https://en.wikipedia.org/wiki/Spherical_coordinate_system) article because Unity uses a non conventional axis layout
            cartesianVector.x = Mathf.Cos(phi) * Mathf.Sin(theta);
            cartesianVector.z = Mathf.Sin(phi) * Mathf.Sin(theta);
            cartesianVector.y = -Mathf.Cos(theta); // multiply by -1 to go from bottom to top

            return cartesianVector;
        }

        /// <summary>
        /// Creates a list of directions in local space covering a sphere
        /// going from left to right, bottom to top.
        /// going Row by Row.
        /// </summary>
        /// <param name="width"> The amount of directions in the horizontal axis </param>
        /// <param name="height"> The amount of directions in the vertical axis </param>
        /// <returns> An array containing all the directions in local space </returns>
        Vector3[] GetAllDirections(int width, int height)
        {
            Vector3[] newRayDirections = new Vector3[width * height];
            int counter = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    newRayDirections[counter] = GetDirection(x / (float)width * Mathf.PI * 2, y / (float)height * Mathf.PI);
                    counter++;
                }
            }
            return newRayDirections;
        }
    }
}