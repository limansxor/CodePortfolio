using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class NormalBlockEffect : TouchBlockEffect
    {
        [SerializeField] AttachmentEffect attack;

        public override void Initialized()
        {
            base.Initialized();

            attachments.Add(attack);
        }

        public void Attack()
        {
            attack?.Execute();
        }
    }
}
