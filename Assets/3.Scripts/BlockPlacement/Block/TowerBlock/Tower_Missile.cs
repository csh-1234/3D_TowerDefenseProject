using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Missile : Tower_Placement
{
    public int _TowerPrice = 65;
    protected override void Awake()
    {
        towerID = 102;
        blockType = 102;
        TowerPrice = _TowerPrice;
        base.Awake();
    }
}