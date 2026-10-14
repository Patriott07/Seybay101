using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Dipasang di root prefab BubbleRow
public class PhoneBubble : MonoBehaviour
{
    [Header("Referensi (isi di prefab)")]
    public HorizontalLayoutGroup baris;     // root BubbleRow
    public Image latarBubble;               // child Bubble (Image)
    public LayoutElement layoutBubble;      // LayoutElement di child Bubble
    public TextMeshProUGUI isiTeks;         // TMP di dalam Bubble

    [Tooltip("Harus sama dengan padding kiri/kanan VerticalLayoutGroup di Bubble")]
    public float paddingHorizontal = 20f;

    public void Setup(string teks, bool dariAldo, float maxLebar, Color warna)
    {
        // Kontak = kiri, Aldo = kanan
        baris.childAlignment = dariAldo ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        latarBubble.color = warna;

        isiTeks.text = teks;
        isiTeks.alignment = TextAlignmentOptions.TopLeft;

        // Lebar bubble mengikuti panjang teks, dibatasi maxLebar
        Vector2 pref = isiTeks.GetPreferredValues(teks, Mathf.Infinity, Mathf.Infinity);
        layoutBubble.preferredWidth = Mathf.Min(pref.x + paddingHorizontal * 2f, maxLebar);
    }
}