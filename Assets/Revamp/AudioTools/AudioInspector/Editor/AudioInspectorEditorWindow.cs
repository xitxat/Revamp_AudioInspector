using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace Revamp.AudioTools.AudioInspector
{
    public class AudioInspectorEditorWindow : EditorWindow
    {

        private List<AudioClip> audioClips = new List<AudioClip>();
        private string searchFilter = "t:AudioClip"; // Pre-populated search filter

        private bool isForcedToMono = false;
        private bool isLoadInBkgrnd = false;
        private bool isAmbisonic = false;

        private bool isADPCM = false;
        private bool isPCM = false;
        private bool isVorbis = false;

        private bool isMP3 = false;
        private bool isOGG = false;
        private bool isWAV = false;

        
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

            SetupButtonsAndEvents();
        }

private void SetupButtonsAndEvents()
        {


            // Setup Toggle Event Handlers
            SetupToggle("FORCED_TO_MONO", value => isForcedToMono = value);
            SetupToggle("LOADED_IN_BACKGROUND", value => isLoadInBkgrnd = value);
            SetupToggle("AMBISONIC", value => isAmbisonic = value);
            SetupToggle("adpcm", value => isADPCM = value);
            SetupToggle("pcm", value => isPCM = value);
            SetupToggle("vorbis", value => isVorbis = value);
            SetupToggle("mp3", value => isMP3 = value);
            SetupToggle("ogg", value => isOGG = value);
            SetupToggle("wav", value => isWAV = value);

            // Initial fetch and display of audio clips
            SearchAudioClips(searchFilter, applyFilters: false);
        }

private void SetupToggle(string toggleName, System.Action<bool> toggleAction)
        {
            var toggle = rootVisualElement.Q<Toggle>(toggleName);
            toggle.RegisterValueChangedCallback(evt =>
            {
                toggleAction(evt.newValue);
                SearchAudioClips(searchFilter);
            });
        }

// FILTER
    private void SearchAudioClips(string filter, bool applyFilters = true)
    {
        var allAudioClips = AssetDatabase.FindAssets(filter)
                                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                        .Select(path => AssetDatabase.LoadAssetAtPath<AudioClip>(path))
                                        .Where(clip => clip != null).ToList();  // clip is being defined as a parameter for the lambda

        Debug.Log($"Total AudioClip assets found: {allAudioClips.Count}");

        // Apply LINQ query to filter audio clips based on toggles
        // If this block return true, add to list
        var filteredAudioClips = allAudioClips;

        if (applyFilters)
        {
            filteredAudioClips = allAudioClips.Where(clip =>
            {
                var assetPath = AssetDatabase.GetAssetPath(clip);
                var audioImporter = AssetImporter.GetAtPath(assetPath) as AudioImporter;

                if (audioImporter == null)
                {
                    Debug.LogWarning($"AudioImporter not found for clip: {clip.name}");
                    return false;
                }

                Debug.Log($"Clip: {clip.name}, ForceToMono in Importer: {audioImporter.forceToMono}, ForcedToMono Toggle: {isForcedToMono}");

                // TOGGLES
                if (isForcedToMono != audioImporter.forceToMono) return false;
                if (isLoadInBkgrnd != audioImporter.loadInBackground) return false;
                if (isAmbisonic != audioImporter.ambisonic) return false;

                if (!IsMatchingCompressionFormat(audioImporter.defaultSampleSettings.compressionFormat)) return false;
                if (!IsMatchingExtensionFormat(assetPath)) return false;

                return true; // Keep the clip if all conditions above are met
            }).ToList();
        }

        Debug.Log($"Filtered AudioClip assets count: {filteredAudioClips.Count}");

        audioClips = filteredAudioClips; 

        // GUI 
        UpdateAudioClipListUI();
    }

        private void UpdateAudioClipListUI()
        {
            ScrollView scrollView = rootVisualElement.Q<ScrollView>("results");
            scrollView.Clear();

            if (audioClips.Count > 0)
            {
                foreach (var clip in audioClips)
                {
                    var button = new Button(() => Selection.activeObject = clip)
                    {
                        text = clip.name
                    };
                    scrollView.Add(button);                
                }
            }
            else
            {
                var noResultsLabel = new Label("No AudioClips found. Try changing the search filter or compression format.");
                scrollView.Add(noResultsLabel);
            }
        }

        private bool IsMatchingCompressionFormat(AudioCompressionFormat format)
        {
            // Allow no selection
            // If no compression format is selected, return true for all clips.
            if (!isADPCM && !isPCM && !isVorbis)
            {
                return true;
            }

            // Return true if the clip's format matches the selected format.
            return (isADPCM && format == AudioCompressionFormat.ADPCM) ||
                (isPCM && format == AudioCompressionFormat.PCM) ||
                (isVorbis && format == AudioCompressionFormat.Vorbis);
        }

        private bool IsMatchingExtensionFormat(string assetPath)
        {
            // If no extension format is selected, return true for all clips.
            if (!isMP3 && !isOGG && !isWAV)
            {
                return true;
            }

            string extension = Path.GetExtension(assetPath).ToLower();
            // Return true if the clip's extension matches the selected format.
            return (isMP3 && extension == ".mp3") ||
                (isOGG && extension == ".ogg") ||
                (isWAV && extension == ".wav");
        }




    }
}
