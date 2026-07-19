using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PawVoyage.UI
{
    /// <summary>
    /// 프로토타입 SVG 아이콘을 IMGUI 화면에 그리기 위한 공용 도우미입니다.
    /// </summary>
    public static class UiIconDrawer
    {
        public const string FarmerHealth = "Status/icon_status_health.svg";
        public const string BarnHealth = "Status/icon_status_barn.svg";
        public const string FarmCoin = "Currency/icon_currency_coin.svg";
        public const string Pause = "Actions/icon_action_pause.svg";
        public const string FarmerTool = "Farmer/icon_farmer_tool.svg";
        public const string FarmerAvatar = "Farmer/icon_farmer_avatar.svg";
        public const string CompanionDog = "Companions/icon_companion_dog.svg";
        public const string CompanionCat = "Companions/icon_companion_cat.svg";
        public const string FarmFence = "Farm/icon_farm_fence.svg";
        public const string FarmBarn = "Farm/icon_farm_barn.svg";

        private const string IconAssetRoot = "Assets/_Project/Sprites/UI/Icons/";
        private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

        public static bool Draw(string relativePath, Rect rect, Color tint)
        {
            Sprite sprite = LoadSprite(relativePath);
            if (sprite == null || sprite.texture == null)
            {
                return false;
            }

            Color previousColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, GetTextureCoords(sprite), true);
            GUI.color = previousColor;
            return true;
        }

        public static bool CanLoad(string relativePath)
        {
            return LoadSprite(relativePath) != null;
        }

        public static Sprite GetSprite(string relativePath)
        {
            return LoadSprite(relativePath);
        }

        private static Sprite LoadSprite(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return null;
            }

            if (spriteCache.TryGetValue(relativePath, out Sprite cachedSprite))
            {
                return cachedSprite;
            }

            Sprite sprite = LoadSpriteFromProject(relativePath);
            spriteCache[relativePath] = sprite;
            return sprite;
        }

        private static Rect GetTextureCoords(Sprite sprite)
        {
            Texture texture = sprite.texture;
            Rect textureRect = sprite.textureRect;
            return new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
        }

        private static Sprite LoadSpriteFromProject(string relativePath)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(IconAssetRoot + relativePath);
#else
            string resourcesPath = "UI/Icons/" + relativePath.Replace(".svg", string.Empty);
            return Resources.Load<Sprite>(resourcesPath);
#endif
        }
    }
}
