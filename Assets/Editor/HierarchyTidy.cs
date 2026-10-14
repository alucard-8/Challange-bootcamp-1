#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Soulbreak.EditorTools
{
    public static class HierarchyTidy
    {
        private const string GroupName = "Canvas";
        private const string UndoLabel = "Soulbreak: group UI canvases";

        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/SampleScene.unity",
            "Assets/Scenes/MainMenu.unity",
        };

        [MenuItem("Soulbreak/Hierarchy/Tidy Canvas Groups")]
        public static void RunActiveScene()
        {
            var scene = SceneManager.GetActiveScene();
            int moved = Tidy(scene);
            if (moved > 0) EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[Soulbreak] Hierarchy tidy: grouped {moved} UI root(s) under \"{GroupName}\" in '{scene.name}'.");
        }

        [MenuItem("Soulbreak/Hierarchy/Tidy Canvas Groups", true)]
        private static bool RunActiveSceneValidate() => !Application.isPlaying;

        [MenuItem("Soulbreak/Hierarchy/Tidy All Scenes")]
        public static void RunAllScenes()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var report = new StringBuilder();
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                var scene = EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
                int moved = Tidy(scene);
                if (moved > 0) EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                report.AppendLine($"  {ScenePaths[i]}: {moved} UI root(s) grouped");
                if (i < ScenePaths.Length - 1) report.AppendLine("  (open the other scene and run 'Tidy Canvas Groups' to finish)");
            }

            Debug.Log("[Soulbreak] Hierarchy tidy (all scenes):\n" + report);
        }

        [MenuItem("Soulbreak/Hierarchy/Tidy All Scenes", true)]
        private static bool RunAllScenesValidate() => !Application.isPlaying;

        public static void RunBatch()
        {
            for (int i = 0; i < ScenePaths.Length; i++)
            {
                var scene = EditorSceneManager.OpenScene(ScenePaths[i], OpenSceneMode.Single);
                int moved = Tidy(scene);
                if (moved > 0) EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[Soulbreak] Hierarchy tidy: grouped {moved} UI root(s) under \"{GroupName}\" in '{ScenePaths[i]}'.");
            }
            Debug.Log("[Soulbreak] Hierarchy tidy complete.");
        }

        private static int Tidy(Scene scene)
        {
            var group = FindExistingGroup(scene);
            var toMove = new List<Transform>();

            foreach (var root in scene.GetRootGameObjects())
            {
                Collect(root.transform, group, toMove);
            }

            if (toMove.Count == 0) return 0;

            if (group == null)
            {
                var groupGo = new GameObject(GroupName);
                SceneManager.MoveGameObjectToScene(groupGo, scene);
                Undo.RegisterCreatedObjectUndo(groupGo, UndoLabel);
                group = groupGo.transform;
            }

            Undo.RegisterCompleteObjectUndo(group, UndoLabel);
            for (int i = 0; i < toMove.Count; i++)
            {
                Undo.SetTransformParent(toMove[i], group, true, UndoLabel);
                Debug.Log($"[Soulbreak]   moved '{toMove[i].name}' from '{(toMove[i].parent != null ? toMove[i].parent.name : "<root>")}' into '{GroupName}'.");
            }

            return toMove.Count;
        }

        private static Transform FindExistingGroup(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name != GroupName) continue;
                if (PrefabUtility.IsPartOfPrefabInstance(root)) continue;
                if (root.GetComponent<Canvas>() != null) continue;
                return root.transform;
            }
            return null;
        }

        private static void Collect(Transform t, Transform group, List<Transform> toMove)
        {
            if (t == null) return;

            if (group != null && t == group) return;

            if (IsUiPrefabRoot(t))
            {
                if (group == null || t.parent != group) toMove.Add(t);
                return;
            }

            for (int i = t.childCount - 1; i >= 0; i--)
            {
                Collect(t.GetChild(i), group, toMove);
            }
        }

        private static bool IsUiPrefabRoot(Transform t)
        {
            var go = t.gameObject;

            if (PrefabUtility.IsPartOfPrefabInstance(go) == false) return false;
            if (PrefabUtility.GetOutermostPrefabInstanceRoot(go) != go) return false;

            if (go.GetComponent<Canvas>() != null) return true;
            if (go.GetComponentInChildren<Canvas>(true) != null) return true;

            return false;
        }
    }
}
#endif
