using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;

public class PrefabPreviewUtility : MonoBehaviour
{
    private Image image;
    public GameObject prefabTexture;

    private void Awake()
    {
        image = transform.GetComponent<Image>();
    }
    private void Start()
    {
        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabTexture);

        if (previewTexture != null)
        {
            Sprite sprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            image.sprite = sprite;
        }
    }
}
#endif