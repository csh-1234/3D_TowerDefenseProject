using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ButtonContinue : UI_ButtonBigOrange
{
    protected override void OnEnter()
    {
        if(GameManager.Instance.IsSaved)
        base.OnEnter();
    }
    protected override void OnExit()
    {
        if (GameManager.Instance.IsSaved)
            base.OnExit();
    }
}
