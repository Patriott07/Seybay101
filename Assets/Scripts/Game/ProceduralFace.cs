using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    // Face renderer to change rendered sprite to sprite from the container 
    public SpriteRenderer baseFaceRenderer;
    public SpriteRenderer hairRenderer;
    public SpriteRenderer eyesRenderer;
    public SpriteRenderer noseRenderer;
    public SpriteRenderer mouthRenderer;

    // Face container
    [System.Serializable]
    public class Face
    {
        public List<Sprite> phaseOne;
        public List<Sprite> phaseTwo;
        public List<Sprite> phaseTree;

        public List<Sprite> baseFace; 
        public List<Sprite> hair; 
        public List<Sprite> eyes; 
        public List<Sprite> nose; 
        public List<Sprite> mouth; 
    }

    [SerializeField]
    Face face;

   void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ProceduralGenerateFace();
        }
    }

    void ProceduralGenerateFace()
    {
        int _genRandom = Random.Range(0, face.baseFace.Count);
        baseFaceRenderer.sprite = face.baseFace[_genRandom];

        _genRandom = Random.Range(0, face.hair.Count);
        hairRenderer.sprite = face.hair[_genRandom];


        _genRandom = Random.Range(0, face.eyes.Count);
        eyesRenderer.sprite = face.eyes[_genRandom];


        _genRandom = Random.Range(0, face.nose.Count);
        noseRenderer.sprite = face.nose[_genRandom];


        _genRandom = Random.Range(0, face.mouth.Count);
        mouthRenderer.sprite = face.mouth[_genRandom];
    } 
}
