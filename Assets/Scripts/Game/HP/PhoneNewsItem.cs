using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Prefab 1 baris berita di panel Browse (seperti rangkaian berita terkini).
/// Struktur: root (Button) -> Thumbnail (Image), Judul (TMP).
/// </summary>
public class PhoneNewsItem : MonoBehaviour
{
    public Button tombol;
    public TextMeshProUGUI teksJudul;
    public Image gambarThumbnail;

    public void Setup(string judul, Sprite thumbnail, UnityAction onClick)
    {
        if (tombol == null) tombol = GetComponent<Button>();

        if (teksJudul != null) teksJudul.text = judul;

        if (gambarThumbnail != null)
        {
            gambarThumbnail.sprite = thumbnail;
            gambarThumbnail.enabled = thumbnail != null;
        }

        if (tombol != null)
        {
            tombol.onClick.RemoveAllListeners();
            tombol.onClick.AddListener(onClick);
        }
    }
}