using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TowerLoadOut : MonoBehaviour
{
    public Button Exit;
    public GameObject NoOtherCardsAvailable;
    public List<Transform> EquipSlotList;
    public List<Transform> UnEquipSlotList;

    private void Awake()
    {
        Exit.onClick.AddListener(ExitTowerLoadOut);
        Exit.onClick.AddListener(FindObjectOfType<UI_LobbyScene>().TowerCard);
    }

    private void Start()
    {
        for (int i = 0; i < GameManager.Instance.EquipTowerList.Count; i++)
        {
            string towerName = $"TowerLoadoutCard/{GameManager.Instance.EquipTowerList[i]}";
            GameObject prefab = Resources.Load<GameObject>(towerName);
            if (prefab == null)
            {
                continue;
            }
            Instantiate(prefab, EquipSlotList[i]);
        }

        for (int i = 0; i < GameManager.Instance.UnEquipTowerList.Count; i++)
        {
            string towername = $"TowerLoadoutCard/{GameManager.Instance.UnEquipTowerList[i]}";
            GameObject prefab = Resources.Load<GameObject>(towername);
            if (prefab == null)
            {
                continue;
            }
            Instantiate(prefab, UnEquipSlotList[i]);
        }
    }

    private void Update()
    {
        int nullCount=0;
        for (int i = 0; i < GameManager.Instance.UnEquipTowerList.Count; i++)
        {
            string towerName = $"TowerLoadoutCard/{GameManager.Instance.UnEquipTowerList[i]}";
            GameObject prefab = Resources.Load<GameObject>(towerName);

            if (prefab == null)
            {
                nullCount++;
                continue;
            }
        }
        if (nullCount != GameManager.Instance.UnEquipTowerList.Count)
        {
            NoOtherCardsAvailable.SetActive(false);
        }
        else
        {
            NoOtherCardsAvailable.SetActive(true);
        }
    }

    private void ExitTowerLoadOut()
    {
        SoundManager.Instance.Play("Click18", SoundManager.Sound.Effect);
        gameObject.SetActive(false);
    }
}
