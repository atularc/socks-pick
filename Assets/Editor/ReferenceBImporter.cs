using UnityEditor;
using UnityEngine;
namespace SockPairs.Editor
{
    public sealed class ReferenceBImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.EndsWith("/ReferenceB.png"))return;
            var i=(TextureImporter)assetImporter;i.textureType=TextureImporterType.Default;i.maxTextureSize=2048;
            i.npotScale=TextureImporterNPOTScale.None;i.mipmapEnabled=false;i.isReadable=false;i.alphaSource=TextureImporterAlphaSource.None;
            i.filterMode=FilterMode.Bilinear;i.wrapMode=TextureWrapMode.Clamp;i.textureCompression=TextureImporterCompression.CompressedHQ;
            string[] names={"Standalone","WebGL","Android","iPhone"};
            var formats=new[]{TextureImporterFormat.DXT1,TextureImporterFormat.DXT1,TextureImporterFormat.ETC2_RGB4,TextureImporterFormat.ASTC_4x4};
            for(int n=0;n<names.Length;n++)i.SetPlatformTextureSettings(new TextureImporterPlatformSettings{name=names[n],overridden=true,maxTextureSize=2048,format=formats[n],compressionQuality=100});
        }
    }
}
