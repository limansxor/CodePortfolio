using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BattlePacket;
using Google.Protobuf.Collections;
using NGle.Solitaire.Auth;
using NGle.Solitaire.Data;
using NGle.Solitaire.Network;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Support;
using UnityEngine;
using UnityEngine.Events;
using static NGle.Solitaire.Data.UserData;
using static NGle.Solitaire.Data.UserData.LocalUserBattleInfo;

namespace NGle.Solitaire.RunGame
{
    public class BattleRunGameMgr : SingletonRunGameMgr<BattleRunGameMgr>
    {
        
        [SerializeField] BattleGameUI gameUI;

        [SerializeField] AI ai;

        private bool isDrawEventEnable;

        private int round;
        private int roundMax;

        AttackType tempAttackType = AttackType.Blink;
        private Coroutine challengerPageNextCoroutine;

        public Canvas evtCanvas;

        #region override

        public override void Initialized()
        {
            base.Initialized();

            NLog.Log("PreviousInit 실행 ");

            isDrawEventEnable = true;
            isRecvBattleVsinfoEnable = true;
            isGameEndEeventExecuted = true;

            runGameEvent = new BattleRunGameEvent();
            (runGameEvent as BattleRunGameEvent).AddEvtGameOut(OnOtherReGameOut);
        }

        
        public override void GameInit(int stage, RunGameMgr runGameMgr)
        {
            base.GameInit(stage, runGameMgr);

            gameUI.Initialized();

            ClientData.Instance.UserData.BattleInfo.PlayerInfo.StageIdNext();

            ClientData.Instance.UserData.BattleInfo.endInfo = null;

            (runGameEvent as BattleRunGameEvent).AddEvtAttack(OnAttack);
        }

        public void OnGameRun() => gameBoard.OnGameRun();
   
        public void ChallengerPageNextEnd()
        {
            if (challengerPageNextCoroutine != null) StopCoroutine(challengerPageNextCoroutine);
        
            challengerPageNextCoroutine = null;
        }


        public override void OnTimeStart()
        {
            // 타임 시작
            gameUI.TimerStart();
        }

        public override void ScenePause(bool pause)
        {
            if(pause == false)
            {
                //var reqPacket = PacketHelper.CreateMessage<C2SBattleReconnect>();

                //reqPacket.Roomid = ClientData.Instance.UserData.BattleInfo.MatchingInfo.roomId;
                //reqPacket.Sid = AuthProvider.Instance.Provider.ServerId;
              
                //WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
            }
        }

        public override void OnGamePause()
        {
            // 배틀 게임은 멈추지 않는다.
        }

        public override void ClearRunGameData()
        {
            base.ClearRunGameData();
            ClientData.Instance.UserData.BattleInfo.UnRegisterListener();
        }

        #endregion

        #region 게임 결과 관련

        public override void OnTimeOver()
        {
            if (isDrawEventEnable)
            {
                isDrawEventEnable = false;

                if (ai.aiRunCoroutine != null) StopCoroutine(ai.aiRunCoroutine);

                gameBoard.GamePause(true);

                ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Add(false);
                ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.Add(false);

             

                if (ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Count < 3)
                {
                    (gameUI as BattleGameUI).isEndDraw = true;
                    gameUI.Draw();
                }
                else
                {
                    int playerCount = 0;
                    int ChallengerCount = 0;

                    ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.ForEach(item => { if (item) playerCount++; });
                    ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.ForEach(item => { if (item) ChallengerCount++; });

                    if (playerCount == ChallengerCount)
                    {
                        (gameUI as BattleGameUI).isEndDraw = true;
                        gameUI.Draw();
                    }
                    else if (playerCount > ChallengerCount) gameUI.GameWin();
                    else gameUI.GameLose();
                }
            }
        }

        public override void OnGameEnd() // 이건 확실 게임 승리 
        {
            if (ai.aiRunCoroutine != null) StopCoroutine(ai.aiRunCoroutine);

            ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Add(true);
            ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.Add(false);

            gameUI.GameWin();
            gameBoard.GamePause(true);

            isDrawEventEnable = false;
        }

        public void GameLose()
        {
            ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Add(false);
            ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.Add(true);

            gameUI.GameLose();
            (gameBoard as BattleGameBoard).GamePause(true);

            isDrawEventEnable = false;
        }

        #endregion

        #region 개발단에서 도움 되는 
        public void OnForcedVictory()//치트 목적 
        {
            var reqPacket = PacketHelper.CreateMessagePacket<PassBattleBlock>();
            reqPacket.Effect = (int)tempAttackType; // 물음표인지 블링크 인지
            reqPacket.Remove.Add(-1);
            reqPacket.Remove.Add(-1);
            reqPacket.Phaseend = 1; // 고정 값 보내기 
            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);

            //OnGameEnd();

           // OnPageNext();
        }
        #endregion

        #region AI
        public void InitializedAI(int round)
        {
            NLog.Log("AI 초기화 ");
            ai.Initialized(round);
        }
        #endregion

        #region 클라 -> 서버

      

        public override void OnShuffle()
        {
            gameBoard.Shuffle();

            if (ClientData.Instance.UserData.BattleInfo.isAI == false)
            {
                var reqPacket = PacketHelper.CreateMessagePacket<PassBattleBlock>();
                reqPacket.Remove.Add(-100);
                reqPacket.Remove.Add(-100);
                reqPacket.Effect = 100;
                reqPacket.Phaseend = 0;

                WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
            }
        }


        public override void OnBlockRemove(Action<Vector2Int, int> remove, Vector2Int idx, List<BlockPlace> blockPlace)
        {

            base.OnBlockRemove(remove, idx, blockPlace);

            gameUI.ChangeProcessBar(gameBoard.GetProcess());

            var reqPacket = PacketHelper.CreateMessagePacket<PassBattleBlock>();
            reqPacket.Effect = 0;
            reqPacket.Remove.Add(gameBoard.PreClickIdx());
            reqPacket.Remove.Add(gameBoard.To1D(idx));
            reqPacket.Phaseend = runGameData.blockCount == 0 ? 1 : 0;


            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);

        }
        public override void OnHitHammer(Action<Vector2Int, int> remove, Vector2Int idx, List<BlockPlace> blockPlaces)
        {
            base.OnHitHammer(remove, idx, blockPlaces);

            gameUI.ChangeProcessBar(gameBoard.GetProcess());

            var reqPacket = PacketHelper.CreateMessagePacket<PassBattleBlock>();
            reqPacket.Effect = 1;
            reqPacket.Remove.Add(gameBoard.To1D(gameBoard.HammerPairIdx().first));
            reqPacket.Remove.Add(gameBoard.To1D(gameBoard.HammerPairIdx().last));

            reqPacket.Phaseend = runGameData.blockCount == 0 ? 1 : 0;
            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
        }


        public void OnRematching(bool isMatching) //BattleResultDialogUI 관련
        {
        
        }

        public void OnSendEmoticon(int type)
        {
            NLog.Log("이모티콘 메시지 통신 보내기 ");

            var reqPacket = PacketHelper.CreateMessagePacket<PassBattleEmoji>();
            reqPacket.Emoji = type;
            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
        }

        public void OnReconnect()
        {
            var reqPacket = PacketHelper.CreateMessagePacket<C2SBattleReconnect>();
            reqPacket.Roomid = ClientData.Instance.UserData.BattleInfo.MatchingInfo.roomId;

            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
            isRecvBattleVsinfoEnable = true;
        }

        public void OnAttack(AttackType attackType, bool isMy, List<BlockPlace> idxs) // 공격을 한 경우 //
        {
            if (isMy == false) return; // 받는 경우 안들어 온다 
            if (gameBoard.GetTouchEnable() == false) return;
            if (idxs == null) return;

            var reqPacket = PacketHelper.CreateMessagePacket<PassBattleBlock>();
            reqPacket.Effect = (int)attackType; // 물음표인지 블링크 인지
          //  if (reqPacket.Remove == null) return;

            reqPacket.Remove.Add(idxs[0].idx + reqPacket.Effect * 1000); // 구분을 위해서 1000 단위에 이팩트 정보를 보낸다

            if (idxs.Count > 1) reqPacket.Remove.Add(idxs[1].idx + reqPacket.Effect * 1000);
            else
            {
                reqPacket.Remove.Add(-1);
            }

            reqPacket.Phaseend = 0; // 고정 값 보내기 
            WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);

            NLog.Log($"어텍 메시지 통신 보내기 {idxs[0].ToString()}");

        }

        #endregion

        #region 서버-> 클라

        private bool isGameEndEeventExecuted;
        public override void ContinueUpdate()
        {
            if (ClientData.Instance.UserData.BattleInfo.isAI == false)
            {
                if (gameBoard.GetTouchEnable())
                {
                    RecvRemoveBlock();

                    RecvEmoticon();

                    RecvBattleVsinfo();
                }
                RecvGameEnd();
              
            }
        }

        public void RecvEmoticon()
        {
            if (ClientData.Instance.UserData.BattleInfo.emoji != null)
            {
                int type = ClientData.Instance.UserData.BattleInfo.emoji.emoji;
                gameUI.RecvEmoticon(type);

                ClientData.Instance.UserData.BattleInfo.emoji = null;
            }
        }

        public void OnOtherReGameOut()
        {
            //NLog.Log($"isReGameOtherDecision = {ClientData.Instance.UserData.BattleInfo.isReGameOtherDecision} | isReGameDecision {ClientData.Instance.UserData.BattleInfo.isReGameDecision}");

            NLog.Log("상대방 플레어가 나갔습니다 " + ClientData.Instance.UserData.BattleInfo.isReGameDecision);

            if (ClientData.Instance.UserData.BattleInfo.isReGameDecision) // 방지 책 있는데 
            {
              
                BattleResultDialogUI.OnOutChallenger();

                ClientData.Instance.UserData.BattleInfo.isReGameDecision = false;
            }
        }

        public void RecvRemoveBlock() // 삭제된 블록 보내기 // 터치 가능 할때만 
        {
            // 데이터는 무조건 2개 날라 갈수 밖에 없다 // 공격에서도 날러 온다 
            if (ClientData.Instance.UserData.BattleInfo.removeDatas != null && 
                ClientData.Instance.UserData.BattleInfo.removeDatas.Count > 0 &&
                challengerPageNextCoroutine == null) // 순서대로 오겠지 믿음의 영역 
            {
                RemoveData removeData = ClientData.Instance.UserData.BattleInfo.removeDatas[0]; // 하나씩 읽어 드린다 

                if (removeData == null) return;

                List<int> idxs = new List<int>() {
                    removeData.f.x == -1 ? -1 : gameBoard.To1D(removeData.f),
                    (removeData.l.x == -1 ) ? -1 : gameBoard.To1D(removeData.l) };

                if (idxs[0] == -1)
                {
                  
                    ClientData.Instance.UserData.BattleInfo.removeDatas.RemoveAt(0);
                    return; // 안정 장치
                }
         

                switch (removeData.effect)
                {
                    case 0:
                    case 1:
                        (gameBoard as BattleGameBoard).ChallengerMatchingBlock(removeData.f, removeData.l, removeData.t);

                        gameUI.ChangeChallengerProcessBar((gameBoard as BattleGameBoard).GetChallengerProcess());
                        break;
                    case (int)AttackType.Question:
                        {
                            List<BlockPlace> blockPlaces = new List<BlockPlace>()
                            {
                             new BlockPlace(idxs[0] % ((int)AttackType.Question * 1000)  , gameBoard.GetBlockPos(idxs[0] % ((int)AttackType.Question * 1000)  ))
                             };
                            if (idxs[1] != -1) blockPlaces.Add(new BlockPlace(idxs[1] % ((int)AttackType.Question * 1000), gameBoard.GetBlockPos(idxs[1] % ((int)AttackType.Question * 1000) )));


                            (runGameEvent as BattleRunGameEvent).onAttack.Invoke(AttackType.Question, false, blockPlaces);
                        }
                        break;
                    case (int)AttackType.Blink:
                        {
                            List<BlockPlace> blockPlaces = new List<BlockPlace>()
                            {
                             new BlockPlace(idxs[0] % ((int)AttackType.Blink * 1000) , gameBoard.GetBlockPos(idxs[0] % ((int)AttackType.Blink * 1000) ) )
                             };
                            if (idxs[1] != -1) blockPlaces.Add(new BlockPlace(idxs[1] % ((int)AttackType.Blink * 1000), gameBoard.GetBlockPos(idxs[1] % ((int)AttackType.Blink * 1000) )));

                            (runGameEvent as BattleRunGameEvent).onAttack.Invoke(AttackType.Blink, false, blockPlaces); 
                        }
                        break;
                    case 100: // 위에서 처리 
                        NLog.Log("셔플 받음 ");
                        RecvShuffle();
                        break;
                }


                if (removeData.isPageNext)
                {
                    challengerPageNextCoroutine = StartCoroutine((gameBoard as BattleGameBoard).ChallengerNextPageCoroutine());
                }
                ClientData.Instance.UserData.BattleInfo.removeDatas.RemoveAt(0);
            }
        }

        public void RecvGameEnd()
        {
            if (isGameEndEeventExecuted == false) return;

            if (ClientData.Instance.UserData.BattleInfo.endInfo != null && gameBoard.StatePause() == false)
            {
               // StopAllCoroutines(); // 모든 코르틴 종료 
                if (ClientData.Instance.UserData.BattleInfo.endInfo.result == 2)
                {
                    OnTimeOver();
                    (runGameEvent as BattleRunGameEvent).onBattleGameTimeOver?.Invoke();
                }
                else if (ClientData.Instance.UserData.BattleInfo.endInfo.result == 1) // 승
                {
                    NLog.Log("게임 승리 메시지 받았습니다 ");
                    OnGameEnd();
                }
                else
                {
                    GameLose();

                    foreach (var item in (gameBoard as BattleGameBoard).challengerBlocks)
                    {
                        if(item != null)
                        item.Remove();
                    }
                  

                }

                //
                isGameEndEeventExecuted = false;
            }

        }


        private bool isRecvBattleVsinfoEnable;

        private void RecvBattleVsinfo() // 계속 조정 ~ 
        {

            if (isRecvBattleVsinfoEnable)
            {

                if (ClientData.Instance.UserData.BattleInfo.endInfo == null &&
                    ClientData.Instance.UserData.BattleInfo.continueInfo != null)

                {
                    NLog.Log("<color=green> RecvBattleVsinfo </color>");

                    ContinueInfo info = ClientData.Instance.UserData.BattleInfo.continueInfo;
                    // ClientData.Instance.UserData.BattleInfo.PlayerInfo?.type == StageData.BattleType.BATTLE_3_2_MATCH &&

                    if (ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Count != info.round)
                    {
                        // 씬 다시 불러 오기

                        NLog.Log("RecvBattleVsinfo.NextRound currentRound : " + ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Count);

                        ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Add(false);
                        ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.Add(true);
                        SceneManager.Instance.PushScene(SceneType.MultiGame,
                            new BattleParameter() { battleStartType = BattleStartType.NextRound },
                            new SimplifyLoading());
                        isRecvBattleVsinfoEnable = false;
                    }
                    else // 라운드 증가 후에 변화 주기 
                    {
                        NLog.Log("RecvBattleVsinfo Page and remove ");
                        // 게임 중일 때 발동  
                        //if ((_gameBoard as BattleGameBoard).GetChallengerPage() != info.pageNum)
                        //{
                        //    // 페이지 변화 주고
                        (gameBoard as BattleGameBoard).ChallengerPageSkip(info.pageNum);
                        //  }

                        for (int i = 0; i < info.removes.Count; i += 2)
                        {
                            (gameBoard as BattleGameBoard).ReconnectRemove(new List<int> { info.removes[i], info.removes[i + 1] });
                        }

                        //    info.removes.ForEach((item) => _gameBoard.ReconnectRemove(item)); // 셔플도 포함 되어 있다 -100 이면 셔플 한다 

                        (gameBoard as BattleGameBoard).BattleBlockCountCheck();


                        gameUI.ReconnectSetTime(runGameData.starRewardTimes[0] - (info.time / 1000));  // 시간값 재 셋팅
                        gameUI.ChangeChallengerProcessBar((gameBoard as BattleGameBoard).GetChallengerProcess());

                        (gameBoard as BattleGameBoard).ChallengerGameRun(); 

                        ClientData.Instance.UserData.BattleInfo.continueInfo = null;
                        isRecvBattleVsinfoEnable = false;
                    }
                }
            }
        }

        public void RecvShuffle() // 셔플이 왔어요 
        {
            (gameBoard as BattleGameBoard)?.ChallengerShuffle();
        }

        public override void OnEvtBlockRemove() { }

        #endregion


    }
}
