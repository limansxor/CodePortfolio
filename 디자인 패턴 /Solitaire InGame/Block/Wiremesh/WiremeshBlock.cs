using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class WiremeshBlock : Block
    {
        WiremeshBlockView view;
        WiremeshBlockEffect vfx;
        public WiremeshBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as WiremeshBlockView;
            this.vfx = vfx as WiremeshBlockEffect;
        }

        public override void Remove(int combo)
        {
            base.Remove(combo);
            vfx.Remove();
        }
    }
}
