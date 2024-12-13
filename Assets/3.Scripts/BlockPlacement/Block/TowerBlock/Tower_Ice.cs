using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Ice : Tower_Placement
{
    public int _TowerPrice = 10;
    protected override void Awake()
    {
        towerID = 108;
        blockType = 108;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}
