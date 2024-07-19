using System;
using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.Dialog;
using NGle.Solitaire.Scene;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Support;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;



namespace NGle.Solitaire.RunGame
{

    public class StageRunGameMgr : SingletonRunGameMgr<StageRunGameMgr>
    {
        public AssetReference bgmSound;
        private AudioClip bgmSoundClip;

        public GameObject pauseCover;

        [SerializeField] GameUI gameUI;
        [SerializeField] ItemBoard itemBoard;

        // Start is called before the first frame update
        public override void Initialized()
        {
            pauseCover.gameObject.SetActive(true);

            base.Initialized();
            runGameEvent = new StageRunGameEvent();

            gameUI = ClientData.Instance.runGameDataFileLink.GetStageGameUIPrefab();
            itemBoard = ClientData.Instance.runGameDataFileLink.GetItemBoardPrafab();
        }

        public override void GameInit(int stage, RunGameMgr runGameMgr) // start 에 해당 합니다 ;
        {
            base.GameInit(stage , runGameMgr);

            gameUI.Initialized();

            itemBoard.Initialized();

            bgmSoundClip = AssetManager.Instance.LoadAsset<AudioClip>(bgmSound);
        }

        public void OnHint() => gameBoard.OnHint();
        public void OnHammerMode() => gameBoard.OnHammerMode();

        public override void OnBlockRemove(Action<Vector2Int, int> remove, Vector2Int idx, List<BlockPlace> blockPlaces)
        {
            base.OnBlockRemove(remove, idx, blockPlaces);

            gameUI.UpdateState();
        }

        public override void OnEvtBlockRemove() 
        {
            gameUI.UpdateState();
        }

        public override void OnHitHammer(Action<Vector2Int, int> remove, Vector2Int idx, List<BlockPlace> blockPlaces)
        {
            base.OnHitHammer(remove, idx, blockPlaces);

            gameUI.UpdateState();

            itemBoard.HammerRelease();
        }

        public override void OnTimeStart()
        {
            // 타임 시작
            gameUI.TimerStart();
            SoundManager.Instance.PlayBGM(bgmSoundClip);

            pauseCover.gameObject.SetActive(false); 

        }

        public override void OnShuffle()
        {
            gameUI.RunShuffleHand();
        }

        public override void OnGameEnd()
        {
            SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_hurryup").Load());
            NLog.Log("게임 끝 판정 mgr ");

            runGameData.IncreaseScore(true);
            gameBoard.GameEnd();
            gameUI.Clear();

            SoundManager.Instance.StopBGM();
        }

        public override void OnTimeOver()
        {
            NLog.Log("게임 오버  판정 mgr ");
            gameUI.Fail( OnGameContinue, bgmSoundClip);
            gameBoard.GamePause();

            SoundManager.Instance.StopBGM();
        }

        public override void OnGamePause() 
        {
            gameUI.TimerPause();
            gameBoard.GamePause();
            
        }

        public override void ScenePause(bool pause)
        {
            NLog.Log("StageRunGameMgr>OnScenePause");
        }

        public void OnGameContinue()
        {
            gameUI.TimerStart();
            gameBoard.GameContinue();
        }

        public override void ContinueUpdate() { }
        
    }
}
