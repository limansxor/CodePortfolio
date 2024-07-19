using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGle.Solitaire.Asset;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using NGle.Solitaire.Support;
using UnityEngine.UI;
using NGle.Solitaire.Data;
using static NGle.Solitaire.Data.UserData;
using System;

namespace NGle.Solitaire.RunGame
{
    public class BattleGameUI : GameUIAbs
    {
        [SerializeField] Transform emoticonParent;

        List<Sprite> gradeSprites;
        List<Sprite> profileSprites;


        [SerializeField] ProgressBar processBar;
        [SerializeField] BattleTimeUI battleTimeUI;
        [SerializeField] BattleState battleState;

        [SerializeField] BattleGameWinSequence battleGameWinSequence;
        [SerializeField] BattleGameLoseSequence battleGameLoseSequence;
        [SerializeField] BattleGameDrawSequence battleGameDrawSequence;

        [SerializeField] BattleGameStartSequence battleGameStartSequence;

        [SerializeField] Button emoticonPopBtn;
        [SerializeField] RectTransform emoticonPanel;
        [SerializeField] Button[] emoticonBtns;

        // model

        CharacterType characterType;

        int round;
        int maxRound;

        public bool isEndDraw;

        private bool isEmoticonPop = false;

        RunGameData runGameData;

        public void Initialized()
        {
            runGameData = BattleRunGameMgr.Instance.runGameData;

            isEndDraw = false;

            emoticonParent.gameObject.SetActive(false);

            gradeSprites = ClientData.Instance.AssetDataFileLink.GetGradeSprites();
            profileSprites = ClientData.Instance.AssetDataFileLink.GetProfilePictureSprites();

               // ClientData.Instance.AssetDataFileLink.profilePictures

            // profileSprites.Select(item => AssetManager.Instance.LoadAsset<Sprite>(item)).ToList();

            processBar.Initialized(BattleRunGameMgr.Instance.runGameData.pageMax);
            battleTimeUI.Initialized();
        
            round = ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.Count;
            maxRound = ClientData.Instance.UserData.BattleInfo.PlayerInfo.stageIds.Length;

            GameRandom r = new GameRandom(); // 지울 랜덤 
            r.Init(UnityEngine.Random.Range(0, 100));
            //r.Get(2)
            characterType = CharacterType.Nao;

            emoticonPanel.gameObject.SetActive(false);

            emoticonPopBtn.onClick.AddListener(() => {
                isEmoticonPop = !isEmoticonPop;
                emoticonPanel.gameObject.SetActive(isEmoticonPop);
                });

            for (int i = 0; i < emoticonBtns.Length; i++)
            {
                int n = i;
                emoticonBtns[i].onClick.AddListener(() =>
                {
                    SendEmoticon( n);
                    emoticonPanel.gameObject.SetActive(false);
                });
            }

            GameStartSequence();
        }
        public override void GameStartSequence()
        {
          
            emoticonParent.gameObject.SetActive(true);

            battleGameStartSequence.Run(characterType, round, maxRound, gradeSprites);

            battleState.Initialized(characterType, maxRound > 1,
                gradeSprites,
                profileSprites
                );
        }

      
        private void OnRecvAttack(int type, int[] sendIdxs)
        {
            NLog.Log("내 쪽에 해골 뿌립니다 ");
        }

        public void TimerStart()
        {
            battleTimeUI.GameStart();
        }

        public void Draw()
        {
            battleTimeUI.isRun = false;
            battleGameDrawSequence.Run(maxRound == 0 ? BattleGameType.Single : BattleGameType.Multi,
                round,
                maxRound,
                OnResultPop);
            AddMarks();
        }

        public void GameWin()
        {
            battleTimeUI.isRun = false;
            battleGameWinSequence.Run(maxRound == 0 ? BattleGameType.Single : BattleGameType.Multi,
                round,
                maxRound,
                OnResultPop);

            AddMarks();
        }

        public void GameLose()
        {
            battleTimeUI.isRun = false;
            battleGameLoseSequence.Run(maxRound == 0 ? BattleGameType.Single : BattleGameType.Multi,
               round,
                maxRound,
                OnResultPop);
            AddMarks();
        }

        public void AddMarks( )
        {
            List<bool> wins = new List<bool>();
            List<bool> challengerWins = new List<bool>();

            ClientData.Instance.UserData.BattleInfo.PlayerInfo.wins.ForEach((item) => wins.Add(item));
            ClientData.Instance.UserData.BattleInfo.ChallengerInfo.wins.ForEach((item) => challengerWins.Add(item));

            battleState.WinMarksUpdate(wins, challengerWins);
        }

        public void OnResultPop()
        {
            EndInfo endInfo = ClientData.Instance.UserData.BattleInfo.endInfo;

            if (isEndDraw == false)
            {
                ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint = endInfo.gp; // 값 입력 
                ClientData.Instance.UserData.PointDataGroup.UserProfile.CROWN = endInfo.crown;

                if (endInfo.gp > ClientData.Instance.UserData.AchievementDataGroup.highest_gp)
                    ClientData.Instance.UserData.AchievementDataGroup.highest_gp = endInfo.gp;
            }

            GpData gpData = new GpData
            {
                preRank = runGameData.preGrade,
                grade = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPGrade,
                preGp = runGameData.preGP,
                addGp = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint - runGameData.preGP,
                preCrown = runGameData.preCrown,
                crown = ClientData.Instance.UserData.PointDataGroup.UserProfile.CROWN,
                crownConversionPoint = runGameData.configData.crown_gp,
                gp = ClientData.Instance.UserData.PointDataGroup.UserProfile.GPPoint,
                minGpTable = runGameData.configData.GradeMinPointTabel
            };

         BattleResultDialogUI.DoModal(gradeSprites,
                ClientData.Instance.gameSetting.waitCounter,
                gpData
                );
        }

        // 이벤트 어디서 할지 알려야 한다.
        //public float SendAttact(AttackType type) => comboAttackEffect.Attack(characterType, true, type);

        //public float RecvAttack(AttackType type) => comboAttackEffect.Attack(characterType, false, type);

        public void SendEmoticon(int type)
        {
            battleState.Emoticon(characterType, true, type);

            BattleRunGameMgr.Instance.OnSendEmoticon(type);
        }
        public void RecvEmoticon(int type)
        {
            battleState.Emoticon(characterType, false, type);
        }

        public void ChangeProcessBar(float gauge)
        {
            processBar.Player(gauge);
        }

        public void ChangeChallengerProcessBar(float gauge)
        {
            processBar.Challenger(gauge);
        }

        public void ReconnectSetTime(float time)
        {
            battleTimeUI.SetViewTime(time);
        }
       
    }

}
