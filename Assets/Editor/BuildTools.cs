using System;
using System.IO;
using SockPairs.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace SockPairs.Editor
{
    public static class BuildTools
    {
        const string ScenePath="Assets/Scenes/Laundry.unity";
        [MenuItem("Sock Pairs/Refresh themed loading screen")]
        public static void RefreshLoadingScreen()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play mode before saving the loading scene.");
            var game=UnityEngine.Object.FindFirstObjectByType<SockGame>();
            if(!game){EditorSceneManager.OpenScene(ScenePath);game=UnityEngine.Object.FindFirstObjectByType<SockGame>();}
            game.EnsureSceneUI();game.RefreshLoadingView();
            var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();if(camera)camera.backgroundColor=UI.Board;
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);EditorSceneManager.SaveScene(game.gameObject.scene);
            AssetDatabase.SaveAssets();
        }
        [MenuItem("Sock Pairs/Play the game")]
        public static void Preview()
        {
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.delayCall+=()=>{EditorApplication.isPlaying=true;};
        }
        [MenuItem("Sock Pairs/1. Configure project")]
        public static void Setup()
        {
            PlayerSettings.companyName="Satura Games";PlayerSettings.productName="Sock Pairs";
            PlayerSettings.bundleVersion="1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.saturagames.sockpairs");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.saturagames.sockpairs");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
            PlayerSettings.colorSpace=ColorSpace.Gamma;
            PlayerSettings.stripEngineCode=true;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL,ManagedStrippingLevel.High);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.High);
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.iOS,ManagedStrippingLevel.High);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode=1;
            PlayerSettings.iOS.targetOSVersionString="15.0";
            PlayerSettings.iOS.requiresFullScreen=false;
            PlayerSettings.WebGL.template="PROJECT:SockPairs";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.dataCaching=true;
            PlayerSettings.WebGL.initialMemorySize=64;
            PlayerSettings.WebGL.maximumMemorySize=256;
            PlayerSettings.WebGL.exceptionSupport=WebGLExceptionSupport.None;
            PlayerSettings.WebGL.powerPreference=WebGLPowerPreference.LowPower;
            QualitySettings.SetQualityLevel(0,true);QualitySettings.antiAliasing=0;QualitySettings.shadows=ShadowQuality.Disable;
            QualitySettings.vSyncCount=0;QualitySettings.globalTextureMipmapLimit=0;
            foreach(var file in Directory.GetFiles("Assets/Resources/Art","*.png"))
            {
                if(file.Contains("reference-sheet"))continue;
                if(file.Contains("Cosy")||file.Contains("ReferenceB")){AssetDatabase.ImportAsset(file.Replace('\\','/'),ImportAssetOptions.ForceUpdate);continue;}
                if(file.Contains("FreshLaundryBackground")||file.Contains("FreshBasket")){AssetDatabase.ImportAsset(file.Replace('\\','/'),ImportAssetOptions.ForceUpdate);continue;}
                if(file.Contains("FabricSocks")||file.Contains("CandyIcons")||file.Contains("CandySurface")||file.Contains("CandyDetails")||file.Contains("CandyAwning")||file.Contains("CandyRibbon")||file.Contains("CandyControls")||file.Contains("CandyNavigation")){AssetDatabase.ImportAsset(file.Replace('\\','/'),ImportAssetOptions.ForceUpdate);continue;}
                var imp=AssetImporter.GetAtPath(file.Replace('\\','/')) as TextureImporter;
                if(imp==null)continue;
                imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;
                imp.mipmapEnabled=false;imp.isReadable=false;imp.alphaIsTransparency=true;
                imp.textureCompression=TextureImporterCompression.Uncompressed;imp.maxTextureSize=file.Contains("CandyBasket")?1024:512;
                imp.filterMode=FilterMode.Bilinear;
                if(file.Contains("round")||file.Contains("button"))imp.spriteBorder=new Vector4(36,36,36,36);
                imp.SaveAndReimport();
            }
            string atlasPath="Assets/Resources/Art/Laundry.spriteatlas";
            var atlas=AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
            if(!atlas)
            {
                atlas=new SpriteAtlas();
                var files=System.Array.FindAll(Directory.GetFiles("Assets/Resources/Art","*.png"),f=>!f.Contains("reference-sheet"));
                var textures=new UnityEngine.Object[files.Length];
                for(int i=0;i<files.Length;i++)textures[i]=AssetDatabase.LoadAssetAtPath<Texture2D>(files[i].Replace('\\','/'));
                atlas.Add(textures);atlas.SetIncludeInBuild(true);
                atlas.SetPackingSettings(new SpriteAtlasPackingSettings{padding=4,enableRotation=false,enableTightPacking=false});
                atlas.SetTextureSettings(new SpriteAtlasTextureSettings{filterMode=FilterMode.Bilinear,generateMipMaps=false,readable=false,sRGB=true});
                atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name="DefaultTexturePlatform",maxTextureSize=2048,textureCompression=TextureImporterCompression.Uncompressed});
                AssetDatabase.CreateAsset(atlas,atlasPath);
            }
            var serializedAtlas=new SerializedObject(atlas);
            serializedAtlas.FindProperty("m_EditorData.packables").ClearArray();serializedAtlas.ApplyModifiedPropertiesWithoutUndo();
            var packed=new System.Collections.Generic.List<UnityEngine.Object>();
            foreach(var file in Directory.GetFiles("Assets/Resources/Art","*.png"))
                if(!file.Contains("ReferenceB")&&!file.Contains("CosyRoom")&&!file.Contains("FreshBasket")&&!file.Contains("FreshLaundryBackground")&&!file.Contains("FabricSocks")&&!file.Contains("CandyIcons")&&!file.Contains("CandySurface")&&!file.Contains("CandyDetails")&&!file.Contains("CandyAwning")&&!file.Contains("CandyRibbon")&&!file.Contains("CandyControls")&&!file.Contains("CandyNavigation"))packed.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(file.Replace('\\','/')));
            atlas.Add(packed.ToArray());
            atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name="DefaultTexturePlatform",maxTextureSize=1024,textureCompression=TextureImporterCompression.CompressedHQ});
            var atlasTargets=new[]{"Standalone","WebGL","Android","iPhone"};
            var atlasFormats=new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_6x6};
            for(int i=0;i<atlasTargets.Length;i++)atlas.SetPlatformSettings(new TextureImporterPlatformSettings{name=atlasTargets[i],overridden=true,maxTextureSize=1024,format=atlasFormats[i],textureCompression=TextureImporterCompression.CompressedHQ,compressionQuality=75});
            EditorUtility.SetDirty(atlas);
            Directory.CreateDirectory("Assets/Scenes");
            // Keep the scene's testing switch when configuration regenerates the bootstrap scene.
            bool testUnlock=false;
            if(File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                var existing=UnityEngine.Object.FindFirstObjectByType<SockGame>();
                if(existing)
                {
                    // Preserve the authored CanvasScaler, scene hierarchy and Inspector edits.
                    existing.EnsureSceneUI();EditorUtility.SetDirty(existing);
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),ScenePath);
                    EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
                    AssetDatabase.SaveAssets();ValidateLevels();
                    Debug.Log("SOCK PAIRS: configured platforms; preserved existing scene and canvas.");
                    return;
                }
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cam=new GameObject("Camera",typeof(Camera),typeof(AudioListener));
            cam.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;cam.GetComponent<Camera>().backgroundColor=UI.Hex("F5EEFF");
            cam.GetComponent<Camera>().orthographic=true;cam.transform.position=new Vector3(0,0,-10);
            var game=new GameObject("SockPairs",typeof(SockGame)).GetComponent<SockGame>();
            game.unlockAllLevelsForTesting=testUnlock;game.EnsureSceneUI();
            EditorUtility.SetDirty(game);
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();ValidateLevels();
            Debug.Log("SOCK PAIRS: project configured for Android ARM64, iOS, WebGL, and Windows preview.");
        }
        [MenuItem("Sock Pairs/2. Validate all 100 levels")]
        public static void ValidateLevels()
        {
            var book=JsonUtility.FromJson<LevelBook>(File.ReadAllText("Assets/Resources/Data/levels.json"));
            foreach(var c in book.levels){string error=Puzzle.Validate(c);if(error!=null)throw new Exception(error);}
            Debug.Log("SOCK PAIRS: all 100 level solutions pass in Unity.");
        }
        [MenuItem("Sock Pairs/Build/Web browser (Brotli)")]
        public static void Web()
        {
            OptimizeWeb();
            PlayerSettings.WebGL.template="PROJECT:SockPairs";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback=true;
            PlayerSettings.WebGL.dataCaching=true;
            Build(BuildTarget.WebGL,"Builds/Web");
        }
        [MenuItem("Sock Pairs/Build/YouTube Playables (host compression)")]
        public static void YouTube()
        {
            OptimizeWeb();
            PlayerSettings.WebGL.template="PROJECT:YouTube";
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback=false;
            PlayerSettings.WebGL.dataCaching=false;
            Build(BuildTarget.WebGL,"Builds/YouTube");
        }
        [MenuItem("Sock Pairs/Build/Android App Bundle")]
        public static void Android()
        {EditorUserBuildSettings.buildAppBundle=true;Build(BuildTarget.Android,"Builds/Android/SockPairs.aab");}
        [MenuItem("Sock Pairs/Build/Android test APK")]
        public static void AndroidApk()
        {EditorUserBuildSettings.buildAppBundle=false;Build(BuildTarget.Android,"Builds/Android/SockPairs.apk");}
        [MenuItem("Sock Pairs/Build/iOS Xcode project")]
        public static void IOS(){Build(BuildTarget.iOS,"Builds/iOS");}
        [MenuItem("Sock Pairs/Build/Windows preview")]
        public static void Windows(){Build(BuildTarget.StandaloneWindows64,"Builds/Windows/SockPairs.exe",true);}
        public static void InspectArt()
        {
            Setup();
            foreach(var name in new[]{"FabricSocks","CandyIcons","CandyDetails","CandyAwning"})
            {
                var t=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/"+name+".png");
                Debug.Log("ART AUDIT "+name+" "+t.width+"x"+t.height+" "+t.format+" memory="+UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(t));
            }
        }
        public static void VisualPreview(){Setup();Windows();}
        [MenuItem("Sock Pairs/Build/Both browser targets")]
        public static void BrowserTargets(){Web();YouTube();}
        public static void FreshLaundryRelease(){Setup();Windows();Web();YouTube();}
        public static void Deliver()
        {
            Setup();
            var failures=new System.Collections.Generic.List<string>();
            Action[] tasks={Windows,Web,YouTube,AndroidApk,Android,IOS};
            foreach(var task in tasks)
            {
                try{task();}catch(Exception e){failures.Add(task.Method.Name+": "+e.Message);Debug.LogError(e);}
            }
            File.WriteAllText("Builds/delivery-status.txt",failures.Count==0?"All six build targets succeeded.":string.Join("\n",failures));
            if(failures.Count>0)throw new Exception("Some builds failed; see Builds/delivery-status.txt");
        }
        static void OptimizeWeb()
        {
            UnityEditor.WebGL.UserBuildSettings.codeOptimization=UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
            PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL,Il2CppCodeGeneration.OptimizeSize);
            PlayerSettings.WebGL.debugSymbolMode=WebGLDebugSymbolMode.Off;
        }
        static void ApplyPortraitBuildSettings(BuildTarget target)
        {
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait=true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            PlayerSettings.allowedAutorotateToLandscapeLeft=false;
            PlayerSettings.allowedAutorotateToLandscapeRight=false;
            PlayerSettings.defaultScreenWidth=540;PlayerSettings.defaultScreenHeight=960;
            PlayerSettings.resizableWindow=target!=BuildTarget.StandaloneWindows64;
            if(target==BuildTarget.StandaloneWindows64)PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            // Web uses a portrait CSS surface inside any host aspect ratio.
            // Do not request a browser/YouTube orientation lock.
        }
        static void Build(BuildTarget target,string path,bool development=false)
        {
            ApplyPortraitBuildSettings(target);
            QualitySettings.SetQualityLevel(0,true);
            PlayerSettings.SplashScreen.show=false;
            ValidateLevels();Directory.CreateDirectory(Path.GetDirectoryName(path));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=path,target=target,
                options=development?BuildOptions.Development:BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/"+target+"-report.txt","Result: "+report.summary.result+"\nBytes: "+report.summary.totalSize+"\nTime: "+report.summary.totalTime);
            Debug.Log("SOCK PAIRS BUILD: "+target+" "+report.summary.totalSize+" bytes");
        }
    }
}
