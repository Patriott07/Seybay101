using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Satu baris kontak di Inbox
public class PhoneContactItem : MonoBehaviour
{
    public Button tombol;
    public Image avatar;
    public TextMeshProUGUI namaText;
    public TextMeshProUGUI previewText;
    public GameObject titikUnread;

    public void Setup(PhoneContactSO kontak, string preview, bool unread, Action onClick)
    {
        namaText.text = kontak.namaKontak;
        previewText.text = preview;

        if (avatar != null)
        {
            avatar.sprite = kontak.avatar;
            avatar.enabled = kontak.avatar != null;
        }

        if (titikUnread != null)
            titikUnread.SetActive(unread);

        tombol.onClick.RemoveAllListeners();
        tombol.onClick.AddListener(() => onClick?.Invoke());
    }
}