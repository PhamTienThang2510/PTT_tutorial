using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PTT.Tutorial
{
    /// <summary>
    /// Marks a button the tutorial can focus. While focused it is drawn above the overlay
    /// (a Canvas with override sorting), then restored to how it was.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        [TutorialID(TutorialIDKind.Button)] public string buttonID;

        [Tooltip("Sorting order used while focused. Must be above the overlay's canvas.")]
        public int focusedSortingOrder = 10;

        Canvas btnCanvas;
        GraphicRaycaster graphicRaycaster;
        bool addedCanvas, addedRaycaster;
        bool prevOverrideSorting;
        int prevSortingOrder;
        bool isFocused;

        public bool IsFocused => isFocused;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(Click);
        }

        void OnEnable()
        {
            UITutorialManager.OnButtonFocus += TriggerButtonFocus;
        }

        void OnDisable()
        {
            UITutorialManager.OnButtonFocus -= TriggerButtonFocus;
            if (isFocused) RemoveFocus(immediate: true);
        }

        public void TriggerButtonFocus(string id)
        {
            bool focusMe = !string.IsNullOrEmpty(buttonID) && id == buttonID;
            if (focusMe && !isFocused) ApplyFocus();
            else if (!focusMe && isFocused) RemoveFocus(immediate: false);
        }

        void ApplyFocus()
        {
            if (btnCanvas != null)
            {
                // Refocused while our added Canvas was waiting to be destroyed: keep it.
                StopAllCoroutines();
            }
            else
            {
                addedCanvas = !TryGetComponent(out btnCanvas);
                if (addedCanvas) btnCanvas = gameObject.AddComponent<Canvas>();
                prevOverrideSorting = btnCanvas.overrideSorting;
                prevSortingOrder = btnCanvas.sortingOrder;
            }

            addedRaycaster = !TryGetComponent(out graphicRaycaster);
            if (addedRaycaster) graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();

            btnCanvas.overrideSorting = true;
            btnCanvas.sortingOrder = focusedSortingOrder;
            isFocused = true;
        }

        void RemoveFocus(bool immediate)
        {
            isFocused = false;

            // GraphicRaycaster must go before the Canvas it depends on.
            if (addedRaycaster && graphicRaycaster != null) Destroy(graphicRaycaster);
            graphicRaycaster = null;

            if (btnCanvas == null) return;

            if (!addedCanvas)
            {
                // The button had its own Canvas: put its settings back.
                btnCanvas.overrideSorting = prevOverrideSorting;
                btnCanvas.sortingOrder = prevSortingOrder;
                btnCanvas = null;
            }
            else if (immediate || !gameObject.activeInHierarchy)
            {
                Destroy(btnCanvas);
                btnCanvas = null;
            }
            else
            {
                StartCoroutine(DestroyCanvasAfterFrame());
            }
        }

        IEnumerator DestroyCanvasAfterFrame()
        {
            yield return null; // wait until the GraphicRaycaster is gone
            if (btnCanvas != null && !isFocused)
            {
                Destroy(btnCanvas);
                btnCanvas = null;
            }
        }

        void Click()
        {
            if (!isFocused) return;
            var manager = UITutorialManager.Instance;
            if (manager != null) manager.OnFocusedButtonClicked(buttonID);
        }
    }
}
