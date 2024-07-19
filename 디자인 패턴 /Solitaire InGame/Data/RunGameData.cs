using System;
using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    [System.Serializable]
    public class RunGameData
    {
        public int currentstageId { get; private set; } // 초기화 시키는 것은 데이터에 의해서 이다
        public int goldReward { get; private set; }
        public int[] starRewardTimes { get; private set; }
        public string[] loadDataInfo { get; private set; }
        public string[] loadDataInfo2 { get; private set; }
        public string[] loadDataInfo3 { get; private set; }
        public int widthCount { get; private set; }
        public int hightCount { get; private set; }
        public bool isEnd { get; private set; }
        public int score { get; private set; }
        public int itemScore { get; private set; }
        public int comboCount { get; private set; } 
        public float remainingTime { get; private set; }
        public void SetRemainingTime(float val) => remainingTime = val;
        public void AddRemainingTime(float val) => remainingTime = Mathf.Max(remainingTime - val, 0);
        public bool isGamePause { get; private set; }

        public int blockMaxCount { get; private set; }
        public void SetBlockMaxCount(int val) => blockMaxCount = val;
        public int blockCount { get; private set; }
        public void SetBlockCount(int val) => blockCount = val;
        public int matchingCount { get; private set; }
        public void SetMatchingCount(int val) => matchingCount = val;
        public bool isHammerMode;
        public int currentPage;


        public int preGP { get; private set; }
        public int preStar { get; private set; }
        public int preCrown { get; private set; }
        public int preGrade { get; private set; }
        public int pageMax { get; private set; }

        public ConfigData configData { get; private set; }

        public RunGameData(int stageId)
        {
            if (stageId == -1) { NLog.LogError("없는 stageId "); return; }
            LocalUserStageData localUserStageData = ClientData.Instance.UserData.StageDataGroup.GetLocalStageInfo(stageId);

            int preStar = 0;
            if (localUserStageData != null)
            {
                NLog.Log("localUserStageData.star " + localUserStageData.star + "localUserStageData.score " + localUserStageData.score);
                preStar = localUserStageData.star;
            }


            StageData stageData = ClientData.Instance.TableData.GetData<StageData>(TableData.Type.TABLE_STAGE, stageId);

            ClientData.Instance.UserData.BattleInfo.block_count_type = stageData.block_count_type;

            List<int> minVals = new List<int>();
            int n = 1; // 무조건 1 부터 시작
            GradeData gradeData = null;
            while (true)
            {
                gradeData = ClientData.Instance.TableData.GetData<GradeData>(TableData.Type.TABLE_GRADE, n);
                if (gradeData == null) break;
                minVals.Add(gradeData.gp_min);
                //  NLog.Log("gradeData : " + gradeData.gp_min);
                n++;
            }
            gradeData = ClientData.Instance.TableData.GetData<GradeData>(TableData.Type.TABLE_GRADE, n - 1);
            minVals.Add(gradeData.gp_max); // +1 할때 오류 안나게 하기 위해서 



            configData = new ConfigData
            {
                id = stageData.id,
                block_count_type = stageData.block_count_type,
                block_layout = stageData.block_layout,
                //   stageData.block_layout,  //
                time_limit = stageData.time_limit,//90,
                time_limit_3 = stageData.time_limit_3, //70,
                time_limit_2 = stageData.time_limit_2,//40,
                pair = stageData.pair,
                free_shuffle = 10,
                score_time = stageData.score_time,
                score_block = stageData.score_block, //100,
                score_combo_10 = stageData.score_combo_10, //1070,
                quest_score = stageData.quest_score, //5000,
                quest_goods_id = stageData.quest_goods_id,//100
                crown_gp = ClientData.Instance.TableData.GetData<GPConversionData>(TableData.Type.TABLE_GP, 1).crown_gp,
                star_gp = ClientData.Instance.TableData.GetData<GPConversionData>(TableData.Type.TABLE_GP, 1).star_gp,
                GradeMinPointTabel = minVals,

                block_layout_2 = stageData.block_layout_2,
                block_layout_3 = stageData.block_layout_3
            };

            if (configData == null) { NLog.LogError("API 통신 오류로 불러온데이터가 없습니다."); return; }

            int widthCount = ClientData.Instance.gameSetting.maxBlockByType[configData.block_count_type - 1].x;
            int hightCount = ClientData.Instance.gameSetting.maxBlockByType[configData.block_count_type - 1].y;

            int cnt = 0;

            cnt = 100;

            string[] layout = Parselayout(configData.block_layout);

            string[] layout2 = Parselayout(configData.block_layout_2);

            string[] layout3 = Parselayout(configData.block_layout_3);

            //  NLog.Log("layout2.Length  " + layout2 + " layout3.Length " + layout3);

            currentstageId = stageId;
            goldReward = cnt;
            starRewardTimes = new int[] { configData.time_limit, configData.time_limit_3, configData.time_limit_2 };
            this.widthCount = widthCount;
            this.hightCount = hightCount;
            loadDataInfo = layout;
            loadDataInfo2 = layout2;
            loadDataInfo3 = layout3;
            this.preStar = preStar;
            preCrown = ClientData.Instance.UserData.PointDataGroup.UserProfile.CROWN;
            preGP = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint;
            preGrade = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPGrade;

            remainingTime = configData.time_limit;

            int page = 1;
            if (loadDataInfo2 != null) page++;
            if (loadDataInfo3 != null) page++;
            pageMax = page;

            score = 0;
        }

        private string[] Parselayout(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;
            if (raw.Length < 90) return null;
            string block_layout = raw.Replace("\"", "");

            return block_layout.Split(',');
        }

        public AttackType GetAttackType()
        {
            return (AttackType)UnityEngine.Random.Range(0, 2);
        }

        public void IncreaseScore(bool isEnd = false)
        {
            if (isEnd)
            {
                score += configData.score_time * ((int)remainingTime);
            }
            else
            {
                if (comboCount >= ClientData.Instance.gameSetting.comboCountMAX)
                    score += configData.score_combo_10;

                score += configData.score_block;
            }
        }

        public void IncreaseCombo() // 이것의 의미를 잘 알아야 한다 
        {
            if (comboCount >= ClientData.Instance.gameSetting.comboCountMAX)
                comboCount = 0;

            comboCount++;

        }

      

    }
}

