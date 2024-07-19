using System;
using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Support;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace NGle.Solitaire.RunGame
{
    // ============== 공용으로 쓸 ================
    public enum DefAniType { RemoveBorder, RemoveComboBorder, Selected, SelectedCombo }
    public enum RemoveType { Normal, Combo, Key, Lock }

 

   // public enum BlockColor { black, blue, brown, darkbrown, green, orange, pink, purple, red, white, yellow }

    //public class Analyze
    //{
    //    public static readonly BlockType[] DynamicTypes = { BlockType.Normal, BlockType.Question, BlockType.Blinker };

    //    public static readonly BlockType[] ObstacleTypes = { BlockType.Stone, BlockType.LockBlock, BlockType.None };

    //    public static readonly BlockType[] TouchTypes = { BlockType.Normal, BlockType.Question, BlockType.Blinker, BlockType.KeyBlock };

    //    public static bool DynamicType(BlockType blockType)
    //    {
    //        foreach (var item in DynamicTypes)
    //        {
    //            if (item == blockType)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public static bool ObstacleType(BlockType blockType)
    //    {
    //        foreach (var item in ObstacleTypes)
    //        {
    //            if (item == blockType)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public static bool TouchType(BlockType blockType)
    //    {
    //        foreach (var item in TouchTypes)
    //        {
    //            if (item == blockType)
    //            {
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}

    public enum RecvRemoveType
    {
        Normal, Hammer 
    }
    public enum BattleGameType
    {
        Single, Multi
    }
    public enum CharacterType
    {
        Nao, Paul, Boss
    }
    public enum ProjectileType {
        Neo_Attack, Boss_Attack, Guage
    }

    public enum FixedEffectType
    {
        Neo_Lock, Neo_Attack, Boss_Lock, Boss_Attack, Gain
    }

    public enum AttackType
    {
        Question =3, Blink 
    }

    public enum AttackPrefabType
    {
        Attack, ComboPowerAttack
    }

    public class DirectionState
    {
        public DirectionState(bool isLeft, bool isRight, bool isUp, bool isDown)
        {
            this.isLeft = isLeft;
            this.isRight = isRight;
            this.isUp = isUp;
            this.isDown = isDown;
        }
        public bool isLeft;
        public bool isRight;
        public bool isUp;
        public bool isDown;

    }
    public enum Direction { LEFT, RIGHT, TOP, BOTTOM, CENTER }

    public struct BattlePlayerInfo
    {
        public string name;
        public int profile;
        public bool[] wins;
        public int rank;
        public int gp;
    }
    public struct GpData
    {
        public int preRank;
        public int grade;
        public int preGp;
        public int addGp;
        public int preCrown;
        public int crown;
        public int crownConversionPoint;
        public int gp;
        public List<int> minGpTable;
    }
    public struct StageResultData
    {
        public int stageId;
        public int preStar;
        public int curtStar;
        public int allStar;
        public int conversionPoint;
        public int score;
        public int itemScore;
        //public int totalScore;
        public int rewardGold;
        public int evtRewardGold;
        public bool isMission;
    }


    [Serializable]
    public class TAndH
    {
        public Text score;
        public Image hyphen;

    }
    public class RunGameRandom
    {

        static public GameRandom Get(string className, string fnName, int seed = -1)
        {
            GameRandom r = new GameRandom();
            switch (className + "|" + fnName)
            {
                case "AbsBlockFactory|RegisterSpriteGroup":
                    r.Init((int)DateTime.Now.Ticks);
                    break;
                case "GameBoardModel|LayoutConverter":
                case "GameBoardModel|Shuffle":
                    if (seed == -1) r.Init(UnityEngine.Random.Range(0, int.MaxValue));
                    else r.Init(seed);
                    break;
                //case "GameBoardModel|PlaceBlock":
                //    r.Init(UnityEngine.Random.Range(0, int.MaxValue));
                //    break;
                case "GameBoardModel|AttackBlock":
                    r.Init(UnityEngine.Random.Range(0, int.MaxValue));
                    break;
                case "GameBoardModel|Hint":
                    r.Init(UnityEngine.Random.Range(0, int.MaxValue));
                    break;

                default:
                    r.Init(51);
                    break;
            }


            return r;
        }
    }

    public static class SkeletonCommon
    {
        public static float Duration(SkeletonGraphic ani, string aniName)
        {
            AnimationStateData stateData = ani.SkeletonDataAsset.GetAnimationStateData();
            Spine.Animation animation = stateData.SkeletonData.FindAnimation(aniName);

            return animation.Duration;
        }

        public static float Duration(SkeletonAnimation ani, string aniName)
        {
            AnimationStateData stateData = ani.SkeletonDataAsset.GetAnimationStateData();
            Spine.Animation animation = stateData.SkeletonData.FindAnimation(aniName);

            return animation.Duration;
        }


        public static float Run(SkeletonGraphic ani) // 애니메이션 이름 잘 못 찾는 것 같다 쓰지 말자 
        {
            Spine.AnimationState state = ani.AnimationState;
            string name = state.Tracks.Items[0].Animation.Name;
            state.SetAnimation(0, name, false);

            AnimationStateData stateData = ani.SkeletonDataAsset.GetAnimationStateData();
            Spine.Animation animation = stateData.SkeletonData.FindAnimation(name);

            return animation.Duration;
        }


    }
}
