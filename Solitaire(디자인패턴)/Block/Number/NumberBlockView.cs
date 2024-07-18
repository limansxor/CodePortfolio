using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class NumberBlockView : TouchBlockView
    {
        private Sprite enableImg;
        private Sprite disableImg;

        public void RegSprite(List<Sprite> sprites)
        {
            this.enableImg = sprites[0];
            this.disableImg = sprites[1];

            SetSprite(disableImg);
        }

        public void Active()
        {
            SetSprite(enableImg);
        }
    }
}
