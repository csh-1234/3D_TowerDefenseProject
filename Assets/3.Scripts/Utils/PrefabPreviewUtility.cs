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
        // 프리팹의 미리보기 이미지를 가져옴
        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabTexture);

        if (previewTexture != null)
        {
            // Texture2D를 Sprite로 변환
            Sprite sprite = Sprite.Create(
                previewTexture,
                new Rect(0, 0, previewTexture.width, previewTexture.height),
                new Vector2(0.5f, 0.5f)
            );

            // Image 컴포넌트에 Sprite 할당
            image.sprite = sprite;
        }
        else
        {
            Debug.LogWarning("프리팹 미리보기를 가져올 수 없습니다.");
        }
    }
}
#endif