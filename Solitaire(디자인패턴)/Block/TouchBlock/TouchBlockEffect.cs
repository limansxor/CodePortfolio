using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class TouchBlockEffect : BlockEffect
    {
        public SelectAttachmentEffect sel;
        public HintAttachmentEffect hint;
        public AttachmentEffect hammer;
        public ComboAttachmentEffect combo;

        public override void Initialized()
        {
            base.Initialized();

            attachments = new List<Attachment>()
            {
                sel,
                hint,
                hammer,
                combo
            };
        }

        public void ComboRun(int n)
        {
            combo.ShowCombo(n);
        }

        public void ComboInit()
        {
            sel.SelectedComboInit();
        }

        public void LinkBorder(bool isCombo)
        {
            sel.RemoveBorder(isCombo);
        }

        public void Release()
        {
            sel.Release();
        }

        public void Selected(bool isCombo)
        {
            sel.Selected(isCombo);
        }

        public void HideHint()
        {
            hint.Release();
        }
        public void Hint()
        {
            hint.Execute(-1, true);
        }

     



    }
}
