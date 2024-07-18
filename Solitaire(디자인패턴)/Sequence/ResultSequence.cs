using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Support;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using static NGle.Solitaire.Data.UserData;

namespace NGle.Solitaire.RunGame
{
    public abstract class ResultSequence : MonoBehaviour
    {
        public Transform target;
        public SkeletonGraphic startSpine;
        public AssetReference appearSound;
        private AudioClip appearSoundClip;

        public void Run(BattleGameType type,
            int stageNum,
            int stageMax,
            UnityAction onPopResultDialog)
        {
            appearSoundClip = AssetManager.Instance.LoadAsset<AudioClip>(appearSound);
            target.gameObject.SetActive(true);
            StartCoroutine(RunCoroutine(type, stageNum, stageMax, onPopResultDialog));

        }
        public virtual IEnumerator RunCoroutine(BattleGameType type,
            int round,
            int roundMax,
            UnityAction onPopResultDialog)
        {
            List<string> animationNames = new List<string>();
            foreach (var item in startSpine.Skeleton.Data.Animations.Items)
            {
                animationNames.Add(item.Name);
            }
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.PlaySfx(appearSoundClip);

            yield return new WaitForSeconds(StartSpinePlay(animationNames[0]));

            if (ClientData.Instance.UserData.BattleInfo.isAI)
            {
                // 결과 소켓 통신 크라운이랑 Gp 포인트 업데이트
                ClientData.Instance.UserData.AnalyseAICombat();
            }

            yield return new WaitUntil(() => ClientData.Instance.UserData.BattleInfo.endInfo != null);

          

            if (ClientData.Instance.UserData.BattleInfo.endInfo.end == 1) //(round + 1 == roundMax)
            {
                startSpine.gameObject.SetActive(false); // win Lose 스파인 안보이게 하기 
              
                onPopResultDialog.Invoke();
                yield break;
            }
            else
            {
                NLog.Log("BattleStartType.NextRound");

                SceneManager.Instance.PushScene(SceneType.MultiGame,
                           new BattleParameter() { battleStartType = BattleStartType.NextRound },
                           new SimplifyLoading());
                yield break;
            }
        }


        public float StartSpinePlay(string name)
        {
            startSpine.AnimationState.SetAnimation(0, name, false);

           return SkeletonCommon.Duration(startSpine, name);
        }
    }
}
