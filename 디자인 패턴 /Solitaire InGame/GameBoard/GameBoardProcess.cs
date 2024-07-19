using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Support;
using System;
using UnityEngine.Events;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    public class GameBoardProcess : MonoBehaviour
    {
        RunGameMgr mgr;
        public void Initialized(RunGameMgr mgr)
        {
            this.mgr = mgr;
        }
        public IEnumerator StartFlipCoroutine(GameBoardModel model, List<Block> blocks, Action onFlipEnd)
        {
        
            float add = mgr.runGameData.blockCount * Time.deltaTime;
            float c = add;

            if (add < 1) add = 1; // 1보다 작으면 너무 빠르다 

            //float flipAniTime = 0;

            foreach (var item in blocks)
            {
                if (item == null) continue;
                
                item.StartPlay();

                if (c < 0.0f) //0.6 -0.5 0.1
                {
                    c = add;
                    yield return null;
                }
                else
                {
                    c -= 0.3f;
                }

            }

            yield return new WaitForSeconds(ClientData.Instance.gameSetting.flipAniTime);

            onFlipEnd.Invoke();
        }

        public IEnumerator DrawLineCoroutine(
            LineRenderer linePrefab, Transform target, int zBuffer, Material[] lineMats,
            Action<Vector2Int, Vector2Int, bool> LinkBorder,
            Action<Vector2Int, Vector2Int,int, int, BlockType, INGameState> Remove,
            Action ColliderExpand,
            List<Vector3> positions, List<Vector2Int> paths,
       Vector2Int first, Vector2Int last,int kind, BlockType type,
        int combo , INGameState state)
        {
            //NLog.Log("라인 그리기 시작 ");

            LineRenderer line = Instantiate(linePrefab, target);

            if (combo>1)
            {
                line.material = lineMats[1];
            }
            else
            {
                line.material = lineMats[0];
            }

            //line.material.renderQueue = zBuffer;

            Color color = new Color(1, 1, 1, 1);
            line.materials[0].color = color;

            line.positionCount = paths.Count;

            for (int i = 0; i < paths.Count; i++) line.SetPosition(i, positions[0]);

            float oneStep = 0;

            float myParentPosX = target.position.x;
            float myParentPosY = target.position.y;


            //if (paths.Count > 1)
            for (int i = 0; i < paths.Count; i++) // paths.Count - 1
            {

                float addX = -0.118f+0.088f-0.04f; //+ 0.125f-117;
                float addY = 0.16f-0.09f-0.02f;
                float posX = positions[i].x + myParentPosX;
                float posY = positions[i].y + myParentPosY;
                if (i == 0)
                {
                    posX = Mathf.Lerp(positions[0].x, positions[1].x, 0.4f) + myParentPosX;
                    posY = Mathf.Lerp(positions[0].y, positions[1].y, 0.45f) + myParentPosY;
                }
                else if (i == paths.Count - 1)
                {
                    posX = Mathf.Lerp(positions[paths.Count - 2].x, positions[paths.Count - 1].x, 0.6f) + myParentPosX;
                    posY = Mathf.Lerp(positions[paths.Count - 2].y, positions[paths.Count - 1].y, 0.55f) + myParentPosY;
                }
                for (int j = i; j < paths.Count; j++)
                {
                    line.SetPosition(j, new Vector3(posX + addX, posY + addY,  0)); // 뒷값을 다 통일 시킨다 // z 축 조정 
                }

                if (oneStep >= 1)
                {
                    // 6번만 실행 
                    yield return null;
                    oneStep = 0;
                }
                else
                {
                    oneStep += 6 / paths.Count; // 여기 값을 작게 만들면 
                }
            }
            // _lineRenderer.SetPosition(paths.Count - 1, positions[paths.Count - 1]);

            yield return null;

            LinkBorder(first, last, combo>1);

            Remove(first, last, combo, kind, type, state);

            ColliderExpand();

            //yield return new WaitForSeconds(0.1f);
            float t = 0.16f;
           

            while (t>0)
            {
                t -= Time.deltaTime;
                color.r = Mathf.Lerp(1, 0.1f, t);
                color.g = Mathf.Lerp(1, 0.1f, t);
                color.b = Mathf.Lerp(1, 0.1f, t);
                //color.a = Mathf.Lerp(1, 0, t);
                line.materials[0].color = color;
                yield return null;
            }
        

            Destroy(line.gameObject);
        }

        public IEnumerator UnlockAni(GameBoardModel model, Action<Vector2Int,int> onRemove ,int kind, int combo) // 콤보 영향 안받지만 혹시 모르니까 
        {
            NLog.Log("UnlockAni 동작 시작 합니다." + kind + " " + model.widCnt);

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < model.unlockObjInfos[kind].Idxs.Count; i++)
            {
            //    NLog.Log("unLockObjInfos " + unLockObjInfos[i]);
                onRemove.Invoke(model.unlockObjInfos[kind].Idxs[i], combo);

                
                yield return new WaitForSeconds(Time.deltaTime*2);
            }
            yield break;
        }

        public IEnumerator HammerCoroutine(Block[] blocks, Vector2Int f , Vector2Int l, int combo ,int kind, BlockType t, INGameState state
            , Action<Vector2Int , Vector2Int , int , int , BlockType , INGameState> onRemove ) // GameBoardModel model,
        {

            float duration = 0; 
            foreach (var item in blocks)
            {
                (item as TouchBlock).HitHammer();

                duration = (item as TouchBlock).HammerDuration();
            }

            
            yield return new WaitForSeconds(duration);
            onRemove.Invoke(f,l, combo, kind,t, state) ;

              yield break;
        }

    }
}
