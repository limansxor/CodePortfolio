using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class BlinkerBlockView : NormalBlockView
    {
        [SerializeField] BlinkerAttachment blinker;

        public override void Initialized()
        {
            base.Initialized();

            attachments.Add(blinker);

            blinker.Initialized();
        }

        public void Blinker() => blinker.Execute();

    }
}
