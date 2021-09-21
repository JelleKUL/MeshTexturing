using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Functions to edit the data of a localisedImage
/// </summary>
public class LocalisedImageController : MonoBehaviour
{
    [SerializeField]
    private bool UpdateInEditor = false;

    public LocalisedImageScriptableObject localisedImage;

    private void OnDrawGizmosSelected()
    {
        if (!UpdateInEditor) return;

        UpdateImage();
    }

    /// <summary>
    /// Updates the LocalisedImage scriptable object with the current Transform
    /// </summary>
    void UpdateImage()
    {
        if (!localisedImage) return;

        localisedImage.SetTransform(transform.position, transform.rotation);
    }

    /// <summary>
    /// Set the material of the object
    /// </summary>
    /// <param name="mat">The material to set</param>
    public void SetMaterial(Material mat)
    {
        if (TryGetComponent(out MeshRenderer rend))
        {
            rend.material = mat;
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": No MeshRenderer attached");
        }

    }
}
