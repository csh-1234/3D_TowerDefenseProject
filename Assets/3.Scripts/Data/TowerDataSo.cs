using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class TowerDataSo : ScriptableObject
{
    public string Name = "";
    public string Element = "";
    public string Damage = "";
    public string Range = "";
    public string FireRate = "";
    public string Price = "";
    public string Info = "";
    public string UpgradePrice = "";
    public string SellPrice = "";

    public Sprite TowerImage;
}