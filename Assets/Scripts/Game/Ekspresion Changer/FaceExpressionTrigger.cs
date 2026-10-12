using UnityEngine;

public class FaceExpressionTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private FaceExpressionChanger expressionChanger;

    [SerializeField] private InspectableObject inspectableObject;

    [Header("Ekspresi")]
    [SerializeField] private bool randomExpression = true;

    [SerializeField] private FaceExpression fixedExpression =
        FaceExpression.Angry;

    private bool lastInspectionState = false;

    private void Reset()
    {
        expressionChanger =
            GetComponentInParent<FaceExpressionChanger>();

        inspectableObject =
            GetComponentInParent<InspectableObject>();
    }

    private void Awake()
    {
        if (expressionChanger == null)
        {
            expressionChanger =
                GetComponentInParent<FaceExpressionChanger>();
        }

        if (inspectableObject == null)
        {
            inspectableObject =
                GetComponentInParent<InspectableObject>();
        }
    }

    private void Update()
    {
        if (inspectableObject == null ||
            expressionChanger == null)
        {
            return;
        }

        bool currentInspectionState =
            inspectableObject.GetIsInspect();

        if (currentInspectionState == lastInspectionState)
            return;

        lastInspectionState =
            currentInspectionState;

        if (currentInspectionState)
        {
            Debug.Log(
                "[FaceExpressionTrigger] NPC mulai di-inspect."
            );

            expressionChanger.ApplyLockedExpression(
                randomExpression,
                fixedExpression
            );
        }
        else
        {
            Debug.Log(
                "[FaceExpressionTrigger] NPC keluar dari inspect."
            );

            expressionChanger.ResetExpression();
        }
    }

    private void OnEnable()
    {
        GameEvent.GenerateNewNPCView += OnNewNPC;
    }

    private void OnDisable()
    {
        GameEvent.GenerateNewNPCView -= OnNewNPC;
    }

    private void OnNewNPC()
    {
        lastInspectionState = false;

        if (expressionChanger != null)
        {
            expressionChanger.ResetExpression();
        }
    }

    public void Trigger(FaceExpression expression)
    {
        if (expressionChanger == null)
            return;

        expressionChanger.SetExpression(expression);
    }

    public void TriggerNeutral()
    {
        if (expressionChanger == null)
            return;

        expressionChanger.ResetExpression();
    }
}