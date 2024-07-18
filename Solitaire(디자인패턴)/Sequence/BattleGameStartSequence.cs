using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.AddressableAssets;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Sound;
using DG.Tweening;
using static NGle.Solitaire.Data.UserData;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class BattleGameStartSequence : MonoBehaviour
    {
        public Transform startInfoUI;

        [Serializable]
        public class UserState
        {
            public Transform parent;
            public Transform victory;
            public Image[] marks;
            [SerializeField] Image rank;
            [SerializeField] Text userName;
            [SerializeField] Text gp;
            [SerializeField] Image profile;

            public void Show(bool isRound, List<bool> wins, int rank, string name, int profile, int gp, List<Sprite> rankSprites)
            {
                victory.gameObject.SetActive(isRound);

                foreach (var item in marks) item.gameObject.gameObject.SetActive(false);

                for (int i = 0; i < wins.Count; i++)
                {
                    marks[i * 2].gameObject.SetActive(wins[i]);
                    marks[i * 2 + 1].gameObject.SetActive(!wins[i]);
                }
//                Support.NLog.Log("rank " + rank);
                this.rank.sprite = rankSprites[rank];
                userName.text = name.Length > 10 ? name.Substring(0, 8) + "..." : name;

                // profile = Mathf.Clamp(profile,1, profileSprites.Count) - 1;
                
                this.profile.sprite = ClientData.Instance.AssetDataFileLink.GetProfilePictureSprite(profile);//profileSprites[profile];

                this.gp.text = gp.ToString();
            }
        }


        [SerializeField] SkeletonGraphic battle_ig_ready;
        [SerializeField] SkeletonGraphic battle_ig_round123;

        [SerializeField] UserState challenger;
        [SerializeField] UserState player;


        [SerializeField] AssetReference sleigh_bell;
        AudioClip _sleigh_bell;

        [SerializeField] AssetReference voice_ready;
        AudioClip _voice_ready;

        [SerializeField] AssetReference voice_go;
        AudioClip _voice_go;

        [SerializeField] AssetReference[] spine_matchis;
        List<Sprite> _spine_matchis;

        public Image[] playerCharacters;
        public Image[] challengerCharacters;

        public AssetReference bgmSound;
        private AudioClip bgmSoundClip;

        public void Awake()
        {
            startInfoUI.gameObject.SetActive(false);
        }


        public void Run(CharacterType type,int round, int roundMax, List<Sprite> rankSprites)
        {
            bgmSoundClip = AssetManager.Instance.LoadAsset<AudioClip>(bgmSound);

            startInfoUI.gameObject.SetActive(true);

            StageInfo playerInfo = ClientData.Instance.UserData.BattleInfo.PlayerInfo;
            StageInfo challengerInfo = ClientData.Instance.UserData.BattleInfo.ChallengerInfo;

            player.Show(roundMax > 1, playerInfo.wins, playerInfo.grade, playerInfo.nick, playerInfo.profile, playerInfo.gp, rankSprites);
            challenger.Show(roundMax > 1, challengerInfo.wins, challengerInfo.grade, challengerInfo.nick, challengerInfo.profile, challengerInfo.gp, rankSprites);

            challenger.parent.gameObject.SetActive(false);
            player.parent.gameObject.SetActive(false);

            _sleigh_bell = AssetManager.Instance.LoadAsset<AudioClip>(sleigh_bell);
            _voice_ready = AssetManager.Instance.LoadAsset<AudioClip>(voice_ready);
            _voice_go = AssetManager.Instance.LoadAsset<AudioClip>(voice_go);

            _spine_matchis = new List<Sprite>();

            for (int i = 0; i < spine_matchis.Length; i++)
            {
                _spine_matchis.Add(AssetManager.Instance.LoadAsset<Sprite>(spine_matchis[i]));
            }


            foreach (var item in playerCharacters)
            {
                item.gameObject.SetActive(false);
                item.sprite = _spine_matchis[(int)type];
                item.SetNativeSize();
            }

            foreach (var item in challengerCharacters) item.gameObject.SetActive(false);

            StartCoroutine(RunCoroutine(round, roundMax));
            StartCoroutine(SoundCoroutine(roundMax > 1));
        }

        private string[] spineNames = new string[] {"round01", "round02", "round03" }; 
        public IEnumerator RunCoroutine(int round, int roundMax)
        {
            battle_ig_ready.gameObject.SetActive(false);
            battle_ig_round123.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.5f);
            // 빠르게 떨어짐 
          

            playerCharacters[0].gameObject.SetActive(true);
            playerCharacters[0].gameObject.GetComponent<DOTweenAnimation>().DOPlay();

            challengerCharacters[0].gameObject.SetActive(true);
            challengerCharacters[0].gameObject.GetComponent<DOTweenAnimation>().DOPlay();

            // yield return new WaitForSeconds(0.3f);
            battle_ig_ready.gameObject.SetActive(roundMax ==1);
            battle_ig_round123.gameObject.SetActive(roundMax > 1);
         
             battle_ig_round123.AnimationState.SetAnimation(0, spineNames[round], false);
        


            yield return new WaitForSeconds(0.8f);

            playerCharacters[0].gameObject.SetActive(false);
            challengerCharacters[0].gameObject.SetActive(false);

            playerCharacters[1].gameObject.SetActive(true);
            challengerCharacters[1].gameObject.SetActive(true);

            challenger.parent.gameObject.SetActive(true);
            player.parent.gameObject.SetActive(true);

            foreach (var item in challenger.parent.GetComponents<DOTweenAnimation>()) item.DOPlay();
            foreach (var item in player.parent.GetComponents<DOTweenAnimation>()) item.DOPlay();

            // 느리게 올리감 
            yield return new WaitForSeconds( round>0 ? 3.0f-0.5f : 2.5f - 0.5f);

            startInfoUI.gameObject.SetActive(false);
            BattleRunGameMgr.Instance.OnGameRun();  
        }

        public IEnumerator SoundCoroutine(bool isRound)
        {
            yield return new WaitForSeconds(0.5f);
            SoundManager.Instance.PlaySfx(_sleigh_bell);
            yield return new WaitForSeconds(isRound == false? 0.88f : 0.88f+0.5f );
            SoundManager.Instance.PlaySfx(_voice_ready);
            yield return new WaitForSeconds(0.9f);
            SoundManager.Instance.PlaySfx(_voice_go);
            yield return new WaitForSeconds(0.9f);
            SoundManager.Instance.PlayBGM(bgmSoundClip);
        }
    }
}
