using UnityEditor;
using UnityEngine;
namespace SockPairs.Editor
{
    public class ArtImporter:AssetPostprocessor
    {
        public override uint GetVersion(){return 12;}
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Resources/Art/")||!assetPath.EndsWith(".png")||assetPath.Contains("/Cosy")||assetPath.EndsWith("/ReferenceB.png"))return;
            var i=(TextureImporter)assetImporter;
            i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;
            i.isReadable=false;i.mipmapEnabled=false;i.alphaIsTransparency=true;
            i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=512;
            if(assetPath.EndsWith("FabricSocks.png")){i.textureType=TextureImporterType.Default;i.maxTextureSize=1024;}
            if((assetPath.EndsWith("CandyIcons.png")||assetPath.EndsWith("CandyDetails.png"))){i.textureType=TextureImporterType.Default;i.maxTextureSize=1024;}
            if(assetPath.EndsWith("CandyBasket.png"))i.maxTextureSize=1024;
            if(assetPath.EndsWith("FabricSocks.png")||(assetPath.EndsWith("CandyIcons.png")||assetPath.EndsWith("CandyDetails.png")))
            {
                i.textureCompression=TextureImporterCompression.CompressedHQ;i.compressionQuality=100;
                var names=new[]{"Standalone","WebGL","Android","iPhone"};
                var formats=new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_4x4};
                for(int p=0;p<names.Length;p++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                {name=names[p],overridden=true,maxTextureSize=i.maxTextureSize,format=formats[p],compressionQuality=100});
            }
            if(assetPath.Contains("CandySurface")){i.textureType=TextureImporterType.Default;i.maxTextureSize=512;}
            if(assetPath.Contains("CandyDetails")||assetPath.Contains("CandyAwning")||assetPath.Contains("CandyRibbon")||assetPath.Contains("CandyControls")||assetPath.Contains("CandyNavigation")){i.textureType=TextureImporterType.Default;i.maxTextureSize=1024;}
            if(assetPath.EndsWith("FreshBasket.png"))
            {
                i.textureType=TextureImporterType.Default;i.maxTextureSize=512;i.npotScale=TextureImporterNPOTScale.ToNearest;
                i.textureCompression=TextureImporterCompression.CompressedHQ;
                var targets=new[]{"Standalone","WebGL","Android","iPhone"};
                var formats=new[]{TextureImporterFormat.DXT5,TextureImporterFormat.DXT5,TextureImporterFormat.ETC2_RGBA8,TextureImporterFormat.ASTC_6x6};
                for(int p=0;p<targets.Length;p++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=targets[p],overridden=true,maxTextureSize=512,format=formats[p],compressionQuality=75});
                i.filterMode=FilterMode.Bilinear;return;
            }
            if(assetPath.EndsWith("FreshLaundryBackground.png"))
            {
                i.textureType=TextureImporterType.Default;i.maxTextureSize=1024;i.alphaSource=TextureImporterAlphaSource.None;
                i.textureCompression=TextureImporterCompression.CompressedHQ;i.npotScale=TextureImporterNPOTScale.ToNearest;
                var targets=new[]{"Standalone","WebGL","Android","iPhone"};
                var formats=new[]{TextureImporterFormat.DXT1,TextureImporterFormat.DXT1,TextureImporterFormat.ETC2_RGB4,TextureImporterFormat.ASTC_6x6};
                for(int p=0;p<targets.Length;p++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=targets[p],overridden=true,maxTextureSize=1024,format=formats[p],compressionQuality=75});
                i.filterMode=FilterMode.Bilinear;return;
            }
            i.npotScale=(assetPath.Contains("FabricSocks")||assetPath.Contains("CandyIcons")||assetPath.Contains("CandyDetails"))?TextureImporterNPOTScale.ToNearest:TextureImporterNPOTScale.None;i.filterMode=FilterMode.Bilinear;
            if(assetPath.EndsWith("round.png")||assetPath.EndsWith("button.png"))i.spriteBorder=new Vector4(36,36,36,36);
        }
    }
}
