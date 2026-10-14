using Schema.data;
using UnityEngine;
using UnityEngine.UI;

public class ToggleBarScriptItemBuy : MonoBehaviour
{
    [SerializeField]
    private ItemOnDay item;
    private Toggle toggle;
    private Text textName;

    public void Init(ItemOnDay val)
    {
        item = val;
        // setup component here
        textName.text = $"{val.name_item} <color=\"yellow\">({val.price})</color>";
    }
}
