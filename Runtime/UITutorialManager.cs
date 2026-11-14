using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PTT.Tutorial
{
    /// <summary>
    /// Runs tutorial sequences: dims the screen with an overlay, lifts the focused UIButton
    /// above it and shows a tooltip next to it. Steps advance when the focused button is
    /// clicked, or when the overlay is tapped for steps without a button.
    /// The overlay must be a full-screen child of a Screen Space - Overlay canvas.
    /// </summary>
    public class UITutorialManager : MonoBehaviour
    {
        public static UITutorialManager Instance { get; private set; }

        /// <summary>Raised with the ID of the button to focus; null or empty clears focus.</summary>
        public static event Action<string> OnButtonFocus;

        [Tooltip("Full-screen panel that dims the screen. Keep it the last child of its canvas.")]
        public GameObject overlayPanel;
        public GameObject tooltipPrefab;
        public List<TutorialSequence> TutorialSequences = new List<TutorialSequence>();

        [Tooltip("Raised with the sequence ID when a sequence finishes.")]
        public UnityEvent<string> OnSequenceCompleted = new UnityEvent<string>();

        [Header("Debug (right-click the component header to run)")]
        [TutorialID(TutorialIDKind.Sequence)] public string testSequenceID;
        [TutorialID(TutorialIDKind.Button)] public string testButtonID;

        public int currentStepIndex;

        public bool IsTutorialActive => isTutorialActive;

        bool isTutorialActive;
        string currentlyFocusedButtonID;
        Action currentlyFocusedButtonCallback;
        GameObject currentTooltip;
        TooltipUI tooltip;
        Button overlayButton;
        TutorialSequence currentSequence;
        readonly Dictionary<string, UIButton> buttonDictionary = new Dictionary<string, UIButton>();
        readonly Dictionary<Button, UnityAction> tutorialClickActions = new Dictionary<Button, UnityAction>();

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("More than one UITutorialManager in the scene; UIButton will talk to the last one.", this);
            Instance = this;
            CacheAllButtons();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            ResetButtonActions();
        }

        void CacheAllButtons()
        {
            buttonDictionary.Clear();
            foreach (UIButton button in FindObjectsByType<UIButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (string.IsNullOrEmpty(button.buttonID)) continue;
                if (buttonDictionary.ContainsKey(button.buttonID))
                {
                    Debug.LogWarning($"Duplicate tutorial button ID '{button.buttonID}' on '{button.name}'. Only the first one is used.", button);
                    continue;
                }
                buttonDictionary[button.buttonID] = button;
            }
            ResetButtonActions();
        }

        /// <summary>Call after spawning UI at runtime so new UIButtons can be found.</summary>
        public void RefreshButtons() => CacheAllButtons();

        [ContextMenu("Debug/Start Test Sequence")]
        void DebugTutorialSequence() => StartTutorial(testSequenceID);

        [ContextMenu("Debug/Focus Test Button")]
        void DebugFocusOnButton() => FocusOnButton(testButtonID, "This is a debug tooltip!");

        // ---------------- Sequences ----------------

        public bool HasSequence(string sequenceID) =>
            TutorialSequences.Exists(seq => seq.sequenceName == sequenceID);

        public void StartTutorial(string sequenceID)
        {
            TutorialSequence sequence = TutorialSequences.Find(seq => seq.sequenceName == sequenceID);
            if (sequence == null || sequence.steps.Count == 0)
            {
                Debug.LogWarning($"Tutorial sequence '{sequenceID}' is missing or empty.", this);
                return;
            }

            if (isTutorialActive) ResetButtonActions();

            isTutorialActive = true;
            currentSequence = sequence;
            currentStepIndex = 0;
            FocusOnNextButton();
        }

        public void FocusOnNextButton()
        {
            if (!isTutorialActive || currentStepIndex >= currentSequence.steps.Count)
            {
                EndTutorial();
                return;
            }

            UITutorialStep step = currentSequence.steps[currentStepIndex];
            if (!ShowOverlay())
            {
                EndTutorial();
                return;
            }
            overlayButton.interactable = false;

            if (!string.IsNullOrEmpty(step.focusButtonID))
            {
                UIButton targetButton = FindButton(step.focusButtonID);
                if (targetButton == null)
                {
                    Debug.LogError($"Tutorial button '{step.focusButtonID}' not found. Skipping step {currentStepIndex}.", this);
                    currentStepIndex++;
                    FocusOnNextButton();
                    return;
                }

                ActivateParents(targetButton.transform);
                OnButtonFocus?.Invoke(step.focusButtonID);

                Button buttonComponent = targetButton.GetComponent<Button>();
                RemoveTutorialAction(buttonComponent);
                UnityAction clickAction = () => OnStepCompleted(step);
                buttonComponent.onClick.AddListener(clickAction);
                tutorialClickActions[buttonComponent] = clickAction;
            }
            else
            {
                // No button: tap the overlay to continue.
                OnButtonFocus?.Invoke(null);
                overlayButton.interactable = true;
                overlayButton.onClick.RemoveAllListeners();
                overlayButton.onClick.AddListener(() => OnStepCompleted(step));
            }

            ShowTooltip(step.focusButtonID, step.message);
        }

        void OnStepCompleted(UITutorialStep step)
        {
            if (!isTutorialActive) return;
            step.OnStepCompleted?.Invoke();
            currentStepIndex++;
            FocusOnNextButton();
        }

        public void EndTutorial()
        {
            bool wasActive = isTutorialActive;
            string finished = currentSequence?.sequenceName;

            isTutorialActive = false;
            currentSequence = null;
            OnButtonFocus?.Invoke(null);
            HideTooltip();
            if (overlayPanel != null) overlayPanel.SetActive(false);
            ResetButtonActions();

            if (wasActive) OnSequenceCompleted?.Invoke(finished);
        }

        // ---------------- Single focus (outside sequences) ----------------

        /// <summary>Focus one button; the callback runs when the player clicks it.</summary>
        public void FocusOnButton(string buttonID, string tooltipText = "", Action onFocusedButtonClicked = null)
        {
            currentlyFocusedButtonID = buttonID;
            currentlyFocusedButtonCallback = onFocusedButtonClicked;

            if (!ShowOverlay()) return;
            overlayButton.interactable = false;

            UIButton target = FindButton(buttonID);
            if (target != null) ActivateParents(target.transform);
            OnButtonFocus?.Invoke(buttonID);

            if (!string.IsNullOrEmpty(tooltipText)) ShowTooltip(buttonID, tooltipText);
            else HideTooltip();
        }

        public void StopFocus()
        {
            if (overlayPanel != null) overlayPanel.SetActive(false);
            OnButtonFocus?.Invoke(null);
            HideTooltip();
            currentlyFocusedButtonID = null;
            currentlyFocusedButtonCallback = null;
        }

        /// <summary>Called by UIButton when a focused button is clicked.</summary>
        public void OnFocusedButtonClicked(string buttonID)
        {
            if (isTutorialActive || string.IsNullOrEmpty(buttonID) || buttonID != currentlyFocusedButtonID) return;
            Action callback = currentlyFocusedButtonCallback;
            StopFocus();
            callback?.Invoke();
        }

        // ---------------- Overlay & tooltip ----------------

        bool ShowOverlay()
        {
            if (overlayPanel == null)
            {
                Debug.LogError("UITutorialManager: overlayPanel is not assigned.", this);
                return false;
            }
            overlayPanel.SetActive(true);
            overlayPanel.transform.SetAsLastSibling();
            if (overlayButton == null && !overlayPanel.TryGetComponent(out overlayButton))
            {
                overlayButton = overlayPanel.AddComponent<Button>();
                overlayButton.transition = Selectable.Transition.None;
            }
            return true;
        }

        void HideTooltip()
        {
            if (tooltip != null) tooltip.gameObject.SetActive(false);
        }

        void ShowTooltip(string buttonID, string tooltipText)
        {
            if (tooltipPrefab == null || overlayPanel == null) return;

            if (currentTooltip == null)
            {
                currentTooltip = Instantiate(tooltipPrefab, overlayPanel.transform);
                tooltip = currentTooltip.GetComponent<TooltipUI>();
            }

            tooltip.SetText(tooltipText);
            tooltip.gameObject.SetActive(true);

            UIButton targetButton = string.IsNullOrEmpty(buttonID) ? null : FindButton(buttonID);
            if (targetButton == null)
            {
                PositionAtCenter();
                return;
            }

            // The button may have just been activated or be inside a LayoutGroup that has
            // not been laid out yet this frame: rebuild so its position is correct.
            RebuildLayout(targetButton.transform as RectTransform);
            RebuildLayout(tooltip.transform as RectTransform);
            PositionNearButton(targetButton.GetComponent<RectTransform>());
        }

        static void RebuildLayout(RectTransform rect)
        {
            if (rect == null) return;
            Canvas.ForceUpdateCanvases();
            for (Transform t = rect; t != null; t = t.parent)
            {
                if (t.TryGetComponent(out LayoutGroup _) || t.TryGetComponent(out ContentSizeFitter _))
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)t);
            }
        }

        void PositionAtCenter()
        {
            RectTransform toolTipRect = tooltip.GetComponent<RectTransform>();
            toolTipRect.anchorMin = toolTipRect.anchorMax = toolTipRect.pivot = new Vector2(0.5f, 0.5f);
            toolTipRect.anchoredPosition = Vector2.zero;
            if (tooltip.arrowImage != null) tooltip.arrowImage.gameObject.SetActive(false);
        }

        public void PositionNearButton(RectTransform targetButtonRect)
        {
            RectTransform toolTipRect = tooltip.GetComponent<RectTransform>();
            RectTransform canvasRect = overlayPanel.GetComponent<RectTransform>();
            toolTipRect.anchorMin = toolTipRect.anchorMax = toolTipRect.pivot = new Vector2(0.5f, 0.5f);

            // Use the button's visual centre (not its pivot) so any pivot works.
            Vector3 worldCenter = targetButtonRect.TransformPoint(targetButtonRect.rect.center);
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out Vector2 localPoint))
                return;

            // Button height in overlay space (handles scaled parents).
            float buttonHeight = targetButtonRect.rect.height * targetButtonRect.lossyScale.y / canvasRect.lossyScale.y;
            float tooltipHeight = toolTipRect.rect.height;
            float tooltipWidth = toolTipRect.rect.width;
            Vector2 canvasCenter = canvasRect.rect.center;
            localPoint -= canvasCenter;

            float spaceAbove = canvasRect.rect.height / 2 - (localPoint.y + buttonHeight / 2);
            float spaceBelow = (localPoint.y - buttonHeight / 2) + canvasRect.rect.height / 2;
            bool placeAbove = spaceAbove >= tooltipHeight || spaceAbove >= spaceBelow;
            const float verticalOffset = 10f;

            float desiredY = placeAbove
                ? localPoint.y + buttonHeight / 2 + tooltipHeight / 2 + verticalOffset
                : localPoint.y - buttonHeight / 2 - tooltipHeight / 2 - verticalOffset;

            float halfW = tooltipWidth / 2, halfH = tooltipHeight / 2;
            float desiredX = Mathf.Clamp(localPoint.x, -canvasRect.rect.width / 2 + halfW, canvasRect.rect.width / 2 - halfW);
            desiredY = Mathf.Clamp(desiredY, -canvasRect.rect.height / 2 + halfH, canvasRect.rect.height / 2 - halfH);

            toolTipRect.anchoredPosition = new Vector2(desiredX, desiredY);

            if (tooltip.arrowImage == null) return;

            RectTransform arrowRect = tooltip.arrowImage;
            arrowRect.gameObject.SetActive(true);
            float maxOffset = halfW - arrowRect.rect.width / 2;
            float deltaX = Mathf.Clamp(localPoint.x - desiredX, -maxOffset, maxOffset);

            if (placeAbove)
            {
                arrowRect.localEulerAngles = Vector3.zero; // points down
                arrowRect.anchoredPosition = new Vector2(deltaX, -halfH + arrowRect.rect.height / 2);
            }
            else
            {
                arrowRect.localEulerAngles = new Vector3(0, 0, 180); // points up
                arrowRect.anchoredPosition = new Vector2(deltaX, halfH - arrowRect.rect.height / 2);
            }
        }

        // ---------------- Helpers ----------------

        static void ActivateParents(Transform target)
        {
            if (target.gameObject.activeInHierarchy) return;
            for (Transform t = target; t != null; t = t.parent)
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
        }

        UIButton FindButton(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (buttonDictionary.TryGetValue(id, out UIButton button) && button != null) return button;

            // Not cached (spawned later?): search again, including inactive objects.
            foreach (UIButton btn in FindObjectsByType<UIButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (btn.buttonID == id)
                {
                    buttonDictionary[id] = btn;
                    return btn;
                }
            }
            return null;
        }

        void RemoveTutorialAction(Button button)
        {
            if (tutorialClickActions.TryGetValue(button, out UnityAction existing))
            {
                if (button != null) button.onClick.RemoveListener(existing);
                tutorialClickActions.Remove(button);
            }
        }

        void ResetButtonActions()
        {
            foreach (var pair in tutorialClickActions)
                if (pair.Key != null) pair.Key.onClick.RemoveListener(pair.Value);
            tutorialClickActions.Clear();
        }
    }
}
