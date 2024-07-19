using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine.Unity;
using System.Threading.Tasks;

namespace NGle.Solitaire.RunGame
{
    public abstract class RemoveStrategy
    {
        public SkeletonAnimation ani;
        public virtual void Remove()
        {
            Spine.AnimationState state = ani.AnimationState;

           string aniName = state.Tracks.Items[0].Animation.Name;

           ani.AnimationState.SetAnimation(0, aniName, false);
        }
    }

    public class NormalRemove : RemoveStrategy { }

    public class ComboRemove : RemoveStrategy { }

    public class KeyRemove : RemoveStrategy { }

    public class LockRemove : RemoveStrategy { }


    [Serializable]
    public class RemoveAni
    {
        public RemoveType type;
        public SkeletonAnimation ani;
    }

    public abstract class BlockEffect : MonoBehaviour
    {
        [SerializeField] protected Transform _body;
        [SerializeField] protected Transform _defEffect;

        public RemoveStrategy removeStrategy = null;

        [SerializeField] protected List<RemoveAni> removeAnis;
        Dictionary<RemoveType, SkeletonAnimation> dicRemoveAni;

        protected List<Attachment> attachments;

        private void Start()
        {
            Initialized();
        }

        public virtual void Initialized()
        {
            _body.gameObject.SetActive(false);
            _defEffect.gameObject.SetActive(true);

            for (int i = 0; i < _defEffect.childCount; i++)
            {
                _defEffect.GetChild(i).gameObject.SetActive(false);
            }

            dicRemoveAni = new Dictionary<RemoveType, SkeletonAnimation>();
            foreach (var item in removeAnis)
            {
                dicRemoveAni.Add(item.type, item.ani);
            }
        }

        public void setRemoveStrategy(RemoveStrategy removeStrategy)
        {
            this.removeStrategy = removeStrategy;
        }
        public RemoveStrategy getRemoveStrategy()
        {
            return removeStrategy;
        }

        public virtual async  void Remove()
        {
            attachments?.ForEach((a) => a?.Remove());

            if (gameObject.activeSelf == false) return;

            await Task.Yield();

            if (dicRemoveAni == null)
            {
                dicRemoveAni = new Dictionary<RemoveType, SkeletonAnimation>();
                foreach (var item in removeAnis) dicRemoveAni.Add(item.type, item.ani);
            }
           
            if (removeStrategy != null)
            {
                if (removeStrategy is NormalRemove)
                {
                    NormalRemove normalRemove = (NormalRemove)removeStrategy;

                    if (dicRemoveAni== null) return;
                    normalRemove.ani = dicRemoveAni[RemoveType.Normal];
                 
                    normalRemove.ani.gameObject.SetActive(true);
                    normalRemove.Remove();
                }
                else if (removeStrategy is ComboRemove)
                {
                    ComboRemove comboRemove = (ComboRemove)removeStrategy;

                    if (dicRemoveAni == null) return;
                    comboRemove.ani = dicRemoveAni[RemoveType.Combo];
                    comboRemove.ani.gameObject.SetActive(true);
                    comboRemove.Remove();
                }
                else if (removeStrategy is KeyRemove)
                {
                    KeyRemove keyRemove = (KeyRemove)removeStrategy;

                    if (dicRemoveAni == null) return;
                    keyRemove.ani = dicRemoveAni[RemoveType.Key];
                    keyRemove.ani.gameObject.SetActive(true);
                    keyRemove.Remove();
                }
                else if (removeStrategy is LockRemove)
                {
                    LockRemove lockRemove = (LockRemove)removeStrategy;

                    if (dicRemoveAni == null) return;
                    lockRemove.ani = dicRemoveAni[RemoveType.Lock];
                    lockRemove.ani.gameObject.SetActive(true);
                    lockRemove.Remove();
                }
            }
            else
            {

                if (dicRemoveAni[RemoveType.Normal] == null) return;
                SkeletonAnimation ani = dicRemoveAni[RemoveType.Normal];
                dicRemoveAni[RemoveType.Normal].gameObject.SetActive(true);
               
                Spine.AnimationState state = ani.AnimationState;

                string aniName = state.Tracks.Items[0].Animation.Name;

                ani.AnimationState.SetAnimation(0, aniName, false);
            }
        }

        //public virtual void Remove()
        //{
        //   // RemoveEffect(combo>1 ? RemoveType.Combo :RemoveType.Normal  );
        //}

        //public void RemoveEffect(RemoveType rType)
        //{
        //    foreach (var item in removeAnis)
        //    {
        //        if (item.type == rType)
        //        {
        //            item.ani.gameObject.SetActive(true);

        //            Spine.AnimationState state = item.ani.AnimationState;
             
        //            string aniName = state.Tracks.Items[0].Animation.Name;
 
        //            item.ani.AnimationState.SetAnimation(0, aniName, false);
        //            break;
        //        }
        //    }
        //}

    }

}
