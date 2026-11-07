from pathlib import Path
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text();s=s.replace('                float y=item.anchoredPosition.y;','''                float y=item.anchoredPosition.y;
                // A shadow follows its face's anchoring, even when its depth
                // crosses the threshold between a fixed and pinned element.
                if(item.name.EndsWith(" shadow"))
                {
                    var face=screen.Find(item.name.Substring(0,item.name.Length-7)) as RectTransform;
                    if(face)y=face.anchoredPosition.y;
                }''')
# Loading scene colour stays consistent through compilation/domain reload.
s=s.replace('            if(Application.isPlaying&&loading', '''            if(loading){var image=loading.GetComponent<Image>();if(image)image.color=UI.Cream;}
            var surroundImage=uiCanvas.transform.Find("Surround");if(surroundImage&&surroundImage.TryGetComponent<Image>(out var surroundFace))surroundFace.color=UI.Cream;
            if(Application.isPlaying&&loading''')
p.write_text(s)
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text();s=s.replace('                var titleCorners=new Vector3[4];title.GetWorldCorners(titleCorners);','''                var titleCorners=new Vector3[4];title.GetWorldCorners(titleCorners);
                var titleShadow=GameObject.Find("Title ribbon shadow").GetComponent<RectTransform>();
                if(Mathf.Abs(title.anchoredPosition.y-titleShadow.anchoredPosition.y-14)>1)throw new Exception("Title shadow detached on level "+level);''')
p.write_text(s)
