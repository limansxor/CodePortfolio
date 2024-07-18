using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
  
    public class OctopusBlock : TouchBlock
    {
        OctopusBlockView view;
        OctopusBlockEffect vfx;
        public OctopusBlock(BlockView view, BlockEffect vfx) : base(view, vfx)
        {
            this.view = view as OctopusBlockView;
            this.vfx = vfx as OctopusBlockEffect;
        }

        public SkeletonAnimation GetOctopusAni() => view.GetOctopusAni();

        public override void StartPlay()
        {
            view.StartPlay();
        }

        public override void Pause()
        {
            view.Pause();
        }

        public override void Continue()
        {
            view.Continue();
        }
    }
}
