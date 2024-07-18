using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class LockBlock : Block
    {
        LockBlockView view;
        LockBlockEffect vfx;
        public LockBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as LockBlockView;
            this.vfx = vfx as LockBlockEffect;
        }

        public override void StartPlay()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipForward();
        }
        public override void Pause()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipReverse();
        }

        public override async void Continue()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipForward();
            await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.filpTime));
            view.FlipRelease();
        }


    }
}
