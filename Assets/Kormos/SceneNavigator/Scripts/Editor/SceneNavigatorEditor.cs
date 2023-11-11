using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Kormos.SceneNavigator.Editor
{
    #region CLASSES

    public class SceneNavigatorEditor : EditorWindow
    {
        #region FIELDS

        private readonly Dictionary<string, string> _scenePaths = new();

        #endregion

        #region METHODS

        [MenuItem("Tools/Kormos/Scene Navigator")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(SceneNavigatorEditor));
            window.titleContent = new GUIContent("Scene Navigator");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Kormos/SceneNavigator/Editor/SceneNavigatorEditor.uxml");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Kormos/SceneNavigator/Editor/SceneNavigatorEditor.uss");
            visualTree.CloneTree(root);
            root.styleSheets.Add(styleSheet);
            SetupContainers();
        }

        private void SetupContainers()
        {
            //get scenes dropdown
            var scenesDropdownField = rootVisualElement.Q<DropdownField>("scenes-dropdown");
            scenesDropdownField.choices.Clear();

            //get all scenes
            var sceneAssets = AssetDatabase.FindAssets("t:Scene");
            foreach (var sceneAsset in sceneAssets)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneAsset);
                var sceneName = scenePath[(scenePath.LastIndexOf('/') + 1)..];
                sceneName = sceneName[..sceneName.LastIndexOf('.')];
                _scenePaths.Add(sceneName, scenePath);
                scenesDropdownField.choices.Add(sceneName);
            }

            //set value as active scene
            var activeScene = SceneManager.GetActiveScene();
            var activeSceneName = activeScene.name;
            var match = scenesDropdownField.choices.Find(match => match == activeSceneName);
            scenesDropdownField.value = match ?? scenesDropdownField.choices[0];

            //set button events
            var allScenesAdditiveLoadButton = rootVisualElement.Q<Button>("all-scenes-additive-load-button");
            allScenesAdditiveLoadButton.clicked += () =>
            {
                var sceneName = scenesDropdownField.value;
                var scenePath = _scenePaths[sceneName];
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            };

            var allScenesSingleLoadButton = rootVisualElement.Q<Button>("all-scenes-single-load-button");
            allScenesSingleLoadButton.clicked += () =>
            {
                var sceneName = scenesDropdownField.value;
                var scenePath = _scenePaths[sceneName];
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            };

            var allScenesRefreshButton = rootVisualElement.Q<Button>("all-scenes-refresh-button");
            allScenesRefreshButton.clicked += () =>
            {
                _scenePaths.Clear();
                scenesDropdownField.choices.Clear();
                var sceneAssetsLocal = AssetDatabase.FindAssets("t:Scene");
                foreach (var sceneAsset in sceneAssetsLocal)
                {
                    var scenePath = AssetDatabase.GUIDToAssetPath(sceneAsset);
                    var sceneName = scenePath[(scenePath.LastIndexOf('/') + 1)..];
                    sceneName = sceneName[..sceneName.LastIndexOf('.')];
                    _scenePaths.Add(sceneName, scenePath);
                    scenesDropdownField.choices.Add(sceneName);
                }
            };

            var presetsEmptyLabel = rootVisualElement.Q<Label>("presets-empty-label");
            var allScenesPresetSaveButton = rootVisualElement.Q<Button>("all-scenes-preset-save-button");
            allScenesPresetSaveButton.clicked += () =>
            {
                var ddSceneName = scenesDropdownField.value;
                var presetsContainer = rootVisualElement.Q<VisualElement>("presets-container");

                var hasAlreadyPreset = false;
                foreach (var child in presetsContainer.Children())
                {
                    var childLabel = child.Q<Label>("scene-name-label");
                    var childSceneName = childLabel.text;
                    if (childSceneName == ddSceneName)
                    {
                        hasAlreadyPreset = true;
                        break;
                    }
                }

                if (hasAlreadyPreset) return;

                var scenePresetContainerTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Kormos/SceneNavigator/Editor/ScenePresetContainer.uxml");
                var scenePresetContainer = scenePresetContainerTemplate.CloneTree();
                var sceneNameLabel = scenePresetContainer.Q<Label>("scene-name-label");
                var sceneSingleLoadButton = scenePresetContainer.Q<Button>("scene-single-load-button");
                var sceneAdditiveLoadButton = scenePresetContainer.Q<Button>("scene-additive-load-button");
                var sceneRemoveButton = scenePresetContainer.Q<Button>("scene-remove-button");
                var sceneMoveUpButton = scenePresetContainer.Q<Button>("scene-move-up-button");
                var sceneMoveDownButton = scenePresetContainer.Q<Button>("scene-move-down-button");

                var sceneName = ddSceneName;
                sceneSingleLoadButton.clicked += () =>
                {
                    var scenePath = _scenePaths[sceneName];
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                };

                sceneAdditiveLoadButton.clicked += () =>
                {
                    var scenePath = _scenePaths[sceneName];
                    EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                };

                sceneRemoveButton.clicked += () =>
                {
                    presetsContainer.Remove(scenePresetContainer);
                    if (presetsContainer.childCount == 0)
                    {
                        presetsEmptyLabel.style.display = DisplayStyle.Flex;
                    }
                };

                sceneMoveUpButton.clicked += () =>
                {
                    var index = presetsContainer.IndexOf(scenePresetContainer);
                    if (index == 0) return;
                    presetsContainer.RemoveAt(index);
                    presetsContainer.Insert(index - 1, scenePresetContainer);
                };

                sceneMoveDownButton.clicked += () =>
                {
                    var index = presetsContainer.IndexOf(scenePresetContainer);
                    if (index == presetsContainer.childCount - 1) return;
                    presetsContainer.RemoveAt(index);
                    presetsContainer.Insert(index + 1, scenePresetContainer);
                };


                presetsContainer.Add(scenePresetContainer);
                sceneNameLabel.text = $"{ddSceneName}";
                presetsEmptyLabel.style.display = DisplayStyle.None;
            };
        }

        #endregion
    }

    #endregion
}