using System.Collections.Generic;
using UnityEngine;

namespace JelleKUL.MeshMapping
{
    [CreateAssetMenu(fileName = "NewLocalisedImageList", menuName = "ScriptableObjects/Localised Image List")]
    public class LocalisedImageListScriptableObject : ScriptableObject
    {
        public GameObject imageSpherePrefab;

        public List<LocalisedImageScriptableObject> images;

        public void SpawnImages()
        {
            if (!imageSpherePrefab) return;

            foreach (var item in images)
            {
                string name = "ImageSphere - " + item.name;
                GameObject newObj = GameObject.Find(name);

                if (newObj == null)
                {
                    newObj = Instantiate(imageSpherePrefab, item.position, item.GetRotation());
                    newObj.name = name;
                }
                else
                {
                    newObj.transform.position = item.position;
                    newObj.transform.rotation = item.GetRotation();
                }

                if (newObj.TryGetComponent(out LocalisedImageController controller))
                {
                    controller.localisedImage = item;
                    controller.SetMaterial(item.material);
                }

            }
        }
    }
}
