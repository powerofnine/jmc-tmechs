using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

namespace TMechs.FX
{
    public class BlurFade : MonoBehaviour
    {
        private static BlurFade instance;

        public float fadeTime = .5F;
        private DepthOfField blurLayer;

        private float minBlur;
        private float maxBlur;

        private void Awake()
        {
            instance = this;
            Volume volume = GetComponent<Volume>();
            volume.profile.TryGet(out blurLayer);

            if (blurLayer)
            {
                minBlur = blurLayer.farMaxBlur.min;
                maxBlur = blurLayer.farMaxBlur.value;

                blurLayer.farMaxBlur.value = minBlur;

                volume.enabled = true;
                blurLayer.active = false;
            }
        }

        public static void Fade(bool enabled)
        {
            if (instance)
            {
                instance.StopAllCoroutines();
                instance.StartCoroutine(instance._Fade(enabled));
            }
        }

        private IEnumerator _Fade(bool enabled)
        {
            if (!blurLayer)
            {
                Debug.LogError("No blur layer detected");
                yield break;
            }

            float time = .0F;

            float start = enabled ? minBlur : maxBlur;
            float end = enabled ? maxBlur : minBlur;

            blurLayer.farMaxBlur.value = start;
            blurLayer.active = true;

            while (time < fadeTime && blurLayer)
            {
                time += Time.unscaledDeltaTime;
                blurLayer.farMaxBlur.value = Mathf.Lerp(start, end, time / fadeTime);
                yield return null;
            }

            blurLayer.active = enabled;
        }
    }
}