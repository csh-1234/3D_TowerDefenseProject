using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Basic : Tower_Placement
{
    public int _TowerPrice = 10;
    protected override void Awake()
    {
        blockType = 101;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}