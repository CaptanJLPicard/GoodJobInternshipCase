#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GoodJobInternshipCase.Config;
using GoodJobInternshipCase.Rendering;
using GoodJobInternshipCase.Core;

namespace GoodJobInternshipCase.Editor
{
    /// <summary>
    /// Editor utility for quick game setup.
    /// Creates all necessary prefabs and assets.
    /// </summary>
    public class GoodJobInternshipCaseSetup : EditorWindow
    {
        private static readonly string[] ColorNames = { "Blue", "Green", "Pink", "Purple", "Red", "Yellow" };

        [MenuItem("Tools/GoodJobInternshipCase/Setup Game")]
        public static void ShowWindow()
        {
            GetWindow<GoodJobInternshipCaseSetup>("GoodJobInternshipCase Setup");
        }

        private void OnGUI()
        {
            GUILayout.Label("GoodJobInternshipCase Quick Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This will create:\n" +
                "- Block Prefab (in Assets/Prefabs/)\n" +
                "- GameConfig asset (in Assets/ScriptableObjects/)\n" +
                "- Sprite assignments will be auto-configured",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Block Prefab", GUILayout.Height(30)))
            {
                CreateBlockPrefab();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Create GameConfig", GUILayout.Height(30)))
            {
                CreateGameConfig();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Setup Complete Scene", GUILayout.Height(40)))
            {
                SetupScene();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "After setup, assign:\n" +
                "1. Block Prefab to BlockPool\n" +
                "2. GameConfig to GameManager\n" +
                "3. References in GameManager inspector",
                MessageType.Warning);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Create Block Prefab")]
        public static void CreateBlockPrefab()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Create block GameObject
            GameObject block = new GameObject("Block");

            // Add SpriteRenderer
            SpriteRenderer sr = block.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            // Try to load a default sprite
            Sprite defaultSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Blue_Default.png");
            if (defaultSprite != null)
            {
                sr.sprite = defaultSprite;
            }

            // Add BlockVisual component
            block.AddComponent<BlockVisual>();

            // Save as prefab
            string prefabPath = "Assets/Prefabs/Block.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(block, prefabPath);

            // Cleanup
            DestroyImmediate(block);

            Debug.Log($"Block prefab created at: {prefabPath}");
            EditorGUIUtility.PingObject(prefab);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Create GameConfig")]
        public static void CreateGameConfig()
        {
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            // Create config
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();

            // Set default values
            config.Rows = 8;
            config.Columns = 8;
            config.ColorCount = 4;
            config.ThresholdPreset = ThresholdPreset.Easy;

            // Setup sprites
            config.ColorSprites = new BlockSpriteSet[6];

            for (int i = 0; i < ColorNames.Length; i++)
            {
                string colorName = ColorNames[i];

                config.ColorSprites[i] = new BlockSpriteSet
                {
                    ColorName = colorName,
                    DefaultSprite = LoadSprite($"{colorName}_Default"),
                    SpriteA = LoadSprite($"{colorName}_A"),
                    SpriteB = LoadSprite($"{colorName}_B"),
                    SpriteC = LoadSprite($"{colorName}_C")
                };
            }

            // Save asset
            string assetPath = "Assets/ScriptableObjects/GameConfig.asset";
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"GameConfig created at: {assetPath}");
            EditorGUIUtility.PingObject(config);
        }

        private static Sprite LoadSprite(string name)
        {
            string path = $"Assets/Sprites/{name}.png";
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [MenuItem("Tools/GoodJobInternshipCase/Setup Scene")]
        public static void SetupScene()
        {
            // Find or create necessary objects

            // 1. Find GameManager or create structure
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                // Find Scripts container or create one
                GameObject scriptsObj = GameObject.Find("Scripts");
                if (scriptsObj == null)
                {
                    scriptsObj = new GameObject("Scripts");
                }

                // Find or create GameManager object
                Transform gmTransform = scriptsObj.transform.Find("GameManager");
                GameObject gmObj;
                if (gmTransform == null)
                {
                    gmObj = new GameObject("GameManager");
                    gmObj.transform.SetParent(scriptsObj.transform);
                }
                else
                {
                    gmObj = gmTransform.gameObject;
                }

                gameManager = gmObj.GetComponent<GameManager>();
                if (gameManager == null)
                {
                    gameManager = gmObj.AddComponent<GameManager>();
                }
            }

            // 2. Create Board object with renderer and pool
            GameObject boardObj = GameObject.Find("Board");
            if (boardObj == null)
            {
                boardObj = new GameObject("Board");
            }

            BoardRenderer boardRenderer = boardObj.GetComponent<BoardRenderer>();
            if (boardRenderer == null)
            {
                boardRenderer = boardObj.AddComponent<BoardRenderer>();
            }

            BlockPool blockPool = boardObj.GetComponent<BlockPool>();
            if (blockPool == null)
            {
                blockPool = boardObj.AddComponent<BlockPool>();
            }

            // 3. Create InputHandler
            GameObject inputObj = GameObject.Find("InputHandler");
            if (inputObj == null)
            {
                inputObj = new GameObject("InputHandler");
                inputObj.transform.SetParent(gameManager.transform.parent);
            }

            GoodJobInternshipCase.Core.InputHandler inputHandler = inputObj.GetComponent<GoodJobInternshipCase.Core.InputHandler>();
            if (inputHandler == null)
            {
                inputHandler = inputObj.AddComponent<GoodJobInternshipCase.Core.InputHandler>();
            }

            // 4. Create FeedbackManager
            GameObject feedbackObj = GameObject.Find("FeedbackManager");
            if (feedbackObj == null)
            {
                feedbackObj = new GameObject("FeedbackManager");
                feedbackObj.transform.SetParent(gameManager.transform.parent);
            }

            GoodJobInternshipCase.Feedback.FeedbackManager feedbackManager = feedbackObj.GetComponent<GoodJobInternshipCase.Feedback.FeedbackManager>();
            if (feedbackManager == null)
            {
                feedbackManager = feedbackObj.AddComponent<GoodJobInternshipCase.Feedback.FeedbackManager>();
            }

            // 5. Assign references via SerializedObject
            SerializedObject gmSO = new SerializedObject(gameManager);

            SerializedProperty boardRendererProp = gmSO.FindProperty("_boardRenderer");
            if (boardRendererProp != null)
                boardRendererProp.objectReferenceValue = boardRenderer;

            SerializedProperty inputHandlerProp = gmSO.FindProperty("_inputHandler");
            if (inputHandlerProp != null)
                inputHandlerProp.objectReferenceValue = inputHandler;

            SerializedProperty feedbackManagerProp = gmSO.FindProperty("_feedbackManager");
            if (feedbackManagerProp != null)
                feedbackManagerProp.objectReferenceValue = feedbackManager;

            // Try to find and assign GameConfig
            GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/ScriptableObjects/GameConfig.asset");
            if (config != null)
            {
                SerializedProperty configProp = gmSO.FindProperty("_config");
                if (configProp != null)
                    configProp.objectReferenceValue = config;
            }

            gmSO.ApplyModifiedProperties();

            // 6. Assign Block Prefab to BlockPool
            GameObject blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Block.prefab");
            if (blockPrefab != null)
            {
                SerializedObject poolSO = new SerializedObject(blockPool);
                SerializedProperty prefabProp = poolSO.FindProperty("_blockPrefab");
                if (prefabProp != null)
                    prefabProp.objectReferenceValue = blockPrefab;
                poolSO.ApplyModifiedProperties();
            }

            // 7. Setup Camera with CameraFit for mobile
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                CameraFit cameraFit = mainCamera.GetComponent<CameraFit>();
                if (cameraFit == null)
                {
                    cameraFit = mainCamera.gameObject.AddComponent<CameraFit>();
                }

                // Assign config to CameraFit
                if (config != null)
                {
                    SerializedObject camFitSO = new SerializedObject(cameraFit);
                    SerializedProperty camConfigProp = camFitSO.FindProperty("_config");
                    if (camConfigProp != null)
                        camConfigProp.objectReferenceValue = config;
                    camFitSO.ApplyModifiedProperties();
                }
            }

            // 8. Add MobileOptimizer
            GameObject optimizerObj = GameObject.Find("MobileOptimizer");
            if (optimizerObj == null)
            {
                optimizerObj = new GameObject("MobileOptimizer");
                optimizerObj.transform.SetParent(gameManager.transform.parent);
            }

            MobileOptimizer mobileOptimizer = optimizerObj.GetComponent<MobileOptimizer>();
            if (mobileOptimizer == null)
            {
                mobileOptimizer = optimizerObj.AddComponent<MobileOptimizer>();
            }

            // Mark scene dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

            Debug.Log("Scene setup complete! Check GameManager inspector for any missing references.");
            Selection.activeGameObject = gameManager.gameObject;
        }
    }
}
#endif
