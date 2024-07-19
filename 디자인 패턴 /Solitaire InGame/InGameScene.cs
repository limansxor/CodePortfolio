using System;
using System.Collections;
using BattlePacket;
using Google.Protobuf;
using NGle.Solitaire.Data;
using NGle.Solitaire.Network;
using NGle.Solitaire.RunGame;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Support;
using UnityEngine;

namespace NGle.Solitaire.Scene
{
    public class InGameScene : SceneInfo
    {
        protected override IEnumerator OnSceneEnabled(Transform parentTm, SceneType precSceneType, params SceneParameter [] sceneParams)
        {
            // 이런식으로 필요한 param 값이 있는 경우 찾는다
            if (sceneParams != null)
            {
                BattleParameter battleParameter = GetSceneParam<BattleParameter>(sceneParams);

                if (battleParameter == null)
                {
                    StageRunGameMgr.Instance.Initialized();
                    StageRunGameMgr.Instance.GameInit(ClientData.Instance.UserData.StageDataGroup.selectedStageId, StageRunGameMgr.Instance);
                }
                else
                {
                    BattleRunGameMgr.Instance.Initialized();

                    while (true)
                    {
                        var battleInfo = ClientData.Instance.UserData.BattleInfo;
                        var stageInfo = battleInfo.ChallengerInfo;

                        if (stageInfo != null)
                        {
                            if (ClientData.Instance.UserData.BattleInfo.isAI)
                            {                            
                                 BattleRunGameMgr.Instance.InitializedAI(ClientData.Instance.UserData.BattleInfo.PlayerInfo == null ? 0 : ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Count);
                            }
                            else
                            {
                                if (ClientData.Instance.UserData.BattleInfo.continueInfo != null) break;

                                NLog.Log("Send to BattleReady ");
                                var reqPacket = PacketHelper.CreateMessagePacket<C2SBattleReady>();
                                WebApiManager.Instance.SendToServer(SocketProcessor.SERVER_TYPE.BATTLE, reqPacket);
                                yield return new WaitUntil(() => battleInfo.IsReadyToBattle);
                            }
                            break;
                        }
                        yield return null;
                    }
                    WebApiManager.Instance.DisconnectServer(SocketProcessor.SERVER_TYPE.MATCHING);

                    BattleRunGameMgr.Instance.GameInit(ClientData.Instance.UserData.BattleInfo.PlayerInfo.curStageId, BattleRunGameMgr.Instance);
                }
            }

           

            yield return null;
        }

        protected override IEnumerator OnSceneStarted(SceneType precSceneType, params SceneParameter [] sceneParams)
        {
            //StageStartSceneParameter parameter = GetSceneParam<StageStartSceneParameter>(sceneParams);
            //  NLog.Log($"InGameScene> stageId:{parameter.stageData.id}");



            //BattleParameter battleParameter = GetSceneParam<BattleParameter>(sceneParams);

            //if (battleParameter != null)
            //{
            //    yield return new WaitForSeconds(0.3f);
            //}
            //else
            //{
            //    yield return null;
            //}
            yield break;
          
        }

        private void OnReceivedSocketMessage(IMessage message)
        {   
            if (message is S2CBattleStart)
            {
                // game Start 
                NLog.Log("Battle Game Start");

               // (message as S2CBattleStart).Cur
            }
          
        }

        protected override void OnSceneDisabled(params SceneParameter [] sceneParams)
        {   
            //if (type == SceneType.World) ClientData.Instance.runGameData = null;
            // TODO : SocketProcessor 관련 임시 수정용( SceneManager 의 pushScene 함수에서 이관)
       
        }

        protected override void OnScenePause(bool pause)
        {
 
        }

        protected override void OnClientQuit()
        {
            NLog.Log("InGameScene>OnClientQuit");
        }

      
    }
}

