using System.IO;
using System.Linq;
using SockPairs.Core;
using UnityEditor;
using UnityEngine;

namespace SockPairs.Editor
{
    public class LevelWorkbench : EditorWindow
    {
        LevelBook book;int index;Vector2 scroll;string validation="";
        [MenuItem("Sock Pairs/Level workbench")]
        static void Open(){GetWindow<LevelWorkbench>("Sock Pairs Levels");}
        void OnEnable(){Reload();}
        void Reload(){if(File.Exists("Assets/Resources/Data/levels.json"))book=JsonUtility.FromJson<LevelBook>(File.ReadAllText("Assets/Resources/Data/levels.json"));}
        void OnGUI()
        {
            if(book==null){Reload();return;}
            EditorGUILayout.LabelField("100 solvable laundry days",EditorStyles.boldLabel);
            index=EditorGUILayout.IntSlider("Level",index+1,1,100)-1;
            var c=book.levels[index];scroll=EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField("Chapter "+c.chapter+" / "+c.type);
            c.seed=EditorGUILayout.IntField("Layout seed",c.seed);
            c.pairs=EditorGUILayout.IntSlider("Pairs",c.pairs,3,73);
            c.designs=EditorGUILayout.IntSlider("Designs",c.designs,3,24);
            c.layers=EditorGUILayout.IntSlider("Pile layers",c.layers,1,5);
            c.basket=EditorGUILayout.IntSlider("Basket slots",c.basket,0,5);
            c.washer=EditorGUILayout.IntSlider("Washer pairs",c.washer,0,Mathf.Min(9,c.pairs-3));
            c.line=EditorGUILayout.IntSlider("Clothesline pairs",c.line,0,Mathf.Min(8,c.pairs-3-c.washer));
            c.knots=EditorGUILayout.IntSlider("Knotted pairs",c.knots,0,Mathf.Min(6,c.pairs-3));
            c.wet=EditorGUILayout.IntSlider("Wet pairs",c.wet,0,Mathf.Min(5,c.pairs-3-c.knots));
            c.sizes=EditorGUILayout.IntSlider("Sized pairs",c.sizes,0,Mathf.Min(4,c.pairs-3-c.knots-c.wet));
            c.odd=EditorGUILayout.IntSlider("Odd socks",c.odd,0,3);
            c.seconds=EditorGUILayout.IntField("Three-star target seconds",c.seconds);
            c.coins=EditorGUILayout.IntField("First-win coins",c.coins);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("New layout")){c.seed+=7919;Validate(c);}
            if(GUILayout.Button("Validate"))Validate(c);
            if(GUILayout.Button("Save all"))
            {
                bool valid=true;
                foreach(var level in book.levels){try{if(Puzzle.Validate(level)!=null)valid=false;}catch{valid=false;}}
                if(valid){File.WriteAllText("Assets/Resources/Data/levels.json",JsonUtility.ToJson(book));AssetDatabase.Refresh();validation="Saved all 100 validated levels.";}
                else validation="Not saved: at least one level has an invalid layout. Reduce pair counts or increase layers.";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(validation.Length==0?"Changing a seed makes a new deterministic layout. Every saved level must pass its solution witness.":validation,MessageType.Info);
            try
            {
                var layout=LevelFactory.Create(c);
                Rect area=GUILayoutUtility.GetRect(420,520);EditorGUI.DrawRect(area,new Color(.95f,.87f,.74f));
                foreach(var s in layout.socks.OrderByDescending(s=>s.depth))
                {
                    if(s.slot>=30)continue;
                    float cell=area.width/6;
                    Rect r=new Rect(area.x+s.slot%6*cell+s.depth*2,area.y+s.slot/6*92+s.depth*4,cell*.9f,90);
                    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/sock_"+s.design.ToString("00")+".png");
                    if(texture)GUI.DrawTexture(r,texture,ScaleMode.ScaleToFit,true);
                }
                EditorGUILayout.LabelField(layout.socks.Count+" socks; "+layout.solution.Count+" solution actions");
            }catch(System.Exception e){EditorGUILayout.HelpBox(e.Message,MessageType.Warning);}
            EditorGUILayout.EndScrollView();
        }
        void Validate(LevelConfig c){try{validation=Puzzle.Validate(c)??"Solvable with no helpers.";}catch(System.Exception e){validation=e.Message;}}
    }
}
