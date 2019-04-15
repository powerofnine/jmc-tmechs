using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TMechs.Data.Settings
{
    [AddComponentMenu("")]
    public sealed class SettingsApplier : MonoBehaviour
    {
        private PostProcessProfile profile;

        private ColorGrading grading;
        
        private void Awake()
        {
            PostProcessVolume pp = gameObject.AddComponent<PostProcessVolume>();

            pp.isGlobal = true;
            
            profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            pp.profile = profile;

            grading = profile.AddSettings<ColorGrading>();
            grading.gamma.overrideState = true;
            grading.postExposure.overrideState = true;
        }

        private void Update()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();

            if (display != null)
            {
                grading.postExposure.value = Mathf.Clamp(display.brightness, -2F, 2F);
                grading.gamma.value.w = Mathf.Clamp(display.gamma, -1F, 1F);
                QualitySettings.vSyncCount = display.vsync ? 1 : 0;
            }
        }

        private void OnDestroy()
        {
            DestroyImmediate(profile);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject go = new GameObject("Settings Applier");
            DontDestroyOnLoad(go);

            go.AddComponent<SettingsApplier>();
        }
    }
}