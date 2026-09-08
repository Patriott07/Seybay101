using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using Schema.data;

public class ProceduralFace : MonoBehaviour
{
    [System.Serializable]
    public class FacePart
    {
        public List<Sprite> items;
    }

    [System.Serializable]
    public class FaceContainer
    {
        public List<Sprite> baseFace;
        public List<Sprite> hair;
        public List<Sprite> eyes;
        public List<Sprite> nose;
        public List<Sprite> mouth;
        public List<Sprite> bodyClothes;
        public List<Sprite> shoes;
        public List<Sprite> accessories;
    }

    [Header("Face Renderers")]
    public SpriteRenderer baseFaceRenderer;
    public SpriteRenderer hairRenderer;
    public SpriteRenderer eyesRenderer;
    public SpriteRenderer noseRenderer;
    public SpriteRenderer mouthRenderer;

    [Header("Body Renderers")]
    public SpriteRenderer bodyClothesRenderer;

    [Header("Gender Containers")]
    public FaceContainer faceMan;
    public FaceContainer faceWoman;

    public Gender currentGender;
    private FaceContainer currentFace;

    void Start()
    {
        // SetGender(currentGender);
        ProceduralGenerateAll();
        SetRandColor();
    }

    void OnEnable()
    {
        GameEvent.GenerateNewNPCView += ProceduralGenerateAll;
        GameEvent.OnSetGander += SetGender;
    }

    void OnDisable()
    {
        GameEvent.GenerateNewNPCView -= ProceduralGenerateAll;
        GameEvent.OnSetGander -= SetGender;
    }

    public void SetRandColor()
    {
        if (baseFaceRenderer != null)
            baseFaceRenderer.color = RandomSkinTone();

        if (hairRenderer != null)
            hairRenderer.color = RandomHairColor();

        if (noseRenderer != null)
            noseRenderer.color = baseFaceRenderer.color;

        if (mouthRenderer != null)
            mouthRenderer.color = RandomLipColor();   
    }

    Color RandomSkinTone()
    {
        Color[] tones = new Color[]
        {
            new Color(0.92f, 0.78f, 0.56f),
            new Color(0.82f, 0.65f, 0.42f),
            new Color(0.72f, 0.52f, 0.32f),
            new Color(0.62f, 0.42f, 0.25f),
            new Color(0.52f, 0.34f, 0.18f),
            new Color(0.95f, 0.85f, 0.70f),
            new Color(0.85f, 0.68f, 0.48f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomHairColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.10f, 0.08f, 0.05f),
            new Color(0.20f, 0.14f, 0.08f),
            new Color(0.35f, 0.25f, 0.12f),
            new Color(0.65f, 0.50f, 0.25f),
            new Color(0.80f, 0.65f, 0.35f),
            new Color(0.70f, 0.12f, 0.10f),
            new Color(0.55f, 0.45f, 0.35f),
            new Color(0.08f, 0.08f, 0.08f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomEyeColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.10f, 0.10f, 0.05f),
            new Color(0.15f, 0.20f, 0.40f),
            new Color(0.20f, 0.50f, 0.30f),
            new Color(0.50f, 0.40f, 0.15f),
            new Color(0.25f, 0.15f, 0.05f),
            new Color(0.05f, 0.05f, 0.10f),
            new Color(0.35f, 0.25f, 0.15f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomLipColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.85f, 0.15f, 0.20f),
            new Color(0.95f, 0.30f, 0.35f),
            new Color(0.70f, 0.10f, 0.15f),
            new Color(0.90f, 0.50f, 0.50f),
            new Color(0.95f, 0.70f, 0.70f),
            new Color(0.65f, 0.08f, 0.12f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomClothesColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.15f, 0.25f, 0.45f),
            new Color(0.10f, 0.15f, 0.30f),
            new Color(0.50f, 0.50f, 0.50f),
            new Color(0.20f, 0.35f, 0.20f),
            new Color(0.45f, 0.30f, 0.15f),
            new Color(0.70f, 0.35f, 0.20f),
            new Color(0.30f, 0.20f, 0.45f),
            new Color(0.55f, 0.25f, 0.15f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomShoeColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.08f, 0.08f, 0.08f),
            new Color(0.12f, 0.10f, 0.06f),
            new Color(0.20f, 0.15f, 0.10f),
            new Color(0.35f, 0.25f, 0.15f),
            new Color(0.05f, 0.05f, 0.05f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomAccColor()
    {
        Color[] tones = new Color[]
        {
            new Color(0.90f, 0.20f, 0.20f),
            new Color(0.20f, 0.50f, 0.80f),
            new Color(0.80f, 0.80f, 0.80f),
            new Color(0.10f, 0.60f, 0.30f),
            new Color(0.90f, 0.70f, 0.10f),
            new Color(0.45f, 0.20f, 0.60f),
        };
        return tones[Random.Range(0, tones.Length)];
    }

    Color RandomColor()
    {
        return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }

    public void SetGender(Gender gender)
    {
        currentGender = gender;
        switch (gender)
        {
            case Gender.Man:
                currentFace = faceMan;
                break;
            case Gender.Woman:
                currentFace = faceWoman;
                break;
            default:
                currentFace = faceMan;
                break;
        }
    }

    public void ProceduralGenerateAll()
    {
        if (currentFace == null)
            return;

        baseFaceRenderer.sprite = GetRandom(currentFace.baseFace);
        hairRenderer.sprite = GetRandom(currentFace.hair);
        eyesRenderer.sprite = GetRandom(currentFace.eyes);
        noseRenderer.sprite = GetRandom(currentFace.nose);
        mouthRenderer.sprite = GetRandom(currentFace.mouth);
        // bodyClothesRenderer.sprite = GetRandom(currentFace.bodyClothes);
        // shoesRenderer.sprite = GetRandom(currentFace.shoes);
        // accessoriesRenderer.sprite = GetRandom(currentFace.accessories);

        SetRandColor();
    }

    public void GenerateFaceByGender(Gender gender)
    {
        // SetGender(gender);
        ProceduralGenerateAll();
    }

    public void GenerateBodyPartByGender(Gender gender)
    {
        // SetGender(gender);
        // bodyClothesRenderer.sprite = GetRandom(currentFace.bodyClothes);
        // shoesRenderer.sprite = GetRandom(currentFace.shoes);
        // accessoriesRenderer.sprite = GetRandom(currentFace.accessories);
    }

    T GetRandom<T>(List<T> list)
    {
        if (list == null || list.Count == 0)
            return default;
        return list[Random.Range(0, list.Count)];
    }
}
