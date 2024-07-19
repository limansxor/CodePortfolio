using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using UnityEngine.Playables;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class SelectAttachmentEffect : AttachmentEffect
    {
        [Serializable]
        public class DefAni
        {
            public DefAniType defAniType;
            public SkeletonAnimation ani;
        }

        public DefAni[] defAnis;
        private Dictionary<DefAniType, SkeletonAnimation> dicAni;

        private void Start()
        {
           
        }

        public override void Initialized()
        {
            base.Initialized();

            // 실수로 숨기지 않을까봐 장치 

            dicAni = new Dictionary<DefAniType, SkeletonAnimation>();
            foreach (var item in defAnis)
            {
                dicAni.Add(item.defAniType, item.ani);
                item.ani.gameObject.SetActive(false);
            }
        }

        public void RemoveBorder(bool isCombo)
        {
            SkeletonAnimation ani;
            if (isCombo)
            {
                dicAni[DefAniType.RemoveBorder].gameObject.SetActive(false);

                 ani = dicAni[DefAniType.RemoveComboBorder];
                ani.gameObject.SetActive(true);
                string name = ani.AnimationName;
                ani.AnimationState.SetAnimation(0, name, false);


            }
            else
            {
                 ani = dicAni[DefAniType.RemoveBorder];
                ani.gameObject.SetActive(true);
                string name = ani.AnimationName;
                ani.AnimationState.SetAnimation(0, name, false);
            }

        }

        public override void Remove()
        {
            base.Remove();

            if(gameObject.activeSelf)
            StartCoroutine(RemoveCoroutine());
        }

        IEnumerator RemoveCoroutine()
        {
            float t = 1;
            while (t > 0) // 0.5f
            {
                foreach (var item in defAnis)
                {
                    item.ani.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1, t);
                }

                t -= Time.deltaTime * (1 / ClientData.Instance.gameSetting.removeTime);
                yield return null;
            }

            foreach (var item in defAnis) item.ani.gameObject.SetActive(false);
        }

        public  void Select()
        {
            // 형식 맞추려고 하기는 했는데 ... 
        }
        public void Selected(bool isCombo)
        {
            dicAni[DefAniType.Selected].gameObject.SetActive(false);
            dicAni[DefAniType.SelectedCombo].gameObject.SetActive(false);

            if (isCombo)
            {
                SkeletonAnimation ani = dicAni[DefAniType.SelectedCombo];
                ani.gameObject.SetActive(true);
                ani.GetComponent<PlayableDirector>().Play();
            }
            else
            {
                SkeletonAnimation ani = dicAni[DefAniType.Selected];
                ani.gameObject.SetActive(true);
                ani.GetComponent<PlayableDirector>().Play();
            }
        }

        public void SelectedComboInit()
        {
            dicAni[DefAniType.SelectedCombo].gameObject.SetActive(false);
            SkeletonAnimation ani = dicAni[DefAniType.Selected];
            ani.gameObject.SetActive(true);
            ani.GetComponent<PlayableDirector>().time = 1; // idle로 넘어 가기 위한 
            ani.GetComponent<PlayableDirector>().Play();

        }

        public override void Release()
        {
            base.Release();
        }
        
    }
}
