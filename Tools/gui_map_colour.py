from pathlib import Path
p=Path('Assets/Scripts/Runtime/LevelMapView.cs');s=p.read_text().replace('Step=260,Height=26150','Step=236,Height=23750')
s=s.replace('readonly float[] xs=', 'readonly Color[] nodeColors={UI.Hex("E46550"),UI.Hex("55A8AD"),UI.Hex("A27CCC"),UI.Hex("6CAC7C"),UI.Hex("E5AE43"),UI.Hex("DA7EA4")};\n        readonly float[] xs=')
s=s.replace('Color color=locked?UI.Hex("9FC6B7"):active?UI.Pink:UI.Green;', 'Color color=active?UI.Pink:nodeColors[(level-1)%nodeColors.Length];')
s=s.replace('Color.Lerp(color,Color.white,.13f)','Color.Lerp(color,Color.white,.15f)')
s=s.replace('row.number.color=locked?UI.Ink:Color.white;', 'row.number.color=(level-1)%nodeColors.Length==4&&!active?UI.Ink:Color.white;')
s=s.replace('row.milestone.text=level%10==1?"CHAPTER "+((level-1)/10+1):"";', 'row.milestone.text=active?"PLAY":level%10==1?"CHAPTER "+((level-1)/10+1):"";row.milestone.color=active?UI.Pink:UI.Muted;')
s=s.replace('0,140,600,44,29','0,143,600,48,34')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/CandyPathGraphic.cs');s=p.read_text().replace('Strip(vh,54,UI.Hex("9CBEAC"));Strip(vh,40,UI.Hex("E8EBCD"));','Strip(vh,68,UI.Hex("D9B893"));Strip(vh,54,UI.Hex("FFF0CD"));Strip(vh,8,UI.Hex("EBCD9C"));');p.write_text(s)
p=Path('Assets/Scripts/Runtime/UI.cs');s=p.read_text().replace('face.sprite=Clay();face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=1.2f;', '''face.sprite=Clay();face.type=Image.Type.Sliced;face.pixelsPerUnitMultiplier=1.2f;
            if(iconOnly)
            {
                if(!pill)pill=Resources.Load<Sprite>("Art/pill");
                face.sprite=pill;face.type=Image.Type.Simple;face.preserveAspect=true;
                var depth=Circle(r,"Round button depth",0,-5,Mathf.Min(w,h),Color.Lerp(color,Ink,.25f));depth.transform.SetAsFirstSibling();
            }''');p.write_text(s)
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text().replace('UI.Panel(screen,"Cream play surface",0,0,1030,1840,UI.Cream);','UI.Panel(screen,"Cream play surface",0,-185,1030,1490,UI.Cream);')
# For later boards, keep the top row and selected/basket areas on cream too.
s=s.replace('            bool early=c.basket==0;','''            bool early=c.basket==0;
            var mat=screen.Find("Cream play surface") as RectTransform;
            var matShadow=screen.Find("Cream play surface shadow") as RectTransform;
            if(!early){mat.anchoredPosition=new Vector2(0,-125);mat.sizeDelta=new Vector2(1030,1610);matShadow.anchoredPosition=new Vector2(0,-132);matShadow.sizeDelta=mat.sizeDelta;}''')
p.write_text(s)
