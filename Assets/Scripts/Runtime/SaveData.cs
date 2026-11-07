using System;
using SockPairs.Core;
using UnityEngine;

namespace SockPairs
{
    [Serializable] public class SaveData
    {
        public int version=1, unlocked=1, coins=150, decor, theme,spots,hints,shuffles;
        public int[] stars=new int[100];
        public bool[] collection=new bool[24];
        public bool sound=true, music=true, haptics=true, reduceMotion;
        public Snapshot active;
        public static SaveData Read(string text)
        {
            SaveData s=null;
            try{if(!string.IsNullOrEmpty(text))s=JsonUtility.FromJson<SaveData>(text);}catch(Exception){ }
            if(s==null)s=new SaveData();
            s.unlocked=Mathf.Clamp(s.unlocked,1,100);s.coins=Mathf.Max(0,s.coins);
            if(s.stars==null||s.stars.Length!=100)s.stars=new int[100];
            if(s.collection==null||s.collection.Length!=24)s.collection=new bool[24];
            // JsonUtility may materialize a null nested serializable class as an
            // empty object. Treat that sentinel as no resumable level.
            if(s.active!=null)
            {
                var a=s.active;
                bool invalid=a.level<1||a.level>s.unlocked||a.socks==null||a.socks.Length==0||a.capacity<0||a.capacity>7||a.selected< -1||a.selected>=a.socks.Length;
                if(!invalid)
                {
                    for(int i=0;i<a.socks.Length;i++)
                    {var sock=a.socks[i];if(sock==null||sock.id!=i||sock.design<0||sock.design>=24||sock.slot<0||sock.slot>=36){invalid=true;break;}}
                    if(a.basket!=null)foreach(int id in a.basket)if(id<0||id>=a.socks.Length)invalid=true;
                }
                if(invalid)s.active=null;
            }
            return s;
        }
    }
    [Serializable] public class Snapshot
    {
        public int level,selected,matched,capacity,moves,helpers,levelRevision;
        public float elapsed;
        public int[] basket;
        public Sock[] socks;
        public bool lost,adUsed;
    }
}
