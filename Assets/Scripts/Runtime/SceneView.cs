using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SockPairs
{
    // References point at authored scene objects. Runtime binding never recreates the layout.
    public sealed class SceneView : MonoBehaviour
    {
        [Serializable] public class Binding { public string key; public Component target; }
        public string id;
        public List<Binding> bindings = new List<Binding>();
        Dictionary<string,Component> lookup;
        readonly Dictionary<string,(int size,Color color)> textStyles=new Dictionary<string,(int,Color)>();
        public (int size,Color color) TextStyle(string key){if(!textStyles.TryGetValue(key,out var style)){var t=Get<Text>(key);style=(t.fontSize,t.color);textStyles[key]=style;}return style;}
        public T Get<T>(string key) where T:Component
        {
            if(lookup==null){lookup=new Dictionary<string,Component>();foreach(var item in bindings)lookup.Add(item.key,item.target);}
            if(!lookup.TryGetValue(key,out var target)||!target)throw new InvalidOperationException(id+": missing scene binding "+key);
            return target as T ?? target.GetComponent<T>();
        }
        public void Set(string key,Component target){bindings.Add(new Binding{key=key,target=target});lookup=null;}
        public void Text(string key,string value){Get<Text>(key).text=value;}
        public void Show(string key,bool visible){Get<Transform>(key).gameObject.SetActive(visible);}
        public void Button(string key,Action action)
        {
            var button=Get<Button>(key);button.onClick.RemoveAllListeners();
            button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();action();});
        }
    }
}
