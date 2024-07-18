using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Spine.Unity;
using NGle.Solitaire.Support;
using UnityEngine.Playables;
using System;
using System.Threading.Tasks;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class GameUI : GameUIAbs
    {
        [SerializeField] GameUIModel _model;

        [SerializeField] Text[] stageText;
        [SerializeField] Text[] matchableTotalText;
        [SerializeField] Text[] pairMatchCountText;

        [SerializeField] TimeGaugeUI timeGauge;

        [SerializeField] SkeletonGraphic cat;
        [SerializeField] SkeletonGraphic sirenCat;

        [SerializeField] Canvas timelineRes;
        [SerializeField] PlayableDirector startTimeLine;
        [SerializeField] PlayableDirector clearTimeLine;
        [SerializeField] GameFailSequence gameFailSequence;

        [SerializeField] Text[] timeDisplayTexts;

        [SerializeField] Button pauseBtn;

        [SerializeField] GameClearSequence gameClearSequence;

        [SerializeField] SkeletonGraphic shuffleHand;

        RunGameMgr mgr;

        public void Initialized(int addTime = 0)
        {
            mgr = StageRunGameMgr.Instance;

            foreach (var item in stageText) item.text = ClientData.Instance.UserData.StageDataGroup.selectedStageId.ToString("00");

            UpdateState();

            timeGauge.Initialized(addTime);

            foreach (var item in timeDisplayTexts) item.text =
                    (mgr.runGameData.starRewardTimes[0] / 60).ToString() + ":" + (mgr.runGameData.starRewardTimes[0] % 60).ToString("00");

            pauseBtn.onClick.AddListener(() => {
                StageRunGameMgr.Instance.OnGamePause();
                PauseDialogUI.DoModal(StageRunGameMgr.Instance.OnGameContinue);
            });

            sirenCat.gameObject.SetActive(false);

            GameStartSequence();

            mgr.runGameEvent.AddEventRunGameDataCreate( () => //  async
            {
                mgr.runGameEvent.AddEvtUpdateCombo(OnComboInit);
                mgr.runGameEvent.AddEvtRemove(OnRemove);
            });
            // StartCoroutine(RunGameDataInitCoroutine());

        }

        int preCombo = 0; 
        public void OnRemove(List<BlockPlace> blockPlaces)
        {
            if (sirenCat.gameObject.activeSelf == false)
            {
                cat.gameObject.SetActive(true);
                if(mgr.runGameData.comboCount < 2)
                {
                    cat.AnimationState.SetAnimation(0, "idle", true);
                }
                if(preCombo > 1)
                {

                }
                else if (mgr.runGameData.comboCount > 1  )
                {
                    cat.AnimationState.SetAnimation(0, "combo", true);
                }
                
                preCombo = mgr.runGameData.comboCount;
            }

        }

        public void OnComboInit()
        {
            //  CatAni(false);
           cat.AnimationState.SetAnimation(0, "idle", true);
        }


        public void RunShuffleHand()
        {
            shuffleHand.gameObject.SetActive(true);
            shuffleHand.AnimationState.ClearTrack(0);
            shuffleHand.AnimationState.SetAnimation(0, "animation", false);
            //SkeletonCommon.Duration(shuffleHand, "animation");

            //await Task.Delay((int)(1000 * SkeletonCommon.Duration(shuffleHand, "animation")-50) );
            //shuffleHand.gameObject.SetActive(false);
        }

        public override void GameStartSequence()
        {
            timelineRes.gameObject.SetActive(true);
            startTimeLine.gameObject.SetActive(true);
            startTimeLine.Play();
        }

        public void UpdateState()
        {
            foreach (var item in matchableTotalText) item.text = mgr.runGameData.blockCount.ToString();
            foreach (var item in pairMatchCountText) item.text = mgr.runGameData.matchingCount.ToString();
        }

        public void TimerStart()
        {
            timeGauge.isRun = true;
        }

        public void TimerPause()
        {
            timeGauge.isRun = false;
        }

        public void Clear()
        {
            timeGauge.isRun = false;
            //ClearTimeLine();
            gameClearSequence.Run();
            OnComboInit();
        }

        public void Fail(Action onContinue, AudioClip BGM)
        {
            timeGauge.timeWarning.gameObject.SetActive(false);

            gameFailSequence.Run(onContinue, BGM);


        }
        //private void ClearTimeLine()
        //{
        //    timelineRes.gameObject.SetActive(true);
        //    clearTimeLine.gameObject.SetActive(true);
        //    clearTimeLine.Play();
        //}

    }
}
