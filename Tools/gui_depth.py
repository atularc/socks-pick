from pathlib import Path
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text()
a=s.index('        public static Image Panel(');b=s.index('        public static RectTransform ClippedLayer',a)
s=s[:a]+'''        public static Image ClayBox(Transform parent,string name,float x,float y,float w,float h,Color color,bool round=false)
        {
            var i=Rect(parent,name,x,y,w,h).gameObject.AddComponent<ClayImage>();
            i.roundness=round?.5f:.18f;i.color=color;i.raycastTarget=false;return i;
        }
        public static Image Panel(Transform parent,string name,float x,float y,float w,float h,Color color)
        {
            Box(parent,name+" shadow",x,y-9,w,h,new Color(.10f,.22f,.18f,.17f));
            return ClayBox(parent,name,x,y,w,h,color);
        }
'''+s[b:]
a=s.index('            var face=Box(r,"Face"');b=s.index('            var label=Label',a)
s=s[:a]+'''            var depth=iconOnly?Circle(r,"Button depth",0,-6,Mathf.Min(w,h),Color.Lerp(color,Ink,.30f)):Box(r,"Button depth",0,-7,w,h,Color.Lerp(color,Ink,.30f));
            var face=ClayBox(r,"Face",0,0,w,h,color,iconOnly);face.raycastTarget=true;
'''+s[b:]
p.write_text(s)
p=Path('Assets/Scripts/Runtime/LevelMapView.cs');s=p.read_text()
s=s.replace('row.face=UI.Circle(row.root,"Level",0,0,184,UI.Pink);', 'row.face=UI.ClayBox(row.root,"Level",0,0,184,184,UI.Pink,true);')
s=s.replace('row.door=UI.Circle(row.root,"Soft centre",0,5,161,UI.Pink);', 'row.door=UI.Circle(row.root,"Soft centre",0,5,161,Color.clear);row.door.gameObject.SetActive(false);')
s=s.replace('var highlight=UI.Box(row.root,"Clay highlight",0,64,82,9,new Color(1,1,1,.32f));','')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text()
s=s.replace('MapHud();\n            UI.Label(screen,"Your laundry adventure",0,510,900,70,44,UI.Ink);','''MapHud();
            var mapSurface=UI.Panel(screen,"Map reading surface",0,-100,990,1280,new Color(1,.98f,.94f,.88f));
            UI.Label(screen,"Your laundry adventure",0,510,900,70,44,UI.Ink);''')
# Put map heading on its own cream banner with a bounded shadow.
s=s.replace('var mapSurface=UI.Panel(screen,"Map reading surface",0,-100,990,1280,new Color(1,.98f,.94f,.88f));','''var mapSurface=UI.Panel(screen,"Map reading surface",0,-110,990,1130,new Color(1,.98f,.94f,.88f));
            UI.Panel(screen,"Map title card",0,510,920,100,UI.Cream);''')
s=s.replace('NewScreen("album");UI.Button','NewScreen("album");UI.Panel(screen,"Album surface",0,0,1030,1840,UI.Cream);UI.Button')
# The new title is always on a panel; leave the room visible around its edges.
s=s.replace('UI.Label(screen,"DAY "+(save.active!=null?save.active.level:save.unlocked).ToString("00"),x,-675,width,42,27,UI.Muted);', 'UI.Label(button.transform,"DAY "+(save.active!=null?save.active.level:save.unlocked).ToString("00"),0,-82,width,32,24,Color.white);')
# Nav footer: icon and live label each have room for a third context line.
s=s.replace('new Vector2(0,-46)','new Vector2(0,-25)').replace('new Vector2(width-24,75)','new Vector2(width-24,60)')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text().replace('label.text!="Basket  "','label.text!="Waiting socks  "')
s=s.replace('face.type!=UnityEngine.UI.Image.Type.Sliced','!(face is ClayImage)')
p.write_text(s)
