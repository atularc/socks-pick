from pathlib import Path
p=Path('Assets/Scripts/Runtime/LevelMapView.cs');s=p.read_text()
s=s.replace('Step=204,Height=20550','Step=244,Height=24550').replace('public Image shadow,face,halo;','public Image shadow,face,halo,door;')
s=s.replace('0,45,1080,1460','0,-125,1080,1120').replace('(Height-1460)','(Height-1120)').replace('(next-1)*Step-600','(next-1)*Step-340')
s=s.replace('row.shadow=UI.Circle(row.root,"Level shadow",0,-12,177,UI.Green);\n            row.face=UI.Sprite(row.root,"Level",UI.Detail(3),0,0,250,250);row.face.raycastTarget=true;', '''row.shadow=UI.Box(row.root,"Level shadow",0,-8,190,190,UI.Line);
            row.face=UI.Box(row.root,"Level",0,0,190,190,UI.Mint);row.face.raycastTarget=true;
            UI.Box(row.root,"Machine controls",0,62,162,32,UI.Cream);
            UI.Circle(row.root,"Dial",54,62,16,UI.Ink);
            UI.Circle(row.root,"Door rim",0,-24,133,UI.Ink);
            row.door=UI.Circle(row.root,"Door",0,-24,112,UI.Cream);''')
s=s.replace('0,0,170,100,74,Color.white','0,-24,110,100,60,UI.Ink').replace(';UI.Stroke(row.number,UI.Hex("794CA9"),3)','')
s=s.replace('"Lock",0,0,70,80','"Lock",0,-24,70,80')
a=s.index('            UI.Circle(locked,"Arch"');b=s.index('            row.stars=',a)
s=s[:a]+'            UI.Glyph(locked,"lock",0,0,55,UI.Muted);\n'+s[b:]
s=s.replace('(i-1)*46,-112,39,39','(i-1)*35,-113,27,27')
a=s.index('                Color color=');b=s.index('\n            }',a)
s=s[:a]+'''                Color color=locked?UI.Line:level==current?UI.Pink:UI.Green;
                row.face.color=color;row.shadow.color=Color.Lerp(color,UI.Ink,.25f);
                row.door.color=locked?UI.Mint:UI.Cream;
                row.button.interactable=!locked||unlockAll;row.halo.color=new Color(1,.98f,.91f,.55f);row.halo.gameObject.SetActive(level==current);
                row.number.text=locked?"":level.ToString();row.number.rectTransform.anchoredPosition=new Vector2(0,-24);row.locked.SetActive(locked);
                for(int star=0;star<3;star++){row.stars[star].gameObject.SetActive(!locked&&save.stars[index]>0);row.stars[star].color=star<save.stars[index]?UI.Gold:UI.Line;}
'''+s[b:]
p.write_text(s)
p=Path('Assets/Scripts/Runtime/CandyPathGraphic.cs');s=p.read_text().replace('Strip(vh,60,UI.Hex("BA96EC"));Strip(vh,42,UI.Hex("E3CAFF"));','Strip(vh,12,UI.Hex("94B8A6"));Strip(vh,5,UI.Hex("E5E8CC"));').replace('segments=48','segments=20');p.write_text(s)
# Deactivate old, costly atlases and backgrounds without losing source assets.
p=Path('Assets/Editor/ArtImporter.cs');s=p.read_text().replace('return 8','return 9');needle='            i.npotScale='
s=s.replace(needle,'''            if(assetPath.EndsWith("FreshLaundryBackground.png"))
            {
                i.textureType=TextureImporterType.Sprite;i.maxTextureSize=1024;i.alphaSource=TextureImporterAlphaSource.None;
                i.textureCompression=TextureImporterCompression.CompressedHQ;i.npotScale=TextureImporterNPOTScale.ToNearest;
                var targets=new[]{"Standalone","WebGL","Android","iPhone"};
                var formats=new[]{TextureImporterFormat.DXT1,TextureImporterFormat.DXT1,TextureImporterFormat.ETC2_RGB4,TextureImporterFormat.ASTC_6x6};
                for(int p=0;p<targets.Length;p++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=targets[p],overridden=true,maxTextureSize=1024,format=formats[p],compressionQuality=75});
                i.filterMode=FilterMode.Bilinear;return;
            }
'''+needle)
p.write_text(s)
# Lowest preset: keep UI crisp, disable 3D work. No mipmaps exist on UI textures.
p=Path('ProjectSettings/QualitySettings.asset');s=p.read_text().replace('m_CurrentQuality: 5','m_CurrentQuality: 0')
for target in ['Android','Standalone','WebGL','iPhone']:
 import re
 s=re.sub(r'(    '+target+r':) \d+',r'\g<1> 0',s)
s=s.replace('globalTextureMipmapLimit: 1','globalTextureMipmapLimit: 0');p.write_text(s)
# Same bounded web rendering resolution on both distribution targets.
for name in ['SockPairs','YouTube']:
 p=Path('Assets/WebGLTemplates')/name/'index.html';s=p.read_text().replace('#f5eeff','#fff9ef').replace('#591995','#234d4a').replace('#fa38a6','#d95442').replace('#ae7aef','#36826c').replace('#9673c7','#54736d')
 s=s.replace('Math.min(window.devicePixelRatio||1,2)','Math.min(window.devicePixelRatio||1,1.5,720/Math.max(1,canvas.clientWidth))')
 s=s.replace('font-family:Arial,sans-serif','font-family:ui-rounded,system-ui,sans-serif')
 s=s.replace('width:220px;height:12px','width:200px;height:10px')
 p.write_text(s)
p=Path('Assets/Editor/BuildTools.cs');s=p.read_text().replace('QualitySettings.antiAliasing=0;', 'QualitySettings.SetQualityLevel(0,true);QualitySettings.antiAliasing=0;')
s=s.replace('if(file.Contains("reference-sheet"))continue;', 'if(file.Contains("reference-sheet"))continue;\n                if(file.Contains("FreshLaundryBackground")){AssetDatabase.ImportAsset(file.Replace(\'\\\\\',\'/\'),ImportAssetOptions.ForceUpdate);continue;}')
s=s.replace('if(!file.Contains("CandySocks")', 'if(!file.Contains("FreshLaundryBackground")&&!file.Contains("CandySocks")')
s=s.replace('            PlayerSettings.SplashScreen.show=false;', '            QualitySettings.SetQualityLevel(0,true);\n            PlayerSettings.SplashScreen.show=false;')
p.write_text(s)
