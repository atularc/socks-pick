from pathlib import Path
p=Path('Assets/Scripts/Runtime/SockGame.cs')
s=p.read_text()
s=s.replace('"HUD strip",0,880,1080,160,UI.Cream','"HUD strip",0,880,1080,160,UI.Ink')
s=s.replace('540,100,74,UI.Ink,TextAnchor.MiddleLeft','540,100,74,UI.Cream,TextAnchor.MiddleLeft')
s=s.replace('new Color(1,.98f,.94f,.88f)','UI.Trail')
s=s.replace('"Your laundry adventure"','"Laundry trail"')
s=s.replace('UI.Label(screen,"Laundry trail",0,510,900,70,44,UI.Ink);','UI.Label(screen,"Laundry trail",0,525,900,55,44,UI.Ink);\n            UI.Label(screen,"Day "+next+" of 100  |  Swipe up to explore",0,483,850,40,28,UI.Ink).name="Map guidance";')
s=s.replace('"Map title card shadow","Laundry trail"','"Map title card shadow","Laundry trail","Map guidance"')
s=s.replace('510+extra-(name.EndsWith("shadow")?14:0)','510+extra+(name=="Laundry trail"?15:name=="Map guidance"?-27:0)-(name.EndsWith("shadow")?14:0)')
s=s.replace('"Map navigation",0,-825,1080,270,UI.Cream','"Map navigation",0,-825,1080,270,UI.Ink')
s=s.replace('label.text=title=="PLAY"?(save.active!=null?"Continue":"Play"):title;','label.text=title=="PLAY"?(save.active!=null?"Continue":"Play day "+save.unlocked):title;')
s=s.replace('"DAY "+(save.active!=null?save.active.level:save.unlocked).ToString("00")','save.active!=null?"DAY "+save.active.level.ToString("00"):"NEXT LEVEL"')
s=s.replace('"Cream play surface",0,-185,1030,1490,UI.Cream','"Cream play surface",0,-185,1030,1490,UI.Board')
s=s.replace('DrawDrawer();\n            sockLayer=UI.ClippedLayer(screen,"Socks",590','DrawDrawer();UI.BasketFront(basketFrame);\n            sockLayer=UI.ClippedLayer(screen,"Socks",590')
s=s.replace('UI.BasketImageRect(basketFrame).width*.07f','UI.BasketImageRect(basketFrame).width*.16f')
s=s.replace('Vector2 slot=UI.BasketSlot(basketFrame,i);','Vector2 slot=UI.CompletedPairSlot(basketFrame,i,visible);')
s=s.replace('UI.BasketSlot(basketFrame,Mathf.Min(4,matchedIndex))','UI.CompletedPairSlot(basketFrame,Mathf.Min(4,matchedIndex),Mathf.Min(5,matchedIndex+1))')
s=s.replace('float row=columns==4?(puzzle.layout.config.pairs<=6?350:275):220;\n                return new Vector2((s.slot%columns-(columns-1)*.5f)*step,BoardY(435-s.slot/columns*row));', '''int rows=Mathf.CeilToInt(puzzle.layout.config.pairs*2f/columns);
                float row=columns==4?260:205;
                // Preserve a compact, evenly spaced grid on tall screens.
                float center=rows<=3?70:20;
                float top=center+(rows-1)*row*.5f;
                return new Vector2((s.slot%columns-(columns-1)*.5f)*step,top-s.slot/columns*row+(BoardY(200)-200)*.35f);''')
s=s.replace('scale=1.55f;','scale=1.38f;')
s=s.replace('win?Color.clear:new Color(.08f,.20f,.18f,.58f)','new Color(.06f,.15f,.13f,win?.50f:.65f)')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/LevelMapView.cs');s=p.read_text()
s=s.replace('readonly float[] xs={230,-90,-300,-80,240,300,20,-270};','readonly float[] xs={0,-235,0,235};')
s=s.replace('245,UI.Gold','250,UI.Gold')
s=s.replace('0,143,600,48,34,UI.Muted','0,153,420,48,32,UI.Cream')
s=s.replace('Color color=active?UI.Pink:nodeColors[(level-1)%nodeColors.Length];','Color color=active?UI.Pink:locked?UI.Hex("86AFA3"):UI.Gold;')
s=s.replace('row.number.color=(level-1)%nodeColors.Length==4&&!active?UI.Ink:Color.white;','row.number.color=active?Color.white:UI.Ink;')
s=s.replace('active?"PLAY":level%10==1?"CHAPTER "+((level-1)/10+1):"";row.milestone.color=active?UI.Pink:UI.Muted;','active?"YOU ARE HERE":"";row.milestone.color=UI.Cream;')
# Start level one above the footer, not touching the rounded stage edge.
s=s.replace('140+index*Step','190+index*Step')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/CandyPathGraphic.cs');s=p.read_text()
a=s.index('                float t2=');b=s.index('                var n=',a)
s=s[:a]+'''                float smooth=t*t*(3-2*t);
                var p=new Vector2(Mathf.Lerp(previous,0,smooth),Mathf.Lerp(-step,0,t));
                var tangent=new Vector2(-previous*6*t*(1-t),step).normalized;
'''+s[b:]
s=s.replace('Strip(vh,68,UI.Hex("D9B893"));Strip(vh,54,UI.Hex("FFF0CD"));Strip(vh,8,UI.Hex("EBCD9C"));','Strip(vh,70,UI.Hex("345E53"));Strip(vh,52,UI.Hex("D3B57C"));Strip(vh,32,UI.Hex("F1D69C"));')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text().replace('UI.BasketSlot(basket,0))','UI.CompletedPairSlot(basket,0,1))');p.write_text(s)
