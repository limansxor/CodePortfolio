using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;
using UnityEngine;
using UnityEngine.Events;
using static NGle.Solitaire.Data.UserData.LocalUserBattleInfo;

namespace NGle.Solitaire.RunGame
{
    public class BattleGameBoard : GameBoard
    {
        [SerializeField] Transform challengerCellParent;
        [SerializeField] Transform challengerVfxParent;

        [SerializeField] BlockFactory challengerFactory;

        [SerializeField] GameBoardModel battleModel;

        List<Block> challengerExistingBlocks;

        public List<GameCell> challengerCells;
        public List<Block> challengerBlocks;

        private Coroutine challengerBlinkerGimmickCoroutine;

        private Attack attack;
        private ComboPowerAttack comboPowerAttack;

        public override void Initialized(RunGameMgr runGameMgr)
        {
            base.Initialized(runGameMgr);

            battleModel.InitGameBoard(null, null);

            challengerBlocks = new List<Block>();//new Block[data.widthCount * data.hightCount];
            challengerFactory.Initialized(runGameMgr);

            challengerExistingBlocks = new List<Block>();
            //_onChallengerPageNext = onChallengerPageNext;

            challengerCells = new List<GameCell>();

            int n = 0;
            for (int j = 0; j < runGameMgr.runGameData.hightCount; j++)
            {
                for (int i = 0; i < runGameMgr.runGameData.widthCount; i++)
                {
                    ChallengerCreateCell(
                         battleModel.types[i, j],
                         battleModel.kinds[i, j], // 숫자 블록 때문에
                         new Vector2Int(i, j),
                         battleModel.cellPositions[i, j]);
                    n++;
                }
            }

            battleModel.BlockCountCheck(true);

            model.isGameEnd = false;

            (runGameMgr.runGameEvent as BattleRunGameEvent).AddEvtAttack(OnAttack);

            (runGameMgr.runGameEvent as BattleRunGameEvent).onFindAttackBlock = OnFindAttackBlock;

            attack = ClientData.Instance.runGameDataFileLink.GetAttackPrefabs((runGameMgr as BattleRunGameMgr).evtCanvas.transform, AttackPrefabType.Attack);

            comboPowerAttack = ClientData.Instance.runGameDataFileLink.GetAttackPrefabs((runGameMgr as BattleRunGameMgr).evtCanvas.transform, AttackPrefabType.ComboPowerAttack) as ComboPowerAttack;
        }

        public void ChallengerCreateCell(BlockType type, int kind, Vector2Int idx, Vector3 pos)
        {
            GameCell gameCell = ClientData.Instance.runGameDataFileLink.CreateGameCell(challengerCellParent, pos);

            gameCell.Register(challengerFactory, type, kind, kind % 1000, idx, pos, challengerVfxParent); // pos 필요한 이유 efffect 넣어야 하기 때문에 

            challengerBlocks.Add(gameCell.GetBlock());

            if (kind != -1)
                challengerExistingBlocks.Add(gameCell.GetBlock());

            challengerCells.Add(gameCell);

            //return gameCell;
        }

        public override void OnGameRun()
        {
            base.OnGameRun();
            StartCoroutine(process.StartFlipCoroutine(battleModel, challengerBlocks,
                BattleRunGameMgr.Instance.OnFlipEnd)); // 두군데 같이 뒤집는 거 
        }

        public int GetChallengerPage() => battleModel.currentPage;

        public void ChallengerPageSkip(int curtPage)
        {
            NLog.Log("Challenger 모델 변화 ");
            battleModel.isPause = true;
            battleModel.currentTouchDatas = new List<TouchData>();
            battleModel.touchPhaseEndedCnt = 0;

            // 오브젝트 삭제
            challengerCells?.ForEach((c) => c.Destroy());
            for (int i = challengerCellParent.childCount - 1; i >= 0; i--) Destroy(challengerCellParent.GetChild(i).gameObject);


            NLog.Log("오브젝트 삭제  ");

            battleModel.PageSkip(curtPage);

            challengerCells = new List<GameCell>();
            challengerBlocks = new List<Block>();// new Block[data.widthCount * data.hightCount];

            int n = 0;
            for (int j = 0; j < runGameMgr.runGameData.hightCount; j++)
            {
                for (int i = 0; i < runGameMgr.runGameData.widthCount; i++)
                {
                    ChallengerCreateCell(
                    battleModel.types[i, j],
                    battleModel.kinds[i, j], // 숫자 블록 때문에

                    new Vector2Int(i, j),
                    battleModel.cellPositions[i, j]);
                    n++;
                }
            }

            //string output = string.Join(", ", battleModel.kinds.Cast<int>());
            //NLog.Log($"Skip _model.kinds : " + output);
        }

        public IEnumerator ChallengerNextPageCoroutine()
        {

            NLog.Log("Challenger 넥스트 타임 ");
            battleModel.isPause = true;
            battleModel.currentTouchDatas = new List<TouchData>();
            battleModel.touchPhaseEndedCnt = 0;

            yield return new WaitForSeconds(0.1f);
            int n = 0;
            bool isRemove = false;
            for (int j = 0; j < battleModel.types.GetLength(1); j++)
            {
                for (int i = 0; i < battleModel.types.GetLength(0); i++)
                {
                    if (battleModel.types[i, j] != BlockType.None)
                    {
                        challengerBlocks[n].Remove(0);
                        isRemove = true;
                    }
                    n++;

                }

            }

            if (isRemove) yield return new WaitForSeconds(0.5f);
            // 오브젝트 삭제
            challengerCells?.ForEach((c) => c.Destroy()); 
            for (int i = challengerCellParent.childCount - 1; i >= 0; i--) Destroy(challengerCellParent.GetChild(i).gameObject);

            NLog.Log("오브젝트 삭제  ");
            ClientData.Instance.UserData.BattleInfo.removeDatas.Clear();
            ClientData.Instance.UserData.BattleInfo.removeDatas = null;
            ClientData.Instance.UserData.BattleInfo.removeDatas = new List<RemoveData>();

            if (battleModel.NextPageInit())
            {
                challengerCells = new List<GameCell>();
                challengerBlocks = new List<Block>();

                n = 0;
                for (int j = 0; j < runGameMgr.runGameData.hightCount; j++)
                {
                    for (int i = 0; i < runGameMgr.runGameData.widthCount; i++)
                    {
                        ChallengerCreateCell(
                 battleModel.types[i, j],
                 battleModel.kinds[i, j], // 숫자 블록 때문에
                 new Vector2Int(i, j),
                 battleModel.cellPositions[i, j]);

                        n++;
                    }
                }

                StartCoroutine(process.StartFlipCoroutine(battleModel, challengerBlocks, BattleRunGameMgr.Instance.ChallengerPageNextEnd));
            }
        }
        #region Attack

        public void ChangeBlockTypes( AttackType type, bool isSend, List<int> idxs) => idxs.ForEach((idx) => ChangeBlockType(type, isSend, idx )); // 공격할때 
                                                                                                                                                   //{
        private void ChangeBlockType(AttackType type, bool isSend, int n)
        {
            Block block = null;
            if (isSend)
            {
                if (challengerBlocks[n] != null &&
                    challengerBlocks[n].GetEnableActive() &&
                    challengerCells[n].Property.type == BlockType.Normal)
                {
                    NormalBlock attackBlock = (challengerBlocks[n] as NormalBlock);

                    attackBlock.Hide();//Attact(); // 셀이 두개의 블록을 가질 수도 있다.

                    challengerBlocks.RemoveAt(n);

                    if (challengerBlocks == null) return;
                    block = challengerFactory.ChangeType(type, challengerCells[n], challengerVfxParent);

                    challengerExistingBlocks.Add(block);   //~~~~~~ 바뀐 블록 지울 수 있게 저장을 한다  

                    challengerCells[n].ChangeBlock(block, AttackType.Question == type ? BlockType.Question : BlockType.Blinker);
                    challengerBlocks.Insert(n, block);

                    if (block is BlinkerBlock)
                    {
                        (block as BlinkerBlock).ChangeStart();
                    }
                    else if (block is QuestionBlock)
                    {
                        (block as QuestionBlock).ChangeStart();
                    }
                }
            }
            else
            {
                if (blocks[n] != null &&
                    blocks[n].GetEnableActive() &&
                    gameCells[n].Property.type == BlockType.Normal)
                {
                    NormalBlock attackBlock = (blocks[n] as NormalBlock);
                    attackBlock.Hide();//Attact();

                    blocks.RemoveAt(n);

                    if (gameCells == null) return;
                    block = blockFactory.ChangeType(type, gameCells[n], cellParent);

                    gameCells[n].ChangeBlock(block, AttackType.Question == type ? BlockType.Question : BlockType.Blinker);
                    blocks.Insert(n, block);

                    //_blocks[n]?.Initialized();
                    if (block is BlinkerBlock)
                    {
                        (block as BlinkerBlock).ChangeStart();
                    }
                    else if (block is QuestionBlock)
                    {
                        (block as QuestionBlock).ChangeStart();
                    }


                }
            }

        }

        public List<BlockPlace> OnFindAttackBlock(AttackType type, bool isMy)
        {
            List<BlockPlace> blockPlaces =  new List<BlockPlace>();

            if (isMy) // 반대로 처리 | 내가 공격 하면 상대방 블록 변경 
            {
                List<int> idxs = battleModel.AttackBlock(type);
                if (idxs == null) return null;

                idxs.ForEach(i => blockPlaces.Add(new BlockPlace(i, challengerBlocks[i].GetPos())));
            }
            else
            {
                List<int> idxs = model.AttackBlock(type);
                if (idxs == null) return null;
                idxs.ForEach(i => blockPlaces.Add(new BlockPlace(i, blocks[i].GetPos())));
            }

            return blockPlaces;
        }


        public void BlinkerGimmickAdd(bool isSend, List<BlockPlace> indexs) => indexs.ForEach(p => BlinkerGimmickAdd(isSend, p.idx));

        public void BlinkerGimmickAdd(bool isSend, int index)
        {
            if (isSend)
            {
                if (challengerBlinkerGimmickCoroutine != null)
                {
                    StopCoroutine(challengerBlinkerGimmickCoroutine);
                }

                if (battleModel.types[To2D(index).x, To2D(index).y] != BlockType.None)
                {
                    battleModel.blinkerBlocks.Add(To2D(index));
                }

                challengerBlinkerGimmickCoroutine = StartCoroutine(ChallengerGimmickCoroutine());
            }
            else
            {
                if (blinkerGimmickCoroutine != null)
                {
                    StopCoroutine(blinkerGimmickCoroutine);
                }

                if (model.types[To2D(index).x, To2D(index).y] != BlockType.None)
                {
                    model.blinkerBlocks.Add(To2D(index));

                    model.AttackBlockTypeChange(AttackType.Blink,index);
                }

                blinkerGimmickCoroutine = StartCoroutine(BlinkerGimmickCoroutine());
            }
        }

        private IEnumerator ChallengerGimmickCoroutine()
        {
            int n = 0;
            while (battleModel.blinkerBlocks.Idxs.Count > 0)
            {
                if (n >= battleModel.blinkerBlocks.Idxs.Count) n = 0;

                (challengerBlocks[To1D(battleModel.blinkerBlocks.Idxs[n])] as BlinkerBlock).Blinker();

                n++;
                yield return new WaitForSeconds(1.0f);
            }

            challengerBlinkerGimmickCoroutine = null;
            yield break;
        }
        #endregion

        public override void FlipEnd()
        {
            base.FlipEnd();

            if(challengerBlinkerGimmickCoroutine == null)
            challengerBlinkerGimmickCoroutine = StartCoroutine(ChallengerGimmickCoroutine()); // 계속 반복 할 것이라서 
        }

        public override  void GamePause(bool isGameEnd = false)
        {
            if (isGameEnd )
            {
                HammerModeHide();
                StartCoroutine(GamePauseCoroutine());
            }
            else
            {
                base.GamePause(isGameEnd);

                challengerBlocks.ForEach((b) =>
                {
                    if (b is TouchBlock)
                    {
                        (b as TouchBlock).Pause();
                    }

                });
            }

        }

        private IEnumerator GamePauseCoroutine()
        {
            base.GamePause(true);

            yield return new WaitForSeconds(0.5f);

            challengerBlocks.ForEach((b) =>
            {
                if (b is TouchBlock)
                {
                    (b as TouchBlock).Pause();
                }

            });
        }


        private BlockType GetChallengerType(Vector2Int idx) => battleModel.GetType(idx);

        public bool ChallengerMatchingBlock(Vector2Int f, Vector2Int l, RecvRemoveType t)
        {

            if (GetChallengerType(f) == BlockType.Blinker) battleModel.blinkerBlocks.Idxs.Remove(f);

            if (GetChallengerType(l) == BlockType.Blinker) battleModel.blinkerBlocks.Idxs.Remove(l);


            if (t == RecvRemoveType.Normal)
            {
                ChallengerRemove(f, l, CheckGameState());
            }
            else
            {
                StartCoroutine(ChallengerHitHammer(f, l, CheckGameState()));
            }

            battleModel.Break(f);
            battleModel.Break(l);
            battleModel.BlockCountCheck(false);


            if ( (runGameMgr.runGameData as BattleRunGameData).otherBlockCount == 0)
            {
                return false;
            }
            return true;
        }

        public void ChallengerRemove(Vector2Int f, Vector2Int l, INGameState state) //  INGameState state
        {

            challengerBlocks[To1D(f)]?.Remove(0);
            challengerBlocks[To1D(l)]?.Remove(0);

            //_challengerBlocks[FlattenIdx(l)].ComboRun(combo);

            //NLog.Log("Remove 타입 " + type);
            //if (kind != -1 && type == BlockType.KeyBlock)
            //{
            //    StartCoroutine(_process.UnlockAni(_model, LockRemove, kind, combo));
            //}

            if (state == INGameState.GameEnd)
            {
                NLog.Log("게임 끝 판정 Board ");
                // _onGameEnd?.Invoke();
            }
            else
            {
                if (state == INGameState.NextPage)
                {
                    NLog.Log(" 넥스트 페이지 판정 Board");

                    BattleRunGameMgr.Instance.OnFlipEnd();
                }
            }

           
        }
        #region Reconnect

        private void RecvAttack(AttackType attackType, List<int> recvIdx)
        {
            List<BlockPlace> blockPlaces = new List<BlockPlace>();
            recvIdx.ForEach(n => blockPlaces.Add(new BlockPlace(n, blocks[n].GetPos())));

           // ClientData.Instance.runGameData.onAttack.Invoke(attackType, false, blockPlaces); // 이것 살려야 한다 
        }

        private int isPreReconnectIndex = -1;
        public void ReconnectRemove(List<int> idxs)
        {
            if ((int)AttackType.Blink * 1000 <= idxs[0] &&
                    idxs[0] < ((int)AttackType.Blink + 1) * 1000)
            {
                // 어텍 관련이라 
                idxs[0] -= (int)AttackType.Blink * 1000;
                idxs[1] -= (int)AttackType.Blink * 1000;
                RecvAttack(AttackType.Blink, idxs); // 셀 타입 여기서 바꾼다 
            }
            else if ((int)AttackType.Question * 1000 <= idxs[0] &&
                    idxs[0] < ((int)AttackType.Question + 1) * 1000)
            {
                // 어텍 관련이라
                idxs[0] -= (int)AttackType.Question;
                idxs[1] -= (int)AttackType.Question;
                RecvAttack(AttackType.Question, idxs);
            }
            else if (idxs[0] >= 0)
            {
                foreach (var n in idxs)
                {
                    Vector2Int idx = battleModel.To2D(n);

                    challengerCells[n].Remove();
                    if (GetChallengerType(idx) == BlockType.Blinker) battleModel.blinkerBlocks.Idxs.Remove(idx);
                    battleModel.Break(idx);

                    if (challengerBlocks[n] != null)
                        challengerBlocks[n].Remove(); // 어차피 인터랙션 활성화는 안 돼 있어서
                }

            }
            else if (-100 == idxs[0])
            {
                // 셔플 해야 한다.
                ChallengerShuffle();
            }
            // isPreReconnectIndex = n;
        }

        public void BattleBlockCountCheck() => battleModel.BlockCountCheck(false);

        #endregion
        public void ChallengerShuffle()
        {
            battleModel.Shuffle();
            int n = 0;
            for (int j = 0; j < battleModel.higCnt; j++)
            {
                for (int i = 0; i < battleModel.widCnt; i++)
                {
                    if (challengerBlocks[n] is NormalBlock)
                    {
                        int k = battleModel.kinds[i, j];
                        if (ClientData.Instance.blockPropertyData.GetBlockProperty(battleModel.types[i, j]).isDynamic)
                            (challengerBlocks[n] as NormalBlock).Shuffle(runGameMgr.blockSpriteMapper.mixedSprites[k]);
                    }
                    n++;
                }
            }
        }

        public IEnumerator ChallengerHitHammer(Vector2Int f, Vector2Int l, INGameState state)
        {

            challengerCells[To1D(f)].HitHammer();
            challengerCells[To1D(l)].HitHammer();

            yield return new WaitForSeconds((challengerBlocks[To1D(f)] as TouchBlock).HammerDuration());

            ChallengerRemove(f, l, state);
        }

        private INGameState CheckGameState()
        {
            INGameState state = INGameState.Continue;

            if ((runGameMgr.runGameData as BattleRunGameData).otherBlockCount == 2)
            {
                if (battleModel.currentPage < battleModel.pageNumMax - 1) // 0<1
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

        public float GetChallengerProcess()
        {
            return battleModel.process;
        }

        public void OnAttack(AttackType type, bool isMy, List<BlockPlace> recvIdxs)  // 공격을 받은 경우 // 공격 한경우도 포함 
      => StartCoroutine(OnAttackCoroutine( type,  isMy, recvIdxs));

        IEnumerator OnAttackCoroutine(AttackType type, bool isMy, List<BlockPlace> recvIdxs)
        {
            List<int> idxs = new List<int>();

            if (recvIdxs == null) yield break;

            if (isMy == false) // 내가 받은 메시지 이상 값 체크 
            {
                foreach (var item in recvIdxs)
                {
                    if (item.pos.x == 8787)
                    {
                        yield break;
                    }
                }
            }

            recvIdxs.ForEach(b => idxs.Add(b.idx)); // 받은 메시지 인덱스 전환 

            if (isMy == false) // 받은 경우 터치 불가 
            {
                gameCells[idxs[0]].PauseInteraction();
                if (idxs.Count > 1) gameCells[idxs[1]].PauseInteraction();
            }

            yield return new WaitForSeconds((runGameMgr.runGameData as BattleRunGameData).attackAniDuration);
          
            ChangeBlockTypes(type, isMy, idxs);

            if (type == AttackType.Blink)
            {
                BlinkerGimmickAdd(isMy, recvIdxs.ToList());
            }

            if (isMy == false) // 받은 경우 터치 불가 해제 
            {
                gameCells[idxs[0]].ContinueInteraction();
                if (idxs[1] != -1) gameCells[idxs[1]].ContinueInteraction();
            }
        }

        public void ChallengerGameRun()
        {
            StartCoroutine(process.StartFlipCoroutine(battleModel, challengerBlocks, BattleRunGameMgr.Instance.ChallengerPageNextEnd));
        }

   


    }



}
