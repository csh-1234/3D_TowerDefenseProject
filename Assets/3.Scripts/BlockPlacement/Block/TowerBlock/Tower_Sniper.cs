using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Sniper : Tower_Placement
{
    public int _TowerPrice = 50;
    protected override void Awake()
    {
        towerID = 107;
        blockType = 107;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}
