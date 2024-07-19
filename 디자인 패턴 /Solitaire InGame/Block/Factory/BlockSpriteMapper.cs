using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

using NGle.Solitaire.Support;

using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class BlockSpriteMapper : MonoBehaviour
    {
        [Serializable]
        public class SpriteGroup
        {
            public SpriteGroup(string name, List<Sprite> sprites)
            {
                this.name = name;
                this.sprites = sprites;
            }
            public string name;

            public List<Sprite> sprites;
        }


        public List<Sprite> mixedSprites { get; private set; }

        private List<Sprite> RegisterSpriteGroup()
        {
            List<Sprite> mixedSprites = new List<Sprite>();

            string[] SpriteGroupNames = { "black", "blue", "brown", "darkbrown", "green", "orange", "pink", "purple", "red", "white", "yellow" };
            
            List<SpriteGroup> spriteGroups = new List<SpriteGroup>();
            int len = ClientData.Instance.gameSetting.GroupCount;

            string path = "Assets/AssetData/Sprites/block_Img/";

            for (int i = 0; i < SpriteGroupNames.Length; i++)
            {
                List<Sprite> sprites = new List<Sprite>();

                for (int j = 0; j < len; j++)
                {
                    string name = SpriteGroupNames[i] +"/" + SpriteGroupNames[i] + "_" + (j + 1).ToString("D2") +".png";
           
                    sprites.Add(AssetManager.Instance.LoadAsset<Sprite>(path + name));
                }

                spriteGroups.Add(new SpriteGroup(SpriteGroupNames[i], sprites));
            }

            GameRandom gameRandom = RunGameRandom.Get("AbsBlockFactory","RegisterSpriteGroup");

            while (spriteGroups.Count != 0)
            {
                for (int i = 0; i < spriteGroups.Count; i++)
                {
                    int ran = gameRandom.Get(0, spriteGroups[i].sprites.Count);
                    mixedSprites.Add(spriteGroups[i].sprites[ran]);
                    spriteGroups[i].sprites.RemoveAt(ran);

                    if (spriteGroups[i].sprites.Count == 0)
                    {
                        spriteGroups.Remove(spriteGroups[i]);
                    }
                }
            }

            return mixedSprites;
        }

        public void Initialized()
        {
            mixedSprites = RegisterSpriteGroup();
        }
    }
}
