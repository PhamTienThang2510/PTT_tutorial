using System.Collections.Generic;
using UnityEngine;

namespace PTT.Tutorial
{
    /// <summary>
    /// The list of button and sequence IDs for one project.
    /// Create one per project: Assets > Create > PTT > Tutorial ID Database.
    /// Only the Editor reads it (for dropdowns and constant generation); nothing needs it at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialIDDatabase", menuName = "PTT/Tutorial ID Database")]
    public class TutorialIDDatabase : ScriptableObject
    {
        public List<string> buttonIDs = new List<string>();
        public List<string> sequenceIDs = new List<string>();

        [Tooltip("Where 'Generate Constants' writes the C# file with ID constants.")]
        public string constantsFilePath = "Assets/TutorialIDs.g.cs";

        [Tooltip("Namespace of the generated constants. Leave empty for the global namespace.")]
        public string constantsNamespace = "";

        public List<string> GetList(TutorialIDKind kind) =>
            kind == TutorialIDKind.Button ? buttonIDs : sequenceIDs;
    }
}
