using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class BattleRunGameEvent : RunGameEvent
    {
        public Action onGameOut { get; private set; }
        public void AddEvtGameOut(Action evt) => onGameOut = evt;

        public Action onBattleGameTimeOver { get; private set; }
        public void RegBattleGameTimeOver(Action evt) => onBattleGameTimeOver = evt;

        public Func<AttackType, bool, List<BlockPlace>> onFindAttackBlock; // 배틀 보스에서 불러 온다

        public Action<AttackType, bool, List<BlockPlace>> onAttack { get; private set; }
        public void AddEvtAttack(Action<AttackType, bool, List<BlockPlace>> action) => onAttack += action;
    }
}
