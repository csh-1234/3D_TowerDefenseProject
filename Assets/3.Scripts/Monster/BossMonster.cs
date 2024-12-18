using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossMonster : Monster
{
    private Animator anim;
    private bool isStart = false;
    [SerializeField] Slider bossHpBar;
    [SerializeField] TextMeshProUGUI HealthText;
    [SerializeField] WaveManager waveInfo;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.Instance.BossCount = 1;
    }
    protected override void Update()
    {
        if(waveInfo.isFirstBattleClicked == true && isStart == false)
        {
            StartCoroutine(DoRoar());
            isStart = true;
            bossHpBar.gameObject.SetActive(true);
        }
        base.Update();
        bossHpBar.value = (float)hp / maxHp;
        HealthText.text = $"{hp}/{maxHp}";
    }

    IEnumerator DoRoar()
    {
        anim.SetBool("IsScream", true);
        yield return new WaitForSeconds(3f);
        anim.SetBool("IsWalk", true);
        Initialize(spawnPos);
        isMoving = true;
        yield return new WaitForSeconds(1f);
    }

    public override void TakeDamage(int damage)
    {
        if (IsDead) return;
        if(isMoving == false)
        { return; }

        GameObject damageFontPrefab = Resources.Load<GameObject>("Effects/DamageFont");
        GameObject damageCanvas = GameObject.Find("DamageCanvas");
        DamageFontEffect damageEffect = ObjectManager.Instance.Spawn<DamageFontEffect>(damageFontPrefab, Vector3.zero, Quaternion.identity);

        damageEffect.transform.SetParent(damageCanvas.transform, false);
        damageEffect.SetDamageText(damage.ToString(), transform.position);

        hp -= damage;

        if (hp <= 0)
        {
            hp = 0;
            isMoving = false;
            GameManager.Instance.CurrentMoney += 1000;
            anim.SetBool("IsDead", true);
            StartCoroutine(removeBoss());

        }
    }
    IEnumerator removeBoss()
    {
        yield return new WaitForSeconds(5f);
        Destroy(bossHpBar.gameObject);
        Destroy(this.gameObject);
    }
}
