using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using NGle.Solitaire.Support;

using NGle.Solitaire.Asset;

using System.Threading.Tasks;

using UnityEngine.AddressableAssets;
using NGle.Solitaire.Sound;
using NGle.Solitaire.Data;
using System.Linq;
using Spine.Unity;

using NGle.Solitaire.RunGame.Support;

namespace NGle.Solitaire.RunGame
{

    /// <summary>
    /// Block 생성과 라인 관련 ,인스펙트에서 보이기 위해서 일차원 배열 모델에서는 계산을 위해서 이차원 배열 
    /// </summary>
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] protected Transform cellParent;
        [SerializeField] protected Transform vfxParent;

        LineRenderer linePrafab;

        Material[] lineMats;

        [SerializeField] protected GameBoardModel model;

        [SerializeField] protected List<PlayerGameCell> gameCells;

        [SerializeField] protected List<Block> blocks;

        [SerializeField] protected BlockFactory blockFactory;

        [SerializeField] GameObject hammerModeEffect;

        [SerializeField] protected GameBoardProcess process;

        [SerializeField] Camera boardCamera;

        public Coroutine blinkerGimmickCoroutine { get; set; }

        [SerializeField] OctopusInkEffect octopusInkEffect;
        List<OctopusBlock> octopusBlocks; // 여기서는 지워야 할 것 같다

        protected RunGameMgr runGameMgr { get; private set; } 
        
        public virtual void Initialized(RunGameMgr runGameMgr) 
        {
            this.runGameMgr = runGameMgr;

            process.Initialized(runGameMgr);

            blockFactory.Initialized(runGameMgr);

            hammerModeEffect.gameObject.SetActive(false);

            linePrafab = ClientData.Instance.runGameDataFileLink.GetLineRenderer(); //AssetManager.Instance.LoadAsset("AssetData/Prefabs/Block/Line.prefab").GetComponent<LineRenderer>();

            lineMats = new Material[2];
            lineMats[0] = ClientData.Instance.runGameDataFileLink.GetMaterialAsset(0);//AssetManager.Instance.LoadAsset<Material>("Assets/AssetData/Line/Pattern Material new Normal.mat");
            lineMats[1] = ClientData.Instance.runGameDataFileLink.GetMaterialAsset(1);//AssetManager.Instance.LoadAsset<Material>("Assets/AssetData/Line/Pattern Material new Combo.mat");

            model.InitGameBoard(OnHammerMode, Shuffle);

            blocks = new List<Block>();//Block[data.widthCount * data.hightCount];

            

            gameCells = new List<PlayerGameCell>();

            octopusBlocks = new List<OctopusBlock>();

            int n = 0;
            for (int j = 0; j < runGameMgr.runGameData.hightCount; j++)
            {
                for (int i = 0; i < runGameMgr.runGameData.widthCount; i++)
                {
                    CreateCell(
                             model.types[i, j],
                             model.kinds[i, j], // 숫자 블록 때문에                  
                             new Vector2Int(i, j),
                             model.cellPositions[i, j]);
                    n++;
                }
            }

            if (model.numberBlocks is { Count: > 0 })
            {
                model.numberBlocks[model.numberBlockEnableCount].ForEach(v =>
                {
                    gameCells[To1D(v)].Interaction(true);
                    (blocks[To1D(v)] as NumberBlock)?.Active();
                });
            }

            runGameMgr.runGameEvent.AddEvtUpdateCombo(OnComboInit);
            runGameMgr.runGameEvent.AddEvtRemove(OnRemove);
        }

        private void OnRemove(List<BlockPlace> blockPlaces) { }
        
        public Vector3 GetBlockPos(int n)
        {
            if (blocks[n] != null && blocks[n].IsEnable())
            {
                return blocks[n].GetPos();
            }
            return Vector3.negativeInfinity;
        }

        public List<Vector3> OnAttackBlockPositions(bool isMy)
        {
            List<Vector3> vec3s = new List<Vector3>();
            return vec3s;
        }

        public void CreateCell(BlockType type, int kind, Vector2Int idx, Vector3 pos)
        {
            PlayerGameCell gameCell = ClientData.Instance.runGameDataFileLink.CreatePlayerGameCell(cellParent, pos, To1D(idx));

            gameCell.Register(blockFactory, type, kind, kind %1000 , idx, pos, vfxParent); // pos 필요한 이유 efffect 넣어야 하기 때문에 

            blocks.Add(gameCell.GetBlock());

            gameCells.Add(gameCell);

            if (type == BlockType.Octopus)
            {
                octopusBlocks.Add(gameCell.GetBlock() as OctopusBlock);
            }
        }

        public virtual void OnGameRun() => StartCoroutine(process.StartFlipCoroutine(model, blocks, runGameMgr.OnFlipEnd));

        public virtual void FlipEnd()
        {
            model.isPause = false;

            int n = 0;
            for (int j = 0; j < model.types.GetLength(1); j++)
            {
                for (int i = 0; i < model.types.GetLength(0); i++)
                {
                    if (blocks[n] != null) (blocks[n] as QuestionBlock)?.QuestionFlipEnd();

                    n++;
                }
            }

            if(blinkerGimmickCoroutine == null)
            blinkerGimmickCoroutine = StartCoroutine(BlinkerGimmickCoroutine()); // 계속 반복 할 것이라서

            List<bool> enableTimeFlags = new List<bool>();
            model.OctopusBlocks.ForEach(b => enableTimeFlags.Add(b.Idxs.Count>0));

            List<SkeletonAnimation> anis = new List<SkeletonAnimation>();

            octopusBlocks.ForEach(b => anis.Add(b.GetOctopusAni()));

            octopusInkEffect?.TimerStart(enableTimeFlags, OctopusBlockRemove, anis);
        }

        public virtual void OctopusBlockRemove(int n)
        {
            model.OctopusBlocks[n].Idxs.ForEach(idx => {
                gameCells[To1D(idx)].Remove();
                model.Break(idx);
                blocks[To1D(idx)].Remove();
            });

            // _model.BlockCountCheck(false);
            model.BlockCountCheck(false);

            runGameMgr.OnEvtBlockRemove();

            if (runGameMgr.runGameData.blockCount == 0)
            {               
                if (model.currentPage < model.pageNumMax - 1) // 0<1
                {
                    NextPage();
                }
                else
                {
                    octopusInkEffect.gameObject.SetActive(false);
                    if (this is BattleGameBoard)
                    {
                        if (ClientData.Instance.UserData.BattleInfo.isAI)
                        {
                            runGameMgr.OnGameEnd();
                        }
                    }
                    else
                    {
                        runGameMgr.OnGameEnd();
                    }
                }

                runGameMgr.runGameData.isHammerMode = false;
                hammerModeEffect.gameObject.SetActive(false); // 해머 모드 이팩트 끔
            }
        }

        int preSelBlinkerIdx = 0;
        public IEnumerator BlinkerGimmickCoroutine()
        {
            int n = preSelBlinkerIdx;
            while (model.blinkerBlocks.Idxs.Count > 0)
            {
//                NLog.Log($"BlinkerGimmick 정보 {_model.blinkerBlocks.Idxs.Count} n = {n}");
                if (n >= model.blinkerBlocks.Idxs.Count) n = 0;

                Vector2Int idx = model.blinkerBlocks.Idxs[n];

                if (model.types[idx.x, idx.y] == BlockType.Blinker && blocks[To1D(model.blinkerBlocks.Idxs[n])] is BlinkerBlock)
                (blocks[To1D(model.blinkerBlocks.Idxs[n])] as BlinkerBlock).Blinker();

                n++;

                preSelBlinkerIdx = n;
                yield return new WaitForSeconds(1.0f);
            }

            blinkerGimmickCoroutine = null;
            yield break;
        }

        public virtual void GamePause(bool isGameEnd = false)
        {
            Release();

            model.isPause = true;

             gameCells.ForEach((c) => c.Pause(isGameEnd));
   
            model.isGameEnd = isGameEnd;

            octopusInkEffect.Pause();
        }

        public bool StatePause() => model.isPause;

        public void GameContinue()
        {
            model.isPause = false;

            gameCells.ForEach((c) => c.Continue());

            octopusInkEffect.Contiune();
        }

        public void TouchEnableInit()
        {
            for (int i = 0; i < model.touchEnables.Length; i++) model.touchEnables[i] = true;
        }

        public bool GetTouchEnable()
        {
            if (model.isEnd == false && model.isPause == false)
            {
                return true;
            }
            return false;
        }

        public Vector2Int To2D(int n)
        {
            return model.To2D(n);
        }
        public int To1D(Vector2Int idx) => model.To1D(idx);
        public int To1D(int x, int y) => model.To1D(x, y);
        //public int FlattenIdx(Vector2Int idx) => _model.FlattenIdx(idx);
        //public int FlattenIdx(int x, int y) => _model.FlattenIdx(x, y);
        private Vector2Int SupportUserTouch(List<Vector2Int> selIdxs, int sel)
        {
            // NLog.Log(sel + " 마이너스 1 같은데 ");
            if (sel == -1 || selIdxs.Count <= sel) sel = 0;

            Vector2Int result = selIdxs[sel]; // 젤 가까운 값을 취한다

            if (model.preIdx.x != -1 && runGameMgr.runGameData.isHammerMode == false)
            {
                foreach (var item in model.enablePairIndex)
                {
                    bool isfitst = false;
                    bool isList = false;

                    if (item.first == model.preIdx || item.last == model.preIdx)
                    {
                        isfitst = true;

                    }

                    Vector2Int selIdx = new Vector2Int(-1, -1);
                    foreach (Vector2Int idx in selIdxs)
                    {
                        if (item.first == idx || item.last == idx)
                        {
                            selIdx = idx;
                            isList = true;
                            break;
                        }
                    }

                    if (isfitst && isList)
                    {
                        result = selIdx;
                        break;
                    }
                }
            }
            return result;
        }

        private int GetKind(Vector2Int idx) => model.GetKind(idx);

        private BlockType GetType(Vector2Int idx) => model.GetType(idx);
        private BlockType GetType(int x, int y) => model.GetType(new Vector2Int(x, y));

        private void IndexFind(Vector2 pos, int touchCnt)
        {
            if (!model.touchEnables[touchCnt]) return;

            Vector2 inputPos = boardCamera.ScreenToWorldPoint(pos);
            RaycastHit2D[] hits = Physics2D.RaycastAll(inputPos, Vector2.zero);

            int sel = 0;
            float selDis = float.MaxValue;
            List<Vector2Int> selIdxs = new List<Vector2Int>();
            bool isTouchRangeEnable = false;

            int sp = 0;
            foreach (RaycastHit2D hit in hits)
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj.GetComponent<TouchRange>())
                {
                    isTouchRangeEnable = true;
                    continue;
                }

                Vector2Int idx = hitObj.GetComponent<BlockInteraction>().idx;

                if (selDis > Vector2.Distance(model.GetPosition(idx) + cellParent.position + model.centerPos, inputPos))
                {
                    selDis = Vector2.Distance(model.GetPosition(idx) + cellParent.position + model.centerPos, inputPos);
                    sel = sp;
                }
                if (model.types[idx.x, idx.y] != BlockType.None)
                {
                    selIdxs.Add(idx);
                }
                sp++;
            }

            if (isTouchRangeEnable == false) return;

            if (selIdxs.Count > 0)
            {
                Vector2Int selIdx = SupportUserTouch(selIdxs, sel);

                // if (_model.types[selIdx.x, selIdx.y] != BlockType.None)
                //  {

                model.touchEnables[touchCnt] = false;
                SelectBlock(selIdx); //// 이게 핵심 ~!!!!!

                model.currentTouchDatas.Add(new TouchData(selIdx, runGameMgr.runGameData.isHammerMode, model.kinds[selIdx.x, selIdx.y]));
                //  }

            }
        }

        public void TouchModule(bool isTouchEnable)
        {
            bool isRelease = true;

            //   int tp = 0;
            // foreach (Touch touch in Input.touches)

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (!model.touchEnables[i]) continue;

                int n = i;
                Touch touch = Input.GetTouch(n);

                if (touch.phase == TouchPhase.Began)
                {
                    IndexFind(touch.position, n);

                }
                if (touch.phase == TouchPhase.Ended && isRelease)
                {
                    if (model.currentTouchDatas.Count > 0)
                    {
                        model.touchPhaseEndedCnt++;
                    }

                    isRelease = false;
                }
            }


            // Debug.Log("터치 갯수를 알아 보자 : " + Input.touchCount);
        }

        public void MouseModule(bool isDown)
        {
            if (isDown)
            {
                IndexFind(Input.mousePosition, 0);
            }
            else
            {
                if (model.currentTouchDatas.Count > 0)
                    model.touchPhaseEndedCnt++;
            }
        }

        public void Selected(bool isCombo) // 마우스 나 손가락 을 때었을 때 들어 온다 
        {
            if (model.currentTouchDatas != null)
                if (model.touchPhaseEndedCnt > 0 && model.currentTouchDatas.Count > 0)
                {
                    if (model.isSelectedHammerMode == false)
                    {
                        Block block = blocks[To1D(model.currentTouchDatas[0].index)];
                        if (block == null)
                        {
                            foreach (var item in blocks)
                            {
                                (item as TouchBlock)?.Release();
                            }
                        }
                        else
                        {
                            (block as TouchBlock).Selected(isCombo);
                        }
                        //_blocks[FlattenIdx(_model.currentTouchDatas[0].index)]?.Selected(isCombo); // _model.currentTouchDatas[0].index, _model.currentTouchDatas[0].isHammer
                    }

                    model.touchPhaseEndedCnt--;
                    model.currentTouchDatas.RemoveAt(0);
                }
        }

        public void Release()
        {

            foreach (var item in blocks)
            {
                if (item is TouchBlock)
                {
                    (item as TouchBlock).Release();
                }
            }

        }

        private void OneClick(Vector2Int idx)
        {
            model.isSelectedHammerMode = false;

            gameCells[To1D(idx)].Select(idx.y == 0);


            if (model.preIdx.x != -1)
            {
                gameCells[To1D(model.preIdx)].ContinueInteraction();
            }

            gameCells[To1D(idx)].PauseInteraction();

            model.preIdx = idx;


            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipSelects()[0]);
        }

        private INGameState CheckGameState()
        {
            INGameState state = INGameState.Continue;
            if (runGameMgr.runGameData.blockCount == 2)
            {
                if (model.currentPage < model.pageNumMax - 1) // 0<1
                {
                    state = INGameState.NextPage;
                }
                else
                {
                    state = INGameState.GameEnd;
                }
            }
            return state;
        }

        public void HideHint()
        {
            if (model.preHintPair != null )
            {
                (blocks[To1D(model.preHintPair.first)] as TouchBlock).HideHint();
                (blocks[To1D(model.preHintPair.last)] as TouchBlock).HideHint();

                model.preHintPair = null;
            }
        }
        // public Vector2Int PreClickIdx() => _model.clickIdx;
        public int PreClickIdx() => model.To1D(model.clickIdx);

        protected void UpdateModelInfo(BlockType blockType, int kind, List<Vector2Int> idxs)
        {
            if (blockType == BlockType.Blinker) model.blinkerBlocks.Remove(idxs[0], idxs[1]);
            else if (blockType == BlockType.Key)
            {
                ListIdx listIdx = model.unlockObjInfos[kind];

                listIdx.Idxs.ForEach(idx => {
                    model.kinds[idx.x, idx.y] = -1;
                    model.types[idx.x, idx.y] = BlockType.None;

                });
            }
            else if (blockType == BlockType.Number)
            {
                model.numberBlockEnableCount++;
                if (model.numberBlocks.Count > model.numberBlockEnableCount)
                    model.numberBlocks[model.numberBlockEnableCount].ForEach(v => {
                        gameCells[To1D(v)].Interaction(true);
                        (blocks[To1D(v)] as NumberBlock)?.Active();
                    });
            }
        }

        public void MatingBlock(Vector2Int idx, int combo)// 여기서 사라지다 
        {
            if(model.preHintPair != null&& (blocks[To1D(model.clickIdx)] as TouchBlock).isHint || (blocks[To1D(idx)] as TouchBlock).isHint)
            {
                    (blocks[To1D(model.preHintPair.first)] as TouchBlock).HideHint();
                    (blocks[To1D(model.preHintPair.last)] as TouchBlock).HideHint();

                model.preHintPair = null;
            }

            UpdateModelInfo(GetType(idx), GetKind(idx), new List<Vector2Int>() { model.clickIdx, idx });
          
            gameCells[To1D(model.clickIdx)].Remove();
            gameCells[To1D(idx)].Remove();

            int fIdx = To1D(model.clickIdx) < To1D(idx) ? To1D(model.clickIdx) : To1D(idx);

            StartCoroutine(
                process.DrawLineCoroutine(linePrafab, cellParent, fIdx, lineMats,
                LinkBorder,
                Remove,
                ColliderExpand,
                model.linePos, model.pathResult.paths,
               model.clickIdx, idx, GetKind(idx), GetType(idx),
              combo, CheckGameState())
                );

            model.BreakPair(idx);
        }

        public PairIdx HammerPairIdx()
        {
            return model.hammerPairIdx;
        }

        public void HitHammer(Vector2Int idx, int combo) // 삭제 모듈 실행  // Key 일경우도 생각 
        {

            int kind = model.GetKind(idx);
            BlockType type = model.GetType(idx);


            model.HammerFind(model.GetKind(idx), model.GetType(idx), idx); // 여기서 속성을 바꾸기 때문에 위에 값으로 초기화 시켜야 한다 

            UpdateModelInfo(type, kind, new List<Vector2Int>() { model.hammerPairIdx.first, model.hammerPairIdx.last });
        

            if (model.preHintPair !=null &&  (blocks[To1D(model.hammerPairIdx.first)] as TouchBlock).isHint || (blocks[To1D(model.hammerPairIdx.last)] as TouchBlock).isHint)
            {
                (blocks[To1D(model.preHintPair.first)] as TouchBlock).HideHint();
                (blocks[To1D(model.preHintPair.last)] as TouchBlock).HideHint();

                model.preHintPair = null;
            }

           

            // ============

            runGameMgr.runGameData.isHammerMode = false;
            hammerModeEffect.gameObject.SetActive(false); // 해머 모드 이팩트 끔
            model.autoShuffleCnt = 0;

            //NLog.Log(_model.hammerPairIdx.first + "체크1:" + _model.GetType(_model.hammerPairIdx.first) +
            // " " + _model.GetKind(_model.hammerPairIdx.first) + "  " +
            // _model.hammerPairIdx.last + "체크2:" + _model.GetType(_model.hammerPairIdx.last) + " " +
            // _model.GetKind(_model.hammerPairIdx.last));
            gameCells[To1D(model.hammerPairIdx.first)].Remove();
            gameCells[To1D(model.hammerPairIdx.last)].Remove();


            StartCoroutine(process.HammerCoroutine(new Block[]{
            blocks[To1D(model.hammerPairIdx.first)], blocks[To1D(model.hammerPairIdx.last)] },

            model.hammerPairIdx.first, model.hammerPairIdx.last, combo, kind, type,
            CheckGameState(),
            Remove
            ));


            model.BlockCountCheck(false);
        }
        public void EvtRemove(Vector2Int idx, Vector2Int idx2, BlockType type)
        {
            blocks[To1D(idx)].Remove();
            blocks[To1D(idx2)].Remove();
        }
        public void Remove(Vector2Int f, Vector2Int l, int combo, int kind, BlockType type, INGameState state)
        {
            if (kind == -1) return; // -1 이면 아무일도 일어나지 않는다 
            blocks[To1D(f)].Remove(combo);
            blocks[To1D(l)].Remove(combo);

            (blocks[To1D(l)] as TouchBlock).ComboRun(combo);

            // NLog.Log("Remove 타입 " + type);
            switch(type)
            {
                case BlockType.Key:
                    StartCoroutine(process.UnlockAni(model, LockRemove, kind, combo));
                    break;
                case BlockType.Number:
                     // 여기서 말고 해머랑 맞추는 곳에서 처리 해야 한다 
                    //_model.numberBlockEnableCount++;
                    //if (_model.numberBlocks.Count > _model.numberBlockEnableCount)
                    //    _model.numberBlocks[_model.numberBlockEnableCount].ForEach(v => {
                    //    gameCells[To1D(v)].Interaction(true);
                        
                    //    (_blocks[To1D(v)] as NumberBlock)?.Active();
                    //});

                    break;
                case BlockType.Octopus:

                    for (int i = 0; i < model.OctopusBlocks.Count; i++)
                    {
                        if (model.OctopusBlocks[i].Idxs == null || model.OctopusBlocks[i].Idxs.Count == 0) continue;
                        if (model.OctopusBlocks[i].Idxs[0] == f || model.OctopusBlocks[i].Idxs[0] == l )
                        {
                            octopusInkEffect.TimerDrop(i);
                        }
                    }

                    break;
                default:
                    break;
            }



            if (state == INGameState.GameEnd)
            {
                NLog.Log("게임 끝 판정 Board ");
                if (this is BattleGameBoard)
                {
                    if(ClientData.Instance.UserData.BattleInfo.isAI)
                    {
                        runGameMgr.OnGameEnd();
                    }
                }
                else
                {
                    runGameMgr.OnGameEnd();
                }
            }
            else
            {
                if (combo >= 10)
                {
                    OnHammerMode();
                }

                if (state == INGameState.NextPage)
                {
                    //  NLog.Log(" 넥스트 페이지 판정 Board");
                  NextPage();
                }
            }

            blocks[To1D(f)] = null;
            blocks[To1D(l)] = null;
        }

        private void SelectBlock(Vector2Int idx)
        {
            //  NLog.Log("선택된 인덱스 " + idx);
            if (runGameMgr.runGameData.isHammerMode) // BlockSelect
            {
                Release();

                if (gameCells[To1D(idx)].Property.enableTouch) // _model.types[idx.x, idx.y]
                {
                    HammerRemove(idx);
                    
                    if (model.isSelectedHammerMode)
                    {
                        SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetComboAudioClipDatas()[10]);
                        SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipSelects()[1]);
                    }
                }
            }
            else
            {
                switch (model.BlockSelect(idx))
                {
                    case ClickState.OneClick: // 터치거나 처음 클릭 했을때 클릭 

                        OneClick(idx);
                        break;
                    case ClickState.OtherOneClick: // 안맞는 부분 클릭
                                                   //    Debug.Log("아더 클릭 BlockSelect ");
                        Release();// 전에 클릭 한 정을 초기화 할지 말지 
                        OneClick(idx);
                        break;
                    case ClickState.TwoClick:

                        BlockRemove(idx);
                        break;
                    case ClickState.Cancel:
                        Release();
                        break;
                }
            }

            GapBetweenBlocks(idx);

        }

        public virtual void BlockRemove(Vector2Int idx) // 이벤트로 삭제중 
        {
            List<BlockPlace> blockPlaces = new List<BlockPlace>()
            {
                new BlockPlace(PreClickIdx(), GetBlockPos(PreClickIdx())),
                new BlockPlace(To1D(idx), GetBlockPos(To1D(idx)))
            };

           runGameMgr.OnBlockRemove(MatingBlock, idx, blockPlaces);
        }

        public virtual void HammerRemove(Vector2Int idx)
        {
            int n1 = To1D(HammerPairIdx().first);
            int n2 = To1D(HammerPairIdx().last);

            List<BlockPlace> blockPlaces = new List<BlockPlace>()
            {
                new BlockPlace(n1, GetBlockPos(n1)),
                new BlockPlace(n2, GetBlockPos(n2))
            };

            runGameMgr.OnHitHammer(HitHammer, idx, blockPlaces);
        }



        private DirectionState GapBetweenBlocks(Vector2Int index)
        {
            DirectionState ds = new DirectionState(false, false, false, false);

            if (index.x > 0 && model.kinds[index.x - 1, index.y] >= 0 &&
                gameCells[To1D(index.x - 1, index.y)].Property.enableTouch == false)
            {
                ds.isLeft = true;

                ColliderExpand(new Vector2Int(index.x - 1, index.y), Direction.LEFT);
            }

            if (index.x < model.widCnt - 1 && model.kinds[index.x + 1, index.y] >= 0 && gameCells[To1D(index.x + 1, index.y)].Property.enableTouch)
            {
                ds.isRight = true;

                ColliderExpand(new Vector2Int(index.x + 1, index.y), Direction.RIGHT);
            }

            if (index.y > 0 && model.kinds[index.x, index.y - 1] >= 0 && gameCells[To1D(index.x, index.y - 1)].Property.enableTouch)
            {
                ds.isUp = true;

                ColliderExpand(new Vector2Int(index.x, index.y - 1), Direction.TOP);
            }

            if (index.y < model.higCnt - 1 && model.kinds[index.x, index.y + 1] >= 0 && gameCells[To1D(index.x, index.y + 1)].Property.enableTouch)
            {
                ds.isDown = true;

                ColliderExpand(new Vector2Int(index.x, index.y + 1), Direction.BOTTOM);
            }

            return ds;
        }

        public void ColliderExpand(Vector2Int idx, Direction dir) => gameCells[To1D(idx)].InteractionEx(dir);

        public void ColliderExpand()
        {
            for (int j = 0; j < model.higCnt; j++)
            {
                for (int i = 0; i < model.widCnt; i++)
                {
                    // i, j 

                    bool isLeftBlank = i > 0 && gameCells[To1D(i - 1, j)].Property.enableTouch;

                    bool isRightBlank = i < model.widCnt - 1 && gameCells[To1D(i + 1, j)].Property.enableTouch;

                    bool isUpBlank = j > 0 && gameCells[To1D(i, j - 1)].Property.enableTouch;

                    bool isDownBlank = j < model.higCnt - 1 && gameCells[To1D(i, j + 1)].Property.enableTouch;

                    gameCells[To1D(i, j)].InteractionEx(isLeftBlank, isRightBlank, isUpBlank, isDownBlank);
                }
            }
        }

        public void LinkBorder(Vector2Int f, Vector2Int l, bool isCombo)
        {
            (blocks[To1D(f)] as TouchBlock).LinkBorder(isCombo);
            (blocks[To1D(l)] as TouchBlock).LinkBorder(isCombo);
        }

        public void OnHammerMode()
        {
            model.comboDuration = 0;
            runGameMgr.runGameData.isHammerMode = true;
            model.isSelectedHammerMode = true;
            hammerModeEffect.gameObject.SetActive(true); // 해머 모드 이팩트 끔

            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetComboAudioClipDatas()[10], true);
        }

        public void HammerModeHide()
        {
            model.comboDuration = 0;
            SoundManager.Instance.StopSFX(ClientData.Instance.AssetDataFileLink.GetComboAudioClipDatas()[10]);
            model.isSelectedHammerMode = false;
            hammerModeEffect.gameObject.SetActive(false); // 해머 모드 이팩트 끔
        }

        public void LockRemove(Vector2Int idx, int combo)
        {
            if (lockRemoveSoundCoroutine == null) lockRemoveSoundCoroutine = StartCoroutine(LockRemoveSoundCoroutine());

            blocks[To1D(idx)].Remove(combo);
        }

        private Coroutine lockRemoveSoundCoroutine = null;
        public IEnumerator LockRemoveSoundCoroutine()
        {
            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_key_hole"));
            yield return new WaitForSeconds(0.1f);
            lockRemoveSoundCoroutine = null;
            yield break;

        }

        public async void GameEnd()
        {
            // 남은 블록 처리

            await Task.Delay(500);

            int n = 0;

            for (int j = 0; j < model.types.GetLength(1); j++)
            {
                for (int i = 0; i < model.types.GetLength(0); i++)
                {
                    if (model.types[i, j] != BlockType.None)
                    {
                        blocks[n].Remove(0); //

                    }
                    n++;

                }

            }
        }
        #region 아이템
        public void OnHint()
        {
            Hint();
        }
        public void Shuffle()
        {
            HideHint();

            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_shuffle").Load());

            model.Shuffle();

            int n = 0;
            for (int j = 0; j < model.higCnt; j++)
            {
                for (int i = 0; i < model.widCnt; i++)
                {
                    if (blocks[n] != null && blocks[n] is NormalBlock)
                    {
                        int k = model.kinds[i, j];
                        if(ClientData.Instance.blockPropertyData.GetBlockProperty( model.types[i,j]).isDynamic)
                        (blocks[n] as NormalBlock).Shuffle(runGameMgr.blockSpriteMapper.mixedSprites[k]);
                    }
                    n++;
                }
            }

        }
        private void Hint()
        {
            if(model.preHintPair !=null )
            {
                Vector2Int preIdx = model.preHintPair.first;

               if ( ClientData.Instance.blockPropertyData.GetBlockCreateProperty(model.types[preIdx.x, preIdx.y]).enableTouch )
                {
                    (blocks[To1D(model.preHintPair.first)] as TouchBlock).HideHint();
                    (blocks[To1D(model.preHintPair.last)] as TouchBlock).HideHint();
                }

                model.preHintPair = null;
            }

            PairIdx pairIdx = model.Hint();
            if (pairIdx == null) return;
     
            (blocks[To1D(pairIdx.first)] as TouchBlock).Hint();
            (blocks[To1D(pairIdx.last)] as TouchBlock).Hint();

            model.preHintPair = pairIdx;
        }
        #endregion

        public void OnComboInit() // 콤보 이펙트는 블록 위에서 이루어 지기 때문에 
        {
            if (model.GetType(model.clickIdx) != BlockType.None)
                (blocks[To1D(model.clickIdx)] as TouchBlock).ComboInit();
        }

        protected IEnumerator NextPageCoroutine()
        {
            RunGameData data = runGameMgr.runGameData;
            int combo = runGameMgr.runGameData.comboCount;

            NLog.Log("넥스트 타임 ");
            model.isPause = true;
            model.currentTouchDatas = new List<TouchData>();
            model.touchPhaseEndedCnt = 0;

            yield return new WaitForSeconds(0.1f);
            int n = 0;
            bool isRemove = false;

            for (int j = 0; j < model.types.GetLength(1); j++)
            {
                for (int i = 0; i < model.types.GetLength(0); i++)
                {
                    if (model.types[i, j] != BlockType.None)
                    {
                        blocks[n].Remove(combo);
                        isRemove = true;
                    }
                    n++;
                }

            } // 남은 블록 제거 

            if (isRemove) yield return new WaitForSeconds(0.5f);
            // 오브젝트 삭제
            gameCells.ForEach((c) => c.Destroy());

            for (int i = cellParent.childCount - 1; i >= 0; i--) Destroy(cellParent.GetChild(i).gameObject);

            if (model.NextPageInit())
            {
                gameCells = new List<PlayerGameCell>();
                blocks = new List<Block>();//Block[data.widthCount * data.hightCount];

                // _blockFactory.Initialized(); //전에 셋팅함 

                //_gameSetting = gameSetting; // 전에 셋팅함 

                n = 0;
                for (int j = 0; j < data.hightCount; j++)
                {
                    for (int i = 0; i < data.widthCount; i++)
                    {
                        // NLog.Log(" 생성 시작  {0} , {1}, {2}", i, j, _model.types[i, j]);

                        CreateCell(
                                model.types[i, j],
                                model.kinds[i, j], // 숫자 블록 때문에
                            
                                new Vector2Int(i, j),
                                model.cellPositions[i, j]);
                        n++;
                    }
                }
                //// UI도 셋팅하고 타임라인 도 셋팅하고

                //_onFlipEnd = onFlipEnd;
                //_onMatingBlock = onMatingBlock;
                //_onPageNext = onPageNext;
                //_onGameEnd = onGameEnd;
                //_onHammerHit = onHammerHit;

                OnGameRun();
            }
        }

        public void NextPage() => StartCoroutine(NextPageCoroutine());
      
        public float GetProcess() => model.process;

        public bool GetPause() => model.isPause;

        public bool GetGameEnd() => model.isGameEnd;

        public List<BlockPlace> GetBlockPlaces(BlockActionType type, Vector2Int idx )
        {
            List<BlockPlace> blockPlaces = new List<BlockPlace>();

            switch(type)
            {
                case BlockActionType.Mating:
                    blockPlaces.Add(new BlockPlace(PreClickIdx(), GetBlockPos(PreClickIdx())));
                    blockPlaces.Add(new BlockPlace(To1D(idx)    , GetBlockPos(To1D(idx))));

                    break;
                case BlockActionType.Hammer:
                    break;
            }

            return blockPlaces;
        }

        private void Update()
        {
            if(GetTouchEnable())
            {
                model.comboDuration += Time.deltaTime;
                if (model.comboDuration >= ClientData.Instance.gameSetting.comboTimeLimit) 
                {
                    if (model.comboDuration != 0)
                    {
                        model.comboDuration = 0;
                       runGameMgr.runGameEvent.onComboInit.Invoke();
                    }
                }
            }

            if (GetTouchEnable() && runGameMgr.runGameData != null)
            {
                // _gameBoard.Selected(_runGameData.comboCount > 0);
                TouchEnableInit();

                if (Input.touchCount > 0)
                {
                    TouchModule(GetTouchEnable());
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        MouseModule(true);
                    }
                    else if (Input.GetMouseButtonUp(0))
                    {
                       MouseModule(false);
                    }
                   Selected(runGameMgr.runGameData.comboCount > 0);
                }

                runGameMgr.ContinueUpdate();
            }
        }

        public void LateUpdate()
        {
            if (GetTouchEnable())
            {
                Selected(runGameMgr.runGameData.comboCount > 0);  // 여기 있어야 한다 

                if (Input.touchCount > 0)
                {
                    TouchModule(GetTouchEnable());
                }
            }
        }

        public void InitComboDuration()
        {
            model.comboDuration = 0;
        }




    }
}
