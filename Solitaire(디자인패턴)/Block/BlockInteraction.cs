using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using UnityEngine;


namespace NGle.Solitaire.RunGame
{
    public class BlockInteraction : MonoBehaviour
    {
        public BlockType type;
        public int kind;
        public Vector2Int idx;

        private void Start()
        {
          //  GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
