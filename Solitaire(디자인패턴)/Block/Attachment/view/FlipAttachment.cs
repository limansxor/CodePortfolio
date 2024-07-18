using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace NGle.Solitaire.RunGame
{
    //public interface IFlip
    //{
    //    void Forward();
    //    void Reverse();
    //    void Release();
    //}

    public enum Flip { None , Forward, Reverse };
    public class FlipAttachment : Attachment //, IFlip
    {
        [SerializeField] Animator flipAni;
        [SerializeField] Animator flipReverseAni;
        [SerializeField] SpriteRenderer cover;

        [SerializeField] float forwardTimeScale = 1.0f;
        [SerializeField] float easingTime = 0.2f;

        [HideInInspector]public float flipAniTime;

        private Coroutine forwardCoroutine;

        public List<GameObject> temporarilyHiddens;

        public override void Initialized()
        {
            base.Initialized();

            flipAni.gameObject.SetActive(false);
            flipReverseAni.gameObject.SetActive(false);

            cover.gameObject.SetActive(true);

        }

        //public void Forward()
        //{
        //    forwardCoroutine = StartCoroutine(ForwardCoroutine());
        // }

        private IEnumerator ForwardCoroutine()
        {
            Initialized();
            cover.gameObject.SetActive(false);
            flipAni.gameObject.SetActive(true);
            flipReverseAni.gameObject.SetActive(false);

            foreach (var item in temporarilyHiddens) item.SetActive(false);
      

            AnimatorStateInfo stateInfo = flipAni.GetCurrentAnimatorStateInfo(0);
            flipAni.speed = forwardTimeScale;

            // 현재 애니메이션의 재생 시간 (normalizedTime) 가져오기
            float currentTime = stateInfo.normalizedTime;

            // 현재 애니메이션의 재생 시간 (초) 계산
            float totalTime = stateInfo.length * currentTime;
            flipAniTime = stateInfo.length;
            //await Task.Delay((int)(1000 * stateInfo.length) - 100);
            yield return new WaitForSeconds(stateInfo.length - easingTime);
            Release();
        }

        //    public void Reverse()
        //{
        //    // Initialized();
           
        //    flipAni.gameObject.SetActive(false);
        //    flipReverseAni.gameObject.SetActive(true);
        //    body.gameObject.SetActive(false);
        //}
        public override void Release()
        {
            cover.gameObject.SetActive(false);
            flipAni.gameObject.SetActive(false);
            flipReverseAni.gameObject.SetActive(false);
            body.gameObject.SetActive(true);
            foreach (var item in temporarilyHiddens) item.SetActive(true);
        }

        public override void Remove()
        {
            if (forwardCoroutine != null) StopCoroutine(forwardCoroutine);

            base.Remove();
        }

        public override void Execute(int index = -1, bool isLoop = false)
        {
            if (gameObject.activeSelf == false) return;
            if((int)Flip.Forward == index)
            {
                forwardCoroutine = StartCoroutine(ForwardCoroutine());
            }
            else if((int)Flip.Reverse == index)
            {
                flipAni.gameObject.SetActive(false);
                flipReverseAni.gameObject.SetActive(true);
                body.gameObject.SetActive(false);
            }
        }

    }
}
