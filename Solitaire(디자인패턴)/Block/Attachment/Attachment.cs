using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public abstract class Attachment : MonoBehaviour
    {
        public Transform body; // 위치 잡는 용이기 때문에 시작 부터 숨긴다;
      
        private void Awake()
        {
            Initialized();
        }

        public virtual void Initialized()
        {
            if (body != null)
            body.gameObject.SetActive(false);
        }

        public abstract void Execute(int index = -1, bool isLoop = false);
        public abstract void Release();

        public virtual void  Remove()
        {
            if (gameObject == null || gameObject.activeSelf == false) return;

            StartCoroutine(RemoveCoroutine());
        }

        private IEnumerator RemoveCoroutine()
        {
         
            //float t = 1;
            //while(t>0)
            //{
            //    t += Time.deltaTime*2;
            //    await Task.Yield()
            //}
            yield return new WaitForSeconds(0.3f);
            gameObject.SetActive(false);
        }


    }
}
