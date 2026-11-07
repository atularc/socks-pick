from pathlib import Path
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text()
s=s.replace('Ink=Hex("234D4A"), Cream=Hex("F3D9AA"), Green=Hex("36826C"), Blue=Hex("377E80"), Pink=Hex("D95442"), Gold=Hex("F4C453"), Purple=Hex("477D76"), Mint=Hex("BAD8C5"), Muted=Hex("43645B"), Line=Hex("86AA97"), Board=Hex("B6CDB3"), Trail=Hex("56877A")','Ink=Hex("4A210E"), Cream=Hex("FFD7AA"), Green=Hex("306D60"), Blue=Hex("245E59"), Pink=Hex("FFA97C"), Gold=Hex("F4C453"), Purple=Hex("FFD6A5"), Mint=Hex("FFDCB7"), Muted=Hex("74513A"), Line=Hex("B97A3B"), Board=Hex("0D4647"), Trail=Hex("0D4647")')
s=s.replace('return ClayBox(parent,name,x,y,w,h,color);','string asset=name=="Cream play surface"||name=="Map reading surface"?"CosyBoard":name=="Panel"||name=="Win"||h>600?"CosyPopup":color==Blue||color==Green?"CosyTeal":"CosySecondary";\n            return CosyArt.Image(parent,name,asset,x,y,w,h);',1)
a=s.index('            var depth=iconOnly?');b=s.index('            var label=Label',a)
s=s[:a]+'''            string asset=iconOnly?"CosyCurrent":color==Pink?"CosyButton":color==Blue||color==Green?"CosyTeal":"CosySecondary";
            Color textColor=asset=="CosyTeal"?Cream:Ink;
            var face=CosyArt.Image(r,"Face",asset,0,0,w,h,!iconOnly);face.raycastTarget=true;
'''+s[b:]
s=s.replace('w-36,h-12,size,Color.white','w-36,h-12,size,textColor').replace('iconSize,Color.white);','iconSize,textColor);')
s=s.replace('if(basketArt)UnityEngine.Object.Destroy(basketArt);basketArt=null;','basketArt=null;')
s=s.replace('{Box(parent,name+" rim",x,y-5,w,h,Line);return ClayBox(parent,name,x,y,w,h,Cream);}','{return CosyArt.Image(parent,name,"CosySecondary",x,y,w,h);}')
s=s.replace('"Art/FreshLaundryBackground"','"Art/CosyRoom"')
s=s.replace('            if(quiet)Wood(parent);','')
s=s.replace('art.color=new Color(1,1,1,quiet?.18f:1f);r.gameObject.AddComponent<FullBleed>();','art.color=Color.white;r.gameObject.AddComponent<FullBleed>();\n            CosyArt.Image(parent,"Wooden screen frame","CosyShell",0,0,1040,1880).gameObject.AddComponent<CosyScreenFrame>();')
s=s.replace('if(!basketArt){var t=Resources.Load<Texture2D>("Art/FreshBasket");basketArt=UnityEngine.Sprite.Create(t,new Rect(0,0,t.width,t.height),new Vector2(.5f,.5f),100);}','if(!basketArt)basketArt=CosyArt.Get("CosyBasket");')
p.write_text(s)

p=Path('Assets/Editor/BuildTools.cs');s=p.read_text()
s=s.replace('if(file.Contains("reference-sheet"))continue;','if(file.Contains("reference-sheet"))continue;\n                if(file.Contains("Cosy")){AssetDatabase.ImportAsset(file.Replace(\'\\\\\',\'/\'),ImportAssetOptions.ForceUpdate);continue;}')
s=s.replace('if(!file.Contains("FreshBasket")','if(!file.Contains("CosyRoom")&&!file.Contains("FreshBasket")')
p.write_text(s)

p=Path('Assets/Scripts/Runtime/StarGraphic.cs');s=p.read_text();a=s.index('        protected override void OnPopulateMesh',s.index('public class StarGraphic'));b=s.index('\n    public class SockVisual',a)
s=s[:a]+'''        public override Texture mainTexture=>CosyArt.Get("CosyStar")?CosyArt.Get("CosyStar").texture:Texture2D.whiteTexture;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var sprite=CosyArt.Get("CosyStar");if(!sprite)return;
            var uv=UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);var r=rectTransform.rect;
            var v=UIVertex.simpleVert;v.color=color==UI.Gold?Color.white:color;
            v.position=new Vector2(r.xMin,r.yMin);v.uv0=new Vector2(uv.x,uv.y);vh.AddVert(v);
            v.position=new Vector2(r.xMin,r.yMax);v.uv0=new Vector2(uv.x,uv.w);vh.AddVert(v);
            v.position=new Vector2(r.xMax,r.yMax);v.uv0=new Vector2(uv.z,uv.w);vh.AddVert(v);
            v.position=new Vector2(r.xMax,r.yMin);v.uv0=new Vector2(uv.z,uv.y);vh.AddVert(v);
            vh.AddTriangle(0,1,2);vh.AddTriangle(0,2,3);
        }
    }
'''+s[b:];p.write_text(s)

p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text()
a=s.index('        void FitMapLayout()');b=s.index('        void FitShopLayout()',a)
s=s[:a]+'''        void FitMapLayout()
        {
            if(currentScreen!="map"||!screen)return;
            Rect area=Screen.safeArea;float scale=Mathf.Min(area.width/1080f,area.height/1920f);
            float extra=Mathf.Max(0,(area.height/Mathf.Max(.0001f,scale)-1920)*.5f);
            foreach(string name in new[]{"Map logo","Map logo title","Settings"})
            {var r=screen.Find(name) as RectTransform;if(r)r.anchoredPosition=new Vector2(r.anchoredPosition.x,810+extra);}
            foreach(string name in new[]{"Map reading surface","Map reading surface shadow"})
            {var r=screen.Find(name) as RectTransform;if(r){r.sizeDelta=new Vector2(1000,1490+extra*2);r.anchoredPosition=new Vector2(0,180-(name.EndsWith("shadow")?14:0));}}
            var viewport=screen.Find("Scrollable level path") as RectTransform;
            if(viewport)viewport.sizeDelta=new Vector2(960,1210+extra*2);
            var bar=screen.Find("Map navigation") as RectTransform;if(bar)bar.anchoredPosition=new Vector2(0,-750-extra);
            var play=screen.Find("PLAY") as RectTransform;if(play)play.anchoredPosition=new Vector2(0,-660-extra);
            var album=screen.Find("Album") as RectTransform;if(album)album.anchoredPosition=new Vector2(0,-840-extra);
        }
'''+s[b:]
# Main play board ends above the basket. Early B screen has one compact teal inset.
s=s.replace('float bottom=-930;','float bottom=currentScreen=="play"?(puzzle.layout.config.basket==0?-460:-370):-930;')
s=s.replace('item.sizeDelta=new Vector2(1030,top-bottom+extra*2);item.anchoredPosition=new Vector2(0,(top+bottom)*.5f','item.sizeDelta=new Vector2(1000,top-bottom+extra*1.35f);item.anchoredPosition=new Vector2(0,(top+bottom)*.5f+extra*.325f')
a=s.index('            MapHud();',s.index('        void Map(int chapter)'));b=s.index('            FitMapLayout();',a)
s=s[:a]+'''            UI.Panel(screen,"Map reading surface",0,180,1000,1490,UI.Trail);
            CosyArt.Image(screen,"Map logo","CosyButton",-45,810,740,140);
            UI.Label(screen,"Sock Pairs",-45,810,720,125,80,UI.Ink).name="Map logo title";
            UI.Button(screen,"Settings",420,810,105,105,UI.Pink,Settings,36);
            var map=screen.gameObject.GetComponent<LevelMapView>()??screen.gameObject.AddComponent<LevelMapView>();
            map.Build(screen,save,next,level=>StartLevel(level),unlockAllLevelsForTesting);
            UI.Box(screen,"Map navigation",0,-750,1040,420,Color.clear,false);
            MapNavigationButton("PLAY",0,850,UI.Pink,1,()=>{if(save.active!=null)Resume();else StartLevel(save.unlocked);});
            MapNavigationButton("Album",0,650,UI.Purple,2,Album);
'''+s[b:]
a=s.index('            var button=UI.Button',s.index('        void MapNavigationButton'));b=s.index('\n        void Shop()',a)
s=s[:a]+'''            bool primary=title=="PLAY";
            string display=primary?(save.active!=null?"Continue level "+save.active.level:"Play level "+save.unlocked):"Album";
            var button=UI.Button(screen,display,x,primary?-660:-840,width,primary?165:135,color,action,primary?64:60);
            button.name=title;var label=button.GetComponentInChildren<Text>();
            label.rectTransform.anchoredPosition=new Vector2(36,2);label.rectTransform.sizeDelta=new Vector2(width-150,120);
            UI.Glyph(button.transform,primary?"play":"album",-width*.35f,2,primary?66:60,UI.Ink);
        }
'''+s[b:]
# Shared top bar in B: one peach plate, divider, pause.
a=s.index('            UI.Panel(screen,"Level pill"',s.index('        void BuildPlayHeader'));b=s.index('            UI.Panel(screen,"Title ribbon"',a)
s=s[:a]+'''            UI.Panel(screen,"Level pill",-80,850,790,130,UI.Cream);
            UI.Label(screen,"Day "+level,-325,850,270,110,55,UI.Ink);
            UI.Box(screen,"Header divider",-160,850,3,74,UI.Ink,false);
            progressLabel=UI.Label(screen,"Pairs "+puzzle.matched+" / "+pairs,100,850,460,110,51,UI.Ink);
            UI.Button(screen,"II",415,850,135,135,UI.Pink,Pause,38);
'''+s[b:]
s=s.replace('UI.Panel(screen,"Title ribbon",0,titleY,940,titleHeight,UI.Mint);','UI.Panel(screen,"Title ribbon",0,titleY,590,titleHeight,UI.Mint);')
s=s.replace('UI.Label(screen,"Find a matching pair",0,titleY,900,titleHeight-10','UI.Label(screen,"Find a pair",0,titleY,550,titleHeight-10')
s=s.replace('return h>100?60:48;','return h>100?56:46;')
s=s.replace('new Vector2(920f*puzzle.matched/pairs,8)','new Vector2(920f*puzzle.matched/pairs,0)')
s=s.replace('"Progress track",0,795,920,8','"Progress track",0,795,920,0').replace('"Pair progress",-460,795,0,8','"Pair progress",-460,795,0,0')
s=s.replace('basketFrame=UI.Basket(screen,0,-790,1200,400);','basketFrame=UI.Basket(screen,0,-700,1040,440);')
s=s.replace('float center=rows<=3?70:20;','float center=rows<=3?80:60;')
# Existing title updates must use the approved concise copy.
s=s.replace('"Find a matching pair"','"Find a pair"')
# Match reference reward hierarchy; retain actual progress data.
s=s.replace('UI.Label(panel,"DAY "+level.ToString("00")+" COMPLETE",0,475,800,70,36,UI.Muted);','UI.Glyph(panel,"album",0,565,100,UI.Ink);')
s=s.replace('UI.Stars(panel,stars,0,285,185);','UI.Stars(panel,stars,0,210,170);')
s=s.replace('UI.Inset(panel,"Coin reward",0,20,690,150);','CosyArt.Image(panel,"Coin reward","CosyTeal",0,-20,710,140);')
s=s.replace('UI.Coin(panel,-220,20,105);','UI.Coin(panel,-235,-20,115);')
s=s.replace('"+"+reward+" Coins",60,20,500,125,70,UI.Ink','"+"+reward+" Coins",70,-20,500,125,65,UI.Cream')
s=s.replace('UI.Panel(panel,"Album reward",0,-220,760,220,UI.Mint);','UI.Panel(panel,"Album reward",0,-245,710,180,UI.Mint);')
s=s.replace('UI.Glyph(panel,"album",-280,-220,90,UI.Green);','UI.Glyph(panel,"album",-275,-245,78,UI.Ink);')
s=s.replace('newDesigns>0?newDesigns+" new sock"+(newDesigns==1?"":"s")+" found!":"A lovely tidy-up!",85,-184,540,70,45','"Album progress",60,-216,500,65,43')
s=s.replace('85,-256,540,64,34,UI.Muted','60,-280,500,60,34,UI.Ink')
s=s.replace('level==100?"Back to Map":"Next",0,-500,740,155','level==100?"Back to Map":"Next level",0,-500,710,160')
s=s.replace('UI.Label(panel,"Lovely matching!",0,height*.5f-105,820,135,77,UI.Ink);','UI.Label(panel,"Lovely\\nmatching!",0,405,740,200,80,UI.Ink);')
p.write_text(s)
