using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Poison : Tower_Placement
{
    public int _TowerPrice = 10;
    protected override void Awake()
    {
        towerID = 106;
        blockType = 106;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}
