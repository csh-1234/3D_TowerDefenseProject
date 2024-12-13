using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private int CurrentGold;  //기본 골드

    //사용 예시
    //private void Start()
    //{
    //    Managers.Instance.Currency.OnGoldChanged += UpdateGoldText;
    //    CurrentGold.text = $"{CurrentGold.text = $"{string.Format("{0:N0}", Managers.Instance.Currency.GetCurrentGold())}"}";
    //}
    //private void UpdateGoldText(int goldAmount) { CurrentGold.text = $"{string.Format("{0:N0}", goldAmount)}"; }
    //
    //ex) Managers.Instance.Currency.AddGold(goldAmount); 이게 호출되면
    // 갱신된 골드를 매개변수로 

    public event Action<int> OnGoldChanged;
    public void SetGold(int addAmount)
    {
        CurrentGold = addAmount;
        OnGoldChanged?.Invoke(CurrentGold);
    }
    public void AddGold(int addAmount)
    {
        CurrentGold += addAmount;
        OnGoldChanged?.Invoke(CurrentGold);
    }
    public void RemoveGold(int addAmount)
    {
        //일단 골드가 마이너스가 되는걸 막아놓음. 강화나 구매시 체크하는 로직 필요
        CurrentGold = Mathf.Max(0, CurrentGold - addAmount);
        OnGoldChanged?.Invoke(CurrentGold);
    }
    public int GetCurrentGold() { return CurrentGold; }
}
