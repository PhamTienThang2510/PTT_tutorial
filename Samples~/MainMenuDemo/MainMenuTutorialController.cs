using System.Collections;
using PTT.Tutorial;
using UnityEngine;

/// <summary>
/// Starts the main-menu tutorial when the scene opens.
/// Built by: PTT > Build Main Menu Tutorial Demo.
/// </summary>
public class MainMenuTutorialController : MonoBehaviour
{
    public UITutorialManager tutorialManager;
    [TutorialID(TutorialIDKind.Sequence)] public string sequence = "seq_main_menu";

    [Tooltip("On for the demo: play every time. Off: play only until the player finishes it once (saved in PlayerPrefs).")]
    public bool playEveryTime = true;

    string PrefsKey => "tut_done_" + sequence;

    void OnEnable() => tutorialManager.OnSequenceCompleted.AddListener(OnCompleted);
    void OnDisable() => tutorialManager.OnSequenceCompleted.RemoveListener(OnCompleted);

    IEnumerator Start()
    {
        // Wait one frame so the UI is laid out before the tooltip is positioned.
        yield return new WaitForEndOfFrame();

        if (playEveryTime || PlayerPrefs.GetInt(PrefsKey) != 1)
            tutorialManager.StartTutorial(sequence);
    }

    void OnCompleted(string finishedSequence)
    {
        if (finishedSequence != sequence) return;
        PlayerPrefs.SetInt(PrefsKey, 1);
        PlayerPrefs.Save();
    }

    public void ReplayTutorial() => tutorialManager.StartTutorial(sequence);

    public void OnMenuButton(string buttonName) => Debug.Log($"[Menu] {buttonName} clicked");
}
