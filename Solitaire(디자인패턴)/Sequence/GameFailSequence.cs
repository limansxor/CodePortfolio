using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using UnityEngine.AddressableAssets;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Sound;
using Spine;
using System.Threading.Tasks;
using UnityEngine.UI;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Support;
using System;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class GameFailSequence : MonoBehaviour
    {
        public Transform parent;
        public Image dim;
        public AssetReference timeOverVoice;
        private AudioClip _timeOverVoice;

        public SkeletonGraphic igTimeover;

        public AssetReference cntAlarm;
        private AudioClip _cntAlarm;

        public Task Take { get; private set; }

        public SkeletonGraphic ig_321go;

        public SkeletonGraphic ig_failed;
        public SkeletonGraphic ig_failed_btn;
        public AssetReference failVoice;
        private AudioClip _failVoice;
        public AssetReference failSfx;
        private AudioClip _failSfx;
        public Button failCloseBtn;
        public Button retryBtn;

        public AssetReference se_time_count;
        private AudioClip _se_time_count;
        public AssetReference voice_go;
        private AudioClip _voice_go;
        public AssetReference se_clock_fly;
        private AudioClip _se_clock_fly;
        public AssetReference se_heart_absorption;
        private AudioClip _se_heart_absorption;
        // ==========================
        private Coroutine runCoroutine;
        private Coroutine timeExtensionPopupCoroutine = null;
        private bool isStopTimeExtensionPopup;

        public TimeGaugeUI timeGaugeUI;

        public Action onContinue;

        private AudioClip BGM;

        RunGameData runGameData;

        private void LoadAsset()
        {
            
            _timeOverVoice = AssetManager.Instance.LoadAsset<AudioClip>(timeOverVoice);
            _cntAlarm = AssetManager.Instance.LoadAsset<AudioClip>(cntAlarm);
            _failVoice = AssetManager.Instance.LoadAsset<AudioClip>(failVoice);
            _failSfx = AssetManager.Instance.LoadAsset<AudioClip>(failSfx);

            _se_time_count = AssetManager.Instance.LoadAsset<AudioClip>(se_time_count);
            _voice_go = AssetManager.Instance.LoadAsset<AudioClip>(voice_go);
            _se_clock_fly = AssetManager.Instance.LoadAsset<AudioClip>(se_clock_fly);
            _se_heart_absorption = AssetManager.Instance.LoadAsset<AudioClip>(se_heart_absorption);
        }

        private void Awake()
        {
            parent.gameObject.SetActive(false);
        }

        
        public void Run( Action onContinue, AudioClip BGM)
        {
            runGameData = StageRunGameMgr.Instance.runGameData;

            LoadAsset();
            parent.gameObject.SetActive(true);
            dim.gameObject.SetActive(true);


            failCloseBtn.onClick.RemoveAllListeners();
            failCloseBtn.onClick.AddListener(async () => {
                ig_failed_btn.AnimationState.SetAnimation(0, "03_touch", false);
               await Task.Delay((int)(1000 * SkeletonCommon.Duration(ig_failed_btn, "03_touch")));
              SceneManager.Instance.PushScene(SceneType.World);
                failCloseBtn.onClick.RemoveAllListeners();
            });

            retryBtn.onClick.RemoveAllListeners();
            retryBtn.onClick.AddListener(async () => {
                retryBtn.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                await Task.Delay((int)(1000 * SkeletonCommon.Duration(ig_failed_btn, "03_touch")));
                StageStartDialogUI.DoModal(runGameData.currentstageId, () => SceneManager.Instance.PushScene(SceneType.World));
                retryBtn.onClick.RemoveAllListeners();
            });
           

            retryBtn.gameObject.SetActive(false);

            runCoroutine =StartCoroutine(RunCoroutine());

            this.onContinue = onContinue;
            this.BGM = BGM;
        }
      
        private IEnumerator RunCoroutine()
        {
            yield return StartCoroutine( TimeOver());

            timeExtensionPopupCoroutine = StartCoroutine(TimeExtensionPopup());
        }

        private void FinalFail()
        {
            if(timeExtensionPopupCoroutine != null)
            StopCoroutine(timeExtensionPopupCoroutine);
            TimeExtensionDialogUI.OnClose();
            StartCoroutine(CompleteFail());
        }

        private IEnumerator TimeOver()
        {
            yield return new WaitForSeconds(0.5f);

            SoundManager.Instance.PlaySfx(_timeOverVoice);

            igTimeover.gameObject.SetActive(true);
            igTimeover.AnimationState.SetAnimation(0, "animation", false);

            //yield return new WaitForSeconds(SkeletonCommon.Duration(igTimeover, "animation"));
         
            yield return new WaitForSeconds(_timeOverVoice.length);


            yield break;
        }

        private IEnumerator TimeExtensionPopup()
        {
            TimeExtensionDialogUI.DoModal(FinalFail,
               OnTimeExtensionReady);

            SoundManager.Instance.PlaySfx(_cntAlarm);
            int n = 0; 
            while (n<10)
            {
                yield return new WaitForSeconds(1);
                SoundManager.Instance.PlaySfx(_cntAlarm);

                n++;
            }
            yield return new WaitForSeconds(0.5f);

            FinalFail();

            yield break;
        }

                            
        private IEnumerator CompleteFail()
        {
            // 버튼 재대로 해야 한다
         
            //NLog.Log("CompleteFail 들어 오는가 ?");
            ig_failed.gameObject.SetActive(true);
            ig_failed_btn.gameObject.SetActive(true);

            failCloseBtn.gameObject.SetActive(false);
            retryBtn.gameObject.SetActive(false);

            SoundManager.Instance.PlaySfx(_failVoice);
            SoundManager.Instance.PlaySfx(_failSfx);

            ig_failed.AnimationState.SetAnimation(0, "01_appearance", false);
            ig_failed_btn.AnimationState.SetAnimation(0, "01_appearance", false);
            yield return new WaitForSeconds(SkeletonCommon.Duration(ig_failed_btn, "01_appearance"));

            failCloseBtn.gameObject.SetActive(true);
            retryBtn.gameObject.SetActive(true);

            ClientData.Instance.UserData.StageDataGroup.StageFail(null);
            yield break;
        }

        private void OnTimeExtensionReady(int n)
        {
            StopCoroutine(timeExtensionPopupCoroutine);
            AlertDialogUI.DoModal(AlertDialogUI.Type.Alert,
                 "시간연장 준비완료 되었습니다.",
                 () => { OnTimeExtension(n); });

            if(runCoroutine != null) StopCoroutine(runCoroutine);

            TimeExtensionDialogUI.OnClose();
        }

        // public void 
        private async void OnTimeExtension(int n)
        {
          
            NLog.Log(n + " 연장 아이템 실행 ");
            dim.gameObject.SetActive(false);
//            StopCoroutine(runCoroutine);

            string[] aniNames = { "10sec", "20sec" };
            ig_321go.gameObject.SetActive(true);

            ig_321go.AnimationState.SetAnimation(0, aniNames[n], false);

            StartCoroutine(TimeExtensionSound1());
            StartCoroutine(TimeExtensionSound2());
            StartCoroutine(TimeExtensionSound3());
            await Task.Delay(1300); //(int)(1000* SkeletonCommon.Duration(ig_321go, aniNames[n])) -1000
            int[] addTimes = new int[] { 10, 20 };
            timeGaugeUI.TimeExtensionReady(addTimes[n]);
            await Task.Delay(500);
            timeGaugeUI.isRun = true;

            onContinue.Invoke();
            SoundManager.Instance.PlayBGM(BGM);
            


        }

        private IEnumerator TimeExtensionSound1()
        {   
            SoundManager.Instance.PlaySfx(_se_time_count);
            yield return new WaitForSeconds(0.55f);
            SoundManager.Instance.PlaySfx(_se_time_count);
            yield return new WaitForSeconds(0.55f);
            SoundManager.Instance.PlaySfx(_se_time_count);
            yield return new WaitForSeconds(0.55f);
            SoundManager.Instance.PlaySfx(_voice_go);
        }
        private IEnumerator TimeExtensionSound2()
        {
            yield return new WaitForSeconds(0.72f);
            SoundManager.Instance.PlaySfx(_se_clock_fly);
        }
        private IEnumerator TimeExtensionSound3()
        {
            yield return new WaitForSeconds(1.12f);
            SoundManager.Instance.PlaySfx(_se_heart_absorption);
        }
    }


}
