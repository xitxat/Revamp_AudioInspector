using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace Revamp.AudioTools.AudioInspector
{
    public class AudioInspectorEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Revamp/Audio Inspector")]
        public static void ShowWindow()
        {
            AudioInspectorEditorWindow wnd = GetWindow<AudioInspectorEditorWindow>();
            wnd.titleContent = new GUIContent("Audio Inspector");
        }

        public void OnEnable()
        {
            // Load  UXML, USS
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Revamp/AudioTools/AudioInspector/Docs/AudioInspector.uxml");
            VisualElement root = visualTree.CloneTree();
            rootVisualElement.Add(root);
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Revamp/AudioTools/AudioInspector/Docs/AudioInspector.uss");
            root.styleSheets.Add(styleSheet);
        }
        
    }
}
