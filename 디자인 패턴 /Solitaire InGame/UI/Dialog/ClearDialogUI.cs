using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Dialog;

using UnityEngine.UI;
using Spine.Unity;
using System.Threading.Tasks;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Support;
using NGle.Solitaire.Data;
using NGle.Solitaire.Sound;
using DG.Tweening;

namespace NGle.Solitaire.RunGame
{
    public class ClearDialogUI : DialogBase
    {
        public static void DoModal(GpData gpData, StageResultData stageData)
        {
            DialogManager.CreateDialog<ClearDialogUI>("Dialogs/ClearDialogUI.prefab",
                delegate (ClearDialogUI dialog)
                {
                    if (dialog)
                    {
                        dialog.Show();
                        dialog.InitEvent(gpData, stageData);
                    }
                });
        }
        // 247.5

        public Transform leftUIParent;
        public Transform rightUIParent;
        public Transform closeBtnParent;


        [SerializeField] Sprite[] rankSprites;

        [SerializeField] Image rankImg;
        [SerializeField] Text addPointText;
        [SerializeField] RectTransform popAddTextRect;
        [SerializeField] Text gaugePointText;
        [SerializeField] Image gaugeBarImg;
        [SerializeField] Text currentPointText;
        [SerializeField] Button miniPopupGpBtn;
        [SerializeField] MiniPopupGp miniPopupGp;

        [SerializeField] SkeletonGraphic rankDownEffect;
        [SerializeField] SkeletonGraphic rankUpEffect;
        [SerializeField] SkeletonGraphic rankUpGaugeEffect;

        [Space(20)]
        // == Right

        [SerializeField] SkeletonGraphic starAni;
        [SerializeField] Transform[] digits;
        [SerializeField] Text[] stageNum;
        [SerializeField] SkeletonGraphic clear;
        [SerializeField] Text score;
        [SerializeField] Text reward;
        [SerializeField] Button replayBtn;
        [SerializeField] Image challengePop;
        [SerializeField] Button nextBtn;

        [SerializeField] Button miniPopupScoreBtn;
        [SerializeField] MiniPopupScore miniPopupScore;
        [SerializeField] Button miniPopupRewardBtn;
        [SerializeField] MiniPopupReward miniPopupReward;

        [SerializeField] Button closeBtn;

        [SerializeField] SkeletonGraphic firework;

        protected override void OnHide()
        {

        }

        protected override void OnRestore()
        {

        }

        protected override void OnShow()
        {

        }

        private void InitEvent(GpData gpData, StageResultData stageData)
        {

            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                
             //   SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_cancel_button").Load());
                Close();
                SceneManager.Instance.PushScene(SceneType.World);

            });

            GPView(gpData , stageData);
            //  StageView(stageData);

            rightUIParent.gameObject.SetActive(false);
            closeBtnParent.gameObject.SetActive(false);

            miniPopupGp.gameObject.SetActive(false);
            miniPopupScore.gameObject.SetActive(false);
            miniPopupReward.gameObject.SetActive(false);

            // 움직이는 패널
        }

        private void PopupEvent(GpData gpData , StageResultData stageData)
        {
            miniPopupGp.Initialized(
                stageData.allStar,
              stageData.curtStar - stageData.preStar,
                stageData.conversionPoint,
                gpData.crown,
                gpData.crown - gpData.preCrown,
                gpData.crownConversionPoint);

            miniPopupGpBtn.onClick.RemoveAllListeners();
            miniPopupGpBtn.onClick.AddListener(() => miniPopupGp.gameObject.SetActive(true));

            miniPopupGpBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();


            // ===========  ===========  ===========  ===========  ===========  ===========


            miniPopupScore.Initialized(stageData.score, stageData.itemScore);

            miniPopupScoreBtn.onClick.RemoveAllListeners();
            miniPopupScoreBtn.onClick.AddListener(() => miniPopupScore.gameObject.SetActive(true));

            miniPopupScoreBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

            // ===========  ===========  ===========  ===========  ===========  ===========

            miniPopupReward.Initialized(stageData.rewardGold, stageData.evtRewardGold);

            miniPopupRewardBtn.onClick.RemoveAllListeners();
            miniPopupRewardBtn.onClick.AddListener(() => miniPopupReward.gameObject.SetActive(true));

            miniPopupRewardBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();
        }

        public float[] miniPopMoveRange = { -200, 200 };

        private void UpdateRankUI(int grade, int gaugeGp, int gaugeLength, int currnetGp, int min)
        {
            grade = Mathf.Clamp(grade, 0, rankSprites.Length - 1);
            rankImg.sprite = rankSprites[grade];
            gaugePointText.text = gaugeGp.ToString() + "/" + (grade == rankSprites.Length - 1 ? "∞" : gaugeLength.ToString());
            float amount = 0;
            if (gaugeLength > 0) amount = Mathf.Clamp((float)(gaugeGp - min) / (gaugeLength - min), 0, 1);
            gaugeBarImg.fillAmount = amount;
            currentPointText.text = currnetGp.ToString();


            Vector2 prePos = popAddTextRect.anchoredPosition;

            popAddTextRect.anchoredPosition = new Vector2(Mathf.Lerp(miniPopMoveRange[0], miniPopMoveRange[1], amount), prePos.y);
        }

        private void GPView(GpData gpData, StageResultData stageData) => GpViewInit(gpData, stageData);
        //{
        //    GpViewInit(gpData, stageData);
        //    //  StartCoroutine(GpViewRun( gpData));
        //}
        private void GpViewInit(GpData gpData, StageResultData stageData)
        {
            rankDownEffect.gameObject.SetActive(false);
            rankUpEffect.gameObject.SetActive(false);
            rankUpGaugeEffect.gameObject.SetActive(false);

            addPointText.text = gpData.addGp.ToString();
            
            int gaugeGp = gpData.preGp - gpData.minGpTable[gpData.preRank];

            // UpdateRankUI(gpData.preRank, gaugeGp, gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank], gpData.preGp); // 기존방
            UpdateRankUI(gpData.preRank, gpData.preGp , gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp , gpData.minGpTable[gpData.preRank]);

            StartCoroutine(GpViewRun(gpData, stageData));
        }

        private IEnumerator GpViewRun(GpData gpData , StageResultData stageData)
        {
            GlobalGameObjects.Instance.VisibleGlobalGameObject(false);
            
            yield return new WaitForSeconds(0.1f);
       
            int appGp = Mathf.Abs(gpData.addGp);

            // float divide = appGp * (0.003f * (100f / appGp));//Time.deltaTime; // (넣을 add) / (100 *0.003)
            float divide = (float)(appGp * Time.deltaTime *1.25f) ;

            int gaugeLength = gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank];

            float addGp = appGp;
            float gaugeGpf = gpData.preGp - gpData.minGpTable[gpData.preRank];
            float gpf = gpData.preGp;

            //  UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
            UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);

            popAddTextRect.gameObject.SetActive(gpData.addGp != 0);
            if (gpData.addGp > 0)
            {
                SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("UP_Point").Load());
            }
            else if(gpData.addGp < 0)
            {
                SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("Down_Point").Load());
            }
       

            if (gpData.addGp > 0)
            {
                while (addGp > 0)
                {
                    addGp -= divide;
                    gaugeGpf += divide;
                    gpf += divide;

                    if (gaugeGpf >= gaugeLength)
                    {
                        // UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
                        UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);
                        // 효과 보여 주기
                        rankUpEffect.gameObject.SetActive(true);
                        rankUpEffect.AnimationState.SetAnimation(0, "animation", false);
                        rankUpGaugeEffect.AnimationState.SetAnimation(0, "animation", false);

                        SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_rankup"));

                        gpData.preRank++;
                        gaugeLength = gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank];
                        gaugeGpf = 0;

                        yield return new WaitForSeconds(0.5f);
                    }

                    //UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
                    UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);
                    yield return null;
                }
            }
            else if (gpData.addGp < 0)
            {
                while (addGp > 0)
                {
                    addGp -= divide;
                    gaugeGpf -= divide;
                    gpf -= divide;

                    if (gaugeGpf < 0 && gpData.preRank > 0)
                    {
                        //UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
                        UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);
                        // 효과 보여 주기
                        rankDownEffect.gameObject.SetActive(true);
                        rankDownEffect.AnimationState.SetAnimation(0, "animation", false);
                        gpData.preRank--;
                        gaugeLength = gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank];
                        gaugeGpf = gaugeLength;
                        SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_rankdown"));
                        yield return new WaitForSeconds(0.5f);
                    }

                    //UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
                    UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);
                    yield return null;
                }
            }
            //UpdateRankUI(gpData.grade, (int)gaugeGpf, gaugeLength, gpData.gp);
            UpdateRankUI(gpData.preRank, gpData.gp, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);

            NLog.Log("최종 랭크 " + gpData.grade);

            yield return new WaitForSeconds(0.5f);

            // == 두번째 패널 열기 
            StartCoroutine(UIMove(gpData, stageData));
          
         
            
            GlobalGameObjects.Instance.VisibleGlobalGameObject(true);
            GlobalGameObjects.Instance.RefreshTransformWithValue(true, CooperationUI.Cash, CooperationDetailUI.Gold);
           
        }
        float downTime = 0.3f;

        private  IEnumerator StageView(StageResultData stageData)
        {
            starAni.gameObject.SetActive(true);

            foreach (var item in digits) item.gameObject.SetActive(false);

            digits[stageData.stageId.ToString().Length - 1].gameObject.SetActive(true);
            foreach (var item in stageNum) item.text = stageData.stageId.ToString();

            clear.gameObject.SetActive(false);
            score.text = stageData.score ==0 ? "0" : (stageData.score + stageData.itemScore).ToString("###,###");
            reward.text = "x" + (stageData.rewardGold ==0 ? "0" : (stageData.rewardGold + stageData.evtRewardGold).ToString("###,###") );

            yield return new WaitForSeconds(downTime); // 

            replayBtn.onClick.RemoveAllListeners();
            replayBtn.onClick.AddListener(() => {
                Close();
                StageStartDialogUI.DoModal(stageData.stageId, () => SceneManager.Instance.PushScene(SceneType.World) );
            });
            if (replayBtn.gameObject.GetComponent<ButtonClickEvent>() == null)
            replayBtn.gameObject.AddComponent<ButtonClickEvent>();

            challengePop.gameObject.SetActive(stageData.curtStar < 3);


            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(() =>
            {
                // ClientData

                NLog.Log("<color=blue> 다음 씬 셋팅</color> ");

                Close();

                if(ClientData.Instance.TableData.GetData<Data.StageData>(TableData.Type.TABLE_STAGE, stageData.stageId +1) == null)
                {
                    Data.LanguageData languageData = ClientData.Instance.TableData.GetData<Data.LanguageData>(TableData.Type.TABLE_LANGUAGE, 343);
                    AlertDialogUI.DoModal(AlertDialogUI.Type.Alert, languageData.kr.Replace("\\n", "\n"), ()=> SceneManager.Instance.PushScene(SceneType.World));
                }
                else
                {
                    StageStartDialogUI.DoModal(stageData.stageId + 1, () => SceneManager.Instance.PushScene(SceneType.World));
                }


            });
            nextBtn.gameObject.AddComponent<ButtonClickEvent>();

            StartCoroutine(StarViewCoroutine(stageData.curtStar, stageData.isMission));

        }

        string[] appearanceNames = new string[] { "star01_appearance", "star02_appearance2", "star03_appearance3" };
        string[] idleNames = new string[] { "star01_idle", "star02_idle", "star03_idle" };
        private IEnumerator StarViewCoroutine(int n, bool isMissionClear)
        {
            starAni.AnimationState.SetAnimation(0, appearanceNames[n - 1], false);
           // SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetAudioClipData("UP_Point").Load());
           // SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetAudioClipData("Down_Point").Load());
            StartCoroutine( StarSoundCoroutine(n));
            yield return new WaitForSeconds(SkeletonCommon.Duration(starAni, appearanceNames[n - 1]));
            starAni.AnimationState.SetAnimation(0, idleNames[n - 1], true);

           // yield return new WaitForSeconds(0.5f);
          //  if (isMissionClear)
           // {
                clear.gameObject.SetActive(true);
                clear.AnimationState.SetAnimation(0, "01_appearance", false);
            //}

          
        }

        private IEnumerator StarSoundCoroutine(int n)
        {
            yield return new WaitForSeconds(0.28f);
            switch (n)
            {
                case 1:
                   
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[0]);
                   
                    break;
                case 2:
                  
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[0]);
                    yield return new WaitForSeconds(0.2f);
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[1]);
                  
                    break;
                case 3:
                 
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[0]);
                    yield return new WaitForSeconds(0.2f);
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[1]);
                    yield return new WaitForSeconds(0.2f);
                    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[2]);
                   
                    break;

            }
            yield return new WaitForSeconds(0.2f);
            //for (int i = 0; i < n; i++)
            //{
            //    SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetstarGradeAudioClips()[i]);
            //    yield return new WaitForSeconds(0.2f);
            //}
            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("stage_clear").Load());
        }
    


        private IEnumerator UIMove(GpData gpData, StageResultData stageData)
        {
            float correctionVal = 1;
            Canvas canvas = FindCanvas();
            if (canvas != null)
            {
                correctionVal = canvas.GetComponent<RectTransform>().lossyScale.x;
            }
            Vector2 leftTarget = new Vector2(-250 * correctionVal, 0);
            Vector2 RightTarget = new Vector2(250 * correctionVal, 0);

            leftUIParent.DOMove(leftTarget, 0.3f).SetEase(Ease.InOutCubic);
            yield return new WaitForSeconds(0.3f);

            StartCoroutine(StageView(stageData));
            PopupEvent(gpData, stageData);
            rightUIParent.gameObject.SetActive(true);
            rightUIParent.DOMove(RightTarget, downTime).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(downTime);

            firework.gameObject.SetActive(false);
            // 무빙이 끝난 후에 이벤트와 별 보이기 



            closeBtnParent.gameObject.SetActive(true);
        }

        private Canvas FindCanvas()
        {
            Transform currentTransform = transform;

            // 부모 캔버스를 찾을 때까지 반복
            while (currentTransform != null)
            {
                // 현재 Transform이 Canvas 컴포넌트를 가지고 있는지 확인
                Canvas canvas = currentTransform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    // 부모 캔버스를 찾았을 때의 로직
                    Debug.Log("부모 캔버스를 찾았습니다.");
                    return canvas;
                }

                // 부모 Transform으로 이동
                currentTransform = currentTransform.parent;
            }

            return null;
        }


    }
}
