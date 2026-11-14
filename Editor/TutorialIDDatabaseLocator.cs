using UnityEditor;
using UnityEngine;

namespace PTT.Tutorial.Editor
{
    /// <summary>Finds (and caches) the project's TutorialIDDatabase asset.</summary>
    public static class TutorialIDDatabaseLocator
    {
        const string DefaultPath = "Assets/TutorialIDDatabase.asset";
        static TutorialIDDatabase cached;
        static bool searched;

        [InitializeOnLoadMethod]
        static void Init()
        {
            EditorApplication.projectChanged += () => { searched = false; cached = null; };
        }

        public static TutorialIDDatabase Find()
        {
            if (cached != null) return cached;
            if (searched) return null;
            searched = true;

            string[] guids = AssetDatabase.FindAssets("t:" + nameof(TutorialIDDatabase));
            if (guids.Length == 0) return null;
            if (guids.Length > 1)
                Debug.LogWarning($"Found {guids.Length} TutorialIDDatabase assets; using {AssetDatabase.GUIDToAssetPath(guids[0])}. Keep only one per project.");

            cached = AssetDatabase.LoadAssetAtPath<TutorialIDDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return cached;
        }

        public static TutorialIDDatabase FindOrCreate()
        {
            var db = Find();
            if (db != null) return db;

            db = ScriptableObject.CreateInstance<TutorialIDDatabase>();
            AssetDatabase.CreateAsset(db, AssetDatabase.GenerateUniqueAssetPath(DefaultPath));
            AssetDatabase.SaveAssets();
            searched = false;
            cached = db;
            Debug.Log($"Created {AssetDatabase.GetAssetPath(db)}", db);
            return db;
        }

        /// <summary>Adds the ID to the database if it is not there yet.</summary>
        public static void EnsureID(TutorialIDKind kind, string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            var db = FindOrCreate();
            var list = db.GetList(kind);
            if (list.Contains(id)) return;
            Undo.RecordObject(db, "Add Tutorial ID");
            list.Add(id);
            EditorUtility.SetDirty(db);
        }
    }
}
