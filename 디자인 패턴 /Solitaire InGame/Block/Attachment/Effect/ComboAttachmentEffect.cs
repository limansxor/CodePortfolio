using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;


namespace NGle.Solitaire.RunGame
{
    public class ComboAttachmentEffect : AttachmentEffect
    {
        public SkeletonAnimation comboBGAni;
        public Transform oneDigit;
        public Transform twoDigit;
        public SkeletonAnimation[] numAnis;

        public override void Initialized()
        {
            base.Initialized();

            comboBGAni.GetComponent<DG.Tweening.DOTweenAnimation>().DORewind();
        }

        public override void Remove()
        {
            // 아무것도 하지 않는다 ;
        }

        string[] aniNames = {"combo_number_0", "combo_number_1", "combo_number_2", "combo_number_3", "combo_number_4",
    "combo_number_5","combo_number_6","combo_number_7","combo_number_8","combo_number_9"};

        public void ShowCombo(int n)
        {
            Show();

            twoDigit.gameObject.SetActive(false);
            oneDigit.gameObject.SetActive(false);

    
            comboBGAni.AnimationState.SetAnimation(0,"combo_light", false);
            comboBGAni.GetComponent<DG.Tweening.DOTweenAnimation>().DOPlay();

            if (n >= 10)
            {
                twoDigit.gameObject.SetActive(true);

                numAnis[1].AnimationState.SetAnimation(0, aniNames[n / 10], false);
                numAnis[2].AnimationState.SetAnimation(0, aniNames[n % 10], false);
            }
            else
            {
                oneDigit.gameObject.SetActive(true);

                numAnis[0].AnimationState.SetAnimation(0, aniNames[n], false);
            }
        }

    }
}
