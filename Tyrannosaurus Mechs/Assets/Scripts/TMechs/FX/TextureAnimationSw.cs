using System;
using InspectorGadgets.Attributes;
using JetBrains.Annotations;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.FX
{
    public class TextureAnimationSw : MonoBehaviour
    {
        public Texture2D[] frames;
        [Name("FPS")]
        [Min(0)]
        public float fps = 16F;
        [Required]
        public string textureName;

        [Label]
        private int frame;

        private Renderer render;
        private float timer;

        private int textureId;
        
        private void Start()
        {
            render = GetComponent<Renderer>();

            if (!render || frames == null || frames.Length == 0)
            {
                Destroy(this);
                Debug.LogError("TextureAnimationSw does not have a renderer or any frames");
            }

            textureId = Shader.PropertyToID(textureName);
        }

        private void Update()
        {
            if (fps <= Mathf.Epsilon || frames.Length == 0)
                return;
            
            timer -= Time.deltaTime;
            if (timer <= 0F)
            {
                timer = 1F / fps;
                
                if (frame >= frames.Length)
                    frame = 0;
                render.material.SetTexture(textureId, frames[frame]);
                
                frame++;
            }
        }

#if UNITY_EDITOR
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming - called by an addon
        private void AfterInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("This script is more expensive than a shader and should only be used when the latter cannot be." +
                                                "\nThis script also does not do editor-time previews.", UnityEditor.MessageType.Warning);
        }
#endif
    }
}
