using TMPro;
using UnityEngine;

public class UI_IngameScene : MonoBehaviour
{
    private static UI_IngameScene instance;
    public static UI_IngameScene Instance { get { return instance; } }

    public TextMeshProUGUI CurrentHp;
    public TextMeshProUGUI MaxHp;
    public TextMeshProUGUI CurrentMoney;
    public Transform TowerCardLocation1;
    public Transform TowerCardLocation2;
    public GameObject EmptyCard;
    public GameObject PausePanel;
    public static int PopupCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }
        
    }

    private void Start()
    {
        if(GameManager.Instance.clearStage==6)
        {
            GameManager.Instance.bounusMoney += 550;
        }
        GameManager.Instance.CurrentMoney = 50 + GameManager.Instance.bounusMoney;
        SoundManager.Instance.Play("Battlefield", SoundManager.Sound.Bgm);
        TowerCardSet();
    }
     
    private void Update()
    {
        CurrentHp.text = $"{GameManager.Instance.CurrentHp}";
        MaxHp.text = $"/{GameManager.Instance.MaxHp}";
        CurrentMoney.text = $"{GameManager.Instance.CurrentMoney}";

        if (Input.GetButtonDown("Cancel"))
        {
            Time.timeScale = 0f;
            PausePanel.SetActive(true);
        }
    }

    private void TowerCardSet()
    {
        for (int i = 0; i < 8; i++)
        {
            print(GameManager.Instance.EquipTowerList[i]);

            GameObject cardInstance = null;

            if (!string.IsNullOrEmpty(GameManager.Instance.EquipTowerList[i]))
            {
                // 해당 인덱스에 타워가 있는 경우
                if (i < 4)
                {
                    cardInstance = Instantiate(Resources.Load<GameObject>($"InGameTowerCard/{GameManager.Instance.EquipTowerList[i]}"), TowerCardLocation1);
                }
                else
                {
                    cardInstance = Instantiate(Resources.Load<GameObject>($"InGameTowerCard/{GameManager.Instance.EquipTowerList[i]}"), TowerCardLocation2);
                }
            }
            else
            {
                // 해당 인덱스에 타워가 없는 경우 빈 카드
                if (i < 4)
                {
                    cardInstance = Instantiate(EmptyCard, TowerCardLocation1);
                }
                else
                {
                    cardInstance = Instantiate(EmptyCard, TowerCardLocation2);
                }
            }
        }
    }
}
