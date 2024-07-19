using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class QuestionBlockView : NormalBlockView
    {
        [SerializeField] QuestionAttachment question;

        private bool isSelected = false;

        public override void Initialized()
        {
            base.Initialized();

            attachments.Add(question);
            question.Initialized();
        }

        public void QuestionFlipEnd()
        {
            question.FlipEnd();
        }

        public override void Select(bool isTop)
        {
            if (question.gameObject.activeSelf == false) return;

            isSelected = true;

            question.Execute((int)Flip.Forward);

            base.Select(isTop);
        }

        public override void Release()
        {
            if (question.gameObject.activeSelf == false) return;

            if (isSelected)
            {
                question.Execute((int)Flip.Reverse);

                base.Release();
            }

            isSelected = false;
        }
    }
}
