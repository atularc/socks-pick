from pathlib import Path
p=Path('Assets/Scripts/Runtime/SockGame.cs')
s=p.read_text(encoding='utf-8-sig')
def between(start,end,replacement):
 global s
 a=s.index(start);b=s.index(end,a);s=s[:a]+replacement+'\n'+s[b:]
s=s.replace('Application.targetFrameRate=60;QualitySettings.vSyncCount=0;', 'QualitySettings.SetQualityLevel(0,true);\n            Application.targetFrameRate=60;QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=0;QualitySettings.shadows=ShadowQuality.Disable;')
s=s.replace('lastCanvasScale=-1;', 'lastCanvasScale=-1;\n        int displayedSeconds=-1,newDesigns;')
s=s.replace('if(timeLabel)timeLabel.text=TimeText(elapsed);', 'if(timeLabel&&(int)elapsed!=displayedSeconds){displayedSeconds=(int)elapsed;timeLabel.text=TimeText(elapsed);}')
s=s.replace('foreach(var s in puzzle.Socks.Where(s=>s.line&&s.place==Place.Pile))\n                        if(views.TryGetValue(s.id,out var r)&&r)', 'foreach(var s in puzzle.Socks)\n                        if(s.line&&s.place==Place.Pile&&views.TryGetValue(s.id,out var r)&&r)')
s=s.replace('1460+extra*2','1120+extra*2')
s=s.replace('UI.Hex("F5EEFF")','UI.Cream')
between('        void Coins(', '        public void Home()', '''        void Coins(Transform parent,float x=255,float y=850)
        {
            UI.Inset(parent,"Coin counter",x,y,235,95);UI.Coin(parent,x-73,y,65);
            coinLabel=UI.Label(parent,save.coins.ToString(),x+32,y,145,82,45);
        }
        void MapHud()
        {
            var hud=UI.Rect(screen,"Map HUD",0,0,1080,1920);
            UI.Box(hud,"HUD strip",0,880,1080,160,UI.Cream,false);
            UI.Label(hud,currentScreen=="shop"?"Laundry shop":"Sock Pairs",-225,850,540,100,74,UI.Ink,TextAnchor.MiddleLeft);
            Coins(hud);UI.Button(hud,"Settings",455,850,110,100,UI.Purple,Settings,40);
        }
''')
s=s.replace('var backdrop=UI.Sprite(screen,"Soft Candy landscape",Resources.Load<Sprite>("Art/CandyMapBackground"),0,0,1080,1920);backdrop.preserveAspect=false;backdrop.gameObject.AddComponent<FullBleed>();','UI.RoomBackground(screen);')
s=s.replace('MapHud();\n            var map=', 'MapHud();\n            UI.Label(screen,"A little mess. A lovely match.",0,510,900,70,40,UI.Ink);\n            var map=')
s=s.replace('UI.Hex("EDE0FF")','UI.Cream')
between('        void MapNavigationButton(', '        void Shop()', '''        void MapNavigationButton(string title,float x,float width,Color color,int icon,Action action)
        {
            var button=UI.Button(screen,title,x,-825,width,216,color,action,48);
            var label=button.GetComponentInChildren<Text>();label.enabled=true;
            label.text=title=="PLAY"?(save.active!=null?"Continue":"Play"):title;
            label.rectTransform.anchoredPosition=new Vector2(0,-46);
            label.rectTransform.sizeDelta=new Vector2(width-24,75);
            UI.Glyph(button.transform,title=="PLAY"?"play":title=="Album"?"album":"shop",0,43,65,Color.white);
            if(title=="PLAY")
                UI.Label(screen,"DAY "+(save.active!=null?save.active.level:save.unlocked).ToString("00"),x,-675,width,42,27,UI.Muted);
        }
''')
s=s.replace('var awning=UI.Sprite(offers,"Shop awning",UI.Decoration("Awning"),0,690,1080,220);SafeEdgeOffset.Pin(awning.rectTransform,1);', 'var awning=UI.Label(offers,"A little help with the laundry",0,690,950,100,49);SafeEdgeOffset.Pin(awning.rectTransform,1);')
s=s.replace('UI.Sprite(row,"Coin stack",UI.Detail(0),-355,y,265,265)', 'UI.Coin(row,-355,y,150)')
s=s.replace('UI.Sprite(row,"Shuffle arrows",UI.Icon(1),-335,y,230,230)', 'UI.Glyph(row,"restart",-335,y,160,UI.Purple)')
s=s.replace('UI.Label(screen,"Album",0,850,520,130,96);', 'UI.Label(screen,"Sock album",-15,850,560,130,76);\n            UI.Label(screen,"Little finds from every laundry day",0,722,960,70,36,UI.Muted);')
s=s.replace('UI.Hex("82719C")','UI.Muted')
s=s.replace('helpers=0;hintId=-1;', 'helpers=0;newDesigns=0;displayedSeconds=-1;hintId=-1;')
# Common HUD reused by early and later levels: no continuous timer redraw.
a='''            UI.Panel(screen,"Level pill",-375,850,235,90,UI.Purple);'''
b='''            progressFill=null;'''
start=s.index(a,s.index('        void BuildPlay()'))
end=s.index(b,start)+len(b)
s=s[:start]+'''            BuildPlayHeader(c.level,c.pairs,740,100);
'''+s[end:]
start=s.index(a,s.index('        void BuildEarlyPlay()'))
end=s.index('            basketFrame=',start)
s=s[:start]+'''            BuildPlayHeader(puzzle.layout.config.level,puzzle.layout.config.pairs,660,125);
            timeLabel=null;basketLabel=null;
'''+s[end:]
# insert shared HUD before BuildEarlyPlay
idx=s.index('        void BuildEarlyPlay()')
s=s[:idx]+'''        void BuildPlayHeader(int level,int pairs,float titleY,float titleHeight)
        {
            UI.Panel(screen,"Level pill",-375,850,235,90,UI.Purple);
            UI.Label(screen,"Day "+level,-375,850,220,80,43,Color.white);
            UI.Inset(screen,"Pairs pill",0,850,455,90);
            progressLabel=UI.Label(screen,"Pairs  "+puzzle.matched+" / "+pairs,0,850,435,80,44);
            UI.Button(screen,"II",430,850,110,100,UI.Purple,Pause,38);
            UI.Panel(screen,"Title ribbon",0,titleY,940,titleHeight,UI.Mint);
            statusLabel=UI.Label(screen,"Find a matching pair",0,titleY,900,titleHeight-10,earlyTitleSize(titleHeight),UI.Ink);
            UI.Box(screen,"Progress track",0,785,920,9,UI.Line);
            progressFill=UI.Box(screen,"Pair progress",-460,785,0,9,UI.Green);
            progressFill.rectTransform.pivot=new Vector2(0,.5f);
            progressFill.rectTransform.sizeDelta=new Vector2(920f*puzzle.matched/pairs,9);
        }
        static int earlyTitleSize(float h){return h>100?60:48;}
'''+s[idx:]
s=s.replace('UI.Hex("B29AD8")','UI.Line').replace('UI.Hex("A8C7C1")','UI.Mint')
s=s.replace('UI.Hex("9974C4")','UI.Muted')
s=s.replace('visual.selection=UI.Circle(r,"Selection glow",0,0,285,new Color(1,1,1,.34f)).gameObject;', '''var selection=UI.Rect(r,"Selection glow",0,0,205,205);visual.selection=selection.gameObject;
                    UI.Circle(selection,"Selected fill",0,0,205,UI.Mint);
                    UI.Glyph(selection,"halo",0,0,205,UI.Green);''')
s=s.replace('v.selection.GetComponent<RectTransform>().sizeDelta=new Vector2(140*scale,185*scale);', 'v.selection.transform.localScale=Vector3.one*(160*scale/180f);')
s=s.replace('drawer.Add(design);save.collection[design]=true;', 'drawer.Add(design);if(!save.collection[design])newDesigns++;save.collection[design]=true;')
s=s.replace('DrawSocks(old);UpdateStatus();', '''DrawSocks(old);UpdateStatus();
            if(result==TapResult.Selected&&previous>=0&&previous!=id&&oldMatched==puzzle.matched&&puzzle.layout.config.basket==0)
            {statusLabel.text="Different pair. Try again!";statusLabel.color=UI.Pink;}''')
between('        void UpdateStatus()', '        IEnumerator MatchEffect(', '''        void UpdateStatus()
        {
            if(!progressLabel)return;
            var c=puzzle.layout.config;progressLabel.text="Pairs  "+puzzle.matched+" / "+c.pairs;
            if(progressFill)progressFill.rectTransform.sizeDelta=new Vector2(920f*puzzle.matched/c.pairs,9);
            if(pickLabel)pickLabel.gameObject.SetActive(puzzle.selected<0);
            if(basketLabel)basketLabel.text="Waiting socks  "+puzzle.basket.Count+" / "+puzzle.capacity;
            bool full=puzzle.capacity>0&&puzzle.basket.Count>=puzzle.capacity-1;
            statusLabel.text=full?"Basket almost full!":puzzle.selected>=0?"Now find its partner":"Find a matching pair";
            statusLabel.color=full?UI.Pink:UI.Ink;
        }
''')
s=s.replace('UI.Hex("BC267E"):UI.Hex("7950B7")','UI.Ink:UI.Ink')
s=s.replace('UI.Stroke(priceLabel,color==UI.Pink?UI.Ink:UI.Ink,1.4f);','')
s=s.replace('for(int i=0;i<count;i++)\n            {\n                var r=UI.Box(fxLayer', 'count=Mathf.Min(count,4);\n            for(int i=0;i<count;i++)\n            {\n                var r=UI.Box(fxLayer')
s=s.replace('i<48;i++','i<20;i++')
s=s.replace('UI.Sprite(panel,"Coin",UI.Detail(1),-220,20,170,170);','UI.Coin(panel,-220,20,105);')
s=s.replace('UI.Hex("66239A")','UI.Ink')
s=s.replace('UI.Stars(panel,stars,0,285,260);','UI.Label(panel,"DAY "+level.ToString("00")+" COMPLETE",0,475,800,70,36,UI.Muted);\n            UI.Stars(panel,stars,0,285,185);')
s=s.replace('var next=UI.Button(panel,level==100?', '''UI.Panel(panel,"Album reward",0,-220,760,220,UI.Mint);
            UI.Glyph(panel,"album",-280,-220,90,UI.Green);
            UI.Label(panel,newDesigns>0?newDesigns+" new sock"+(newDesigns==1?"":"s")+" found!":"A lovely tidy-up!",85,-184,540,70,45);
            UI.Label(panel,save.collection.Count(x=>x)+" of 24 designs collected",85,-256,540,64,34,UI.Muted);
            var next=UI.Button(panel,level==100?''')
s=s.replace('if(level==100)UI.Label(panel,"All 100 levels complete!",0,-230,780,100,56,UI.Ink);','if(level==100)UI.Label(panel,"All 100 days complete!",0,-375,780,70,42,UI.Ink);')
s=s.replace('UI.Hex("EBDCFF")','UI.Mint')
s=s.replace('if(win){UI.Clear(screen);var bg=UI.Sprite(screen,"Win background",Resources.Load<Sprite>("Art/CandyWinBackground"),0,0,1080,1920);bg.preserveAspect=false;bg.gameObject.AddComponent<FullBleed>();}', 'if(win){UI.Clear(screen);UI.RoomBackground(screen);}')
s=s.replace('new Color(.27f,.19f,.41f,.58f)','new Color(.08f,.20f,.18f,.58f)')
s=s.replace('var panel=win?UI.Rect(modal,"Win",0,0,900,height):UI.Panel(modal,"Panel",0,0,900,height,UI.Cream).rectTransform;', 'var panel=UI.Panel(modal,win?"Win":"Panel",0,0,900,height,UI.Cream).rectTransform;')
s=s.replace('var banner=UI.Sprite(panel,"Level Complete ribbon",UI.Decoration("Ribbon"),0,height*.5f-100,1010,300);SafeEdgeOffset.Pin(banner.rectTransform,1);','UI.Label(panel,"Lovely matching!",0,height*.5f-105,820,135,77,UI.Ink);')
s=s.replace('UI.Label(panel,title,0,height*.5f-105,790,145,title.Length>18?50:title.Length>12?62:94,UI.Ink);', 'UI.Label(panel,title,0,height*.5f-125,750,130,title.Length>18?44:title.Length>12?52:72,UI.Ink);')
s=s.replace('390,height*.5f-45,115,115','390,height*.5f-45,100,100')
s=s.replace('UI.Label(panel,"Reduced motion"','UI.Label(panel,"Reduced motion"')
s=s.replace('420,90,54,UI.Ink,TextAnchor.MiddleLeft','420,90,46,UI.Ink,TextAnchor.MiddleLeft')
p.write_text(s,encoding='utf-8')
# Shared vector glyphs; no textures or fonts for icons.
p=Path('Assets/Scripts/Runtime/CandyGlyph.cs');s=p.read_text();s=s.replace('vh.Clear();mesh=vh;offset=new Vector2(0,-.025f);shade=new Color(.27f,.08f,.4f,.16f);Draw();offset=Vector2.zero;shade=color;Draw();','vh.Clear();mesh=vh;offset=Vector2.zero;shade=color;Draw();')
s=s.replace('case "lock":','''case "spark":Poly(new Vector2(0,.45f),new Vector2(.13f,.13f),new Vector2(.45f,0),new Vector2(.13f,-.13f),new Vector2(0,-.45f),new Vector2(-.13f,-.13f),new Vector2(-.45f,0),new Vector2(-.13f,.13f));break;
                case "settings":Arc(.27f,0,360,.12f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.29f,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.43f,.13f);}break;
                case "album":Rect(-.42f,-.35f,.37f,.70f);Rect(.05f,-.35f,.37f,.70f);break;
                case "shop":Rect(-.35f,-.35f,.7f,.5f);Line(new Vector2(-.22f,.1f),new Vector2(-.22f,.37f),.1f);Line(new Vector2(.22f,.1f),new Vector2(.22f,.37f),.1f);Line(new Vector2(-.22f,.37f),new Vector2(.22f,.37f),.1f);break;
                case "lock":''');p.write_text(s)
p=Path('Assets/Scripts/Runtime/ButtonMotion.cs');s=p.read_text();s=s.replace('void Animate(float target,float duration){','void Animate(float target,float duration){if(SockGame.Instance&&SockGame.Instance.save!=null&&SockGame.Instance.save.reduceMotion){transform.localScale=Vector3.one;return;}');p.write_text(s)
