using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using NGle.Solitaire.Support;
using System;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using NGle.Solitaire.Sound;

namespace NGle.Solitaire.RunGame
{
    public class OctopusInkEffect : MonoBehaviour
    {
        private List<SkeletonGraphic> skeletonGraphics;

        public List<SkeletonAnimation> octopusTimers;

        float updateTime;
        bool isGameStart = false;
        List<bool> timeFlags;
        List<bool> activateds;

        System.Action<int> onEffect;

        public void Awake()
        {
            skeletonGraphics = new List<SkeletonGraphic>();
            for (int i = 0; i < transform.childCount; i++) skeletonGraphics.Add(transform.GetChild(i).GetComponent<SkeletonGraphic>());

            timeFlags = new List<bool>();
            activateds = new List<bool>();
            ClientData.Instance.gameSetting.OctopusInkTimes.ForEach(i => { timeFlags.Add(false); activateds.Add(true); });
        }

        public void Run()
        {
            if (gameObject.activeSelf)
                StartCoroutine(RunCoroutine());
        }

        private IEnumerator RunCoroutine()
        {
            ChooseRandomNumbers(0, 4, 3).ForEach(n => transform.GetChild(n).gameObject.SetActive(true));

            yield return new WaitForSeconds(SkeletonCommon.Run(skeletonGraphics[0]));

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        List<int> ChooseRandomNumbers(int minValue, int maxValue, int count)
        {
            if (count > maxValue - minValue + 1)
            {
                throw new System.ArgumentException("선택할 수 있는 숫자의 범위보다 더 많은 숫자를 선택하려고 합니다.");
            }

            List<int> availableNumbers = new List<int>();
            for (int i = minValue; i <= maxValue; i++)
            {
                availableNumbers.Add(i);
            }

            List<int> selectedNumbers = new List<int>();
            GameRandom r = new GameRandom();
            r.Init((int)DateTime.Now.Ticks);
            System.Random random = new System.Random();

            for (int i = 0; i < count; i++)
            {
                int selectedIndex = random.Next(availableNumbers.Count);
                selectedNumbers.Add(availableNumbers[selectedIndex]);
                availableNumbers.RemoveAt(selectedIndex);
            }

            return selectedNumbers;
        }

        public void TimerStart(List<bool> timeFlags, Action<int> onEffect , List<SkeletonAnimation> octopusTimers)
        {
            isGameStart = true;
            updateTime = 1f;
            this.onEffect = onEffect;

            this.timeFlags = timeFlags;

            this.octopusTimers = octopusTimers;
        }

        public void TimerDrop(int n)
        {
            timeFlags[n] = false;
        }

        public void DisableTimer(int n)
        {
            timeFlags[n] = false;
        }

        public void Pause()
        {
            isGameStart = false;

            octopusTimers.ForEach(ani =>
            {
                ani.AnimationState.TimeScale = 0;

            });
        }
        public void Contiune()
        {
            isGameStart = true;

            octopusTimers.ForEach(ani =>
            {
                ani.AnimationState.TimeScale = 1;

            });
        }

        private void Update()
        {
            if (isGameStart)
            {
                updateTime += Time.deltaTime;
                for (int i = 0; i < ClientData.Instance.gameSetting.OctopusInkTimes.Count; i++)
                {                              
                    if (timeFlags[i] && activateds[i] && ClientData.Instance.gameSetting.OctopusInkTimes[i] < updateTime) 
                    {
                        Run();
                        onEffect.Invoke(i);
                        SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_octopus"));
                        activateds[i] = false;
                    }
                }
               
            }
        }

    }
}