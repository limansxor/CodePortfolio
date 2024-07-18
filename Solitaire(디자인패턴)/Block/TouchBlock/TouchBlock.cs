using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class TouchBlock : Block, ITouch
    {
        public bool isHint { get; private set;} 
        TouchBlockView view;
        TouchBlockEffect vfx; 
        public TouchBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as TouchBlockView;
            this.vfx = vfx as TouchBlockEffect;
        }

        public void ComboRun(int combo) { if (combo > 1) vfx.ComboRun(combo); }

        public void ComboInit() => vfx.ComboInit();

        public void Select(bool isTop) => view.Select(isTop); // property.idx.y == 0

        public void Selected(bool isCombo)
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.Selected(isCombo);
            vfx.Selected(isCombo);

        }

        public void Release()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.Release();
            vfx.Release();
        }

        public void HideHint()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;
            isHint = false;
           // view.Hint();
            vfx.HideHint();
        }

      
        public void Hint()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;
            isHint = true;

                view.Hint();
                vfx.Hint();
        }

        public override void StartPlay()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipForward();
        }
        public override void Pause()
        {
            if (view == null ||  view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipReverse();
        }

        public override async void Continue()
        {
            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.FlipForward();
            await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.filpTime));
            view.FlipRelease();
        }

        public void LinkBorder(bool isCombo)
        {
            vfx.LinkBorder(isCombo);
        }

        public float HammerDuration()
        {
            return vfx.hammer.duration;
        }

        public void HitHammer()
        {
            vfx.hammer.Execute();
        }


    }
}
