using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NGle.Solitaire.Data;
using UnityEngine;

namespace NGle.Solitaire.RunGame
{
    public class PlayerGameCell : GameCell
    {
        [SerializeField] InteractionAttachment interaction; // 기본적으로 가지고 시작 

        public override void Register(BlockFactory factory, BlockType type, int kind, int sub, Vector2Int idx, Vector3 pos, Transform vfxTarget)
        {
            base.Register(factory, type, kind, sub, idx, pos, vfxTarget);
            if (type == BlockType.None)
            {
                interaction.Remove();
            }
            else
            {
                if (ClientData.Instance.blockPropertyData.GetBlockCreateProperty(type).enableTouch == false) interaction.Remove();
                if (ClientData.Instance.blockPropertyData.GetBlockProperty(type).enableTouch == false) interaction.SetActive(false);

                if (interaction.gameObject.activeSelf)
                {
                    interaction.Initialized(type, kind, idx);
                }
            }

        }

        public Vector3 GetPos()
        {
            return transform.position;
        }

        public override void Select(bool isTop)
        {
            base.Select(isTop);

            interaction.SetActive(false);
        }

        public override void Pause(bool isGameEnd)
        {
            interaction.SetActive(false);
  
            base.Pause(isGameEnd);
        }

        public override async void Continue()
        {
            base.Continue();

            await Task.Delay((int)(1000 * ClientData.Instance.gameSetting.filpTime));
            if (property.enableTouch) interaction.SetActive(true);
        }

        public void PauseInteraction()
        {
            interaction.SetActive(false);
        }
        public void ContinueInteraction()
        {
            if(property.enableTouch)interaction.SetActive(true);
        }
        public void Interaction(bool enable)
        {
            property.enableTouch = enable;
         if(property.enableTouch)interaction.SetActive(enable);
            
        }

        public void InteractionEx(Direction dir)
        {
            if (property.enableTouch) interaction.Extension(dir);
        }

        public void InteractionEx(bool isLeftBlank, bool isRightBlank, bool isUpBlank, bool isDownBlank)
        {
            if (property.enableTouch) interaction.Extension(isLeftBlank, isRightBlank, isUpBlank, isDownBlank);
        }

        public override void Remove()
        {
            base.Remove();
            property.enableTouch = false;
            interaction.SetActive(false);
        }


    }
}
