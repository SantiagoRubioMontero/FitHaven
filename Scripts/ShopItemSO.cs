using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="ShopMenu", menuName = "New shop item", order = 1)]//Nos permite crear objetos de este tipo desd eel interfaz de UNITY
public class ShopItemSO : ScriptableObject
{
    public string title;
    public string description;
    public int baseCost;
    public string id;

}
