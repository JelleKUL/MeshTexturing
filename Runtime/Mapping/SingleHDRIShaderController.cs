using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls for the Single HDRI depth Mapper
/// </summary>
public class SingleHDRIShaderController : MonoBehaviour
{
    [SerializeField]
    private bool UpdateShaderInEditor = false;
    [SerializeField]
    private Material singleHDRIMaterial;
    [SerializeField]
    private LocalisedImageScriptableObject localisedImage;

    private void OnDrawGizmosSelected()
    {
        if (!UpdateShaderInEditor) return;

        UpdateMaterial();
    }

    /// <summary>
    /// Updates the material with a texture, position and rotation
    /// </summary>
    public void UpdateMaterial()
    {
        if (!(singleHDRIMaterial && localisedImage))
        {
            Debug.LogWarning("No material or localisedImage Set");
            return;
        }

        singleHDRIMaterial.SetTexture("_MainTex", localisedImage.image);
        singleHDRIMaterial.SetVector("_HDRIPos", transform.position);
        singleHDRIMaterial.SetVector("_HDRIRot", transform.rotation.eulerAngles * Mathf.Deg2Rad);

    }
}
