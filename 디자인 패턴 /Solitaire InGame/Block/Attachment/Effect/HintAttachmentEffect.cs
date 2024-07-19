using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;
using UnityEngine.Playables;

namespace NGle.Solitaire.RunGame
{
    public class HintAttachmentEffect : AttachmentEffect
    {
        public override void Initialized()
        {
            base.Initialized();
        }

        public override  void Execute(int index = -1, bool isLoop = false)
        {
            base.Execute(index, isLoop);
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Remove()
        {    
            base.Remove();
        }
    }

}
