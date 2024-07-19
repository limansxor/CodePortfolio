using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{

    public class LockBlockView : BlockView
    {
        [SerializeField] protected FlipAttachment flip;
        public override void Initialized()
        {
            base.Initialized();
            attachments.Add(flip);
        }

        public void FlipForward() => flip.Execute((int)Flip.Forward);

        public void FlipReverse() => flip.Execute((int)Flip.Reverse);

        public void FlipRelease() => flip.Release();

   
    }
}