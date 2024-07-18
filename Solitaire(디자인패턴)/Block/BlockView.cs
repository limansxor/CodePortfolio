using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{

    public abstract class BlockView : MonoBehaviour // 예외 사항 좌물쇠 
    {  
        public Transform body;

        protected List<Attachment> attachments;

        public bool isRemove { get; protected set; }

        public void Start() // 페이드에 가려 지기 때문에 Start 에서 초기화 해도 된다. 
        {
            Initialized();
        }

        public void SetSprite(Sprite sprite)
        {
            body.gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        public virtual void Initialized()
        {
            attachments = new List<Attachment>();
            isRemove = false;
        }

        public virtual void Remove()
        {
            isRemove = true;
            if (gameObject.activeSelf == false) return;

            attachments?.ForEach((a) => a.Remove());

            StartCoroutine(RemoveCoroutine());
        }

        IEnumerator RemoveCoroutine()
        {
            float t = 1;
            while (t > 0) // 0.5f
            {
                body.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, t);
                if (body.childCount != 0 && body.GetChild(0).gameObject.GetComponent<SpriteRenderer>() != null)
                    body.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, t);

                t -= Time.deltaTime * (1/ ClientData.Instance.gameSetting.removeTime);
                yield return null;
            }

            body.gameObject.SetActive(false);
        }
    }
}
