using System;
using System.Collections.Generic;
using UnityEngine;
using NGle.Solitaire.Support;
using NGle.Solitaire.Data;

namespace NGle.Solitaire.RunGame
{
    [System.Serializable]
    public class ConfigData
    {
        public int id;
        public int block_count_type;
        public string block_layout;
        public int time_limit;
        public int time_limit_3;
        public int time_limit_2;
        public int pair;

        public int free_shuffle;
        public int score_block;
        public int score_time;
        public int score_combo_10;
        public int quest_score;
        public int quest_goods_id;

        public string block_layout_2;
        public string block_layout_3;

        public int crown_gp;
        public int star_gp;

        public List<int> GradeMinPointTabel;
    }

    [System.Serializable]
    public class Config
    {
        public string id;
        public int heart_max;
        public int heart_time;
        public int start_gold;
        public int start_gem;
    }

    [System.Serializable]
    public class Goods
    {
        public int id;
        public int type;
        public int type_id;
        public int count;
        public string rate;
    }

    [System.Serializable]
    public class Item
    {
        public string id;
        public string type;
        public string value;
    }

    [System.Serializable]
    public class Product
    {
        public string id;
        public string goods_id;
        public string pay_type;
        public string pay_price;
        public string aos_inapp_id;
        public string ios_inapp_id;

    }
    [System.Serializable]
    public class Starreward
    {
        public int id;
        public int min_stage_id;
        public int max_stage_id;
        public string goods_id;
    }

    [System.Serializable]
    public class Table
    {
        public Config[] configs;
        public Goods[] goods;
        public Item[] items;
        public Product[] products;
        public ConfigData[] configData;
        public Starreward[] starrewards;
    }

    [Serializable]
    public class ListIdx
    {
        public ListIdx()
        {
            idxs = new List<Vector2Int>();

        }
        public void Add(Vector2Int idx)
        {
            if (idxs == null) idxs = new List<Vector2Int>();

            idxs.Add(idx);
        }

        //public void Add(Vector2Int idx1, Vector2Int idx2)
        //{

        //}

        public void RemoveAt(int n)
        {
            idxs.RemoveAt(n);
        }
        public void RemoveAt(int n1, int n2)
        {
            idxs.RemoveAt(n1);
            idxs.RemoveAt(n2);
        }

        public void Remove(Vector2Int idx)
        {
            idxs.Remove(idx);
        }

        public void Remove(Vector2Int idx1, Vector2Int idx2)
        {
            idxs.Remove(idx1);
            idxs.Remove(idx2);
        }

        private List<Vector2Int> idxs = null ;
        public List<Vector2Int> Idxs { get { return idxs; } }

    }

    public struct IdxAndVal
    {
        public Vector2Int idx;
        public int val;

        public IdxAndVal(Vector2Int idx, int val)
        {
            this.idx = idx;
            this.val = val;
        }
    }

    [Serializable]
    public class InfoSet
    {
        public InfoSet(int[,] kinds, ListIdx[] pairDatas, int count)
        {
            _kinds = kinds;
            _pairDatas = pairDatas;
            _pairCount = count;
        }
        private int _pairCount;
        public int pairCount { get { return _pairCount; } set { _pairCount = value; } }

        private int[,] _kinds;
        public int[,] kinds { get { return _kinds; } }

        private ListIdx[] _pairDatas;
        public ListIdx[] pairDatas { get { return _pairDatas; } }

    }


    [Serializable]
    public class PairIdx
    {
        public PairIdx(Vector2Int first, Vector2Int last)
        {
            _first = first;
            _last = last;
        }
        //public void SetPairIdx(Vector2Int first, Vector2Int last)
        //{
        //    _first = first;
        //    _last = last;
        //}
        [SerializeField] Vector2Int _first;
        public Vector2Int first { get { return _first; } set { _first = value; } }
        [SerializeField] Vector2Int _last;
        public Vector2Int last { get { return _last; } set { _last = value; } }

    }

    [Serializable]
    public class PageInfo
    {
        public PageInfo(List<InfoSet> infoSetList, BlockType[,] types, ListIdx blinkerBlocks, ListIdx[] unlockObjLists,
            Dictionary<int, List<Vector2Int>> numberBlocks,
            int maxKindPair,
            int minIdx, int maxIdx, int seed,
            List<ListIdx> octopusBlocks)
        {
            _infoSetList = infoSetList;
            _types = types;
            _blinkerBlocks = blinkerBlocks;
            _unlockObjLists = unlockObjLists;
            _maxKindPair = maxKindPair;

            this.minIdx = minIdx;
            this.maxIdx = maxIdx;

            this.seed = seed;

            this.numberBlocks = numberBlocks;

            this.octopusBlocks = octopusBlocks;

        }

        private List<InfoSet> _infoSetList;
        public List<InfoSet> infoSetList { get { return _infoSetList; } }

        private BlockType[,] _types;
        public BlockType[,] types { get { return _types; } }

        private ListIdx _blinkerBlocks;
        public ListIdx blinkerBlocks { get { return _blinkerBlocks; } }
        // public List<Vector2Int> RendomInfoBalocks;
        private ListIdx[] _unlockObjLists;
        public ListIdx[] unlockObjLists { get { return _unlockObjLists; } }

        private int _maxKindPair;
        public int maxKindPair { get { return _maxKindPair; } }

        private Dictionary<int, List<Vector2Int>> numberBlocks;
        public Dictionary<int, List<Vector2Int>> NumberBlocks { get { return numberBlocks; }  }

        private List<ListIdx> octopusBlocks; 
        public List<ListIdx> OctopusBlocks { get { return octopusBlocks; } }

        public static void NumberAddData(Dictionary<int, List<Vector2Int>> dictionary, int key, Vector2Int value)
        {
            // 키가 존재하지 않으면 새로운 리스트 생성
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new List<Vector2Int>();
            }

            // 리스트에 데이터 추가
            dictionary[key].Add(value);
        }

        public int minIdx;
        public int maxIdx;

        public int seed;
    }

    public class TouchData
    {
        public TouchData(Vector2Int index, bool isHammer, int kind)
        {
            // this.touchIdx = touchIdx;
            this.index = index;
            this.isHammer = isHammer;
            this.kind = kind;
        }
        // public int touchIdx;
        public Vector2Int index;
        public bool isHammer;
        public int kind;
    }

    public enum ClickState { None, Disenable, OneClick, OtherOneClick, TwoClick, Cancel }

    public enum INGameState { Continue,GameEnd ,NextPage  };
}
