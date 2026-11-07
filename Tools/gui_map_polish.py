from pathlib import Path
p=Path('Assets/Scripts/Runtime/LevelMapView.cs');s=p.read_text().replace('Step=244,Height=24550','Step=260,Height=26150').replace('public Image shadow,face,halo,door;','public Image shadow,face,halo,door;public Text milestone;')
a=s.index('            row.halo=');b=s.index('            row.stars=',a)
s=s[:a]+'''            row.halo=UI.Circle(row.root,"Current halo",0,0,245,UI.Gold);
            row.shadow=UI.Circle(row.root,"Level depth",0,-12,184,UI.Green);
            row.face=UI.Circle(row.root,"Level",0,0,184,UI.Pink);row.face.raycastTarget=true;
            row.door=UI.Circle(row.root,"Soft centre",0,5,161,UI.Pink);
            var highlight=UI.Box(row.root,"Clay highlight",0,64,82,9,new Color(1,1,1,.32f));
            row.button=row.face.gameObject.AddComponent<Button>();row.button.targetGraphic=row.face;
            row.face.gameObject.AddComponent<ButtonMotion>();
            var colors=row.button.colors;colors.disabledColor=Color.white;colors.selectedColor=Color.white;row.button.colors=colors;
            row.button.onClick.AddListener(()=>{SockGame.Instance?.ButtonSound();play(row.level);});
            row.number=UI.Label(row.root,"1",0,1,140,110,75,Color.white);row.number.gameObject.AddComponent<CenteredGlyphs>();
            var locked=UI.Rect(row.root,"Lock",65,65,53,53);row.locked=locked.gameObject;
            UI.Circle(locked,"Lock chip",0,0,53,UI.Cream);UI.Glyph(locked,"lock",0,0,32,UI.Muted);
            row.milestone=UI.Label(row.root,"",0,140,600,44,29,UI.Muted);
'''+s[b:]
a=s.index('                Color color=');b=s.index('\n            }',a)
s=s[:a]+'''                bool active=level==current;
                Color color=locked?UI.Hex("9FC6B7"):active?UI.Pink:UI.Green;
                row.face.color=color;row.shadow.color=Color.Lerp(color,UI.Ink,.22f);
                row.door.color=Color.Lerp(color,Color.white,.13f);
                row.button.interactable=!locked||unlockAll;row.halo.color=UI.Gold;row.halo.gameObject.SetActive(active);
                row.number.text=level.ToString();row.number.color=locked?UI.Ink:Color.white;
                row.number.rectTransform.anchoredPosition=new Vector2(0,1);row.locked.SetActive(locked);
                row.milestone.text=level%10==1?"CHAPTER "+((level-1)/10+1):"";
                for(int star=0;star<3;star++){row.stars[star].gameObject.SetActive(!locked&&save.stars[index]>0);row.stars[star].color=star<save.stars[index]?UI.Gold:UI.Line;}
'''+s[b:]
s=s.replace('readonly float[] xs={300,0,-300,-40,80,-150,150};','readonly float[] xs={230,-90,-300,-80,240,300,20,-270};')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/CandyPathGraphic.cs');s=p.read_text().replace('Strip(vh,12,UI.Hex("94B8A6"));Strip(vh,5,UI.Hex("E5E8CC"));','Strip(vh,54,UI.Hex("9CBEAC"));Strip(vh,40,UI.Hex("E8EBCD"));');p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text().replace('"A little mess. A lovely match.",0,510,900,70,40','"Your laundry adventure",0,510,900,70,44');p.write_text(s)
