using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Flame : Tower_Placement
{
    public int _TowerPrice = 30;
    protected override void Awake()
    {
        towerID = 103;
        blockType = 103;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}