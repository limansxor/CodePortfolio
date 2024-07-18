using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class BattleGameBoardModel : GameBoardModel
    {
        private int[] _sendKinds;
        public int[] sendKinds { get { return _sendKinds; } }

        protected override void CurrentPageValue(ListIdx[] pairDatas, int currentPairCnt, int[,] kinds, BlockType[,] types, ListIdx blinkerBlocks, ListIdx[] unlockObjInfos, Dictionary<int, List<Vector2Int>> numberBlocks, int widCnt, int higCnt, float scaleX, float scaleY, int seed, List<ListIdx> octopusBlocks)
        {
            base.CurrentPageValue(pairDatas, currentPairCnt, kinds, types, blinkerBlocks, unlockObjInfos, numberBlocks, widCnt, higCnt, scaleX, scaleY, seed, octopusBlocks);

            _sendKinds = new int[widCnt * higCnt];

            int n = 0;
            for (int j = 0; j < higCnt; j++)
            {
                for (int i = 0; i < widCnt; i++)
                {
                    _sendKinds[n] = _kinds[i, j];

                    n++;
                }
            }
        }
    }
}
