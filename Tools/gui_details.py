from pathlib import Path
p=Path('Assets/Scripts/Runtime/ClayImage.cs');s=p.read_text().replace('public float roundness=.18f;','public float roundness=.18f,depth=1f;')
s=s.replace('light*highlight)', 'light*highlight*depth)').replace('-light*dark)','-light*dark*depth)').replace('tint.a=color.a;','tint.a=ring==0?0:color.a;');p.write_text(s)
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text().replace('static Sprite round,pill,clay;', '')
a=s.index('        public static Image Box(');b=s.index('        public static Image ClayBox(',a)
s=s[:a]+'''        public static Image Box(Transform parent,string name,float x,float y,float w,float h,Color color,bool rounded=true)
        {
            Image i;
            var r=Rect(parent,name,x,y,w,h);
            if(rounded){var shape=r.gameObject.AddComponent<ClayImage>();shape.depth=0;i=shape;}
            else i=r.gameObject.AddComponent<Image>();
            i.color=color;i.raycastTarget=false;return i;
        }
'''+s[b:]
s=s.replace('var track=Box(r,"Track"','var track=ClayBox(r,"Track"')
s=s.replace('public static void DisposeIcons(){if(clay)UnityEngine.Object.Destroy(clay);clay=null;}', 'public static void DisposeIcons(){}')
a=s.index('        public static Sprite Clay()');b=s.index('        public static void Stars(',a);s=s[:a]+s[b:]
s=s.replace('{if(!pill)pill=Resources.Load<Sprite>("Art/pill");return Sprite(parent,name,pill,x,y,size,size,color);}', '{return ClayBox(parent,name,x,y,size,size,color,true);}')
s=s.replace('{Box(parent,name+" rim",x,y,w,h,Line);return Box(parent,name,x,y,w-4,h-4,Cream);}', '{Box(parent,name+" rim",x,y-5,w,h,Line);return ClayBox(parent,name,x,y,w,h,Cream);}')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text().replace('            var game=SockGame.Instance;', '''            Application.logMessageReceived+=(message,stack,type)=>{if(type==LogType.Exception||type==LogType.Error)Application.Quit(1);};
            var game=SockGame.Instance;''')
s=s.replace('yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"album.png"));','''yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"album.png"));
            typeof(SockGame).GetMethod("OpenAlbumItem",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{0});
            yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"album-detail.png"));
            typeof(SockGame).GetMethod("CloseModal",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,null);''')
s=s.replace('            var order=game.puzzle.layout.solution.ToArray();\n            foreach(int id in order)', '''            var order=game.puzzle.layout.solution.ToArray();
            tap.Invoke(game,new object[]{order[0]});yield return new WaitForSecondsRealtime(.3f);Capture(Path.Combine(output,"selected-sock.png"));
            foreach(int id in order)''',1)
s=s.replace('            game.Home();\n            Debug.Log("SOCK PAIRS SMOKE PASS', '''            game.save.unlocked=25;for(int i=0;i<24;i++)game.save.stars[i]=i%3+1;game.save.active=null;game.Home();
            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"map-progress.png"));
            Debug.Log("SOCK PAIRS SMOKE PASS''')
p.write_text(s)
