using System;
using UnityEngine;
using UnityEngine.UI;
namespace SockPairs
{
    public sealed class SceneToggle:MonoBehaviour
    {
        public Button button;
        public Image track;
        public RectTransform knob;
        public Color onColor=new Color(.188f,.427f,.376f),offColor=new Color(.655f,.741f,.710f);
        public Vector2 onPosition=new Vector2(42,0),offPosition=new Vector2(-42,0);
        public void Bind(bool value,Action changed)
        {
            void Paint(){track.color=value?onColor:offColor;knob.anchoredPosition=value?onPosition:offPosition;}
            Paint();button.onClick.RemoveAllListeners();
            button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();value=!value;Paint();changed();});
        }
    }
}
