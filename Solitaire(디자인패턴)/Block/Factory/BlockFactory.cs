using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.U2D;

using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;
using UnityEngine.Rendering;

namespace NGle.Solitaire.RunGame
{
    public interface IBlockFactory
    {
        Block CreateBlock(BlockType type, int kind, Vector3 pos, Transform targetCell, Transform vfxTarget, int sub = 0);
        BlockView CreateView(BlockType type, int kind, int sub, Transform target);
        BlockEffect CreateVFX(BlockType type, int kind, Transform vfxTarget, Vector3 pos);
    }
    public class BlockFactory : MonoBehaviour , IBlockFactory
    {
        private BlockSpriteMapper blockSpriteMapper;

        protected Transform effectParent;

        public void Initialized(RunGameMgr runGameMgr)
        {
            blockSpriteMapper = runGameMgr.blockSpriteMapper;
        }

        public Block CreateBlock( BlockType type, int kind, Vector3 pos, Transform targetCell, Transform vfxTarget, int sub = 0) // ,int sub, Vector2Int idx, Vector3 pos
        {
            effectParent = vfxTarget;

            Block newBlock;
            BlockView blockView = CreateView(type, kind, sub, targetCell);
            BlockEffect blockVFX = CreateVFX(type, kind, vfxTarget, new Vector3(pos.x, pos.y, 0));

            switch (type)
            {
                case BlockType.Normal:
                    newBlock = new NormalBlock(blockView, blockVFX);
                    break;
                case BlockType.Stone:
                    newBlock = new StoneBlock(blockView, blockVFX);
                    break;
                case BlockType.Question:
                    newBlock = new QuestionBlock(blockView, blockVFX);
                    break;
                case BlockType.Blinker:
                    newBlock = new BlinkerBlock(blockView, blockVFX);
                    break;
                case BlockType.Key:
                    newBlock = new KeyBlock(blockView, blockVFX);
                    break;
                case BlockType.Lock:
                    newBlock = new LockBlock(blockView, blockVFX);
                    break;
                case BlockType.Number:
                    newBlock = new NumberBlock(blockView, blockVFX);
                    break;
                case BlockType.Wiremesh:
                    newBlock = new WiremeshBlock(blockView, blockVFX);
                    break;
                case BlockType.Octopus:
                    newBlock = new OctopusBlock(blockView, blockVFX);
                    break;
                default:
                    newBlock = null;
                    break;
            }

            return newBlock;
        }


        private int logViewCrateCount;

        public BlockView CreateView(BlockType type, int kind, int sub, Transform target) 
        {
            if (kind >= blockSpriteMapper.mixedSprites.Count)
            {
                NLog.Log($"{logViewCrateCount} 번 째 생성 오류 ");
                return null;
            }
           logViewCrateCount++;

           BlockView view = ClientData.Instance.blockPropertyData.CreateBlockViewPrefab(type, target);

            switch (type)
            {
                case BlockType.Normal:
                    (view as NormalBlockView).SetRes(blockSpriteMapper.mixedSprites, kind);
                    break;
                case BlockType.Question:
                    (view as QuestionBlockView).SetRes(blockSpriteMapper.mixedSprites, kind);
                    break;
                case BlockType.Blinker:
                    (view as BlinkerBlockView).SetRes(blockSpriteMapper.mixedSprites, kind);
                    break;
                case BlockType.Stone:     
                    break;
                case BlockType.Key:        
                    view.SetSprite(ClientData.Instance.runGameDataFileLink.GetKeySpriteAsset(kind));
                    break;
                case BlockType.Lock:  
                    view.SetSprite(ClientData.Instance.runGameDataFileLink.GetLockSpriteAsset(kind));
                    break;
                case BlockType.Number:
                    (view as NumberBlockView).RegSprite(ClientData.Instance.runGameDataFileLink.GetNumSpriteAssets(kind));
                    break;
                case BlockType.Wiremesh:
                    break;
                case BlockType.Octopus:
                    (view as OctopusBlockView).SetIcon(kind);
                    break;
                default:
                    view = null;
                    break;
            }
     
            // ==== 일단 다 추가 

            return view;
        }

        public BlockEffect CreateVFX(BlockType type, int kind, Transform vfxTarget, Vector3 pos)
        {

            BlockEffect vfx = ClientData.Instance.blockPropertyData.CreateBlockEffectPrefab(type, vfxTarget, pos);
            //if(vfx != null)
            //vfx.GetComponent<SortingGroup>().sortingOrder = ClientData.Instance.gameSetting.BlockMaxCount;
            switch (type)
            {
 
                case BlockType.Normal:
                case BlockType.Question:         
                case BlockType.Blinker: 
                    break;
                case BlockType.Stone:       
                    break;
                case BlockType.Key:   
                    vfx.setRemoveStrategy(new KeyRemove());
                    break;
                case BlockType.Lock:
          
                    vfx.setRemoveStrategy(new LockRemove());
                    break;
                case BlockType.Number:
                    break;
                case BlockType.Wiremesh:
                    break;
                case BlockType.Octopus:
                
                    break;

                default:
                    vfx = null;
                    break;
            }

            return vfx;
        }


        public Block ChangeType(AttackType attackType,  GameCell cell, Transform vfxTarget)
        {
            if(attackType == AttackType.Question)
            {
                return CreateBlock(BlockType.Question,
                        cell.Property.kind,
                        cell.transform.localPosition,
                        cell.transform,
                        vfxTarget
                        );

                //cell.ChangeBlock(block);

               // (block as QuestionBlock).FlipEnd();
            }
            else //if(attackType == AttackType.Blink)
            {
                return CreateBlock(BlockType.Blinker,
                          cell.Property.kind,
                          cell.transform.localPosition,
                          cell.transform,
                          vfxTarget
                          );

                //cell.ChangeBlock(block);

               // (block as BlinkerBlock).blinker.Initialized();
            }
        }

    }
}
