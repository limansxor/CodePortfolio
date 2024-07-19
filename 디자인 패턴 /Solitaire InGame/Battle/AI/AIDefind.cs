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
        [SerializeField] BattleGameBoard board;
        [SerializeField] BattleGameUI ui;
        [SerializeField] GameBoardModel subModel;
        public Coroutine aiRunCoroutine;
        private bool isAniRun = false;

        private List<int> comboUnitPowers;
        private int maxComboPower;
        private int crtComboPower;

        private float addSubTime;

        private Data.StageData.BattleType battleType;
        private S2CBattleInfo battleInfo;

        private int flag = 0;
        private int combo = 0;

        private int blockCountMax = 0;

        private int varyingSpeedCount;

        private BattleRunGameMgr mgr;

        public void InitAIConfig()
        {
            var comboPow = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_COMBOPOWER).GetTableData<combopowerData>(1);
            if (comboPow != null)
                maxComboPower = comboPow.combo_pow_max_point;

            comboUnitPowers = new List<int>();
            var comboPowPoints = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_COMBOPOWERPOINT).AllData<combopowerpointData>();
            if (comboPowPoints != null)
                comboPowPoints.ForEach((item) => comboUnitPowers.Add(item.point));

            crtComboPower = 0;
            addSubTime = 0;

            varyingSpeedCount = 0;
        }

        public void IncreaseCombo()
        {
            if (combo >= 10) { combo = 0; }
            combo++;
        }

        private List<UserData.AIProfile> AIProfileCreate()
        {
            List<UserData.AIProfile> result = new List<UserData.AIProfile>();

            int channel = GetChannel();

            int max = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_AI_PROFILE).GetTableDataCount();

            for (int i = 0; i < max; i++)
            {
                AIProfileData profileData = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_AI_PROFILE).GetTableData<AIProfileData>(i + 1);

                if (profileData.channel == channel)
                {
                    result.Add(new UserData.AIProfile()
                    {
                        channel = profileData.channel,
                        grade = profileData.grade,
                        nicknameTextId = profileData.nickname_text_id,
                        nickname = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_LANGUAGE).GetTableData<LanguageData>(profileData.nickname_text_id).kr,
                        gp = Random.Range(profileData.gp_min, profileData.gp_max),
                        profileId = profileData.profile_picture_id,
                        times = new List<float>(),
                        isActive = true
                    }); 


                }

            }
            //ClientData.Instance.TableData.TextDataImplement.GetText("TEXT.outgame.battle_retry_fail")
            //ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_LANGUAGE)
            return result;
        }

        public void ChangeComboPower(int combo, AttackType type = 0) //ChangeComboPower
        {

            int addComboPower = combo == 0 ? 0 : comboUnitPowers[combo - 1];

            crtComboPower += addComboPower;

            if (crtComboPower >= maxComboPower)
            {
                crtComboPower -= maxComboPower;

                flag = flag == 0 ? 1 : 0;

                AttackType attackType;
                if (flag == 1) { attackType = AttackType.Blink; }
                else { attackType = AttackType.Question; }

                List<BlockPlace> blockPlaces = (BattleRunGameMgr.Instance.runGameEvent as BattleRunGameEvent).onFindAttackBlock.Invoke(attackType, false); // board.FindAttackBlock(attackType, false);

                (BattleRunGameMgr.Instance.runGameEvent as BattleRunGameEvent).onAttack.Invoke(attackType, false, blockPlaces);

            }

        }

        private int GetChannel()
        {
            int channel = 0;
            int gp = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint;

            int max = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_BATTLE_LOADING).GetTableDataCount();

            var loadingDatas = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_BATTLE_LOADING).AllData<battleloadingData>();

            foreach (var item in loadingDatas)
            {
                if (item.gp_min <= gp && gp <= item.gp_max)
                {
                    channel = item.type;
                    break;
                }
            }

            return channel;
        }

        private void SelStageAIData(int stageId, ref UserData.AIProfile profile)
        {
            //  StageAIData stageAIData = new StageAIData();

            var stageData = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_STAGE).GetTableData<Data.StageData>(stageId);
            int ai_id = stageData.stage_ai_id;


            var stageAIAllDatas = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_STAGE_AI).AllData<StageAIData>();
            List<StageAIData> stageAIDatas = new List<StageAIData>();
            foreach (var item in stageAIAllDatas) if (item.id == ai_id) stageAIDatas.Add(item);

            List<int> rates = new List<int>();

            stageAIDatas.ForEach((item) => rates.Add(item.rate));

            int sel = 0;
            int weight = 0;
            int selRandom = 0;
            int total = 0;

            rates.ForEach((item) => total += item);
            selRandom = Mathf.RoundToInt(total * Random.Range(0.0f, 1.0f));


            for (int i = 0; i < rates.Count; i++)
            {
                weight += rates[i];
                if (selRandom <= weight)
                {
                    sel = i;
                    break;
                }
            }
            StageAIData stageAIData = stageAIDatas[sel];

            int maxTime = stageData.time_limit;
            maxTime += stageAIData.spare_time;

            profile.times.Add(maxTime * (stageAIData.block_layout * 0.01f)); // 퍼센트 때문에 ? 
            if (stageAIData.block_layout_2 != 0) profile.times.Add(maxTime * (stageAIData.block_layout_2 * 0.01f));
            if (stageAIData.block_layout_3 != 0) profile.times.Add(maxTime * (stageAIData.block_layout_3 * 0.01f));

        }

        private void Update()
        {
            if (isAniRun && board.GetPause() && board.GetGameEnd())
            {
                if (aiRunCoroutine != null)
                {
                    NLog.Log("AI 종료 ");
                    StopCoroutine(aiRunCoroutine);
                    aiRunCoroutine = null;
                    isAniRun = false;
                }
            }
        }



    }
}
