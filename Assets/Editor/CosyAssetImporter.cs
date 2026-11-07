using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace SockPairs.Editor
{
    public sealed class CosyAssetImporter:AssetPostprocessor
    {
        [Serializable] class Spec { public string name;public float x,y,width,height;public float[] border; }
        [Serializable] class Specs { public Spec[] assets; }
        public override uint GetVersion()=>2;
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Resources/Art/Cosy")||!assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;
            string name=Path.GetFileNameWithoutExtension(assetPath);bool room=name=="CosyRoom";
            importer.isReadable=false;importer.mipmapEnabled=false;importer.alphaIsTransparency=!room;
            importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
            int size=room?1024:name.Contains("Node")||name=="CosyGold"||name=="CosyCurrent"||name=="CosyStar"||name=="CosyVine"||name=="CosyTowels"?256:512;
            importer.maxTextureSize=size;importer.textureType=room?TextureImporterType.Default:TextureImporterType.Sprite;
            importer.npotScale=room?TextureImporterNPOTScale.ToNearest:TextureImporterNPOTScale.None;
            if(room)importer.alphaSource=TextureImporterAlphaSource.None;
            else
            {
                var specs=JsonUtility.FromJson<Specs>(File.ReadAllText("Assets/Editor/CosyAssetSpecs.json"));
                var spec=Array.Find(specs.assets,s=>s.name==name);
                importer.spriteImportMode=SpriteImportMode.Multiple;importer.spritePixelsPerUnit=100;
                importer.GetSourceTextureWidthAndHeight(out int sourceWidth,out int sourceHeight);
                float borderScale=Mathf.Min(1,size/(float)Mathf.Max(sourceWidth,sourceHeight));
                importer.spritesheet=new[]{new SpriteMetaData{name=name,rect=new Rect(spec.x,spec.y,spec.width,spec.height),pivot=new Vector2(.5f,.5f),alignment=9,border=new Vector4(spec.border[0],spec.border[1],spec.border[2],spec.border[3])*borderScale}};
            }
            string[] targets={"Standalone","WebGL","Android","iPhone"};
            var formats=room?new[]{TextureImporterFormat.DXT1,TextureImporterFormat.DXT1,TextureImporterFormat.ETC2_RGB4,TextureImporterFormat.ASTC_6x6}:new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_6x6};
            for(int n=0;n<targets.Length;n++)importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=targets[n],overridden=true,maxTextureSize=size,format=formats[n],compressionQuality=75});
        }
    }
}
