from pathlib import Path
p=Path('Assets/Editor/BuildTools.cs');s=p.read_text();s=s.replace('            atlas.Add(packed.ToArray());EditorUtility.SetDirty(atlas);','''            atlas.Add(packed.ToArray());
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name="DefaultTexturePlatform",maxTextureSize=1024,textureCompression=TextureImporterCompression.CompressedHQ});
            var atlasTargets=new[]{"Standalone","WebGL","Android","iPhone"};
            var atlasFormats=new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_6x6};
            for(int i=0;i<atlasTargets.Length;i++)atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name=atlasTargets[i],overridden=true,maxTextureSize=1024,format=atlasFormats[i],textureCompression=TextureImporterCompression.CompressedHQ,compressionQuality=75});
            EditorUtility.SetDirty(atlas);''')
s=s.replace('        public static void BrowserTargets(){Web();YouTube();}','        public static void BrowserTargets(){Web();YouTube();}\n        public static void FreshLaundryRelease(){Setup();Windows();Web();YouTube();}')
p.write_text(s)
for name in ['SockPairs','YouTube']:
 p=Path('Assets/WebGLTemplates')/name/'index.html';s=p.read_text();s=s.replace('<h1>SOCK PAIRS</h1>','<div class="washer" aria-hidden="true"><i></i></div><h1>Sock Pairs</h1>')
 s=s.replace('</style>', '''.washer{width:88px;height:94px;margin-bottom:28px;border-radius:20px;background:linear-gradient(145deg,#f8fff8,#8bbca9);box-shadow:inset 2px 3px 3px #fff,0 8px 0 #709b8b,0 15px 24px #234d4a20;position:relative}.washer:before{content:"";position:absolute;left:10px;right:10px;top:10px;height:12px;border-radius:5px;background:#fff9ef}.washer i{position:absolute;width:50px;height:50px;left:19px;top:30px;border:7px solid #477d76;border-radius:50%;background:radial-gradient(circle at 30% 25%,#fff9ef,#9bc9bb)}h1{letter-spacing:-1px;font-weight:900}progress{border-radius:10px;overflow:hidden;border:0;box-shadow:inset 0 2px 3px #234d4a20}progress::-webkit-progress-bar{background:#d6e7dd}progress::-webkit-progress-value{background:linear-gradient(#f18267,#d95442);border-radius:10px}
</style>''')
 p.write_text(s)
