from pathlib import Path
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text()
s=s.replace('GameObject.Find("HUD strip").GetComponent<RectTransform>();\n                var corners','GameObject.Find("Wooden screen frame").GetComponent<RectTransform>();\n                var corners')
s=s.replace('if(Mathf.Abs(corners[2].y-area.yMax)>2)throw new Exception("Map HUD does not meet safe top");','if(corners[2].y>area.yMax+2||area.yMax-corners[2].y>30)throw new Exception("Wood frame does not fit safe top");')
s=s.replace('new[]{"Shop","PLAY","Album"}','new[]{"PLAY","Album"}')
s=s.replace('if(Mathf.Abs(nav.anchoredPosition.y-currentBar.anchoredPosition.y)>.01f)','if(Mathf.Abs(nav.anchoredPosition.x)>.01f||nav.anchoredPosition.y-nav.rect.height*.5f<currentBar.anchoredPosition.y-currentBar.rect.height*.5f)')
s=s.replace('!(face is ClayImage)','(!face.sprite||face.type!=UnityEngine.UI.Image.Type.Sliced)')
s=s.replace('if(buttonRect.rect.height!=216)throw new Exception("Navigation buttons must have equal height");','if(buttonRect.rect.height<132)throw new Exception("Navigation hit area is too small");')
s=s.replace('if(Mathf.Abs(top-bottom)>.01f||top<0)throw new Exception("Navigation vertical padding is uneven");','if(top<24||bottom<24)throw new Exception("Navigation lacks safe padding");')
s=s.replace('            Capture(Path.Combine(output,"map.png"));','''            var primary=GameObject.Find("PLAY").GetComponent<RectTransform>();
            var secondary=GameObject.Find("Album").GetComponent<RectTransform>();
            if(primary.anchoredPosition.y-primary.rect.height*.5f-(secondary.anchoredPosition.y+secondary.rect.height*.5f)<24)
                throw new Exception("Stacked map actions overlap or lack spacing");
            Capture(Path.Combine(output,"map.png"));''')
# Include direct reference-B day-three state, not just first-run and later chapters.
s=s.replace('game.save.unlocked=25;for(int i=0;i<24;i++)','game.save.unlocked=3;game.save.stars[0]=game.save.stars[1]=3;game.save.active=null;game.Home();\n            yield return new WaitForSecondsRealtime(.4f);Capture(Path.Combine(output,"map-reference-b.png"));\n            game.save.unlocked=25;for(int i=0;i<24;i++)')
p.write_text(s)
