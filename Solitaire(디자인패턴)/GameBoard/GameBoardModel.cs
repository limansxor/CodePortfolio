using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Support;
using NGle.Solitaire.Data;
using System.Linq;

namespace NGle.Solitaire.RunGame
{
    public class GameBoardModel : MonoBehaviour
    {
        private PathFindLogic _pathFindLogic; // 모델 제어 인데 핵심 알고리즘 까보진 말자
                                              // public PathFindLogic pathFindLogic { get { return _pathFindLogic; } }
        [SerializeField] PairIdx _hammerIdxs;
        public PairIdx hammerIdxs { get { return _hammerIdxs; } }
  
        [SerializeField] Vector2Int _clickIdx; // 한번 클릭 됬는지 두번 클릭 됬는 
        public Vector2Int clickIdx { get { return _clickIdx; } }

        [SerializeField]
        private int _clickCount = 0;
        public int clickCount { get { return _clickCount; } }

        [SerializeField] protected int[,] _kinds;
        public int[,] kinds { get { return _kinds; } }

        [SerializeField] private BlockType[,] _types;
        public BlockType[,] types { get { return _types; } }

        [SerializeField]
        private Vector3[,] _cellPositions;
        public Vector3[,] cellPositions { get { return _cellPositions; } }

        [SerializeField] ListIdx[] pairDatas;// 힌트 계산용 변수
        [SerializeField] ListIdx[] keyPairData;
        [SerializeField] ListIdx[] numPairData;
        [SerializeField] ListIdx[] octPairData;

        List<ListIdx[]> pairDatasList;

        public PairIdx preHintPair = null;
        [SerializeField] List<PairIdx> _enablePairIndex; // 가능한 페어 인덱스 계산
        public List<PairIdx> enablePairIndex { get { return _enablePairIndex; } }

        [Header("알고리즘에 사용 되는 변수 ")]

        public Dictionary<int, List<Vector2Int>> numberBlocks;

        [SerializeField]
        private bool _isClickEnable;
        public bool isClickEnable { get { return _isClickEnable; } set { _isClickEnable = value; } }

        public bool isSelectedHammerMode = false;

        [SerializeField]
        private int _selectStageNum = -1;
        public int selectStageNum { get { return _selectStageNum; } }

        // 유저 케어 들어 갑니다;
        public Vector2Int preClickIndex = new Vector2Int(-1, -1); // 클릭했다고 다 선택 한게 아니다 

        public Vector2Int releaseIndex;

        // 페이지 관련 변수

        private int _currentPage = 0; // 0,1,2
        public int currentPage { get { return _currentPage; } }
        private int _pageNumMax;
        public int pageNumMax { get { return _pageNumMax; } }

        public int autoShuffleCnt = 0;
        //        private int shuffleSum = 0; // 나중에 쓰인다 

        public bool isViewHammerRelease = false;

        public List<PageInfo> pageInfoList;

        public List<TouchData> currentTouchDatas = new List<TouchData>();

        private float _process = 0;
        public float process { get { return _process; } set { _process = value; } }

        public List<Vector3> _linePos;
        public List<Vector3> linePos { get { return _linePos; } }

        private int _currentPairCnt;
        public int currentPairCnt { get { return _currentPairCnt; } }

        [SerializeField] int _widCnt;
        public int widCnt { get { return _widCnt; } }

        [SerializeField] int _higCnt;
        public int higCnt { get { return _higCnt; } }

        [SerializeField] float _scaleX;
        public float scaleX { get { return _scaleX; } }

        [SerializeField] float _scaleY;
        public float scaleY { get { return _scaleY; } }

        public int numberBlockEnableCount;
        
        // 게임 상태 관련
        public bool isEnd;
        public bool isPause;
        public Vector2Int preIdx;
        public bool[] touchEnables;
        public Vector3 centerPos;
        public int touchPhaseEndedCnt;

        //public int widCnt;
        //public int higCnt;

        public PairIdx hammerPairIdx;

        public PathFindLogic.Result pathResult;

        public bool isGameEnd;

        private Action onHammer;
        private Action onShuffle;
        private Action onShuffleCancel;

        private int curtSeed;

        public ListIdx _blinkerBlocks;
        public ListIdx blinkerBlocks { get { return _blinkerBlocks; } }

        [SerializeField]
        private ListIdx[] _unlockObjInfos = new ListIdx[5];
        public ListIdx[] unlockObjInfos { get { return _unlockObjInfos; } }

        [SerializeField]
        private List<ListIdx> octopusBlocks; // 지금은 3개로 고정이긴 한데 
        public List<ListIdx> OctopusBlocks { get { return octopusBlocks; } }

        public float comboDuration;

        RunGameMgr mgr;

        public void Initialized(RunGameMgr mgr)
        {
            this.mgr = mgr;

            isEnd = false;
            isPause = true;

            preIdx = new Vector2Int(-1, -1);

            for (int i = 0; i < touchEnables.Length; i++) touchEnables[i] = true;

            centerPos = new Vector3(-0.06f, 0, 0);

            touchPhaseEndedCnt = 0;

            currentTouchDatas = new List<TouchData>();

            hammerPairIdx = null;

            pathResult = null;

            touchEnables= new bool[10];

            numberBlockEnableCount=0;

            preHintPair = null;

            comboDuration = 0;
        }

        private bool CurrentPageValue(int page)
        {

            NLog.Log(gameObject.name +"-> "+  page + " page 값 확인 " + pageInfoList.Count + " Page 카운");

            //  NLog.Log(pageInfoList[0].infoSetList[0].);
            if (pageInfoList.Count > page)
            {
                PageInfo info = pageInfoList[page];

                CurrentPageValue(pageInfoList[page].infoSetList[info.maxIdx].pairDatas,
                  pageInfoList[page].infoSetList[info.maxIdx].pairCount,
                  pageInfoList[page].infoSetList[info.maxIdx].kinds,
                  pageInfoList[page].types,
                  pageInfoList[page].blinkerBlocks,
                  pageInfoList[page].unlockObjLists,
                   pageInfoList[page].NumberBlocks,
                  widCnt, higCnt,
                  scaleX, scaleY,
                  pageInfoList[page].seed,
                  pageInfoList[page].OctopusBlocks
                  );
                return true;
            }
            return false;
        }
        protected virtual void CurrentPageValue(ListIdx[] pairDatas,
            int currentPairCnt,
            int[,] kinds,
            BlockType[,] types,
            ListIdx blinkerBlocks,
            ListIdx[] unlockObjInfos,
            Dictionary<int,List<Vector2Int>> numberBlocks,
            int widCnt, int higCnt,
            float scaleX, float scaleY,
            int seed,
             List<ListIdx> octopusBlocks)
        {
            this.pairDatas = pairDatas;

            _currentPairCnt = currentPairCnt;

            _kinds = new int[kinds.GetLength(0), kinds.GetLength(1)];
            _types = new BlockType[kinds.GetLength(0), kinds.GetLength(1)];

            for (int j = 0; j < kinds.GetLength(1); j++)
            {
                for (int i = 0; i < kinds.GetLength(0); i++)
                {
                    _kinds[i, j] = kinds[i, j];
                    _types[i,j] = types[i,j];
                }
            }

            _blinkerBlocks = blinkerBlocks;

            _unlockObjInfos = unlockObjInfos;

            _cellPositions = new Vector3[widCnt , higCnt];

            this.numberBlocks = numberBlocks;

            curtSeed = seed;

          

            for (int j = 0; j < higCnt; j++)
            {
                for (int i = 0; i < widCnt; i++)
                {
                    float centerX = -widCnt / 2 + i + (widCnt % 2 == 1 ? 0.0f : 0.5f) ; // 1 기준으로 중앙에 위치 하려면 필요한 0.5f
                    float centerY = higCnt / 2 - j - (higCnt % 2 == 1 ? 0.0f : 0.5f);

                    _cellPositions[i, j] = new Vector3(
                            centerX * scaleX,
                            centerY * scaleY,
                            0);
                }
            }

            this.octopusBlocks = octopusBlocks;

        }
        public void InitGameBoard(Action onHammer,  Action onShuffle) // List<string[,]> rawInfosList = null
        {
            RunGameData data = mgr.runGameData;

            _pathFindLogic = new PathFindLogic();

            _currentPage =0 ;
            _process = currentPage ;

            autoShuffleCnt = 0;
            _pageNumMax = data.pageMax; // 일단 1로 초기화 배틀 게임에서 3으로 해야 한다 이것을 어디에 두워야 하는가 ? 

            pageInfoList = new List<PageInfo>();

            string[,] loadDataInfo = new string[data.widthCount, data.hightCount];
            string[,] loadDataInfo2 = new string[data.widthCount, data.hightCount];
            string[,] loadDataInfo3 = new string[data.widthCount, data.hightCount];

            int n = 0;
            _widCnt = data.widthCount;
            _higCnt = data.hightCount;
            _scaleX = ClientData.Instance.gameSetting.scaleX;
            _scaleY = ClientData.Instance.gameSetting.scaleY;

            for (int j = 0; j < data.hightCount; j++)
            {
                for (int i = 0; i < data.widthCount; i++)
                {
                

                    loadDataInfo[i, j] = data.loadDataInfo[n];
                 
                    n++;
                }
            }

            NLog.Log("1번 패이지 정보 " + string.Join(", ", data.loadDataInfo));

            int seed = ClientData.Instance.UserData.BattleInfo.PlayerInfo == null ? -1 : ClientData.Instance.UserData.BattleInfo.PlayerInfo.curSeed;
            ClientData.Instance.UserData.BattleInfo.PlayerInfo?.SeedNext(seed);

            pageInfoList.Add(LayoutConverter(loadDataInfo, data.configData.pair, data.configData.free_shuffle,seed));


            if (data.loadDataInfo2 != null)
            {
                
                NLog.Log("2번 패이지 정보 " + string.Join(", " , data.loadDataInfo2));
                n = 0;
                for (int j = 0; j < data.hightCount; j++)
                {
                    for (int i = 0; i < data.widthCount; i++)
                    {

                        loadDataInfo2[i, j] = data.loadDataInfo2[n];

                        n++;
                    }
                }

                seed = ClientData.Instance.UserData.BattleInfo.PlayerInfo.curSeed;
                ClientData.Instance.UserData.BattleInfo.PlayerInfo.SeedNext(seed);
                pageInfoList.Add(LayoutConverter(loadDataInfo2, data.configData.pair, 1, seed));
            }

            if (data.loadDataInfo3 != null)
            {
                NLog.Log("3번 패이지 정보 " + string.Join(", ", data.loadDataInfo3));
                n = 0;
                for (int j = 0; j < data.hightCount; j++)
                {
                    for (int i = 0; i < data.widthCount; i++)
                    {

                        loadDataInfo3[i, j] = data.loadDataInfo3[n];
                        n++;
                    }
                }
                seed = ClientData.Instance.UserData.BattleInfo.PlayerInfo.curSeed;
                ClientData.Instance.UserData.BattleInfo.PlayerInfo.SeedNext(seed);
                pageInfoList.Add(LayoutConverter(loadDataInfo3, data.configData.pair, 1, seed));
            }

            CurrentPageValue(_currentPage);

            this.onShuffle = onShuffle;
            this.onHammer = onHammer;

            BlockCountCheck(true); // 셋팅 끝났으면 init
            _isClickEnable = true; // 셋팅이 끝났으니 선택 가능 모드로

          
        }

        public bool NextPageInit()
        {
            // 인스턴스 삭제 
            _currentPage++;
            _process = _currentPage;

            if (CurrentPageValue(_currentPage))
            {
                BlockCountCheck(true); // 셋팅 끝났으면 init
                _isClickEnable = true; // 셋팅이 끝났으니 선택 가능 모드로

                return true;
            }
            return false;
        }

        public void PageSkip(int n)
        {
           int p = Mathf.Clamp(n,0, pageInfoList.Count -1);

            CurrentPageValue(p);
        }

        private PageInfo LayoutConverter(string[,] rawDatas, int textMaxPair, int loopCount, int seed =-1 )
        { 
            // NLog.Log(" [확인 1] " + rowInfos.GetLength(0)+ " [확인 2] " + rowInfos.GetLength(1));

            List<InfoSet> infoSetList = new List<InfoSet>();

            int[,] baseDatas = new int[rawDatas.GetLength(0), rawDatas.GetLength(1)];

            BlockType[,] types = new BlockType[rawDatas.GetLength(0), rawDatas.GetLength(1)];

            ListIdx[] infoBlockClones = new ListIdx[loopCount];

            ListIdx blinkerBlocks = new ListIdx();

            for (int i = 0; i < infoBlockClones.Length; i++)
            {
                infoBlockClones[i] = new ListIdx();
            }

            ListIdx[] unlockObjLists = new ListIdx[5];

           List<IdxAndVal>[] idxAndValLists = new List<IdxAndVal>[5];

            Dictionary<int, List<Vector2Int>> numberBlocks = new Dictionary<int, List<Vector2Int>>();

            List<ListIdx> octopusBlocks = new List<ListIdx>();

            ClientData.Instance.gameSetting.OctopusInkTimes.ForEach(t => octopusBlocks.Add(new ListIdx()));

            int kindLength = 0;

            // ===== 변수 선언부  끝 ======

            for (int j = 0; j < rawDatas.GetLength(1); j++)
            {
                for (int i = 0; i < rawDatas.GetLength(0); i++)
                {
                    // === 초기화  =========
                    baseDatas[i, j] = -1;

                    string[] strs = rawDatas[i, j].Split('|');
                 
                    int fN = int.Parse(strs[0]);
                    int sN = strs.Length > 1 ? int.Parse(strs[1]) : 0;
                    int idxAdd = 0;
                    // ================
                    switch (fN)
                    {
                        case (int)BlockType.Normal:
                        case (int)BlockType.Question:
                        case (int)BlockType.Blinker:

                            foreach (var item in infoBlockClones)
                            {
                                item.Add(new Vector2Int(i, j));
                            }

                            kindLength++;
                            types[i, j] = (BlockType)fN;

                            if (fN == (int)BlockType.Blinker)
                            {
                                blinkerBlocks.Add(new Vector2Int(i, j));
                            }

                            break;
                        case (int)BlockType.Stone:

                            baseDatas[i, j] = 0;
                            types[i, j] = BlockType.Stone;

                            break;
                        case 5:
                        case 6:
                        case 7:

                            idxAdd = (int)BlockType.Key;

                            baseDatas[i, j] = fN - idxAdd;

                            if (sN == 0)
                            {
                                types[i, j] = BlockType.Key;
                                //  keyBlockActive[fN - 5] = true;
                                // keyBlockCount++;
                            }
                            else
                            {
                                types[i, j] = BlockType.Lock;

                                if (unlockObjLists[fN - idxAdd] == null)
                                {
                                    unlockObjLists[fN - idxAdd] = new ListIdx();
                                    idxAndValLists[fN - idxAdd] = new List<IdxAndVal>();
                                }

                                //baseDatas[i, j] = (fN - idxAdd) * 1000 + sN - 1; // 인포 변경하면 스프라이트 넣을 떄 혼난다 
                                //  Debug.Log(" _infos[i, j] " + _infos[i, j]);

                                idxAndValLists[fN - idxAdd].Add(new IdxAndVal(new Vector2Int(i, j),sN));
                            }
                            break;
                        case (int)BlockType.Number:

                            idxAdd = (int)BlockType.Number;

                            types[i, j] = BlockType.Number;
                            baseDatas[i, j] = sN ==0 ? 0 : sN-1; // 서브 가지고

                            PageInfo.NumberAddData(numberBlocks, baseDatas[i, j], new Vector2Int(i, j));

                            break;
                        case (int)BlockType.Wiremesh:

                            types[i, j] = BlockType.Wiremesh;
                            baseDatas[i, j] = -1;

                            break;
                        case (int)BlockType.Octopus:

                            idxAdd = (int)BlockType.Octopus;

                            types[i, j] = BlockType.Octopus;

                            int d = sN == 0 ? 0 : sN - 1;

                            baseDatas[i, j] =  d ; // 서브 가지고

                            octopusBlocks[d].Add(new Vector2Int(i, j));

                            break;
                        default:
                            types[i, j] = BlockType.None;
                            baseDatas[i, j] = -1;
                            break;
                    }

            
                }
            }

            List<IdxAndVal>[] idxAndValOrderLists = new List<IdxAndVal>[5];

            for (int i = 0; i < idxAndValLists.Length; i++) idxAndValOrderLists[i] = idxAndValLists[i]?.OrderBy(x => x.val).ToList();
 

            for (int i = 0; i < idxAndValOrderLists.Length; i++)
            {
                if(idxAndValOrderLists[i] != null)
                idxAndValOrderLists[i].ForEach(item => unlockObjLists[i].Add(item.idx));
            }
            

            // Debug.Log(" [확인 1] " + kindLength + " [확인 1] ");//  + textMaxPair

            int maxKindPair = Mathf.CeilToInt(kindLength / textMaxPair * 2) == 0 ? 1 : Mathf.CeilToInt(kindLength / textMaxPair * 2);// 

            //   ? 18 = 72 /4
            //   ? 18 = 72 /4

            for (int c = 0; c < loopCount; c++) // 클론 맥스 만큼 반복해서 데이터를 확인한다. 
            {
                ListIdx[] pairDatas = new ListIdx[maxKindPair];

                int[,] kinds = new int[rawDatas.GetLength(0), rawDatas.GetLength(1)];

                for (int j = 0; j < rawDatas.GetLength(1); j++)
                {
                    for (int i = 0; i < rawDatas.GetLength(0); i++)
                    {
                        if (ClientData.Instance.blockPropertyData.GetBlockProperty(types[i, j]).isDynamic == false) 
                        {
                            kinds[i, j] = baseDatas[i, j]; // 고정값 쓰는 값 저장 
                        }
                    }
                }

                //  NLog.Log("LayoutConverter seed : " + seed);

                GameRandom gameRandom = RunGameRandom.Get("GameBoardModel", "LayoutConverter", seed);

                int lenCount = infoBlockClones[c].Idxs.Count;
                for (int n = 0; n < maxKindPair; n++) // 4개여 도 2개 일수 있고 페어이기 때문에 4->2 2->1
                {
                    pairDatas[n] = new ListIdx();

                    for (int i = 0; i < textMaxPair * 2; i++) // 같은 갯수 만큼 반복 해서 넣어 준다.
                    {
                        if (lenCount > 0) // 4 여도 갯수 부족하면 2개만들어 갈 수도 있다 
                        {
                            lenCount--;

                            int idx = gameRandom.Get(0, infoBlockClones[c].Idxs.Count);

                            pairDatas[n].Idxs.Add(infoBlockClones[c].Idxs[idx]);

                            kinds[infoBlockClones[c].Idxs[idx].x, infoBlockClones[c].Idxs[idx].y] = n; // n+1
                                                                                                       // Debug.LogFormat("{3}=> _infos[{0}, {1}] = {2}", vector2Int.x, vector2Int.y, _infos[vector2Int.x, vector2Int.y],c);
                            infoBlockClones[c].Idxs.RemoveAt(idx);// 

                        }
                    }
                }

                // 차레 대로 kinds 에 배치 하다 
            
                infoSetList.Add(new InfoSet(kinds, pairDatas, MatchingSearch(kinds, pairDatas)));
            }

            int minCnt = int.MaxValue;
            int maxCnt = 0;
            int min = 0;
            int max = 0; 

            for (int i = 0; i < infoSetList.Count; i++)
            {
               if(minCnt >  infoSetList[i].pairCount ) // min 검증 까지는 뺀다 
                {
                    minCnt = infoSetList[i].pairCount;
                    min = i;
                }
               if(maxCnt < infoSetList[i].pairCount)
                {
                    maxCnt = infoSetList[i].pairCount;
                    max = i;
                }
            }
     
            //int MatchingSearch(int[,] datas, ListIdx[] pairData, int t = -1)

            ListIdx[] keyPairData = new ListIdx[ClientData.Instance.gameSetting.KeyKind]; // 키종류는 3가지 ~~
            ListIdx[] numPairData = new ListIdx[ClientData.Instance.gameSetting.NumKind];
            ListIdx[] octPairData = new ListIdx[ClientData.Instance.gameSetting.OctKind];

            for (int i = 0; i < rawDatas.GetLength(0); i++)
            {
                for (int j = 0; j < rawDatas.GetLength(1); j++)
                {
                    if (types[i, j] == BlockType.Key)
                    {
                        if (keyPairData[infoSetList[0].kinds[i, j]] == null)
                        {
                            keyPairData[infoSetList[0].kinds[i, j]] = new ListIdx();
                        }
                        keyPairData[infoSetList[0].kinds[i, j]].Add(new Vector2Int(i, j));
                    }
                    else if(types[i, j] == BlockType.Number)
                    {
                        if (numPairData[infoSetList[0].kinds[i, j]] == null)
                        {
                            numPairData[infoSetList[0].kinds[i, j]] = new ListIdx();
                        }
                        numPairData[infoSetList[0].kinds[i, j]].Add(new Vector2Int(i, j));
                    }
                    else if(types[i, j] == BlockType.Octopus)
                    {
                        if (octPairData[infoSetList[0].kinds[i, j]] == null)
                        {
                            octPairData[infoSetList[0].kinds[i, j]] = new ListIdx();
                        }
                        octPairData[infoSetList[0].kinds[i, j]].Add(new Vector2Int(i, j));
                    }
                }
            }



            foreach (var item in infoSetList)
            {
                item.pairCount += MatchingSearch(infoSetList[0].kinds, keyPairData); // 0고정 된 것이라서 항상 있는 0번 데이터를 이용한다
                item.pairCount += MatchingSearch(infoSetList[0].kinds, numPairData);
                item.pairCount += MatchingSearch(infoSetList[0].kinds, octPairData);
            }

            return new PageInfo(infoSetList,
                types,
                blinkerBlocks,
                unlockObjLists,
                numberBlocks,
                maxKindPair,
                min,max,seed,
                octopusBlocks);
        }

        public void Break(Vector2Int index)
        {
          //  Debug.Log("파괴 오브젝트 좌표  " + index);
            _kinds[index.x, index.y] = -1;
            _types[index.x, index.y] = BlockType.None;
            DeleteBlinkerBlock(index);
            _isClickEnable = true;
        }

        public void DeleteBlinkerBlock(Vector2Int index)
        {
            if (_types[index.x, index.y] == BlockType.Blinker)
            {
                int n = 0;
                for (int i = 0; i < pageInfoList[_currentPage].blinkerBlocks.Idxs.Count; i++)
                {
                    if (pageInfoList[_currentPage].blinkerBlocks.Idxs[i].x == index.x && pageInfoList[_currentPage ].blinkerBlocks.Idxs[i].y == index.y)
                    {
                        n = i;
                        pageInfoList[_currentPage ].blinkerBlocks.Idxs.RemoveAt(n);
                        break;
                    }
                }
            }
        }

        public void BreakPair(Vector2Int idx) // Vector2Int firstIndex, Vector2Int seccondIndex
        {
            Break(idx);
            Break(preIdx);

            preIdx = new Vector2Int(-1, -1);

            BlockCountCheck(false);
        }

        public Vector3 GetPosition(Vector2Int index)
        {
            return cellPositions[index.x, index.y];
        }

        //public bool CheckSelectBlock(Vector2Int input) 
        //{
        //    if (_isClickEnable == false) return false;
        //    if (Analyze.Type() (GetType(input) == BlockType.Normal ||
        //             GetType(input) == BlockType.Question ||
        //             GetType(input) == BlockType.Blinker ||
        //             GetType(input) == BlockType.KeyBlock)
        //             && GetKind(input) >= 0)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private bool SelectBlock(Vector2Int idx)
        //{
        //    if(GetType(idx) == BlockType.Normal ||
        //        GetType(idx) == BlockType.Question ||
        //        GetType(idx) == BlockType.Blinker )
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        public int GetKind(Vector2Int idx)
        {
            return kinds[idx.x, idx.y];
        }

        public BlockType GetType(Vector2Int idx)
        {
            return types[idx.x, idx.y];
        }

        private bool CheckPair(Vector2Int idx, Vector2Int idx2) // 체크 필ㅣ
        {
            if(GetKind(idx) == GetKind(idx2))
            {
                //if (GetType(idx) == BlockType.Key && GetType(idx2) == BlockType.Key)
                //{
                //    return true;
                //}
              
                if (ClientData.Instance.blockPropertyData.GetBlockProperty(GetType(idx)).isDynamic &&
                      ClientData.Instance.blockPropertyData.GetBlockProperty(GetType(idx2)).isDynamic)
                {
                    return true;
                }
                else if(ClientData.Instance.blockPropertyData.GetBlockProperty(GetType(idx)).isDynamic == false &&
                    GetType(idx) == GetType(idx2))
                {
                    return true;
                }
                else
                {
                   return false;
                }
            }
            return false;
        }

        public ClickState BlockSelect(Vector2Int input)
        {
           // if (CheckSelectBlock(input) == false) return ClickState.None; none 터치도 안된다 
            if (_clickCount == 0 || _clickCount == -1)
            {
                releaseIndex = input;

                OneClick(input);

            //    NLog.Log("최초 선택 OneClick");
                return ClickState.OneClick;
            }
            else
            {
                _isClickEnable = false;
                releaseIndex = _clickIdx;

                if (_clickIdx == input)
                {
                    OneClick(input);

                //    NLog.Log("같은 곳 눌린 상황  OneClick ");
                    return ClickState.OneClick;
                }
              //  _clickPairIndex.last = input;
                //  Debug.Log("다른 곳을 눌러서 아래것 실행 ");
              
                if (CheckPair(input, _clickIdx))
                {
                    PathFindLogic.Result result = _pathFindLogic.BFS(kinds, input, _clickIdx);

                    if (result.turnCount <= 2)
                    {
                        SelectRelase();

                        _linePos = new List<Vector3>();
                        foreach (var item in result.paths)
                        {
                            _linePos.Add(GetPosition(item)); // 라인 그리는 정보 전달 
                        }
                //        NLog.Log("유효 한 클릭 ");

                        pathResult = result;
                        return ClickState.TwoClick;
                    }
                    else
                    {
                    //    NLog.Log("꺽임이 많음  OtherOneClick" + result.turnCount);

                        OneClick(input);

                        return ClickState.OtherOneClick;
                    }

                }
                else
                {
                //     NLog.Log("속성이 다른 오브젝트라 무시무시  OtherOneClick ");
                    OneClick(input);
                    return ClickState.OtherOneClick;
                }

            }
            //    return ClickState.Disenable;
        }

        private void OneClick(Vector2Int input)
        {
            _clickIdx = input;
            _clickCount = 1;
            _isClickEnable = true;
        }

        public void SelectRelase()
        {
            _clickCount = 0;
            preClickIndex = new Vector2Int(-1, -1);
        }

        public void BlockCountCheck(bool isInit) // 남은 블럭 갯수 확인 마작판 끝났는지 확인 // 힌트도 계산중 
        {
            // 힌트 계산 까지 하는 중 
            pairDatas = new ListIdx[pageInfoList[_currentPage].maxKindPair]; // 문제 위치 

            keyPairData = new ListIdx[ClientData.Instance.gameSetting.KeyKind];

            numPairData = new ListIdx[ClientData.Instance.gameSetting.NumKind];

            octPairData = new ListIdx[ClientData.Instance.gameSetting.OctKind];

            pairDatasList = new List<ListIdx[]>()
            {
                pairDatas,
                keyPairData,
                numPairData,
                octPairData
            };

           int blockCount = 0;

            for (int i = 0; i < _widCnt; i++) // 데이터에서 계산 하는 것이기 때문에 구분 해야 한다 
            {
                for (int j = 0; j < _higCnt; j++)
                {
                    // _types[i, j] null 로 바꿔 주기 때문에 이것은 유효하다 
                    if (ClientData.Instance.blockPropertyData.GetBlockCreateProperty(_types[i, j]).enableTouch)  // 숫자 블록들은 다 포함시켜서 계산 해야 한다 
                    {
                        blockCount ++;

                        if(ClientData.Instance.blockPropertyData.GetBlockProperty(_types[i, j]).isDynamic)
                        {
                            if (pairDatas[_kinds[i, j]] == null) pairDatas[_kinds[i, j]] = new ListIdx();

                            pairDatas[_kinds[i, j]].Add(new Vector2Int(i, j));
                        }
                        else if(_types[i, j] == BlockType.Key)
                        {
                            if (keyPairData[_kinds[i, j]] == null) keyPairData[_kinds[i, j]] = new ListIdx();

                            keyPairData[_kinds[i, j]].Add(new Vector2Int(i, j));
                        }
                        else if (_types[i,j] == BlockType.Number)
                        {
                            if (numberBlockEnableCount == kinds[i, j])
                            {
                                if (numPairData[_kinds[i, j]] == null) numPairData[_kinds[i, j]] = new ListIdx();
                                numPairData[_kinds[i, j]].Add(new Vector2Int(i, j));
                            }
                        }
                        else if(_types[i, j] == BlockType.Octopus)
                        {
                            if (octPairData[_kinds[i, j]] == null) octPairData[_kinds[i, j]] = new ListIdx();
                            octPairData[_kinds[i, j]].Add(new Vector2Int(i, j));
                        }

                
                    }
                }
            }

            if (isInit)
            {
                mgr.runGameData.SetBlockMaxCount(blockCount);
                mgr.runGameData.SetBlockCount(blockCount);
            }

            if (mgr.runGameData.blockCount % 2 != 0) NLog.LogError("맞출수 있는 패가 이상합니다. 버그 수정 해야 합니다 . ");

            mgr.runGameData.SetMatchingCount(MatchingSearch());

            _process = (  (float)(mgr.runGameData.blockMaxCount -
                mgr.runGameData.blockCount) / (float)mgr.runGameData.blockMaxCount) + _currentPage ;
                            //  100 -50 /100     50/100
                            //  100 -40  / 100   40 /100 
           // NLog.Log("맞출수 있는 블록 카운터 " + _blockCount);

            if(mgr.runGameData.matchingCount == 0 && mgr.runGameData.blockCount >0)
            {
                if(autoShuffleCnt>0)
                {
                  //  autoShuffleCnt = 0; // 해머 쪽에서 초기화 하고 있으니까 하지 말 것 
                    onHammer?.Invoke();
                }
                else
                {
                    autoShuffleCnt++;
                    onShuffle?.Invoke();
                    BlockCountCheck(false);
                }

            }
        }

        public int MatchingSearch()
        {
            _enablePairIndex = new List<PairIdx>();
            // return stagePageInfoList[_currentPage - 1].infoSetList[0].pairCount;
            int sum = 0;
            foreach (var item in pairDatasList)
            {
                sum += MatchingSearch(_kinds, item);
            }

            return sum;
        }

        private int MatchingSearch(int[,] datas, ListIdx[] pairData, int t = -1)
        {
          
            bool nonMathching = true;
            int count = 0;
            foreach (var item in pairData)
            {
                //∂  Debug.Log("pairInfo 머가 있나 ?" + count );
                if (item != null && item.Idxs != null && item.Idxs.Count > 0)
                {
                    nonMathching = false;
                }
                count++;
            }
            if (nonMathching) return 0;

            int matchingCount = 0;

            for (int i = 0; i < pairData.Length; i++)
            {
                List<int> skipIndexs = new List<int>();
                if (pairData[i] != null && pairData[i].Idxs != null && pairData[i].Idxs.Count > 0) // 삭제 될 수도 있기 때문에 
                {
                    bool[] isCount = new bool[pairData[i].Idxs.Count];

                    for (int n = 0; n < pairData[i].Idxs.Count - 1; n++)
                    {
                        if (Spare(n, skipIndexs))
                        {
                            for (int m = n + 1; m < pairData[i].Idxs.Count; m++)
                            {
                                if (Spare(m, skipIndexs))
                                {
                                    PathFindLogic.Result result =
                             _pathFindLogic.BFS(datas, // 방해 요소 넣고 
                             pairData[i].Idxs[n],  // 두쌍 위치 넣고  
                             pairData[i].Idxs[m]);

                                    if (result.turnCount <= 2) // 유효 하면 
                                    {
                                        _enablePairIndex.Add(new PairIdx(pairData[i].Idxs[n], pairData[i].Idxs[m]));
                                        //    matchingCount++; // 카운팅 계산을 어떻게 바꿔야 하는? 
                                        skipIndexs.Add(n); skipIndexs.Add(m);
                                        isCount[n] = true; isCount[m] = true;
                                        matchingCount++;
                                    }
                                }
                            }
                        }
                    }

                    //foreach (var item in isCount)
                    //{
                    //    if (item) matchingCount++;
                    //}
                }


            }
            if (t == 0)
            {
                //  Debug.Log("키블록" + matchingCount);

            }
            else if (t == 1)
            {
                // Debug.Log("일반 블록 " + matchingCount);

            }

            return matchingCount;
        }

        private bool Spare(int n, List<int> skipIndexs)
        {
            foreach (var item in skipIndexs)
            {
                if (n == item)
                {
                    return false;
                }
            }
            return true;
        }


        int hintNum = -1;
        public PairIdx Hint()
        {
            if (mgr.runGameData.matchingCount == 0) return null;

            GameRandom gameRandom = RunGameRandom.Get("GameBoardModel", "Hint");

            //  NLog.Log("카운트 " + _enablePairIndex.Count);
            hintNum++;
            if (_enablePairIndex.Count <= hintNum) hintNum = 0;

                //  return _enablePairIndex[gameRandom.Get(0, _enablePairIndex.Count)]; //이 숫자가 좀 문제가 ;;

                return _enablePairIndex[hintNum];
        }

        public void HammerFind(int blockNum, BlockType type, Vector2Int index)
        {
          
            for (int i = 0; i < _kinds.GetLength(0); i++)
            {
                for (int j = 0; j < _kinds.GetLength(1); j++)
                {
                    if ( i != index.x || j != index.y )
                    {
                        if (blockNum != kinds[i, j]) continue; 
                         
                        if(ClientData.Instance.blockPropertyData.GetBlockProperty(type).isDynamic )
                        {
                            if (ClientData.Instance.blockPropertyData.GetBlockProperty(_types[i, j]).isDynamic)
                            {
                                NLog.Log($"찾은 블럭 위치 정보  [{i},{j}] , {GetType(new Vector2Int(i, j))}");
                                hammerPairIdx = new PairIdx(index, new Vector2Int(i, j));

                                Break(index);
                                Break(new Vector2Int(i, j));
                                return;
                            }
                        }
                        else
                        {
                            if( type == types[i,j])
                            {
                                NLog.Log($"찾은 블럭 위치 정보  [{i},{j}] , {GetType(new Vector2Int(i, j))}");
                                hammerPairIdx = new PairIdx(index, new Vector2Int(i, j));

                                Break(index);
                                Break(new Vector2Int(i, j));
                                return;
                            }
                        }

                        //if (type == BlockType.Key)
                        //{
                        //    if (blockNum == kinds[i, j] && type == types[i, j])
                        //    {
                              
                        //        NLog.Log("찾은 블럭 위치 정보 여긴 키블록   [{0},{1}] , {2}", i, j, GetType(new Vector2Int(i, j)));
                        //        hammerPairIdx = new PairIdx(index, new Vector2Int(i, j));

                          
                        //        _types[index.x, index.y]= BlockType.None;
                        //        _kinds[index.x, index.y] = -1;
                        //        _types[i, j] = BlockType.None;
                        //        _kinds[i, j] = -1;
                        //        return;
                        //    }
                        //}
                        //else
                        //{
                        //    if (blockNum == _kinds[i, j] &&

                        //        (types[i, j] == BlockType.Normal ||
                        //        types[i, j] == BlockType.Question ||
                        //        types[i, j] == BlockType.Blinker)
                        //  )
                        //    {
                           
                        //        NLog.Log("찾은 블럭 위치 정보  [{0},{1}] , {2}", i, j, GetType(new Vector2Int(i, j)));
                        //        hammerPairIdx = new PairIdx(index, new Vector2Int(i, j));

                        //        _types[index.x, index.y] = BlockType.None;
                        //        _kinds[index.x, index.y] = -1;
                        //        _types[i, j] = BlockType.None;
                        //        _kinds[i, j] = -1;
                        //        return; 
                        //    }
                        //}


                    }
                }
            }
            NLog.Log("망치 효과 잘못 사용 ");
         
           // return null; // 들어 올수가 없는 곳 
        }

        public void Shuffle()
        {
            //List<int> listBlockNum = new List<int>();

            List<Vector2Int> target2Index = new List<Vector2Int>();

            for (int j = 0; j < _higCnt; j++)
            {
                for (int i = 0; i < _widCnt; i++)
                {
                    if (ClientData.Instance.blockPropertyData.GetBlockProperty(types[i, j]).isDynamic)
                    {
                        target2Index.Add(new Vector2Int(i, j));
                    }
                }
            }

            List<Vector2Int> nonceIndexs = new List<Vector2Int>(); // 2개만 가질 것이다

            List<Vector2Int> centerIndexs = new List<Vector2Int>();
           
             int selBlockCnt = 2;

            int seed = ClientData.Instance.UserData.BattleInfo.PlayerInfo == null ? -1 : curtSeed;
            GameRandom gameRandom = RunGameRandom.Get("GameBoardModel", "Shuffle", curtSeed);
            NLog.Log("Shuffle seed : " + curtSeed);

            List<int> selKinds = new List<int>();

            for (int j = 0; j < _higCnt; j++)
            {
                for (int i = 0; i < _widCnt; i++)
                {
                    if(ClientData.Instance.blockPropertyData.GetBlockProperty(types[i, j]).isDynamic)
                    {
                        selKinds.Add(kinds[i,j]);
                    }
                }
            }

            int selKind = selKinds[gameRandom.Get(0, selKinds.Count)];

            bool isRow = false;

            for (int j = 0; j < _higCnt; j++)
            {
                for (int i = 0; i < _widCnt; i++)
                {
                    if (ClientData.Instance.blockPropertyData.GetBlockProperty(types[i, j]).isDynamic)
                    {          
                        // GameRandom gameRandom = RunGameRandom.Get("GameBoardModel", "Shuffle", curtSeed);

                        int sel = gameRandom.Get(0, target2Index.Count); // 이것이 원본
                        int tempKind = _kinds[i, j];
                        _kinds[i, j] = _kinds[target2Index[sel].x, target2Index[sel].y];
                        _kinds[target2Index[sel].x, target2Index[sel].y] = tempKind;

                        target2Index.RemoveAt(sel);

                        // 절대적으로 맞추는 패 
                        if (isRow && centerIndexs.Count==1)
                        {
                            if(centerIndexs[0].y == j)
                            {
                                centerIndexs.Add(new Vector2Int(i, j));
                            }
                        }
                        else
                        {
                            if (centerIndexs.Count < 2) // 두번 째 줄 부터 
                            {
                                    centerIndexs.Add(new Vector2Int(i, j));
                            }
                        }
                    }
                   
                   
                }
                if (centerIndexs.Count == 1) isRow = true;
            }

    
            for (int j = 0; j < _higCnt; j++)
            {
                for (int i = 0; i < _widCnt; i++)
                {
                    if (selKind == _kinds[i, j] && nonceIndexs.Count< selBlockCnt)
                    {
                        nonceIndexs.Add(new Vector2Int(i, j)); 
                    }
                }
            }

            //if (nonceIndexs.Count == 2) // 보완이 필요 해서 주석 처리 
            //{
            //    for (int i = 0; i < 2; i++)
            //    {
            //        _kinds[centerIndexs[i].x, centerIndexs[i].y] = _kinds[nonceIndexs[i].x, nonceIndexs[i].y]; // 센터 값이랑 선택된 kinds 랑 교환 
            //        _kinds[nonceIndexs[i].x, nonceIndexs[i].y] = _kinds[centerIndexs[i].x, centerIndexs[i].y];
            //    }
            //}

            BlockCountCheck(false);
        }

        public void SetSelectStageNum(int n)
        {
            _selectStageNum = n;
        }


        public Vector2Int To2D(int n) => new Vector2Int(n % widCnt, n / widCnt);
   
        public int To1D(Vector2Int idx) => idx.x + widCnt * idx.y;
        public int To1D(int x, int y) => x + widCnt * y;

        //public int FlattenIdx(Vector2Int idx) => idx.x + widCnt * idx.y;
        //public int FlattenIdx(int x, int y) => x + widCnt * y;

        public List<int> AttackBlock(AttackType type)
        {
            NLog.Log($"AttackBlock type = {type.ToString()}");
            List<int> AtBlock = new List<int>();

            List<int> normalBlocks = new List<int>(); // 가능한 블록 

            int n = 0;
            for (int j = 0; j < _types.GetLength(1); j++)
            {
                for (int i = 0; i < _types.GetLength(0); i++)
                {
                    if (_types[i, j] == BlockType.Normal)
                    {
                        normalBlocks.Add(n);
                    }
                    n++;
                }
            }

            if (normalBlocks.Count == 0) return null; // Null 리턴
      
            GameRandom gameRandom = RunGameRandom.Get("GameBoardModel", "AttackBlock");

            for (int i = 0; i < ClientData.Instance.gameSetting.AttackTagetCount; i++)
            {
                int idx = gameRandom.Get(0, normalBlocks.Count);

                AtBlock.Add(normalBlocks[idx]);

                AttackBlockTypeChange(type, idx);
                //if (type == AttackType.Question)
                //{
                //    _types[To2D(normalBlocks[idx]).x, To2D(normalBlocks[idx]).y] = BlockType.Question;
                //}
                //else if (type == AttackType.Blink)
                //{
                //    _types[To2D(normalBlocks[idx]).x, To2D(normalBlocks[idx]).y] = BlockType.Blinker;
                //}

                normalBlocks.RemoveAt(idx);

                if (normalBlocks.Count == 0) break;
            }

            return AtBlock;
        }

        public void AttackBlockTypeChange(AttackType type, int n )
        {
            if (_types[To2D(n).x, To2D(n).y] == BlockType.None) return;
            
            if (type == AttackType.Question)
            {
                _types[To2D(n).x, To2D(n).y] = BlockType.Question;
            }
            else if (type == AttackType.Blink)
            {
                _types[To2D(n).x, To2D(n).y] = BlockType.Blinker;
            }
        }

    }

}