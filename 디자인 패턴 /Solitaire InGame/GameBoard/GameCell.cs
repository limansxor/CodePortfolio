using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace NGle.Solitaire.RunGame
{
    public class GameCell : MonoBehaviour
    {
        [SerializeField] private Block block = null;

        protected BlockProperty property;

        public BlockProperty Property { get { return property; } }

        public virtual void Register(BlockFactory factory, BlockType type, int kind, int sub, Vector2Int idx,  Vector3 pos, Transform vfxTarget)
        {
            block = factory.CreateBlock(type, kind, pos, transform, vfxTarget, sub);
            //block?.Initialized();
            property = new BlockProperty(type, idx, kind, sub);         
        }

        public Block GetBlock()
        {
            return block;
        }


        public void Destroy()
        {
            block?.Destroy();

            for (int i = transform.childCount -1 ; i >=  0; i--)
            {
                GameObject.Destroy(transform.GetChild(i).gameObject);
            } 
        }
        public  void ChangeBlock( Block newBlock, BlockType type) 
        {
            //property.type = type;
            //block.Destroy();
            //block = null;
            //block = newBlock;
            if (gameObject.activeSelf)
            {
                StartCoroutine(ChangeBlockCoroutine( newBlock, type));
            }
        }

        public IEnumerator ChangeBlockCoroutine( Block newBlock, BlockType type)
        {
            property.type = type;
            block.Destroy();
            block = null;
            yield return null;
            block = newBlock;
            yield break;
        }

        public virtual void Pause(bool isGameEnd) 
        {
            if (block == null) return;

            StartCoroutine(PauseCoroutine(isGameEnd));
               
        }

        public IEnumerator PauseCoroutine(bool isGameEnd)
        {
            if (isGameEnd)
                yield return new WaitForSeconds(0.5f);
            block.Pause();
        }

        public virtual void  Continue()
        {
            if (block == null) return;
     
            //block.FlipForward();
            //await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.filpTime));
         
            block.Continue();

        }

        public virtual void Select(bool isTop)
        {
            // if (block == null) return; // 없는 것에 접근 할 이가 없다
            (block as TouchBlock).Select(isTop);
        }



        public virtual void Remove()
        {
            property.type = BlockType.None;
         
            block = null; // 끊기만 하고 효과는 다른 곳에서 
        }

        public void HitHammer()
        {
            if (block != null && block.GetEnableActive() == false) return;

            (block as TouchBlock)?.HitHammer();
            Remove();
        }


    }
}
