using UnityEngine;

[System.Serializable]
public class LuggageItemData
{
    public string itemID;
    public string itemName;
    public Sprite itemSprite;
    public Sprite xrayMaskSprite;
    public Vector2 gridPosition;
    public bool isMetallic;
    public bool isOrganic;
    public bool isContraband;
    public float itemWeight; // TAMBAHAN BARU: Berat barang dalam kg
}