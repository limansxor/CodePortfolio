using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NGle.Solitaire.Support;

namespace NGle.Solitaire.RunGame
{
    public class RunGameEvent
    {
        // =========================
        public void RunGameDataCreateEvtInit()
        {
            onRunGameDataCreate = null;

            onRunGameDataCreate = () => { NLog.Log("새로운 RunData 파일 생성 "); };
        }
        public Action onRunGameDataCreate { get; set; }

        public void AddEventRunGameDataCreate(Action evt) => onRunGameDataCreate += evt;

        // ===============
        public Action onComboInit { get; private set; }
        public void AddEvtUpdateCombo(Action action) => onComboInit += action;

        public Action<List<BlockPlace>> onRemove { get; private set; }
        public void AddEvtRemove(Action<List<BlockPlace>> action) => onRemove += action;

    }
}
