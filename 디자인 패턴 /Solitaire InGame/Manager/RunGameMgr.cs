using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Support;
using System;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Data;
using NGle.Solitaire.Scene;
using System.Threading.Tasks;
using NGle.Solitaire.RunGame.Support;

namespace NGle.Solitaire.RunGame
{

    /// <summary>
    /// 게임을 진행 하는 기본 매니저 이 클레스를 Stage, Rank, Battle
    /// </summary>

    public enum BlockActionType
    {
        Mating,Hammer
    }
    public struct BlockPlace
    {
        public BlockPlace(int idx, Vector3 pos)
        {
            this.idx = idx;
            this.pos = pos;
        }
        public int idx;
        public Vector3 pos;
    }

    public struct AttackBlockData
    {
        public AttackBlockData(int idx, Vector3 pos, AttackType attackType)
        {
            blockPlace = new BlockPlace(idx, pos);
            this.attackType = attackType;
        }
        public BlockPlace blockPlace;
        public AttackType attackType;
    }

    public abstract class SingletonRunGameMgr<T> : RunGameMgr where T: RunGameMgr
    {
        private static T _instance;
        // 외부에서 접근 가능한 싱글톤 인스턴스 속성
        public static T Instance
        {
            get
            {
                // 인스턴스가 없는 경우 새로 생성
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    // Scene에 없는 경우 새로 생성하여 할당
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }
    }

    public abstract class RunGameMgr : MonoBehaviour 
    {
        protected GameBoard gameBoard;
        private ComboAudio comboAudio;

        public BlockSpriteMapper blockSpriteMapper;

        [SerializeField] protected Camera gameCamera;
        [SerializeField] Canvas mainCanvas;

        public RunGameEvent runGameEvent { get; protected set; }

        public RunGameData runGameData { get; protected set; }

        public virtual void Initialized()
        {
            gameBoard = ClientData.Instance.runGameDataFileLink.GetGameBoardPrefab();

            comboAudio = ClientData.Instance.runGameDataFileLink.GetComboAudioPrefab();

            blockSpriteMapper.Initialized();
        }

        public virtual async void GameInit(int stageId, RunGameMgr runGameMgr) // 스테이지 정보를 받고 셋팅
        {
            runGameData = new RunGameData(stageId);

            if (ClientData.Instance.UserData.BattleInfo.PlayerInfo.curStageId == -1)
                SceneManager.Instance.PushScene(SceneType.Title);

            runGameEvent.RunGameDataCreateEvtInit();
  
            gameBoard.Initialized(runGameMgr);

            // 카메라 사이즈 조정 한다 캔버스를 이용 해서 
            int countType = runGameData.configData.block_count_type - 1;
            float rat = mainCanvas.GetComponent<RectTransform>().rect.height / ClientData.Instance.gameSetting.baseResolutionWidth;
            float orthographicSize = 1f >= rat ? ClientData.Instance.gameSetting.OrthographicSize[countType] : rat * ClientData.Instance.gameSetting.OrthographicSize[countType];
            gameCamera.orthographicSize = orthographicSize;

            await Task.Yield();
            runGameEvent.onRunGameDataCreate.Invoke();
        }

        public abstract void ContinueUpdate();

        public abstract void OnEvtBlockRemove();

        public abstract void OnShuffle();

        public abstract void OnTimeStart();

        public abstract void OnTimeOver();

        public abstract void OnGameEnd();

        public abstract void OnGamePause();

        public abstract void ScenePause(bool pause);

        public virtual void OnBlockRemove(Action<Vector2Int,int> remove, Vector2Int idx, List<BlockPlace> blockPlaces)
        {
            runGameData.IncreaseScore();
            runGameData.IncreaseCombo();
          
            comboAudio.Increase();

            remove.Invoke(idx, runGameData.comboCount);

            runGameEvent.onRemove.Invoke(blockPlaces);
        }

        public virtual void OnHitHammer(Action<Vector2Int, int> remove, Vector2Int idx, List<BlockPlace> blockPlaces) => OnBlockRemove(remove, idx, blockPlaces);


        public void Hint()
        {
            if (true) gameBoard.OnHint();
        }

        public void OnFlipEnd()
        {
            gameBoard.FlipEnd();
            OnTimeStart();
        }

        public virtual void ClearRunGameData()
        {
            runGameData = null;
            runGameEvent = null;   
        }

    }
}
