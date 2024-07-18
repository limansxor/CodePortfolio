using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class NumberBlock : TouchBlock
    {
        NumberBlockView view;
        NumberBlockEffect vfx;
        public NumberBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as NumberBlockView;
            this.vfx = vfx as NumberBlockEffect;
        }

        public void Active()
        {
            view.Active();
        }

    }
}
