using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class QuestionBlock : NormalBlock
    {
        QuestionBlockView view;
        QuestionBlockEffect vfx;

        public QuestionBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as QuestionBlockView;
            this.vfx = vfx as QuestionBlockEffect;
        }

        public async void ChangeStart()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;
            // FlipForward();
            StartPlay();
            await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.flipAniTime));
            QuestionFlipEnd();
        }

        public void QuestionFlipEnd()
        {
            view.QuestionFlipEnd();
        }



    }
}
