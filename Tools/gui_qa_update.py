from pathlib import Path
p=Path('Assets/Scripts/Runtime/SmokeTest.cs');s=p.read_text()
s=s.replace('if(label.enabled||!face||!face.sprite.name.StartsWith("navigation")||face.type!=UnityEngine.UI.Image.Type.Simple)\n                    throw new Exception("Navigation must use the complete reference component without duplicate text");','if(!label.enabled||!face||face.type!=UnityEngine.UI.Image.Type.Sliced||string.IsNullOrEmpty(label.text))\n                    throw new Exception("Navigation must use readable live text and a reusable sliced surface");')
s=s.replace('            var game=SockGame.Instance;','''            var game=SockGame.Instance;
            if(QualitySettings.GetQualityLevel()!=0||QualitySettings.antiAliasing!=0||QualitySettings.shadows!=ShadowQuality.Disable)
                throw new Exception("Lowest rendering preset was not applied");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-performanceQA")>=0)
            {yield return Performance(game);Application.Quit(0);yield break;}''')
pos=s.index('        IEnumerator Readability(')
s=s[:pos]+'''        IEnumerator Performance(SockGame game)
        {
            game.save=new SaveData();game.save.reduceMotion=true;game.StartLevel(100,false);
            yield return new WaitForSecondsRealtime(1);
            // Avoid counting warm-up, PNG captures or this audit's allocations.
            System.GC.Collect();long startBytes=System.GC.GetAllocatedBytesForCurrentThread();
            float duration=0,worst=0;int frames=180;
            for(int i=0;i<frames;i++){yield return null;duration+=Time.unscaledDeltaTime;worst=Mathf.Max(worst,Time.unscaledDeltaTime);}
            long allocated=System.GC.GetAllocatedBytesForCurrentThread()-startBytes;
            Debug.Log("GUI PERF: frames="+frames+" meanMs="+(duration/frames*1000).ToString("F2")+" worstMs="+(worst*1000).ToString("F2")+" managedBytesPerFrame="+(allocated/frames));
            foreach(var texture in Resources.FindObjectsOfTypeAll<Texture2D>())
                if(texture.name=="FreshLaundryBackground"||texture.name=="CandySocks")Debug.Log("GUI TEXTURE: "+texture.name+" "+texture.width+"x"+texture.height+" "+texture.format+" bytes="+UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture));
            Debug.Log("GUI PERF COMPLETE: quality="+QualitySettings.names[QualitySettings.GetQualityLevel()]+" graphics="+UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsSortMode.None).Length);
        }
'''+s[pos:]
p.write_text(s)
# Don't draw a redundant opaque fullscreen surface under the illustrated map.
p=Path('Assets/Scripts/Runtime/SockGame.cs');s=p.read_text().replace('currentScreen=name;UI.Wood(screen,save.theme);','currentScreen=name;if(name!="map")UI.Wood(screen,save.theme);');p.write_text(s)
