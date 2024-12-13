using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Drone : Tower_Placement
{
    public int _TowerPrice = 15;
    protected override void Awake()
    {
        towerID = 105;
        blockType = 105;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}
