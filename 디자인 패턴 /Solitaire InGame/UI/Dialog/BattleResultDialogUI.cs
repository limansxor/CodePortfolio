using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattlePacket;
using NGle.Solitaire.Data;
using NGle.Solitaire.Dialog;
using NGle.Solitaire.Network;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Support;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{
    public class BattleResultDialogUI : DialogBase
    {
        protected override void OnHide()
        {

        }

        protected override void OnRestore()
        {

        }

        protected override void OnShow()
        {

        }
        private static BattleResultDialogUI currentDialog;

        public static void DoModal(List<Sprite> rankSprites,
            float waitTime,
           GpData gpData)
        {
            DialogManager.CreateDialog<BattleResultDialogUI>("Dialogs/BattleResultDialogUI.prefab",
                delegate (BattleResultDialogUI dialog)
                {
                    if (dialog)
                    {
                        dialog.Show();
                        dialog.InitEvent(rankSprites,
                            waitTime,
                            gpData);

                        currentDialog = dialog;
                    }
                });
        }

        [SerializeField] Button miniPopupGpBtn;
        [SerializeField] MiniPopupGp miniPopupGp;

        [SerializeField] Button closeBtn;

        [SerializeField] SkeletonGraphic rankDownEffect;
        [SerializeField] SkeletonGraphic rankUpEffect;
        [SerializeField] SkeletonGraphic rankUpGaugeEffect;

        [SerializeField] Text addPointText;

        [SerializeField] Image rankImg;
        [SerializeField] Text gaugePointText;
        [SerializeField] Image gaugeBarImg;
        [SerializeField] Text currentPointText;
        [SerializeField] RectTransform popAddTextRect;

        [SerializeField] Button rematchingBtn;
        [SerializeField] Image disableRematching;
        [SerializeField] List<Image> waitingImgs;
        [SerializeField] Text rematchingCountingTx;

        [SerializeField] Button matchingBtn;

        [SerializeField] Transform matchingParent;
        [SerializeField] Text matchingCountingTx;
        [SerializeField] Text idelMatchingTx;


        List<Sprite> _rankSprites;
        public float[] miniPopMoveRange = { -200, 200 };
        private Coroutine rematchingCoroutine;

        private float _waitTime;
        private Coroutine matchingCoroutrine;

        private bool isRematching;
        private bool isMatching;

        public void InitEvent(List<Sprite> rankSprites,
            float waitTime,
           GpData gpData
            )
        {
            closeBtn.onClick.RemoveAllListeners();
            closeBtn.onClick.AddListener(() =>
            {
                Close();

                if (ClientData.Instance.UserData.BattleInfo.isAI == false)
                {
                    var reqPacket = PacketHelper.CreateMessagePacket<PassBattleRecancel>(); // 리매칭취소 코드                                                                                          
                    WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
                }

                WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);

                ClientData.Instance.UserData.BattleInfo.isAI = false;

             //   SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_cancel_button").Load());

                SceneManager.Instance.PushScene(SceneType.World);
            });
         

            _rankSprites = rankSprites;

            GPView(gpData);

            miniPopupGp.Initialized(ClientData.Instance.UserData.StageDataGroup.GetAcquiredStarCount(),
                0, 0,
              gpData.crown, gpData.crown - gpData.preCrown, gpData.crownConversionPoint);
            miniPopupGp.gameObject.SetActive(false);

            miniPopupGpBtn.onClick.RemoveAllListeners();
            miniPopupGpBtn.onClick.AddListener(() => miniPopupGp.gameObject.SetActive(true));
            miniPopupGpBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

            rematchingBtn.onClick.RemoveAllListeners();
           rematchingBtn.onClick.AddListener(OnRematching);
            rematchingBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

            matchingBtn.onClick.RemoveAllListeners();
            matchingBtn.onClick.AddListener(OnMatching);
            matchingBtn.gameObject.GetOrAddComponent<ButtonClickEvent>();

            rematchingCoroutine = StartCoroutine(RematchingCoroutine(waitTime));
           

            idelMatchingTx.gameObject.SetActive(true);
            matchingParent.gameObject.SetActive(false);

       
            _waitTime = waitTime;

            isRematching = false;


        }
        public static void OnOutChallenger()
        {
            currentDialog.OutChallenger();
        }

        public void OutChallenger()
        {
            //  NLog.Log("상대방 플레어가 나간 버튼 상태 구현 ");
        
            if (rematchingCoroutine != null) StopCoroutine(rematchingCoroutine);
            disableRematching.gameObject.gameObject.SetActive(true);

            AlertDialogUI.DoModal(AlertDialogUI.Type.Alert, ClientData.Instance.TableData.TextDataImplement.GetText("TEXT.outgame.battle_retry_fail")
                , null, null);
        }

        private void GPView(GpData gpData)
        {
            GpViewInit(gpData);
            StartCoroutine(GpViewRun(gpData));
        }

        private void UpdateRankUI(int rank, int gaugeGp, int gaugeLength, int currnetGp , int min)
        {
            rank = Mathf.Clamp(rank, 0, _rankSprites.Count - 1);
            rankImg.sprite = _rankSprites[rank];

            gaugePointText.text = gaugeGp.ToString() + "/" + (rank == _rankSprites.Count - 1 ? "∞" : gaugeLength.ToString());
            float amount = 0;
            // if (gaugeLength > 0) amount = Mathf.Clamp((float)gaugeGp / gaugeLength, 0, 1);
            if (gaugeLength > 0) amount = Mathf.Clamp((float)(gaugeGp - min) / (gaugeLength - min), 0, 1);
            gaugeBarImg.fillAmount = amount;
            currentPointText.text = currnetGp.ToString();

            Vector2 prePos = popAddTextRect.anchoredPosition;

            popAddTextRect.anchoredPosition = new Vector2(Mathf.Lerp(miniPopMoveRange[0], miniPopMoveRange[1], amount), prePos.y);
        }

        private void GpViewInit(GpData gpData)
        {
            rankDownEffect.gameObject.SetActive(false);
            rankUpEffect.gameObject.SetActive(false);
            rankUpGaugeEffect.gameObject.SetActive(false);

            addPointText.text = gpData.addGp.ToString();

            popAddTextRect.gameObject.SetActive(gpData.addGp != 0);
      
            int gaugeGp = gpData.preGp - gpData.minGpTable[gpData.preRank];

            // UpdateRankUI(gpData.preRank, gaugeGp, gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank], gpData.preGp);
            UpdateRankUI(gpData.preRank, gpData.preGp, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);

            StartCoroutine(GpViewRun(gpData));
        }

        private IEnumerator GpViewRun(GpData gpData)
        {
            yield return new WaitForSeconds(0.1f);

            int appGp = Mathf.Abs(gpData.addGp);
            //float divide = appGp * (0.003f * (100f / appGp));//Time.deltaTime; // (넣을 add) / (100 *0.003)
            float divide = appGp * Time.deltaTime*1.25f;
            int gaugeLength = gpData.minGpTable[gpData.preRank + 1] - gpData.minGpTable[gpData.preRank];

            float addGp = appGp;
            float gaugeGpf = gpData.preGp - gpData.minGpTable[gpData.preRank];
            float gpf = gpData.preGp;

            // UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
            UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);


            if (gpData.addGp > 0)
            {
                SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("UP_Point").Load());
            }
            else if (gpData.addGp < 0)
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

                    //  UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
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
                        // UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);
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

                    //UpdateRankUI(gpData.preRank, (int)gaugeGpf, gaugeLength, (int)gpf);\
                    UpdateRankUI(gpData.preRank, (int)gpf, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);
                    yield return null;
                }
            }
            //  UpdateRankUI(gpData.grade, gpData.gp - gpData.minGpTable[gpData.grade], gaugeLength, gpData.gp);
            UpdateRankUI(gpData.preRank, gpData.gp, gpData.minGpTable[gpData.preRank + 1] - 1, gpData.addGp, gpData.minGpTable[gpData.preRank]);

            NLog.Log("최종 값  " + gpData.gp);

            yield break;
        }

        private IEnumerator RematchingCoroutine(float waitTime)
        {
            // 타이밍 재는 중 // 매칭은 가만히
            waitingImgs.ForEach(itme => itme.gameObject.SetActive(false));
            disableRematching.gameObject.gameObject.SetActive(false);
            waitingImgs[0].gameObject.SetActive(true);

            if (ClientData.Instance.UserData.BattleInfo.isAI)
            {
                yield return new WaitForSeconds(1.0f);
                disableRematching.gameObject.gameObject.SetActive(true);
   
                StopCoroutine(rematchingCoroutine);
                OutChallenger();
            }

            float t = waitTime;
            while (t >= 0)
            {

                t -= Time.deltaTime;
                rematchingCountingTx.text = ((int)t).ToString();
                yield return null;
            }

            disableRematching.gameObject.gameObject.SetActive(true);
            //_onRematching.Invoke(false);
            // 
            ClientData.Instance.UserData.BattleInfo.isReGameDecision = false;
        }

        private void OnRematching()
        {
            isRematching = !isRematching;

            if (isRematching)
            {
                waitingImgs[0].gameObject.SetActive(false);
                waitingImgs[1].gameObject.SetActive(true);

                NLog.Log("리매칭 합니다 ");

                var reqPacket = PacketHelper.CreateMessagePacket<C2SBattleReplay>();
                WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
                ClientData.Instance.UserData.BattleInfo.isReGameTry = true;
            }
            else
            {
                if (rematchingCoroutine != null) StopCoroutine(rematchingCoroutine);
                disableRematching.gameObject.gameObject.SetActive(true); // 버튼 비활성 

                if (ClientData.Instance.UserData.BattleInfo.isReGameOtherDecision)
                {
                    NLog.Log("내가 리매칭 취소 합니다 ");
                    ClientData.Instance.UserData.BattleInfo.isReGameDecision = false; // 나도 거부


                    var reqPacket = PacketHelper.CreateMessagePacket<PassBattleRecancel>(); //리매칭취소 코드
                    WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
                }
                    

            }
        }

        private void OnMatching()
        {
            isMatching = !isMatching;

            NLog.Log($"isReGameOtherDecision = {ClientData.Instance.UserData.BattleInfo.isReGameOtherDecision} | isReGameDecision {ClientData.Instance.UserData.BattleInfo.isReGameDecision}");

            if (ClientData.Instance.UserData.BattleInfo.isReGameOtherDecision && //상대방은 거부의사가 없으면
                 ClientData.Instance.UserData.BattleInfo.isReGameDecision) //  내가 거부 한적이 없으면 
            {

               // WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.BATTLE);
               // WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);

                var reqPacket = PacketHelper.CreateMessagePacket<PassBattleRecancel>();// 리매칭취소 코드 
                
                WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);

                //WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);
            }

           // await Task.Yield();

            if (isMatching)
            {
                if (rematchingCoroutine != null) StopCoroutine(rematchingCoroutine);
                disableRematching.gameObject.gameObject.SetActive(true);

                matchingCoroutrine = StartCoroutine(MatchingCoroutine(_waitTime));

                Data.StageData.BattleType mode = ClientData.Instance.UserData.BattleInfo.battleType;

                if (ClientData.Instance.UserData.BattleInfo.MatchingInfo != null)
                    ClientData.Instance.UserData.BattleInfo.MatchingInfo.isNewGame = true;

                ClientData.Instance.UserData.RequestBattleMatching(mode, true);
                //if (ClientData.Instance.UserData.BattleInfo.isAI)
                //{
                //    ClientData.Instance.UserData.RequestBattleMatching(mode, false); // 밖에서 실행 한 것과 같다 
                // }
                //else
                //{
                //    ClientData.Instance.UserData.RequestBattleMatching(mode, true);
                //}

                if (isMatching)
                {
                    NLog.Log("새로운 플레이어를 찾습니다 ");
                }
                //else
                //{
                //    NLog.Log("새로운 플레이어를 찾기 취소  ");
                //}
               

            }
            else
            {

                AlertDialogUI.DoModal(AlertDialogUI.Type.Confirm, ClientData.Instance.TableData.TextDataImplement.GetText("TEXT.outgame.battle_newgame_cancel"),
                    () =>
                    {
                        if (matchingCoroutrine != null) StopCoroutine(matchingCoroutrine);

                        matchingParent.gameObject.SetActive(false);

                        idelMatchingTx.text = ClientData.Instance.TableData.TextDataImplement.GetText("TEXT.outgame.battle_result_newgame");
                        idelMatchingTx.gameObject.SetActive(true);

                        WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);

                        if (isMatching)
                        {
                            NLog.Log("새로운 플레이어를 찾습니다 ");
                        }
                        else
                        {
                            NLog.Log("새로운 플레이어를 찾기 취소  ");
                        }
                    },
                    null

                    );
            }
        }

        private IEnumerator MatchingCoroutine(float waitTime)
        {
            idelMatchingTx.gameObject.SetActive(false);
            matchingParent.gameObject.SetActive(true);
            float t = waitTime;
            while (t >= 0)
            {
                t -= Time.deltaTime*0.9f;
                matchingCountingTx.text = ((int)t).ToString();
                yield return null;
            }
            idelMatchingTx.gameObject.SetActive(true);
            matchingParent.gameObject.SetActive(false);

            NLog.Log("새로운 플레이어를 찾기 취소  ");

            isMatching = !isMatching;
            WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);
            yield break;
        }

    }
}
