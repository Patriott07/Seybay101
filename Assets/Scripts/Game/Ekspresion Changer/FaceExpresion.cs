using System.Collections.Generic;
using UnityEngine;

public enum FaceExpression
{
    Neutral,
    Angry,
    Sad,
    Anxious
}

[RequireComponent(typeof(ProceduralFace))]
public class FaceExpressionChanger : MonoBehaviour
{
    [System.Serializable]
    public class ExpressionSet
    {
        [Tooltip("Mata dan mulut menggunakan index pasangan yang sama.")]
        public List<Sprite> eyes;

        public List<Sprite> mouth;
    }

    [Header("Target")]
    [SerializeField] private ProceduralFace proceduralFace;

    [Header("Expression Sprites")]
    public ExpressionSet angry;
    public ExpressionSet sad;
    public ExpressionSet anxious;

    public FaceExpression CurrentExpression { get; private set; }
        = FaceExpression.Neutral;

    // =========================================================
    // ORIGINAL SPRITE
    // =========================================================

    private Sprite _originalEyes;
    private Sprite _originalMouth;

    private Sprite _appliedEyes;
    private Sprite _appliedMouth;

    private bool _hasOriginal;

    // =========================================================
    // LOCKED EXPRESSION
    // =========================================================

    private FaceExpression _lockedExpression =
        FaceExpression.Neutral;

    private bool _hasLocked;

    private int _pairIdx = -1;

    // =========================================================
    // UNITY
    // =========================================================

    private void Reset()
    {
        proceduralFace = GetComponent<ProceduralFace>();
    }

    private void Awake()
    {
        if (proceduralFace == null)
        {
            proceduralFace = GetComponent<ProceduralFace>();
        }
    }

    private void OnEnable()
    {
        GameEvent.GenerateNewNPCView += OnNewNPCGenerated;
    }

    private void OnDisable()
    {
        GameEvent.GenerateNewNPCView -= OnNewNPCGenerated;
    }

    // =========================================================
    // NEW NPC
    // =========================================================

    private void OnNewNPCGenerated()
    {
        ClearCache();

        _hasLocked = false;
        _lockedExpression = FaceExpression.Neutral;

        _pairIdx = -1;

        CurrentExpression = FaceExpression.Neutral;
    }

    // =========================================================
    // APPLY LOCKED EXPRESSION
    // =========================================================

    public void ApplyLockedExpression(
        bool random,
        FaceExpression fixedExpr)
    {
        if (!_hasLocked)
        {
            if (random)
            {
                _lockedExpression =
                    (FaceExpression)Random.Range(1, 4);
            }
            else
            {
                _lockedExpression = fixedExpr;
            }

            _hasLocked = true;
        }

        SetExpression(_lockedExpression);
    }

    // =========================================================
    // SET EXPRESSION
    // =========================================================

    public void SetExpression(FaceExpression expression)
    {
        if (proceduralFace == null)
            return;

        if (expression == FaceExpression.Neutral)
        {
            ResetExpression();
            return;
        }

        ExpressionSet set = GetSet(expression);

        if (set == null)
            return;

        // Jika wajah sudah berubah karena generate NPC baru,
        // cache lama dianggap tidak valid.
        if (_hasOriginal && !IsStillShowingApplied())
        {
            ClearCache();
        }

        // Simpan sprite asli sebelum expression diterapkan.
        if (!_hasOriginal)
        {
            CacheOriginal();
        }

        // =====================================================
        // PAIR INDEX
        // =====================================================

        if (_pairIdx < 0)
        {
            int eyeCount =
                set.eyes != null ? set.eyes.Count : 0;

            int mouthCount =
                set.mouth != null ? set.mouth.Count : 0;

            int pairCount =
                Mathf.Min(eyeCount, mouthCount);

            if (pairCount > 0)
            {
                _pairIdx = Random.Range(0, pairCount);
            }
            else
            {
                _pairIdx = 0;
            }
        }

        Sprite eye = null;
        Sprite mouth = null;

        if (set.eyes != null && set.eyes.Count > 0)
        {
            eye = set.eyes[_pairIdx % set.eyes.Count];
        }

        if (set.mouth != null && set.mouth.Count > 0)
        {
            mouth = set.mouth[_pairIdx % set.mouth.Count];
        }

        // =====================================================
        // APPLY EYES
        // =====================================================

        if (eye != null &&
            proceduralFace.eyesRenderer != null)
        {
            proceduralFace.eyesRenderer.sprite = eye;
            _appliedEyes = eye;
        }

        // =====================================================
        // APPLY MOUTH
        // =====================================================

        if (mouth != null &&
            proceduralFace.mouthRenderer != null)
        {
            proceduralFace.mouthRenderer.sprite = mouth;
            _appliedMouth = mouth;
        }

        CurrentExpression = expression;
    }

    // =========================================================
    // RESET EXPRESSION
    // =========================================================

    public void ResetExpression()
    {
        if (proceduralFace == null)
            return;

        if (_hasOriginal)
        {
            // Restore eyes
            if (_appliedEyes != null &&
                proceduralFace.eyesRenderer != null &&
                proceduralFace.eyesRenderer.sprite == _appliedEyes)
            {
                proceduralFace.eyesRenderer.sprite = _originalEyes;
            }

            // Restore mouth
            if (_appliedMouth != null &&
                proceduralFace.mouthRenderer != null &&
                proceduralFace.mouthRenderer.sprite == _appliedMouth)
            {
                proceduralFace.mouthRenderer.sprite = _originalMouth;
            }
        }

        // Hanya hapus cache sprite.
        //
        // Expression tetap terkunci.
        // Pair index tetap sama.
        ClearCache();

        CurrentExpression = FaceExpression.Neutral;
    }

    // =========================================================
    // CACHE ORIGINAL
    // =========================================================

    private void CacheOriginal()
    {
        if (proceduralFace == null)
            return;

        _originalEyes =
            proceduralFace.eyesRenderer != null
                ? proceduralFace.eyesRenderer.sprite
                : null;

        _originalMouth =
            proceduralFace.mouthRenderer != null
                ? proceduralFace.mouthRenderer.sprite
                : null;

        _hasOriginal = true;
    }

    // =========================================================
    // CLEAR CACHE
    // =========================================================

    private void ClearCache()
    {
        _hasOriginal = false;

        _originalEyes = null;
        _originalMouth = null;

        _appliedEyes = null;
        _appliedMouth = null;
    }

    // =========================================================
    // CHECK CURRENT SPRITE
    // =========================================================

    private bool IsStillShowingApplied()
    {
        bool eyesOk =
            _appliedEyes == null ||
            (
                proceduralFace.eyesRenderer != null &&
                proceduralFace.eyesRenderer.sprite == _appliedEyes
            );

        bool mouthOk =
            _appliedMouth == null ||
            (
                proceduralFace.mouthRenderer != null &&
                proceduralFace.mouthRenderer.sprite == _appliedMouth
            );

        return eyesOk && mouthOk;
    }

    // =========================================================
    // GET EXPRESSION SET
    // =========================================================

    private ExpressionSet GetSet(FaceExpression expression)
    {
        switch (expression)
        {
            case FaceExpression.Angry:
                return angry;

            case FaceExpression.Sad:
                return sad;

            case FaceExpression.Anxious:
                return anxious;

            default:
                return null;
        }
    }
}