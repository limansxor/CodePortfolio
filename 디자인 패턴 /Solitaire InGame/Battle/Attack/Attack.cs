using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using Spine.Unity;
using UnityEngine;
using System;
using System.Threading.Tasks;
using NGle.Solitaire.Asset;

using DG.Tweening;
using NGle.Solitaire.Support;
using NGle.Solitaire.Sound;

namespace NGle.Solitaire.RunGame
{

    public class Attack : MonoBehaviour
    {
        [Serializable]
        public class AniAndType
        {
            public CharacterType type;
            public SkeletonGraphic graphic;
        }
        protected const string idle = "01_idle";
        protected const string charging = "02_charging";
        protected const string full = "03_full";

        protected const string attack = "animation";

        protected CharacterType characterType;

        public List<AniAndType> aniAndTypes;
     
        protected Dictionary<CharacterType, SkeletonGraphic> dicRunAnis;

        [SerializeField] protected Transform startPoint;

        [SerializeField] protected List<Transform> shakePanels;

        protected float duration;

        protected Coroutine shakePanelCoroutine = null;

        protected List<Vector3> initPositions;

        protected BattleRunGameMgr mgr;

        public virtual void Initialized()// CharacterType
        {
            mgr = BattleRunGameMgr.Instance;

            initPositions = new List<Vector3>();

            shakePanels.ForEach(p => initPositions.Add(p.transform.position));


            this.characterType = CharacterType.Boss;// 간단한 형태에서는 공격 하는 것만

           dicRunAnis = new Dictionary<CharacterType, SkeletonGraphic>();

            aniAndTypes.ForEach(item =>
            {
                item.graphic.gameObject.SetActive(false);

                dicRunAnis.Add(item.type, item.graphic);
            });


            (mgr.runGameEvent as BattleRunGameEvent).AddEvtAttack(OnAttack); // 정상 코드에서 주석 걷어 내기
            (mgr.runGameData as BattleRunGameData).SetAttackAniDuration(SkeletonCommon.Duration(aniAndTypes[0].graphic, attack));

            // this.shakePanels = shakePanels;

        }


        private void OnAttack(AttackType attackType, bool isMy, List<BlockPlace> blockPlaces)
        { 
            if (isMy == false && blockPlaces !=null)
            {
                foreach (var item in blockPlaces)
                {
                    if (item.pos.x == 8787)
                    {
                        return;
                    }
                }

                // 블록 묶고 공격 모션 보이고 투 사체 날라간다 
                dicRunAnis[characterType].gameObject.SetActive(true);
                dicRunAnis[characterType].AnimationState.SetAnimation(0, attack, false);

          //      NLog.Log(" 공격하는 캐릭터 :  " + characterType.ToString());
                duration = SkeletonCommon.Duration(dicRunAnis[characterType], attack);

              
            

                if (blockPlaces != null)
                {
                    blockPlaces.ForEach(b => StartCoroutine(AttackProjectileCoroutine(startPoint.position, b.pos, b.idx)));
                }
                                                                                                      
                SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_comboattack_2"));
            }
        }

        private IEnumerator AttackProjectileCoroutine(Vector3 startPos, Vector3 endPos ,int idx)
        {
            endPos += ClientData.Instance.gameSetting.AttackProjectileAddPos;

            SkeletonAnimation bossLock = ClientData.Instance.runGameDataFileLink.GetFixedEffect(FixedEffectType.Boss_Lock);
            string aniName = ClientData.Instance.runGameDataFileLink.GetFixedEffectAniName(FixedEffectType.Boss_Lock);

            bossLock.gameObject.transform.position = endPos;
            bossLock.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.lockOrderLayer;
            bossLock.AnimationState.SetAnimation(0, aniName, true);
     

            // 투사체 날리기 
            SkeletonAnimation projectile = ClientData.Instance.runGameDataFileLink.GetProjectile(ProjectileType.Boss_Attack);
            aniName = ClientData.Instance.runGameDataFileLink.GetProjectileAniName(ProjectileType.Boss_Attack);

            projectile.gameObject.transform.position = startPos;
            projectile.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.projectileOrderLayer;
            projectile.AnimationState.SetAnimation(0, aniName, true);

            float t = ClientData.Instance.gameSetting.AttackProjectileDuration;

            Vector3 dir = endPos - startPos;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 회전 적용
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // 이동
            projectile.transform.DOMove(endPos, ClientData.Instance.gameSetting.AttackProjectileDuration).SetEase(Ease.InQuad);

            while (t > 0)
            {
                t -= Time.deltaTime;
              //  projectile.transform.Translate(Vector3.right * Time.deltaTime * dir.magnitude * 1 / duration); // 예시로 ()f의 속도로 이동
                yield return null;
            }
            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_comboattack_bomb"));
            GameObject.Destroy(bossLock.gameObject);
            GameObject.Destroy(projectile.gameObject);

            SkeletonAnimation bossAttack = ClientData.Instance.runGameDataFileLink.GetFixedEffect(FixedEffectType.Boss_Attack);
            aniName = ClientData.Instance.runGameDataFileLink.GetFixedEffectAniName(FixedEffectType.Boss_Lock);
            bossAttack.gameObject.transform.position = endPos;

            bossAttack.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.attackEffectOrderLayer;
            bossAttack.AnimationState.SetAnimation(0, aniName, false);

            if(shakePanelCoroutine != null) StopCoroutine(ShakePanelCoroutine());
            shakePanelCoroutine = StartCoroutine(ShakePanelCoroutine());

         

            yield return new WaitForSeconds((mgr.runGameData as BattleRunGameData).attackAniDuration);
            GameObject.Destroy(bossAttack.gameObject);

         

            shakePanelCoroutine = null;
            yield break;
        }

        //protected SkeletonAnimation DynamicRunAni(int order =5 )
        //{

        //}
        private Canvas FindCanvas(Transform trans)
        {
            Transform currentTransform = trans;

            // 부모 캔버스를 찾을 때까지 반복
            while (currentTransform != null)
            {
                // 현재 Transform이 Canvas 컴포넌트를 가지고 있는지 확인
                Canvas canvas = currentTransform.GetComponent<Canvas>();
                if (canvas != null)
                {
                    // 부모 캔버스를 찾았을 때의 로직
                 //   Debug.Log("부모 캔버스를 찾았습니다.");
                    return canvas;
                }

                // 부모 Transform으로 이동
                currentTransform = currentTransform.parent;
            }

            return null;
        }
        public IEnumerator ShakePanelCoroutine() // 흔들기
        {

            float correctionVal = 1;

            float t = 0.5f;

            float mt = t;
            int sign = -1;

            float frameTime = 0.05f;

            for (int i = 0; i < shakePanels.Count; i++)
            {
                if (shakePanels[i].GetComponent<RectTransform>() != null)
                {
                    Canvas canvas = FindCanvas(shakePanels[i]);
                    if (canvas != null) correctionVal = canvas.GetComponent<RectTransform>().rect.height / 720f;
                    break;
                }
            }

            while (t > 0)
            {
                t -= frameTime; //Time.deltaTime;//
                sign *= -1;

                for (int i = 0; i < shakePanels.Count; i++)
                {
                    Vector3 pos = initPositions[i];
                    pos.x += sign * 0.05f * t;
                    pos.y += sign * 0.1f * t;
                    if (shakePanels[i].GetComponent<RectTransform>() != null)
                    {
                        shakePanels[i].transform.position = pos * correctionVal;
                    }
                    else
                    {
                        shakePanels[i].transform.position = pos;
                    }
                }



                // shakePanels.ForEach(p => p.transform.position =  );
                yield return new WaitForSeconds(frameTime);
                // yield return null;
            }



            for (int i = 0; i < initPositions.Count; i++)
            {
                if (shakePanels[i].GetComponent<RectTransform>() != null)
                {
                    Canvas canvas = FindCanvas(shakePanels[i]);
                    if (canvas != null) correctionVal = canvas.GetComponent<RectTransform>().rect.height / 720f;

                    shakePanels[i].transform.position = initPositions[i] * correctionVal;
                }
                else
                {
                    shakePanels[i].transform.position = initPositions[i];
                }

            }
            yield break;
            
        }


    }
}
