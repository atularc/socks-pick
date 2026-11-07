using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SockPairs
{
    public class PlatformBridge : MonoBehaviour
    {
        public Action<string> Loaded;
        public Action<bool> Paused;
        public Action<bool> AudioChanged;
        public bool youtube, ready, audioAllowed=true, rewardedAdsAvailable;
        Action<bool> rewardCallback;
        static string SaveKey => Array.IndexOf(Environment.GetCommandLineArgs(),"-sockQA")>=0?"sock-pairs-qa-v1":"sock-pairs-save-v1";
        #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void SP_Init(string name);
        [DllImport("__Internal")] static extern void SP_Save(string json);
        [DllImport("__Internal")] static extern void SP_Ready();
        [DllImport("__Internal")] static extern void SP_Reward(string reward);
        #endif
        public void Initialize()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-sockResetTestData")>=0)PlayerPrefs.DeleteKey("sock-pairs-save-v1");
            #endif
            #if UNITY_WEBGL && !UNITY_EDITOR
            SP_Init(gameObject.name);
            #else
            SaveLoaded(PlayerPrefs.GetString(SaveKey,""));
            #endif
        }
        public void SaveLoaded(string json){ready=true;Loaded?.Invoke(json);}
        public void SetYouTube(string value){youtube=value=="1";}
        public void SetRewardedAdsAvailable(string value){rewardedAdsAvailable=value=="1";}
        public void HostPause(string value){Paused?.Invoke(value=="1");}
        public void HostAudio(string value){audioAllowed=value=="1";AudioChanged?.Invoke(audioAllowed);}
        public void SaveError(string message){Debug.LogWarning("Platform save: "+message);}
        public void Reward(string reward,Action<bool> callback)
        {
            if(rewardCallback!=null){callback(false);return;}
            rewardCallback=callback;
            #if UNITY_WEBGL && !UNITY_EDITOR
            if(youtube){SP_Reward(reward);return;}
            #endif
            RewardResult("0");
        }
        public void RewardResult(string value){var callback=rewardCallback;rewardCallback=null;callback?.Invoke(value=="1");}
        public void Save(string json)
        {
            if(!ready)return;
            #if UNITY_WEBGL && !UNITY_EDITOR
            SP_Save(json);
            #else
            PlayerPrefs.SetString(SaveKey,json);PlayerPrefs.Save();
            #endif
        }
        public void GameReady()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            SP_Ready();
            #endif
        }
    }
}
