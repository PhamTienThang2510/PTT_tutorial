using UnityEngine;

namespace PTT.Tutorial
{
    public enum TutorialIDKind
    {
        Button,
        Sequence
    }

    /// <summary>
    /// Put on a string field to pick a tutorial ID from the project's TutorialIDDatabase
    /// as a dropdown in the Inspector. An empty string means "None".
    /// </summary>
    public class TutorialIDAttribute : PropertyAttribute
    {
        public readonly TutorialIDKind Kind;

        public TutorialIDAttribute(TutorialIDKind kind)
        {
            Kind = kind;
        }
    }
}
