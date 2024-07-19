using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class StoneBlock : Block
    {
        StoneBlockView view;
        StoneBlockEffect vfx;
        public StoneBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as StoneBlockView;
            this.vfx = vfx as StoneBlockEffect;
        }

        public override void Remove(int combo)
        {
            base.Remove(combo);
            vfx.Remove();
        }
    }
}
