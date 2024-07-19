using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
  
    public class KeyBlock : TouchBlock
    {
        KeyBlockView view;
        KeyBlockEffect vfx;
        public KeyBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as KeyBlockView;
            this.vfx = vfx as KeyBlockEffect;
        }
    }
}
