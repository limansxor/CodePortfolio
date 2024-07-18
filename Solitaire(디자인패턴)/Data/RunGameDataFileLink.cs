using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Asset;
using NGle.Solitaire.Data;
using NGle.Solitaire.RunGame.Support;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;

namespace NGle.Solitaire.RunGame
{
    [System.Serializable]
    public class ProjectileDataAsset
    {
        public ProjectileType type;
        public SkeletonGraphicDataAsset dataAsset;
    }

    [System.Serializable]
    public class FixedEffectDataAsset
    {
        public FixedEffectType type;
        public SkeletonGraphicDataAsset dataAsset;
    }

    [System.Serializable]
    public class AttackPrefab
    {
        public AttackPrefabType attackType;
        public GameObjectAsset attackAsset;
    }



    [CreateAssetMenu(fileName = "RunGameDataFlieLink", menuName = "NGLE/ScriptableData/RunGameDataFlieLink", order = 3)]
    public class RunGameDataFileLink : ScriptableObject
    {
        [SerializeField] GameObjectAsset gameCellAsset;
        public GameCell CreateGameCell(Transform target, Vector3 pos)
        {
            GameCell gameCell = gameCellAsset.Instantiate(target).GetComponent<GameCell>();
            gameCell.transform.localPosition = pos;
            return gameCell;
        }

        [SerializeField] GameObjectAsset playerGameCellAsset;
        public PlayerGameCell CreatePlayerGameCell(Transform target, Vector3 pos,int idx)
        {
            PlayerGameCell gameCell = playerGameCellAsset.Instantiate(target).GetComponent<PlayerGameCell>();
            gameCell.transform.localPosition = new Vector3(pos.x,pos.y, 0.1f);
           // gameCell.GetComponent<SortingGroup>().sortingOrder = idx;
            return gameCell;
        }

        [SerializeField] AssetReference lineRenderer;
        public LineRenderer GetLineRenderer()
        {
            return AssetManager.Instance.LoadAsset(lineRenderer).GetComponent<LineRenderer>(); // lineRenderer.lo //().GetComponent<LineRenderer>();
        }

        [SerializeField] MaterialAsset[] lineMaterialAssets;
        public Material GetMaterialAsset(int n)
        {
            return lineMaterialAssets[n].Load();
        }

        [SerializeField] SpriteAsset[] keySpriteAssets;
        public Sprite GetKeySpriteAsset(int n)
        {
            return keySpriteAssets[n].Load();
        }

        [SerializeField] SpriteAsset[] lockSpriteAssets;
        public Sprite GetLockSpriteAsset(int n)
        {
            return lockSpriteAssets[n].Load();
        }

        [SerializeField] SpriteAsset[] numSpriteAssets;
        public List<Sprite> GetNumSpriteAssets(int n)
        {
            List<Sprite> sprites = new List<Sprite>()
            {
                numSpriteAssets[n*2].Load(),
                numSpriteAssets[n*2+1].Load(),
            };

            return sprites; // 반대로 설정 on , off 순서로 가려고 
        }

        [SerializeField] GameObjectAsset[] octopusSpines;
        public SkeletonAnimation GetOctopusAsset(int n ,Transform target)
        {
            return octopusSpines[n].Instantiate(target).GetComponent<SkeletonAnimation>();
        }

        [SerializeField] GameObjectAsset attackEffect;
        public Attack GetAttack(Transform parent)
        {
            return attackEffect.Instantiate(parent).GetComponent<Attack>();
        }
        [SerializeField] GameObjectAsset comboPowerAttackEffect;

         public ComboPowerAttack GetComboAttack(Transform parent)
        {
            return comboPowerAttackEffect.Instantiate(parent).GetComponent<ComboPowerAttack>();
        }


        [SerializeField] List<ProjectileDataAsset> projectileDataAssets;
         public SkeletonAnimation GetProjectile(ProjectileType projectileType )
        {
            SkeletonGraphicDataAsset dataAsset = projectileDataAssets.Find(l => l.type == projectileType).dataAsset;

          

            return dataAsset.NewSkeletonAnimationGameObject();
        }

        public string GetProjectileAniName(ProjectileType projectileType)
        {
            SkeletonGraphicDataAsset dataAsset = projectileDataAssets.Find(l => l.type == projectileType).dataAsset;

            SkeletonData skeletonData = dataAsset.Load().GetSkeletonData(true);

            foreach (var item in skeletonData.Animations)
            {
                return item.Name;
            }
            return "";
        }

        [SerializeField] List<FixedEffectDataAsset> fixedEffectDataAssets;

        public SkeletonAnimation GetFixedEffect(FixedEffectType fixedEffectType)
        {
            SkeletonGraphicDataAsset dataAsset = fixedEffectDataAssets.Find(l => l.type == fixedEffectType).dataAsset;

            return dataAsset.NewSkeletonAnimationGameObject();
        }
         
        public string GetFixedEffectAniName(FixedEffectType fixedEffectType)
        {
            SkeletonGraphicDataAsset dataAsset = fixedEffectDataAssets.Find(l => l.type == fixedEffectType).dataAsset;

            SkeletonData skeletonData = dataAsset.Load().GetSkeletonData(true);

            foreach (var item in skeletonData.Animations)
            {
                return item.Name;
            }
            return "";
        }

        [SerializeField] GameObjectAsset gameBoardPrerfab;
        public GameBoard GetGameBoardPrefab() => gameBoardPrerfab.Instantiate().GetComponent<GameBoard>();


        [SerializeField] GameObjectAsset comboAudioPrefab;
        public ComboAudio GetComboAudioPrefab() => comboAudioPrefab.Instantiate().GetComponent<ComboAudio>();

        [SerializeField] GameObjectAsset stageGameUIPrefab;
        public GameUI GetStageGameUIPrefab() => stageGameUIPrefab.Instantiate().GetComponent<GameUI>();

        [SerializeField] GameObjectAsset itemBoardPrefab;
        public ItemBoard GetItemBoardPrafab() => itemBoardPrefab.Instantiate().GetComponent<ItemBoard>();

        [SerializeField] List<AttackPrefab> attackPrefabs;
        public Attack GetAttackPrefabs(Transform parent,AttackPrefabType type)
        {
            return attackPrefabs.Find(a => a.attackType == type).attackAsset.Instantiate(parent).GetComponent<Attack>();  
        }
    }
}
