using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

namespace TMechs.Data.Settings
{
    [AddComponentMenu("")]
    public sealed class SettingsApplier : MonoBehaviour
    {
        private VolumeProfile profile;

        private ColorAdjustments colorAdjustments;
        private LiftGammaGain gammaAdjustments;

        private float deltaTime;

        private void Awake()
        {
            Volume pp = gameObject.AddComponent<Volume>();

            pp.isGlobal = true;

            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            pp.profile = profile;

            colorAdjustments = profile.Add<ColorAdjustments>();
            gammaAdjustments = profile.Add<LiftGammaGain>();

            colorAdjustments.postExposure.overrideState = true;
            gammaAdjustments.gamma.overrideState = true;
        }

        private void Update()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();

            if (display != null)
            {
                colorAdjustments.postExposure.value = Mathf.Clamp(display.brightness, -2F, 2F);
                
                Vector4 gamma = gammaAdjustments.gamma.value;
                gamma.w = Mathf.Clamp(display.gamma, -1F, 1F);
                gammaAdjustments.gamma.value = gamma;
                
                QualitySettings.vSyncCount = display.vsync ? 1 : 0;
            }

            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1F;
        }

        private void OnDestroy()
        {
            DestroyImmediate(profile);
        }

        #region Obsolete UI Code

        private void OnGUI()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();

            if (display == null || display.fpsDisplay == DisplaySettings.FpsDisplay.None)
                return;

            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();
            Rect rect = default;

            style.fontSize = h * 2 / 100;

            switch (display.fpsDisplay)
            {
                case DisplaySettings.FpsDisplay.None:
                    break;
                case DisplaySettings.FpsDisplay.TopLeft:
                    style.alignment = TextAnchor.UpperLeft;
                    rect = new Rect(0, 0, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.TopRight:
                    style.alignment = TextAnchor.UpperRight;
                    rect = new Rect(0, 0, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.BottomLeft:
                    style.alignment = TextAnchor.LowerLeft;
                    rect = new Rect(0, h - style.fontSize, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.BottomRight:
                    style.alignment = TextAnchor.LowerRight;
                    rect = new Rect(0, h - style.fontSize, w, style.fontSize);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            style.normal.textColor = Color.yellow;

            float msec = deltaTime * 1000F;
            float fps = 1F / deltaTime;

            string text = $"{msec:0.0} ms ({fps:0.} fps)";
            GUI.Label(rect, text, style);
        }

        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject go = new GameObject("Settings Applier");
            DontDestroyOnLoad(go);

            go.AddComponent<SettingsApplier>();
        }
    }
}