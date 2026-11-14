using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PTT.Tutorial.Editor
{
    /// <summary>Shows [TutorialID] string fields as a dropdown of the IDs in TutorialIDDatabase.</summary>
    [CustomPropertyDrawer(typeof(TutorialIDAttribute))]
    public class TutorialIDDrawer : PropertyDrawer
    {
        const float ButtonWidth = 22f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "[TutorialID] only works on string fields");
                return;
            }

            var kind = ((TutorialIDAttribute)attribute).Kind;
            var db = TutorialIDDatabaseLocator.Find();

            EditorGUI.BeginProperty(position, label, property);
            Rect field = EditorGUI.PrefixLabel(position, label);

            if (db == null)
            {
                // No database yet: plain text field + button to create one.
                Rect text = new Rect(field.x, field.y, field.width - 70, field.height);
                Rect create = new Rect(text.xMax + 2, field.y, 68, field.height);
                property.stringValue = EditorGUI.TextField(text, property.stringValue);
                if (GUI.Button(create, new GUIContent("Create DB", "Create a TutorialIDDatabase asset")))
                    Selection.activeObject = TutorialIDDatabaseLocator.FindOrCreate();
                EditorGUI.EndProperty();
                return;
            }

            List<string> ids = db.GetList(kind);
            string current = property.stringValue;
            bool missing = !string.IsNullOrEmpty(current) && !ids.Contains(current);

            var options = new List<GUIContent> { new GUIContent("None") };
            foreach (string id in ids) options.Add(new GUIContent(id));
            int selected = string.IsNullOrEmpty(current) ? 0 : ids.IndexOf(current) + 1;
            if (missing)
            {
                options.Add(new GUIContent($"⚠ {current} (not in database)"));
                selected = options.Count - 1;
            }

            Rect popup = new Rect(field.x, field.y, field.width - ButtonWidth - 2, field.height);
            Rect button = new Rect(popup.xMax + 2, field.y, ButtonWidth, field.height);

            Color old = GUI.color;
            if (missing) GUI.color = new Color(1f, 0.6f, 0.4f);
            EditorGUI.BeginChangeCheck();
            int picked = EditorGUI.Popup(popup, selected, options.ToArray());
            if (EditorGUI.EndChangeCheck() && !(missing && picked == options.Count - 1))
                property.stringValue = picked == 0 ? "" : ids[picked - 1];
            GUI.color = old;

            if (missing)
            {
                if (GUI.Button(button, new GUIContent("+", $"Add '{current}' to the database")))
                    TutorialIDDatabaseLocator.EnsureID(kind, current);
            }
            else if (GUI.Button(button, new GUIContent("…", "Open the TutorialIDDatabase")))
            {
                Selection.activeObject = db;
                EditorGUIUtility.PingObject(db);
            }

            EditorGUI.EndProperty();
        }
    }
}
