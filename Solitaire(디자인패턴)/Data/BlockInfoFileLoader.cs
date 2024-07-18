using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Support;
using NGle.Solitaire.Data;
using StageData = NGle.Solitaire.Data.StageData;
using NGle.Solitaire.RunGame;
using ConfigData = NGle.Solitaire.RunGame.ConfigData;
using NGle.Solitaire;


public class BlockInfoFileLoader : MonoBehaviour
{

    [SerializeField] ConfigData[] _configDatas;
    public ConfigData[] configDatas { get { return _configDatas; } }

    //[SerializeField] Table _tableData;
    //public Table tableData { get { return _tableData; } }

    GameSetting gameSetting;


    //public RunGameData LoadData(int stageId)
    //{

    //    gameSetting = ClientData.Instance.gameSetting;

    //    ConfigData configData = null;

    //    int preStar = 0; 
    //    LocalUserStageData localUserStageData = ClientData.Instance.UserData.StageDataGroup.GetLocalStageInfo(stageId);

    //    if(localUserStageData != null)
    //    {
    //        NLog.Log("localUserStageData.star " + localUserStageData.star + "localUserStageData.score " + localUserStageData.score);
    //        preStar = localUserStageData.star;
    //    }
    //    //ClientData.Instance.UserData.t
    ////    MainController.Instance.ResourceFileLink.LocalData

    //    StageData stageData = ClientData.Instance.TableData.GetData<StageData>(TableData.Type.TABLE_STAGE, stageId);

    //    ClientData.Instance.UserData.BattleInfo.block_count_type = stageData.block_count_type;

    //    //ClientData.Instance.TableData.GetDataCount(TableData.Type.TABLE_STAGE)

    //    List<int> minVals = new List<int>();
    //    int n = 1; // 무조건 1 부터 시작
    //    GradeData gradeData = null;
    //    while (true)
    //    {
    //        gradeData = ClientData.Instance.TableData.GetData<GradeData>(TableData.Type.TABLE_GRADE, n);
    //        if (gradeData == null) break;
    //        minVals.Add(gradeData.gp_min);
    //      //  NLog.Log("gradeData : " + gradeData.gp_min);
    //        n++;
    //    }
    //    gradeData = ClientData.Instance.TableData.GetData<GradeData>(TableData.Type.TABLE_GRADE, n-1);
    //    minVals.Add(gradeData.gp_max); // +1 할때 오류 안나게 하기 위해서 



    //        configData = new ConfigData
    //        {
    //            id = stageData.id,
    //            block_count_type = stageData.block_count_type,
    //            block_layout = stageData.block_layout,
    //            //   stageData.block_layout,  //
    //            time_limit = stageData.time_limit,//90,
    //            time_limit_3 = stageData.time_limit_3, //70,
    //            time_limit_2 = stageData.time_limit_2,//40,
    //            pair = stageData.pair,
    //            free_shuffle = 10,
    //            score_block = stageData.score_block, //100,
    //            score_combo_10 = stageData.score_combo_10, //1070,
    //            quest_score = stageData.quest_score, //5000,
    //            quest_goods_id = stageData.quest_goods_id,//100
    //            crown_gp = ClientData.Instance.TableData.GetData<GPConversionData>(TableData.Type.TABLE_GP, 1).crown_gp,
    //            star_gp = ClientData.Instance.TableData.GetData<GPConversionData>(TableData.Type.TABLE_GP, 1).star_gp,
    //            GradeMinPointTabel = minVals,

    //            block_layout_2 = stageData.block_layout_2,
    //            block_layout_3 = stageData.block_layout_3
    //        };
   



    //    if (configData == null)
    //    {
    //        NLog.LogError("API 통신 오류로 불러온데이터가 없습니다.");
    //        return null;
    //    }

    //    //_configDatas = new ConfigData[1];
    //    //_configDatas[0] = configData;

    //    int widthCount = gameSetting.maxBlockByType[configData.block_count_type - 1].x;
    //    int hightCount = gameSetting.maxBlockByType[configData.block_count_type - 1].y;

    //    int cnt = 0;

    //    //foreach (var item in tableData.goods)
    //    //{
    //    //    if (item.id == configData.quest_goods_id)
    //    //    {
    //    //        cnt = item.count;
    //    //        break;
    //    //    }
    //    //}

    //    cnt = 100;

    //    string[] layout  = Parselayout(configData.block_layout);

    //    string[] layout2 = Parselayout(configData.block_layout_2);

    //    string[] layout3 = Parselayout(configData.block_layout_3);


    //  //  NLog.Log("layout2.Length  " + layout2 + " layout3.Length " + layout3);


    //    RunGameData runGameData = new RunGameData
    //        (
    //        stageId,
    //        configData,
    //        cnt,
    //        new int[] { configData.time_limit, configData.time_limit_3, configData.time_limit_2 },
    //        widthCount,
    //        hightCount,
    //        layout,
    //        layout2,
    //        layout3,
    //        gameSetting.comboTimeLimit,
    //        preStar,
    //        ClientData.Instance.UserData.PointDataGroup.UserProfile.CROWN,
    //        ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint,
    //        ClientData.Instance.UserData.PointDataGroup.UserProfile.GPGrade
    //        );
       

    //    return runGameData;
    //}

    private string[] Parselayout(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return null;
        if (raw.Length < 90) return null;
        string block_layout = raw.Replace("\"", "");

        return block_layout.Split(',');
    }
}

