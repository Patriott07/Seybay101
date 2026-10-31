using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// Kunci interaksi global. Saat ada panel UI terbuka ATAU proses (coroutine) yang sedang berjalan,
/// semua interaksi dikunci kecuali bagian yang diizinkan.
/// </summary>
public static class InteractionLock
{
    class Entri
    {
        public object owner;
        public Transform[] izin;         
        public Behaviour pemantau;       
    }

    static readonly List<Entri> stack = new List<Entri>();
    static readonly List<MonoBehaviour> dimatikan = new List<MonoBehaviour>();
    static readonly Dictionary<Camera, int> eventMaskAsli = new Dictionary<Camera, int>();
    
    static readonly List<Collider2D> collider2DMatikan = new List<Collider2D>();
    static readonly List<Collider> colliderMatikan = new List<Collider>();

    static readonly List<Transform> pengecualian = new List<Transform>();

    public static bool logDebug = false;

    public static bool IsLocked
    {
        get
        {
            if (Bersihkan())
                Terapkan();

            return stack.Count > 0;
        }
    }

    public static bool CanInteract(Component c)
    {
        if (!IsLocked) return true;
        if (c == null) return false;

        return DiIzinkan(c.transform, stack[stack.Count - 1]);
    }

    public static void Lock(object owner, params Transform[] izin)
    {
        LockInternal(owner, null, izin);
    }

    public static void Unlock(object owner)
    {
        if (Hapus(owner))
            Terapkan();
    }

    public static void UnlockAll()
    {
        stack.Clear();
        Terapkan();
    }

    public static void TambahPengecualian(Transform t)
    {
        if (t != null && !pengecualian.Contains(t))
            pengecualian.Add(t);
    }

    public static IEnumerator Bungkus(Behaviour pemantau, IEnumerator routine, params Transform[] izin)
    {
        object token = new object();
        LockInternal(token, pemantau, izin);

        try
        {
            while (routine.MoveNext())
                yield return routine.Current;
        }
        finally
        {
            Unlock(token);
        }
    }

    static void LockInternal(object owner, Behaviour pemantau, Transform[] izin)
    {
        if (owner == null) return;

        Hapus(owner);   
        stack.Add(new Entri { owner = owner, izin = izin, pemantau = pemantau });

        Terapkan();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset()
    {
        stack.Clear();
        dimatikan.Clear();
        eventMaskAsli.Clear();
        collider2DMatikan.Clear();
        colliderMatikan.Clear();
        pengecualian.Clear();

        SceneManager.sceneLoaded -= SaatSceneDimuat;
        SceneManager.sceneLoaded += SaatSceneDimuat;
    }

    static void SaatSceneDimuat(Scene scene, LoadSceneMode mode)
    {
        // --- PENGAMAN UTAMA ---
        // Setiap kali pindah scene (misal dari PreDay/Story ke MainDesk), 
        // bersihkan SEMUA kunci secara total agar pemain langsung bebas berinteraksi.
        stack.Clear();
        dimatikan.Clear();
        eventMaskAsli.Clear();
        collider2DMatikan.Clear();
        colliderMatikan.Clear();
        
        PulihkanSemua();
        if (logDebug) Debug.Log("[InteractionLock] Scene dimuat: " + scene.name + " - Semua kunci di-reset bersih.");
    }

    static bool Hapus(object owner)
    {
        return stack.RemoveAll(e => e.owner.Equals(owner)) > 0;
    }

    static bool OwnerMati(Entri e)
    {
        if (e.owner == null) return true;

        Object objekUnity = e.owner as Object;
        if (objekUnity != null || e.owner is Object)
        {
            if (objekUnity == null) return true;
        }

        if (e.pemantau != null || e.pemantau is Object)
        {
            if (e.pemantau == null || !e.pemantau.isActiveAndEnabled) return true;
        }

        return false;
    }

    static bool Bersihkan()
    {
        return stack.RemoveAll(OwnerMati) > 0;
    }

    static bool DiIzinkan(Transform t, Entri e)
    {
        if (e.izin == null) return false;

        foreach (Transform root in e.izin)
            if (root != null && t.IsChildOf(root))
                return true;

        return false;
    }

    static bool DiPengecualian(Transform t)
    {
        foreach (Transform p in pengecualian)
            if (p != null && t.IsChildOf(p))
                return true;

        return false;
    }

    static void Terapkan()
    {
        PulihkanSemua();
        Bersihkan();

        if (stack.Count == 0)
        {
            if (logDebug) Debug.Log("[InteractionLock] tidak ada kunci aktif, semua interaksi dibuka.");
            return;
        }

        Entri atas = stack[stack.Count - 1];

        // 1) Objek dunia: matikan event mouse di semua kamera
        Camera[] kamera = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Camera c in kamera)
        {
            if (c == null || eventMaskAsli.ContainsKey(c)) continue;

            eventMaskAsli[c] = c.eventMask;
            c.eventMask = 0;
        }

        // 2) Matikan Collider (2D & 3D) di scene yang tidak diizinkan
        Collider2D[] col2Ds = Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var col in col2Ds)
        {
            if (col != null && col.enabled && !DiIzinkan(col.transform, atas) && !DiPengecualian(col.transform))
            {
                col.enabled = false;
                collider2DMatikan.Add(col);
            }
        }

        Collider[] cols = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var col in cols)
        {
            if (col != null && col.enabled && !DiIzinkan(col.transform, atas) && !DiPengecualian(col.transform))
            {
                col.enabled = false;
                colliderMatikan.Add(col);
            }
        }

        // 3) Nonaktifkan komponen UI pointer di luar izin
        MonoBehaviour[] semua = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour mb in semua)
        {
            if (mb == null || !mb.enabled) continue;
            if (!PunyaHandlerPointer(mb)) continue;
            if (DiIzinkan(mb.transform, atas)) continue;
            if (DiPengecualian(mb.transform)) continue;

            mb.enabled = false;
            dimatikan.Add(mb);
        }
    }

    static void PulihkanSemua()
    {
        foreach (var pasangan in eventMaskAsli)
            if (pasangan.Key != null)
                pasangan.Key.eventMask = pasangan.Value;

        eventMaskAsli.Clear();

        foreach (var col in collider2DMatikan)
            if (col != null) col.enabled = true;
        collider2DMatikan.Clear();

        foreach (var col in colliderMatikan)
            if (col != null) col.enabled = true;
        colliderMatikan.Clear();

        foreach (MonoBehaviour mb in dimatikan)
            if (mb != null)
                mb.enabled = true;

        dimatikan.Clear();
    }

    static bool PunyaHandlerPointer(MonoBehaviour mb)
    {
        return mb is IPointerClickHandler
            || mb is IPointerDownHandler
            || mb is IPointerUpHandler
            || mb is IPointerEnterHandler
            || mb is IPointerExitHandler
            || mb is IBeginDragHandler
            || mb is IDragHandler
            || mb is IEndDragHandler
            || mb is IDropHandler
            || mb is IScrollHandler
            || mb is ISubmitHandler;
    }
}

public static class InteractionLockExtensions
{
    public static Coroutine StartCoroutineTerkunci(this MonoBehaviour pemilik, IEnumerator routine, params Transform[] izin)
    {
        return pemilik.StartCoroutine(InteractionLock.Bungkus(pemilik, routine, izin));
    }
}