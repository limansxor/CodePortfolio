using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using System;
using UnityEngine.Playables;

namespace NGle.Solitaire.RunGame
{
    public abstract class AttachmentEffect : Attachment
    {
        public float duration;

        [SerializeField] protected List<SkeletonAnimation> anis;

        private string GetCurrentAnimationName(SkeletonAnimation skeletonAnim)
        {
            if (skeletonAnim != null && skeletonAnim.AnimationState != null)
            {
                var track = skeletonAnim.AnimationState.GetCurrent(0);
                if (track != null)
                {
                    return track.Animation.Name;
                }
            }
            return null;
        }



        public override void Initialized()
        {
            base.Initialized();

            foreach (var item in anis)
            {
                item.gameObject.SetActive(false);
                var data = item.skeletonDataAsset.GetSkeletonData(true);
                string aniName = GetCurrentAnimationName(item);
                if (!string.IsNullOrWhiteSpace(aniName))
                {
                    var animation = data.FindAnimation(aniName);

                    duration = animation.Duration / item.timeScale - 0.5f;
                }
            }// 모든 애니메이션을 감추고 애니메이션의 길이를 구한다 
        }

        public override void Execute(int index = -1, bool isLoop = false)
        {
            if (gameObject.activeSelf == false || anis == null) return;
            Show();
    
                foreach (var item in anis)
                {
                    string name = item.AnimationName;
                    item.AnimationState.SetAnimation(0, name, isLoop);
                }
       
        
        }

        public void Show()
        {
            if (gameObject.activeSelf == false || anis == null) return;

                anis.ForEach((ani) => ani.gameObject.SetActive(true));
  
        }
        public override void Release()
        {
            if (gameObject.activeSelf == false || anis == null) return;
         
                anis.ForEach((ani) => ani.gameObject.SetActive(false));

        }

    }

}
