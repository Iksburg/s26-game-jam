using UnityEditor;
using UnityEngine;

namespace CatWorld.Cats.Editor
{
    /// <summary>
    /// Создаёт префаб какашки и подключает CatPoopController к префабу кота.
    /// Спрайт и размер какашки настраиваются на префабе вручную.
    /// </summary>
    public static class PoopBuilder
    {
        private const string CatPrefabPath = "Assets/Prefabs/Cat.prefab";
        private const string PoopPrefabPath = "Assets/Prefabs/Poop.prefab";

        [MenuItem("Tools/CatWorld/Setup Poop")]
        public static void Setup()
        {
            var poopPrefab = BuildPoopPrefab();
            AttachToCat(poopPrefab);
            AssetDatabase.SaveAssets();
            Debug.Log("[PoopBuilder] Готово: префаб какашки создан, CatPoopController подключён к коту.");
        }

        private static Poop BuildPoopPrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Poop>(PoopPrefabPath);
            if (existing != null)
                return existing; // не перетираем назначенный вручную спрайт/размер

            EnsureFolder("Assets/Prefabs");

            var temp = new GameObject("Poop");
            temp.AddComponent<SpriteRenderer>(); // спрайт назначит дизайнер
            var collider = temp.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f; // клик-область; масштабируется размером какашки
            collider.isTrigger = true;
            temp.AddComponent<Poop>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, PoopPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab.GetComponent<Poop>();
        }

        private static void AttachToCat(Poop poopPrefab)
        {
            var catRoot = PrefabUtility.LoadPrefabContents(CatPrefabPath);
            try
            {
                var controller = catRoot.GetComponent<CatPoopController>();
                if (controller == null)
                    controller = catRoot.AddComponent<CatPoopController>();

                var so = new SerializedObject(controller);
                so.FindProperty("_poopPrefab").objectReferenceValue = poopPrefab;
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(catRoot, CatPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(catRoot);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string leaf = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
