using UnityEngine;
using UnityEngine.Rendering;

namespace TMechs.Environment
{
    public class ReactiveWater : MonoBehaviour
    {
        private const string REACTIVE_TAG = "Reactive Source";
        
        public int textureWidth = 1024;
        
        [Header("Stamp")]
        public Texture2D stamp;
        public int frames;
        public Vector2Int frameSize = new Vector2Int(256, 256);
        public float fps;
        
        private float ratio;
        private RenderTexture texture;
        private Collider col;
        private Renderer render;
        
        private Material step;
        private Vector2Int textureSize;
        private bool isTextureOld = true;

        private int textureAge;
        
        [SerializeField]
        private int frame;
        private float timer;
        
        private void Start()
        {
            col = GetComponent<Collider>();
            render = GetComponent<Renderer>();
            
            ratio = transform.localScale.z / transform.localScale.x;
            textureSize = new Vector2Int(textureWidth, Mathf.CeilToInt(ratio * textureWidth));
            
            texture = new RenderTexture(textureSize.x, textureSize.y, 0, RenderTextureFormat.R8);

            step = new Material(Shader.Find("Unlit/Transparent")) {mainTexture = stamp};

            render.material.SetTexture("_UnlitColorMap", texture);
        }

        private void Update()
        {
            if (textureAge > 4)
            {
                Graphics.SetRenderTarget(texture);
                
                GL.Clear(true, true, Color.black);
                
                Graphics.SetRenderTarget(null);
                textureAge = -1;
            }

            if (textureAge >= 0)
                textureAge++;
            isTextureOld = true;

            if (fps <= 0)
                return;
            
            timer += Time.deltaTime;
            if (timer >= 1F / fps)
            {
                frame++;

                if (frame >= frames)
                    frame = 0;
                
                timer = 0;
            }
        }

        private void OnDestroy()
        {
            Destroy(texture);
            Destroy(step);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!other.CompareTag(REACTIVE_TAG))
                return;

            Vector3 scale = transform.localScale;
            transform.localScale = Vector3.one;
            Vector2 local = transform.InverseTransformPoint(col.ClosestPoint(other.transform.position)).Remap(Utility.Axis.X, Utility.Axis.Z, 0);
            transform.localScale = scale;
            
            Vector2 planeSize = transform.localScale.Remap(Utility.Axis.X, Utility.Axis.Z, 0) * 10F;

            local += planeSize / 2F;
            local /= planeSize;

            Graphics.SetRenderTarget(texture);

            if (isTextureOld)
                GL.Clear(true, true, Color.black);
            
            isTextureOld = false;
            textureAge = 0;
            
            GL.LoadOrtho();
            step.SetPass(0);
            
            // Frame
            float frameX = Mathf.FloorToInt(frame % (stamp.width / (float)frameSize.x)) * frameSize.x / (float)stamp.width;
            float frameY = 1F - Mathf.FloorToInt(frame / (stamp.height / (float)frameSize.y)) * frameSize.y / (float)stamp.height;
            float frameWidth = frameSize.x / (float)stamp.width;
            float frameHeight = frameSize.y / (float)stamp.height;
            
            GL.Begin(GL.QUADS);
            
            GL.TexCoord2(frameX, frameY);
            GL.Vertex(Vector2.one - local + (Vector2.left + Vector2.up) / textureSize * 100F);
            GL.TexCoord2(frameX + frameWidth, frameY);
            GL.Vertex(Vector2.one - local + (Vector2.right + Vector2.up) / textureSize * 100F);
            GL.TexCoord2(frameX + frameWidth, frameY - frameHeight);
            GL.Vertex(Vector2.one - local + (Vector2.right + Vector2.down) / textureSize * 100F);
            GL.TexCoord2(frameX, frameY - frameHeight);
            GL.Vertex(Vector2.one - local + (Vector2.left + Vector2.down) / textureSize * 100F);

            GL.End();

            Graphics.SetRenderTarget(null);
        }
    }
}
