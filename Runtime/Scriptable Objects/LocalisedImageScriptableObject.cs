using UnityEngine;

[CreateAssetMenu(fileName = "NewLocalisedImage", menuName = "ScriptableObjects/Localised Image")]
public class LocalisedImageScriptableObject : ScriptableObject
{
    [Header("Image Parameters")]
    [Tooltip("The Image to link to this Localised transform")]
    public Texture2D image;
    [Tooltip("The horizontal field of view of the image")]
    [Range(0,360)]
    public float fov = 360;

    [Header("Transform Parameters")]
    [Tooltip("The position of the Image")]
    public Vector3 position;
    [Tooltip("The rotation of the Image, saved as a Vector4 so they can be easily edited in the inspector")]
    public Vector4 rotation;

    [Header("Visual parameters")]
    [Tooltip("The material to add the image to to display it in the scene")]
    public Material material;

    /// <summary>
    /// Returns the rotation as a quaternion
    /// </summary>
    public Quaternion GetRotation()
    {
        return new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
    }

    /// <summary>
    /// Returns the rotation as a eulerAngles
    /// </summary>
    public Vector3 GetRotationEuler()
    {
        return  (new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w)).eulerAngles * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Returns the transform as a trs matrix
    /// </summary>
    public Matrix4x4 GetTrsMatrix()
    {
        return (Matrix4x4.TRS(position, GetRotation(), Vector3.one));
    }

    /// <summary>
    /// Set the transform
    /// </summary>
    /// <param name="pos"> the position as a Vector3 </param>
    /// <param name="rot"> the rotation as a Quaternion </param>
    public void SetTransform(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = new Vector4(rot.x, rot.y, rot.z, rot.w);
    }
}
