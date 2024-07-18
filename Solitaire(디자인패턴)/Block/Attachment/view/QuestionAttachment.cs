using System;
using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

namespace NGle.Solitaire.RunGame
{
    public class QuestionAttachment : Attachment
    {
        public Animator flipAni;
        public Animator flipReverseAni;
        public SpriteRenderer cover;
        private Coroutine forwardCoroutine;
 

        public override void Initialized()
        {
            base.Initialized();
            flipAni.gameObject.SetActive(false);
            flipReverseAni.gameObject.SetActive(false);
            cover.gameObject.SetActive(true); // false
        }

        public void FlipEnd()
        {
            cover.gameObject.SetActive(true);
        }

        //public void Forward()
        //{
        //    if (forwardCoroutine == null)
        //        forwardCoroutine = StartCoroutine(ForwardCoroutine());
        //}

        private IEnumerator ForwardCoroutine()
        {
            Initialized();
            cover.gameObject.SetActive(false);
            flipAni.gameObject.SetActive(true);
            flipReverseAni.gameObject.SetActive(false);

            AnimatorStateInfo stateInfo = flipAni.GetCurrentAnimatorStateInfo(0);

         

            yield return new WaitForSeconds(stateInfo.length -0.1f);
            // await Task.Delay((int)(1000 * stateInfo.length) - 100);
            Release();

            forwardCoroutine = null;
            yield break;
        }

        //public void Reverse()
        //{
        //    Initialized();
        //    flipAni.gameObject.SetActive(false);
        //    flipReverseAni.gameObject.SetActive(true);
  
        //}
        public override void Release()
        {
            flipAni.gameObject.SetActive(false);
            flipReverseAni.gameObject.SetActive(false);
            if (body != null)
                body.gameObject.SetActive(true);
        }

        public override void Remove()
        {
            base.Remove();
        }

        public override void Execute(int index = -1, bool isLoop = false)
        {
           if(index == (int)Flip.Forward)
            {
                if (forwardCoroutine == null)
                    forwardCoroutine = StartCoroutine(ForwardCoroutine());
            }
           else if(index == (int)Flip.Reverse)
            {
                Initialized();
                flipAni.gameObject.SetActive(false);
                flipReverseAni.gameObject.SetActive(true);
            }
        }
    }
}
