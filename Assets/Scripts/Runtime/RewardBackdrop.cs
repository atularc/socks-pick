using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    public sealed class RewardBackdrop:MonoBehaviour
    {
        RenderTexture texture;
        // Once per reward, on the end-of-frame coroutine. The small bilinear texture supplies
        // B's soft-focus gameplay backdrop without an ongoing blur pass or CPU pixel readback.
        public static void Capture(Transform parent)
        {
            var previous=RenderTexture.active;
            var canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
            var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;var oldTarget=camera.targetTexture;
            var full=RenderTexture.GetTemporary(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32);
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=full;Canvas.ForceUpdateCanvases();camera.Render();
            camera.targetTexture=oldTarget;canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;
            var r=UI.Rect(parent,"Soft focus gameplay",0,0,1080,1920);
            var owner=r.gameObject.AddComponent<RewardBackdrop>();
            owner.texture=new RenderTexture(192,Mathf.Max(64,Mathf.RoundToInt(192f*Screen.height/Screen.width)),0,RenderTextureFormat.ARGB32);
            owner.texture.filterMode=FilterMode.Bilinear;owner.texture.Create();
            var material=new Material(Resources.Load<Shader>("Shaders/ReferenceBackdrop"));
            var small=RenderTexture.GetTemporary(owner.texture.width,owner.texture.height,0,RenderTextureFormat.ARGB32);small.filterMode=FilterMode.Bilinear;
            Graphics.Blit(full,owner.texture);Graphics.Blit(owner.texture,small,material,0);Graphics.Blit(small,owner.texture,material,1);
            RenderTexture.active=previous;Destroy(material);RenderTexture.ReleaseTemporary(small);RenderTexture.ReleaseTemporary(full);
            var image=r.gameObject.AddComponent<RawImage>();image.texture=owner.texture;image.raycastTarget=false;r.gameObject.AddComponent<FullBleed>();
        }
        void OnDestroy(){if(texture){if(RenderTexture.active==texture)RenderTexture.active=null;texture.Release();Destroy(texture);}}
    }
}
