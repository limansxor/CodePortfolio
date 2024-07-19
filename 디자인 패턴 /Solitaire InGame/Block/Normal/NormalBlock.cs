using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class NormalBlock : TouchBlock
    {
        NormalBlockView view;
        NormalBlockEffect vfx;
        public NormalBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as NormalBlockView;
            this.vfx = vfx as NormalBlockEffect;
        }

        //public void ReceiveAttack(int kind = 0) { }

        public void Shuffle( Sprite sprite)
        {
            if (view.isRemove) return;

            view.Shuffle( sprite);
          }

        public void Attact() // AttackType attactType
        {
            view.Remove();
            vfx.Attack();
        }

        public void Hide()
        {
            if (view.isRemove) return;
            view.gameObject.SetActive(false);
            vfx.gameObject.SetActive(false);
        }

    }
}
