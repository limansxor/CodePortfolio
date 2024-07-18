using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class BlinkerBlock : NormalBlock
    {
        BlinkerBlockView view;
        BlinkerBlockEffect vfx;
        public BlinkerBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = (view as BlinkerBlockView);
            this.vfx = (vfx as BlinkerBlockEffect);
        }

        //public void BlinkerInitialized() => (view as BlinkerBlockView).Initialized();
        public void Blinker()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;
            view.Blinker();
        }

        public void ChangeStart()
        {
            StartPlay();
        }
        //public void StartFilp()
        //{
        //    //if (view.gameObject.activeSelf == false) return;
        //    FlipForward();
        //    //await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.flipAniTime));
        //    //view.QuestionFlipEnd();
        //}


    }
}
