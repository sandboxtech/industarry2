
using UnityEditor;
using UnityEngine;

namespace W
{
    public class Importer : AssetPostprocessor
    {
        void OnPreprocessTexture() {
            TextureImporter importer = (TextureImporter)assetImporter;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = UnityEngine.SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            settings.aniso = 0;
            settings.filterMode = UnityEngine.FilterMode.Point;
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new UnityEngine.Vector2(0, 0);
            settings.spriteGenerateFallbackPhysicsShape = false;
            settings.wrapMode = UnityEngine.TextureWrapMode.Repeat;
            importer.SetTextureSettings(settings);

            importer.textureType = TextureImporterType.Sprite;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = UnityEngine.FilterMode.Point;
            importer.spritePixelsPerUnit = 16;

        }

    }
}
