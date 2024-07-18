using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Support;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{         
    public class GameClearSequence : MonoBehaviour
    {
        public AssetReference voice_stage_clear;
        private AudioClip _voice_stage_clear;
        public AssetReference se_stage_clear1;
        private AudioClip _se_stage_clear1;

        public Image dim;
        public SkeletonGraphic ig_crear;

        private Coroutine coroutine;
        private WebPacket.ResSuccess _res;

        RunGameData runGameData;

        public void Run()
        {
            runGameData = StageRunGameMgr.Instance.runGameData;

            _res = null;
            NLog.Log("[결과] 서버로 보내는 시간 값 : " + (runGameData.starRewardTimes[0] - (int)runGameData.remainingTime));
            ClientData.Instance.UserData.StageDataGroup.StageSuccess(runGameData.score,
                runGameData.starRewardTimes[0]-(int)runGameData.remainingTime,
                (res) =>
            {
                _res = res;
            });
            _voice_stage_clear = AssetManager.Instance.LoadAsset<AudioClip>(voice_stage_clear);
            _se_stage_clear1 = AssetManager.Instance.LoadAsset<AudioClip>(se_stage_clear1);
            StartCoroutine(PreRunCoroutine());
           
        }
        private IEnumerator PreRunCoroutine()
        {
            //dim.gameObject.SetActive(true);
            ig_crear.gameObject.SetActive(true);
            ig_crear.AnimationState.SetAnimation(0, "animation", false);
            SoundManager.Instance.PlaySfx(_voice_stage_clear);
            SoundManager.Instance.PlaySfx(_se_stage_clear1);
            yield return new WaitForSeconds(SkeletonCommon.Duration(ig_crear, "animation")+ 0.3f);
         
            coroutine = StartCoroutine(RunCoroutine());
        }
        private IEnumerator RunCoroutine()
        {
            yield return new WaitUntil(() => _res != null); // 리소스 올때 까지 대기
          
            ClientData.Instance.UserData.StageDataGroup.SetLocalStageData(StageRunGameMgr.Instance.runGameData.currentstageId, StageRunGameMgr.Instance.runGameData.score, _res.stage.star);

            NLog.Log($"StageSuccess _res.gp = {_res.gp} _res.stage.star = {_res.stage.star}");

            ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint = _res.gp;
            if (_res.gp > ClientData.Instance.UserData.AchievementDataGroup.highest_gp)
                ClientData.Instance.UserData.AchievementDataGroup.highest_gp = _res.gp;

            // 스테이지 제한 값
           
            int currentStar = 1;
            if (StageRunGameMgr.Instance.runGameData.remainingTime>= runGameData.starRewardTimes[1])
            {
                currentStar = 3;
            }
            else if(StageRunGameMgr.Instance.runGameData.remainingTime >= runGameData.starRewardTimes[2])
            {
                currentStar = 2;
            }

            for (int i = 0; i < _res.reward.Count; i++)
            {
                NLog.Log($"_res.reward[{i}] id = {_res.reward[i].id} cnt = {_res.reward[i].cnt}");
             //   _res.reward[i].id = 30005;
                ClientData.Instance.UserData.GoodsDataGroup.AddGoods(_res.reward[i]);
            }

         //   NLog.Log("UI 적용전  GPGrade " + ClientData.Instance.UserData.PointDataGroup.UserProfile.GPGrade);

            int rank = 1;

            for (int i = StageRunGameMgr.Instance.runGameData.configData.GradeMinPointTabel.Count - 1; i >= 0; i--)
            {
                if (StageRunGameMgr.Instance.runGameData.configData.GradeMinPointTabel[i] <= ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint)
                {
                    rank = i;
                    break;
                }
            }

            // _res 서버 값은 나중에 연동 하고 지금 있는 값 부터 잘 살려 보장  

          

            GpData gpData = new GpData
            {
                preRank = runGameData.preGrade,
                grade = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPGrade,
                preGp = runGameData.preGP,
                addGp = _res.gp - runGameData.preGP,
                gp = _res.gp,
                minGpTable = runGameData.configData.GradeMinPointTabel,//setting.BattlePointTabel,
                preCrown = runGameData.preCrown,
                crown = runGameData.preCrown,
                crownConversionPoint = runGameData.configData.crown_gp,
            };

            StageResultData stageResultData = new StageResultData
            {

                stageId = runGameData.currentstageId,
                preStar = runGameData.preStar,
                curtStar = currentStar,
                allStar = _res.star,
                conversionPoint = runGameData.configData.star_gp,
                score = runGameData.score,
                itemScore = runGameData.itemScore,
                //totalScore = 5972,
                rewardGold = _res.reward.Count > 0 ? _res.reward[0].cnt : 0,
                evtRewardGold = _res.reward.Count > 1 ? _res.reward[1].cnt : 0,
                isMission = _res.reward.Count > 0 ? true : false 
            };

            ClearDialogUI.DoModal( gpData, stageResultData);
            yield return new WaitForSeconds(0.15f);
            dim.gameObject.SetActive(false);
            ig_crear.gameObject.SetActive(false);
        }
    }
}