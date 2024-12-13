using UnityEngine;
using UnityEngine.EventSystems;

public class UI_AltarPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public int EmberPrice;
    public int HpPrice;
    public GameObject Hilight;
    public GameObject Outline;
    public GameObject CantBuy;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hilight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hilight.SetActive(false);
    }

    private void Start()
    {
        if (EmberPrice > GameManager.Instance.CurrentEmber || HpPrice > GameManager.Instance.CurrentHp ||
            (gameObject.name == "RecoverPanel" && GameManager.Instance.MaxHp == GameManager.Instance.CurrentHp))
        {
            Outline.SetActive(false);
            CantBuy.SetActive(true);
        }
    }

}
