using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{

    public class BattleTimeUI : MonoBehaviour
    {
        [SerializeField] SkeletonGraphic startAni;
        [SerializeField] SkeletonGraphic finishAni;

        [SerializeField] Text[] timeTexts;

        public bool isRun = false;
        public float remainingTime = 0;
        private float timeMax = 0;
        public void Awake()
        {
            foreach (var item in timeTexts) item.gameObject.SetActive(false);

        }

        public void Initialized()
        {
            startAni.AnimationState.ClearTracks();

            finishAni.AnimationState.ClearTracks();

            timeMax = BattleRunGameMgr.Instance.runGameData.starRewardTimes[0];
            remainingTime = timeMax + 0.1f;

            foreach (var item in timeTexts) item.gameObject.SetActive(true);
            foreach (var item in timeTexts) item.text = ((int)remainingTime).ToString();

            (BattleRunGameMgr.Instance.runGameEvent as BattleRunGameEvent).RegBattleGameTimeOver(OnBattleGameTimeOver);
        }
        public void OnBattleGameTimeOver()
        {
            isRun = false;
            remainingTime = 0;
            foreach (var item in timeTexts) item.text = ((int)remainingTime).ToString();
        }
        public async void GameStart()
        {
            startAni.AnimationState.SetAnimation(0, "animation2", true);
            startAni.AnimationState.SetAnimation(0, "animation2", true);

            await Task.Delay(1000);

            isRun = true;
        }

        public void SetViewTime(float time)
        {
            remainingTime = time;
        }
        public void Update()
        {

            if (isRun)
            {
                if (remainingTime > 0)
                {
                    remainingTime -= Time.deltaTime * (timeMax / (timeMax - 4.0f));

                    foreach (var item in timeTexts) item.text = ((int)remainingTime).ToString();
                }
                else
                {
                    remainingTime = 0;
                    foreach (var item in timeTexts) item.text = ((int)remainingTime).ToString();

                    isRun = false;

                    startAni.AnimationState.ClearTracks();

                    finishAni.AnimationState.ClearTracks();

                    //BattleRunGameMgr.Instance.OnTimeOver();      
                }
            }

        }

    }
}
