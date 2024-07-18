using System.Collections;
using System.Collections.Generic;
using BattlePacket;
using NGle.Solitaire.Data;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Support;
using UnityEngine;


namespace NGle.Solitaire.RunGame
{
    public partial class AI : MonoBehaviour
    {
        public void Initialized(int round) // 배틀게임 매니져에서 실행 
        {
            mgr = BattleRunGameMgr.Instance;

            if (round == 0)
            {
                UserData.AIProfile aiProfile = ClientData.Instance.UserData.BattleInfo.GetActiveAIProfile();
                if (aiProfile == null)
                {
                    ClientData.Instance.UserData.BattleInfo.aiProfiles = AIProfileCreate();
                    aiProfile = ClientData.Instance.UserData.BattleInfo.aiProfiles[0];
                    ClientData.Instance.UserData.BattleInfo.aiProfiles[0].isActive = false;
                }

                battleType = ClientData.Instance.UserData.BattleInfo.battleType;
                S2CBattleInfo battleInfo = new S2CBattleInfo();

                battleInfo.Vsnick = aiProfile.nickname;
                battleInfo.Vsgp = aiProfile.gp;
                battleInfo.Vsprofi = aiProfile.profileId;

                if (battleType == Data.StageData.BattleType.BATTLE_3_2_MATCH)
                {

                    battleInfo.Stages.Add(ClientData.Instance.UserData.AIBattleStages[0]);
                    SelStageAIData(ClientData.Instance.UserData.AIBattleStages[0], ref aiProfile);


                    battleInfo.Stages.Add(ClientData.Instance.UserData.AIBattleStages[1]);
                    SelStageAIData(ClientData.Instance.UserData.AIBattleStages[1], ref aiProfile);

                    battleInfo.Stages.Add(ClientData.Instance.UserData.AIBattleStages[2]);
                    SelStageAIData(ClientData.Instance.UserData.AIBattleStages[2], ref aiProfile);

                    aiProfile.times.ForEach((item) => battleInfo.Seeds.Add(Random.Range(0, int.MaxValue))); //

                }
                else
                {
                    battleInfo.Stages.Add(ClientData.Instance.UserData.AIBattleStages[0]);
                    SelStageAIData(ClientData.Instance.UserData.AIBattleStages[0], ref aiProfile);

                    aiProfile.times.ForEach((item) => battleInfo.Seeds.Add(Random.Range(0, int.MaxValue)));

                }

                aiProfile.times.ForEach((item) => NLog.Log("시간값 체크 " + item));

                this.battleInfo = battleInfo;

                if (round == 0)
                    ClientData.Instance.UserData.BattleInfo.OnReceivedBattleInfo(battleInfo); //서버 대신 구현 

                S2CBattleStart battleStart = new S2CBattleStart();
                battleStart.Type = 1003;
                battleStart.Stime = 1701775200;
                battleStart.Stime = 1701775200;
                battleStart.Cur = round; // 라운드 0 ,1, 2
                ClientData.Instance.UserData.BattleInfo.OnReceivedBattleStart(battleStart); // 서버 대신 구현 

              

                ClientData.Instance.UserData.BattleInfo.curtAIProfile = aiProfile;
            }
            InitAIConfig(); 
            isAniRun = false;
            AIRun();
        }

        public void AIRun() // 시간 관련은 수정이 필요한 부분 이다
        {
          
            if (battleType == Data.StageData.BattleType.BATTLE_3_2_MATCH)
            {
                int n = ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.Count;

                List<float> times = ClientData.Instance.UserData.BattleInfo.curtAIProfile.times;

                aiRunCoroutine = StartCoroutine(AIRunCoroutine(times));
            }
            else
            {
                List<float> times = ClientData.Instance.UserData.BattleInfo.curtAIProfile.times;

                aiRunCoroutine = StartCoroutine(AIRunCoroutine(times));
            }
        }


        public float VaryingTime(float elapsedTime) // 앞으로 개발 할 것들 
        {
            float addTime = 0;// Random.Range(elapsedTime*0.3f, elapsedTime*0.6f); // 1 아 여기서 기본 값 가져 가니까 

            if (varyingSpeedCount > blockCountMax * 0.5f * 0.8f) // 거의 막판 
            {
                addTime = -Random.Range(elapsedTime * 0.3f, elapsedTime * 0.5f);
                if ( (mgr.runGameData as BattleRunGameData).otherBlockCount <= 2)
                {
                    addTime = Mathf.Clamp(addSubTime, 0, addSubTime);
                }
            }
            else if (varyingSpeedCount > blockCountMax * 0.5f * 0.6f)  // 중간
            {
                addTime = -Random.Range(-elapsedTime * 0.1f, elapsedTime * 0.3f);
            }
            else if (varyingSpeedCount > blockCountMax * 0.5f * 0.4f)
            {

            }
            else if (varyingSpeedCount > blockCountMax * 0.5f * 0.2f)
            {
                addTime = Random.Range(-elapsedTime * 0.1f, elapsedTime * 0.3f);
            }
            else  // 시작 점 
            {
                addTime = Random.Range(elapsedTime * 0.3f, elapsedTime * 0.5f);
            }


            //if (subModel.blockCount <= 2)
            //{
            //    addTime = Mathf.Clamp(addSubTime, 0, addSubTime);
            //}
            //else if (varyingSpeedCount > blockCountMax * 0.5f * 0.5f) // 빨라지는  구간 
            //{
            //    addTime = -elapsedTime * (1.0f- (float)subModel.blockCount / (float)blockCountMax)*0.5f; // 7/10
            //}
            //else // 느려지는 구간 
            //{
            //    addTime = elapsedTime *  (float)subModel.blockCount / (float)blockCountMax* 0.5f; // 1/100
            //}



            addSubTime += addTime;
            varyingSpeedCount++;

            return addTime;
        }

        public IEnumerator AIRunCoroutine(List<float> times)
        {
            yield return new WaitUntil(() => board.GetPause() == false);
            yield return new WaitForSeconds(0.1f);
            //yield return new WaitForSeconds(1.0f+0.8f + 2.5f);
            //int stageCount = battleInfo.Stages.Count;
            //int seedCount = battleInfo.Seeds.Count;
            //int 
            blockCountMax = (mgr.runGameData as BattleRunGameData).otherBlockCount;
            NLog.Log("AI 시작 ");
            isAniRun = true;
            foreach (var item in times)
            {

                float time = item;
                int blockPair = (mgr.runGameData as BattleRunGameData).otherBlockCount / 2;
                float elapsedTime = item / blockPair;// 단순하게 하나당 써야할 초 계산     90 20
                                                     // 20 / 45  45 / 0.5   elapsedTime  / blockPair
                                                     //elapsedTime 가운데 시간
                while ((mgr.runGameData as BattleRunGameData).otherBlockCount > 0)
                {
                    float addTime = VaryingTime(elapsedTime);

                    PairIdx pairIdx = subModel.Hint();

                    if(pairIdx == null)
                    {
                        mgr.RecvShuffle();
                        yield return new WaitForSeconds(ClientData.Instance.gameSetting.shuffleTime);
                      
                    }
                    else
                    {

                        IncreaseCombo();

                        if (combo == 10) board.ChallengerMatchingBlock(pairIdx.first, pairIdx.last, RecvRemoveType.Hammer);
                        else board.ChallengerMatchingBlock(pairIdx.first, pairIdx.last, RecvRemoveType.Normal);
                        ChangeComboPower(combo);

                        ui.ChangeChallengerProcessBar(board.GetChallengerProcess());

                        time -= elapsedTime + addTime;
                    }

                    if((mgr.runGameData as BattleRunGameData).otherBlockCount >= 2)
                    yield return new WaitForSeconds(elapsedTime + addTime); // new 안 붙이려다가 가변 값이라 붙임

                }
                NLog.Log("일단 한페이지 끝 ");

                if (subModel.currentPage < mgr.runGameData.pageMax) // 0<3 이런 식이지만 
                {
                    yield return StartCoroutine(board.ChallengerNextPageCoroutine());
                    NLog.Log("새로운 페이지 ");
                    varyingSpeedCount = 0;
                    yield return new WaitForSeconds(ClientData.Instance.gameSetting.filpTime);
                }
                   
            }

           // yield return new WaitForSeconds(0.5f);
            if (battleType == Data.StageData.BattleType.BATTLE_3_2_MATCH)
            {
                mgr.GameLose(); // 결과 보여 주기 
            }
            else // 단판일 경우 
            {
                mgr.GameLose(); // 결과 보여 주기 
            }

            yield break;
        }

      


    }
}
