using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using Spine.Unity;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class OctopusBlockView : TouchBlockView
    {

        private SkeletonAnimation octopus;


        public SkeletonAnimation GetOctopusAni()
        {
            return octopus;
        }

        private void InitAni(SkeletonAnimation ani)
        {
            //ani.AnimationState.TimeScale = 0;
            ani.AnimationState.SetEmptyAnimation(0, 0f);

        }
        private void RunAni(SkeletonAnimation ani)
        {
            // ani.AnimationState.TimeScale = 1;

          //  body.gameObject.SetActive(true);

            Spine.AnimationState state = ani.AnimationState;



            string aniName = ani.AnimationName; //state.Tracks.Items[0].Animation.Name;

            ani.AnimationState.SetAnimation(0, "animation", false);
        }

        public void SetIcon(int n)
        {
            octopus = ClientData.Instance.runGameDataFileLink.GetOctopusAsset(n, body);

            octopus.transform.localPosition = new Vector3(-0.074f, -0.143f, 0);

            InitAni(octopus);
        }



        public void StartPlay()
        {
            RunAni(octopus);
        }

        public void Pause()
        {
            octopus.AnimationState.TimeScale = 0;
        }
        public void Continue()
        {
            octopus.AnimationState.TimeScale = 1;
        }
    }

   
}