using UnityEngine;

namespace LCG
{
    public class BlurEffect : MonoBehaviour
    {
        private static GameObject _gameobject;
        private static BlurEffect _instance;
        public static BlurEffect Instance
        {
            get
            {
                if (null == _instance)
                {
                    _gameobject = new GameObject("BlurEffect");
                    _instance = _gameobject.AddComponent<BlurEffect>();
                    _instance.OnInstance();
                }
                return _instance;
            }
        }
        // The blur iteration shader.
        // Basically it just takes 4 texture samples and averages them.
        // By applying it repeatedly and spreading out sample locations
        // we get a Gaussian blur approximation.
        private Shader blurShader = null;

        // blur material and texture.
        private Material material = null;

        /// Blur iterations - larger number means more blur.
        public int iterations = 2;

        /// Blur spread for each iteration. Lower values
        /// give better looking blur, but require more iterations to
        /// get large blurs. Value is usually between 0.5 and 1.0.
        public float blurSpread = 1f;

        // samplingRate
        public float samplingRate = 0.2f;

        void OnInstance()
        {
            blurShader = ResourceLoader.LoadObject("Shaders/BlurEffectConeTaps", typeof(Shader)) as Shader;

            // Disable if we don't support image effects
            if (!SystemInfo.supportsImageEffects)
            {
                enabled = false;
                return;
            }

            material = new Material(blurShader);
            material.hideFlags = HideFlags.DontSave;

            // Disable if the shader can't run on the users graphics card
            if (!blurShader || !material.shader.isSupported)
            {
                enabled = false;
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

        // Performs one blur iteration.
        void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
        {
            float off = 0.5f + iteration * blurSpread;
            Graphics.BlitMultiTap(source, dest, material,
                new Vector2(-off, -off),
                new Vector2(-off, off),
                new Vector2(off, off),
                new Vector2(off, -off)
            );
        }

        // Downsamples the texture to a quarter resolution.
        void DownSample4x(Texture source, RenderTexture dest)
        {
            float off = 1.0f;
            Graphics.BlitMultiTap(source, dest, material,
                new Vector2(-off, -off),
                new Vector2(-off, off),
                new Vector2(off, off),
                new Vector2(off, -off)
            );
        }

        // Build Blur Textrue.
        RenderTexture BuildBlurTexture(int rtWidth, int rtHeight, Texture source)
        {
            RenderTexture blur = RenderTexture.GetTemporary(rtWidth, rtHeight, 16);
            // Copy source to the 4x4 smaller texture.
            DownSample4x(source, blur);
            // Blur the small texture
            for (int i = 0; i < iterations; i++)
            {
                RenderTexture buffer2 = RenderTexture.GetTemporary(rtWidth, rtHeight, 16);
                FourTapCone(blur, buffer2, i);
                RenderTexture.ReleaseTemporary(blur);
                blur = buffer2;
            }

            return blur;
        }

        // Get blurTexture from camera rendertextrure.
        public Texture GetBlurTexture(Camera camera, Camera camera2 = null)
        {
            if (null == camera)
            {
                return null;
            }

            int rtWidth = (int)(Screen.width * samplingRate);
            int rtHeight = (int)(Screen.height * samplingRate);
            var source = RenderTexture.GetTemporary(rtWidth, rtHeight, 16, RenderTextureFormat.RGB565);

            camera.targetTexture = source;
            camera.Render();
            camera.targetTexture = null;

            if (null != camera2)
            {
                camera2.targetTexture = source;
                camera2.Render();
                camera2.targetTexture = null;
            }

            RenderTexture blur = BuildBlurTexture(rtWidth, rtHeight, source);
            RenderTexture.ReleaseTemporary(source);

            return blur;
        }

        // Destroy
        public void Destroy()
        {
            _instance = null;
            if (null != _gameobject)
            {
                GameObject.Destroy(_gameobject);
            }
            if (null != material)
            {
                UnityEngine.Object.Destroy(material);
            }
            ResourceLoader.UnloadObject("Shaders/BlurEffectConeTaps");
        }
    }
}