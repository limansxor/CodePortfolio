using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class BattleRunGameData : RunGameData
    {
        public BattleRunGameData(int stageId) : base(stageId) { }
   
        public float attackAniDuration { get; private set; }// = 0.0f ;
        public void SetAttackAniDuration(float val) => attackAniDuration = val;

        public int otherBlockCount { get; private set; }
        public void SetOtherBlockCount(int val) => otherBlockCount = val;
    }
}
