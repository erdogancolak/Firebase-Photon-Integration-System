using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeManager : MonoBehaviour
{
    public RectTransform panelContainer;
    public int panelCount = 3;
    public float swipeThreshold = 200f;
    public float lerpSpeed = 10f;

    private int currentPanelIndex = 1;
    private Vector2 targetPosition;

    private bool isSwiping = false;
    private Vector2 startTouchPosition;

    private float panelWidth = 1920f;

    void Start()
    {
        UpdateTargetPosition();
        panelContainer.anchoredPosition = targetPosition;
    }

    void Update()
    {
        if (Pointer.current == null)
            return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            isSwiping = true;
            startTouchPosition = Pointer.current.position.ReadValue();
        }
        else if (Pointer.current.press.wasReleasedThisFrame && isSwiping)
        {
            Vector2 swipeDelta = Pointer.current.position.ReadValue() - startTouchPosition;
            HandleSwipe(swipeDelta);
            isSwiping = false;
        }

        panelContainer.anchoredPosition = Vector2.Lerp(panelContainer.anchoredPosition, targetPosition, lerpSpeed * Time.deltaTime);
    }

    void HandleSwipe(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > swipeThreshold)
        {
            if (delta.x > 0)
                GoToPreviousPanel();
            else
                GoToNextPanel();
        }
    }

    public void GoToPreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            UpdateTargetPosition();
        }
    }

    public void GoToNextPanel()
    {
        if (currentPanelIndex < panelCount - 1)
        {
            currentPanelIndex++;
            UpdateTargetPosition();
        }
    }

    private void UpdateTargetPosition()
    {
        float targetX = -currentPanelIndex * panelWidth;
        targetPosition = new Vector2(targetX, panelContainer.anchoredPosition.y);
    }
}
