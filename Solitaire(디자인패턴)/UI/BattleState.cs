using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using Spine.Unity;
using UnityEngine.Events;
using NGle.Solitaire.Support;
using System.Threading.Tasks;
using static NGle.Solitaire.Data.UserData;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class BattleState : MonoBehaviour
    {
        [Serializable]
        public class UserState
        {
            public Transform victory;
            public Image[] marks;
            [SerializeField] Image rank;
            [SerializeField] Text userName;
       
            [SerializeField] Image profile;
          
            public void Show(bool isRound, List<bool> wins, List<Sprite> rankSprites, int rank, string name, int profile)
            {
                victory.gameObject.SetActive(isRound);

                foreach (var item in marks) item.gameObject.gameObject.SetActive(false);

                for (int i = 0; i < wins.Count; i++)
                {
                    marks[i * 2].gameObject.SetActive(wins[i]);
                    marks[i * 2 + 1].gameObject.SetActive(!wins[i]);
                }

                this.rank.sprite = rankSprites[rank];
                userName.text = name.Length > 10 ? name.Substring(0, 8) + "..." : name;

                //profile = Mathf.Clamp(profile,1, profileSprites.Count) - 1;

                this.profile.sprite = ClientData.Instance.AssetDataFileLink.GetProfilePictureSprite(profile); //  profileSprites[profile];
            }
        }

        [SerializeField] UserState challengerState;
        [SerializeField] UserState playerState;

       

        // 이모티콘
        [SerializeField] List<SkeletonGraphic> emoticonGraphics;
        [SerializeField] SkeletonGraphic challengerEmoticonGraphics;


        /// <summary>
        ///  구분 =====
        /// </summary>
     
        public void Initialized(CharacterType type, bool isRound,
            List<Sprite> rankSprites,
            List<Sprite> profileSprites
            )
        {
            StageInfo playerInfo = ClientData.Instance.UserData.BattleInfo.PlayerInfo;
            StageInfo challengerInfo = ClientData.Instance.UserData.BattleInfo.ChallengerInfo;

            playerState.Show(isRound, playerInfo.wins, rankSprites,playerInfo.grade, playerInfo.nick,playerInfo.profile);
            challengerState.Show(isRound, challengerInfo.wins, rankSprites, challengerInfo.grade, challengerInfo.nick, challengerInfo.profile);

            emoticonGraphics.ForEach(item => item.gameObject.SetActive(false));
            challengerEmoticonGraphics.gameObject.SetActive(false);

        }

        public void WinMarksUpdate(List<bool> wins ,List<bool> challengerWins)
        {
            foreach (var item in playerState.marks) item.gameObject.gameObject.SetActive(false);

            for (int i = 0; i < wins.Count; i++)
            {
                playerState.marks[i * 2].gameObject.SetActive(wins[i]);
                playerState.marks[i * 2 + 1].gameObject.SetActive(!wins[i]);
            }

            foreach (var item in challengerState.marks) item.gameObject.gameObject.SetActive(false);

            for (int i = 0; i < wins.Count; i++)
            {
                challengerState.marks[i * 2].gameObject.SetActive(challengerWins[i]);
                challengerState.marks[i * 2 + 1].gameObject.SetActive(!challengerWins[i]);
            }
        }


        public void Emoticon(CharacterType type, bool isTo, int n)
        {
            string aniName = "ig_emoticon" + (n + 1).ToString("D2");
            NLog.Log("애니메이션 이름 : " + aniName);

            if(isTo)
            {
                emoticonGraphics[(int)type].gameObject.SetActive(true);
                emoticonGraphics[(int)type].AnimationState.SetAnimation(0, aniName, false);
            }
            else
            {
                challengerEmoticonGraphics.gameObject.SetActive(true);
                challengerEmoticonGraphics.AnimationState.SetAnimation(0, aniName, false);
            }
          
        }

    }
}
