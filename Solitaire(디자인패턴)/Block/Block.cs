using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System;
using NGle.Solitaire.Support;


namespace NGle.Solitaire.RunGame
{
    [Serializable]
    public class Block 
    {
       
        [SerializeField] private BlockView view;
        [SerializeField] private BlockEffect vfx;

        public Block(BlockView view , BlockEffect vfx)
        {
         
            this.view = view;
            this.vfx = vfx;
        }

        public Vector3 GetPos()
        {
            return view.transform.position;
        }

        public bool IsEnable()
        {
            return !view.isRemove; 
        }

        public void Destroy()
        {
            GameObject.Destroy(view.gameObject);
            GameObject.Destroy(vfx.gameObject);
        }
        public bool GetEnableActive()
        {
            return view != null && view.gameObject.activeSelf;
        }

        public void Initialized()
        {
            view.Initialized();
            vfx.Initialized();
        }

        //public abstract void Create(BlockView prefab, Vector3 pos, Transform target);
        //public abstract void CreateEffect(BlockEffect prefab, Vector3 pos, Transform target);
        public virtual void Remove(int combo = 0)
        {

            if (view == null || view.gameObject.activeSelf == false || view.isRemove) return;

            view.Remove();

            if (vfx.getRemoveStrategy() == null)
            {
                if (combo > 1) vfx.setRemoveStrategy(new ComboRemove());
                else vfx.setRemoveStrategy(new NormalRemove());
            }
            vfx.Remove();

        }


        public virtual void StartPlay() { }
        public virtual void Pause() { }
        public virtual void Continue() { } 

    }

    public interface ITouch
    {
        public void Select(bool isTop);
        public void Selected(bool isCombo);
        public void Release();
        public void Hint();
        public void ComboRun(int combo);
        public void ComboInit();
    }

    //public interface ITouchBlock  // 조금씩 다르기 때문에 
    //{
    //    public abstract void ComboRun(int combo);

    //    public abstract void ComboInit();

    //    public abstract void Select();

    //    public abstract void Selected(bool isCombo);

    //    public abstract void Release();

    //    public abstract void Hint();
    //}
}
