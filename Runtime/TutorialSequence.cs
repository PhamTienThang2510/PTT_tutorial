using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PTT.Tutorial
{
    [Serializable]
    public class UITutorialStep
    {
        [Tooltip("Button to focus. None = tooltip in the centre, tap anywhere to continue.")]
        [TutorialID(TutorialIDKind.Button)] public string focusButtonID;
        [TextArea] public string message;
        public UnityEvent OnStepCompleted;
    }

    [Serializable]
    public class TutorialSequence
    {
        [TutorialID(TutorialIDKind.Sequence)] public string sequenceName;
        public List<UITutorialStep> steps = new List<UITutorialStep>();
    }
}
