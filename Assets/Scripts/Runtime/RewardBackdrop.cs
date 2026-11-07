using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    [RequireComponent(typeof(RawImage))]
    public sealed class RewardBackdrop:MonoBehaviour
    {
        RenderTexture texture;
        public static void Capture(Transform parent)
        {
            // Used only by the editor authoring pass. Saved popups already own this object.
            var r=UI.Rect(parent,"Soft focus gameplay",0,0,1080,1920);
            var image=r.gameObject.AddComponent<RawImage>();image.raycastTarget=false;
            var owner=r.gameObject.AddComponent<RewardBackdrop>();r.gameObject.AddComponent<FullBleed>();
            if(Application.isPlaying)owner.RefreshCapture();
        }
        public void RefreshCapture()
        {
            Release();
            var previous=RenderTexture.active;
            var canvas=GetComponentInParent<Canvas>();var camera=Camera.main;
            if(!camera)camera=Object.FindFirstObjectByType<Camera>();
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;
            var full=RenderTexture.GetTemporary(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);
            Material material=null;RenderTexture small=null;
            try
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=full;Canvas.ForceUpdateCanvases();camera.Render();
                camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;
                texture=new RenderTexture(192,Mathf.Max(64,Mathf.RoundToInt(192f*Screen.height/Screen.width)),0,RenderTextureFormat.ARGB32);
                texture.filterMode=FilterMode.Bilinear;texture.Create();
                material=new Material(Resources.Load<Shader>("Shaders/ReferenceBackdrop"));
                small=RenderTexture.GetTemporary(texture.width,texture.height,0,RenderTextureFormat.ARGB32);small.filterMode=FilterMode.Bilinear;
                Graphics.Blit(full,texture);Graphics.Blit(texture,small,material,0);Graphics.Blit(small,texture,material,1);
                GetComponent<RawImage>().texture=texture;
            }
            finally
            {
                camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=previous;
                if(material)Destroy(material);if(small)RenderTexture.ReleaseTemporary(small);RenderTexture.ReleaseTemporary(full);
            }
        }
        void Release(){if(texture){GetComponent<RawImage>().texture=null;if(RenderTexture.active==texture)RenderTexture.active=null;texture.Release();Destroy(texture);texture=null;}}
        void OnDisable(){Release();}
        void OnDestroy(){Release();}
    }
}
