using System.Collections;
using System.Collections.Generic;
using NGle.Solitaire.Data;
using NGle.Solitaire.Support;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using System;
using BattlePacket;
using NGle.Solitaire.Network;
using NGle.Solitaire.Asset;
using DG.Tweening;
using NGle.Solitaire.Sound;

namespace NGle.Solitaire.RunGame
{
    public class ComboPowerAttack : Attack
    {

        List<int> comboUnitPowers;
        int maxComboPower;
        int crtComboPower = 0;

        [SerializeField] Transform guageGainTarget;
        [SerializeField] Image guage;

        [SerializeField] Transform cbg_gauge;
        [SerializeField] SkeletonGraphic combopower_text;
        [SerializeField] SkeletonGraphic eff_gauge_flame;
        [SerializeField] SkeletonGraphic eff_gauge_side_light;

        // Combo Power
        [SerializeField] Transform cbg_gaugeAttack;
        [SerializeField] Image guage_blue;

        public override void Initialized()
        {
            base.Initialized();

            characterType = CharacterType.Nao;

            crtComboPower = 0;
            guage.fillAmount = 0;
            Vector2 pos = eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition;
            pos.x = Mathf.Lerp(flameMin, flameMax, 0);
            eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition = pos;

            eff_gauge_side_light.gameObject.SetActive(false);
            eff_gauge_flame.gameObject.SetActive(false);
            cbg_gauge.gameObject.SetActive(true);
            cbg_gaugeAttack.gameObject.SetActive(false);

            var comboPow = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_COMBOPOWER).GetTableData<combopowerData>(1);
            if (comboPow == null) return;
                maxComboPower = comboPow.combo_pow_max_point;
            //else
            //    NLog.LogWarning("<BattleGameUI>failed to get ComboPow Table Data");

            comboUnitPowers = new List<int>();

            var comboPowPoints = ClientData.Instance.TableData.GetLocalTable(TableData.Type.TABLE_COMBOPOWERPOINT).AllData<combopowerpointData>();
            if (comboPowPoints != null)
                comboPowPoints.ForEach((item) => comboUnitPowers.Add(item.point));
            else
                NLog.LogWarning("<BattleGameUI>failed to get ComboPowPoint Table Data");
        }
     
        AttackType attackType = AttackType.Question;
        public void OnRemove(List<BlockPlace> blockPlaces)
        {
            int combo = mgr.runGameData.comboCount;
            int addComboPower = combo == 0 ? 0 : comboUnitPowers[combo - 1];

            crtComboPower += addComboPower;

            ComboPowerGain(crtComboPower / (float)maxComboPower, blockPlaces);

        }
      
        private Coroutine comboPowerGainCoroutine = null;

        private void SetGaugeBar(float g)
        {
            this.guage.fillAmount = g;

            eff_gauge_flame.gameObject.SetActive( g!=1 ||g != 0);
            Vector2 pos = eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition;
            pos.x = Mathf.Lerp(flameMin, flameMax, g);
            eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition = pos;
        }

        private void ComboPowerGain(float guage, List<BlockPlace> blockPlaces)
        {
            this.guage.fillAmount = preGuage; // 이전 게이지로 초기화

            // 프레임 위치 조정 
            eff_gauge_flame.gameObject.SetActive(preGuage != 0);
            Vector2 pos = eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition;
            pos.x = Mathf.Lerp(flameMin, flameMax, preGuage);
            eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition = pos;

            // 투사체 날리기
            StartCoroutine(GainProjectileCoroutine(blockPlaces));

            if (curtGuage <= 1)
            {
                if (comboPowerGainCoroutine != null)
                {
                    StopCoroutine(comboPowerGainCoroutine);
                    preGuage = curtGuage;
                }
                comboPowerGainCoroutine = StartCoroutine(ComboPowerGainCoroutine(guage, blockPlaces));
            }
        }

        private IEnumerator GainProjectileCoroutine(List<BlockPlace> blockPlaces)
        {
            string aniName;
            List<SkeletonAnimation> projectiles = new List<SkeletonAnimation>();
            foreach (var item in blockPlaces)
            {
                SkeletonAnimation projectile = ClientData.Instance.runGameDataFileLink.GetProjectile(ProjectileType.Guage);
                aniName = ClientData.Instance.runGameDataFileLink.GetProjectileAniName(ProjectileType.Guage);

                projectile.gameObject.transform.position = item.pos;
                projectile.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.gainProjectileOrderLayer;
                projectile.AnimationState.SetAnimation(0, aniName, true);

                Vector3 dir = guageGainTarget.position - item.pos;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 회전 적용
                projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
                // 이동
                projectile.transform.DOMove(guageGainTarget.position, ClientData.Instance.gameSetting.ComboPowerGainTime-0.1f).SetEase(Ease.InQuad);

                projectiles.Add(projectile);
            }
            
            yield return new WaitForSeconds(ClientData.Instance.gameSetting.ComboPowerGainTime); // 게이지 바 까지 닿는 시간 

            combopower_text.AnimationState.SetAnimation(0, charging, false); // 얻을 때마다 액션
            eff_gauge_side_light.gameObject.SetActive(true); // 사이드 반짝임 


            projectiles.ForEach(p => GameObject.Destroy(p.gameObject));

            SkeletonAnimation gainEffect = ClientData.Instance.runGameDataFileLink.GetFixedEffect(FixedEffectType.Gain);
            aniName = ClientData.Instance.runGameDataFileLink.GetFixedEffectAniName(FixedEffectType.Boss_Lock);
            gainEffect.gameObject.transform.position = guageGainTarget.position;
            gainEffect.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.gainEffectOrderLayer;
            gainEffect.AnimationState.SetAnimation(0, aniName, false);

            yield return new WaitForSeconds(SkeletonCommon.Duration(gainEffect, aniName));
            GameObject.Destroy(gainEffect.gameObject);

            combopower_text.AnimationState.SetAnimation(0, idle, true);
            eff_gauge_side_light.gameObject.SetActive(false);
        }

        float curtGuage = 0;
        float preGuage = 0;
        private IEnumerator ComboPowerGainCoroutine(float gainGauge, List<BlockPlace> blockPlaces)
        {
            curtGuage = gainGauge;
            yield return new WaitForSeconds(ClientData.Instance.gameSetting.ComboPowerGainTime ); // 게이지 바 까지 닿는 시간 

            combopower_text.AnimationState.SetAnimation(0, charging, false); // 얻을 때마다 액션
            eff_gauge_side_light.gameObject.SetActive(true); // 사이드 반짝임 

            float g = preGuage;//gainGauge;

            gainGauge = Mathf.Clamp(gainGauge, 0, 1);

            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_combogauge"));
            while (g + Time.deltaTime < gainGauge)
            {
                g += Time.deltaTime;
              
                SetGaugeBar(g);
                yield return null;
            }
            SetGaugeBar(gainGauge);

            // 다시 모든 게 초기화
            if (gainGauge == 1)
            {
                yield return new WaitForSeconds(0.1f);
                attackType = attackType == AttackType.Question ? AttackType.Blink : AttackType.Question;
                List<BlockPlace> AttackBlockPlaces = (mgr.runGameEvent as BattleRunGameEvent).onFindAttackBlock.Invoke(attackType, true);
                (mgr.runGameEvent as BattleRunGameEvent).onAttack.Invoke(attackType, true, AttackBlockPlaces);

                crtComboPower -= maxComboPower;

                preGuage = crtComboPower / (float)maxComboPower;
                curtGuage = preGuage;

              
                SetGaugeBar(curtGuage);
            }
            else
            {
                preGuage = gainGauge; // 비로소 여기서 
            }
        }


        private float flameMin = -178f;
        private float flameMax = 184.6f;


        private void OnAttack(AttackType attackType, bool isMy, List<BlockPlace> blockPlaces)
        {
            // base.OnAttack(attackType, isMy, blockPlaces);
            if (isMy)
            {
                this.guage.fillAmount = preGuage;

                eff_gauge_flame.gameObject.SetActive(preGuage != 0);
                Vector2 pos = eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition;
                pos.x = Mathf.Lerp(flameMin, flameMax, preGuage);
                eff_gauge_flame.GetComponent<RectTransform>().anchoredPosition = pos;

                dicRunAnis[characterType].gameObject.SetActive(true);
                dicRunAnis[characterType].AnimationState.SetAnimation(0, attack, false);

                //NLog.Log(" 공격하는 캐릭터 :  " + characterType.ToString());
                duration = SkeletonCommon.Duration(dicRunAnis[characterType], attack);

                if (blockPlaces != null)
                {
                    blockPlaces.ForEach(b => StartCoroutine(AttackProjectileCoroutine(startPoint.position, b.pos)));
                }

                StartCoroutine(ComboPowerGuageCoroutine(duration*1.6f));

                SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_comboattack_1"));
            }
        }

        private IEnumerator AttackProjectileCoroutine(Vector3 startPos, Vector3 endPos)
        {
            endPos += ClientData.Instance.gameSetting.AttackProjectileAddPos;

            SkeletonAnimation Neo_Lock = ClientData.Instance.runGameDataFileLink.GetFixedEffect(FixedEffectType.Neo_Lock);
            string aniName = ClientData.Instance.runGameDataFileLink.GetFixedEffectAniName(FixedEffectType.Neo_Lock);

            Neo_Lock.gameObject.transform.position = endPos;
            Neo_Lock.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.lockOrderLayer;
            Neo_Lock.AnimationState.SetAnimation(0, aniName, true);


            // 투사체 날리기 
            SkeletonAnimation projectile = ClientData.Instance.runGameDataFileLink.GetProjectile(ProjectileType.Neo_Attack);
            aniName = ClientData.Instance.runGameDataFileLink.GetProjectileAniName(ProjectileType.Neo_Attack);

            projectile.gameObject.transform.position = startPos;
            projectile.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.projectileOrderLayer;
            projectile.AnimationState.SetAnimation(0, aniName, true);

            float t = ClientData.Instance.gameSetting.AttackProjectileDuration;

            Vector3 dir = startPos - endPos ;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 회전 적용
            projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // 이동
            projectile.transform.DOMove(endPos, ClientData.Instance.gameSetting.AttackProjectileDuration).SetEase(Ease.InQuad);

            while (t > 0)
            {
                t -= Time.deltaTime;
             //   projectile.transform.Translate(Vector3.right * Time.deltaTime * -dir.magnitude * 1/duration); // 예시로 ()f의 속도로 이동
                yield return null;
            }
            SoundManager.Instance.PlaySfx(ClientData.Instance.AssetDataFileLink.GetAudioClipData("se_comboattack_bomb"));
            GameObject.Destroy(Neo_Lock.gameObject);
            GameObject.Destroy(projectile.gameObject);

            SkeletonAnimation Neo_Attack = ClientData.Instance.runGameDataFileLink.GetFixedEffect(FixedEffectType.Neo_Attack);
            aniName = ClientData.Instance.runGameDataFileLink.GetFixedEffectAniName(FixedEffectType.Neo_Attack);
            Neo_Attack.gameObject.transform.position = endPos;

            Neo_Attack.transform.GetComponent<MeshRenderer>().sortingOrder = ClientData.Instance.gameSetting.attackEffectOrderLayer;
            Neo_Attack.AnimationState.SetAnimation(0, aniName, false);

            if (shakePanelCoroutine != null) StopCoroutine(ShakePanelCoroutine());
            shakePanelCoroutine = StartCoroutine(ShakePanelCoroutine());

            yield return new WaitForSeconds(SkeletonCommon.Duration(Neo_Attack, aniName));
            GameObject.Destroy(Neo_Attack.gameObject);

            shakePanelCoroutine = null;
            yield break;
        }

        private IEnumerator ComboPowerGuageCoroutine( float duration)
        {
         
            //cbg_gauge.gameObject.SetActive(true);
            //cbg_gaugeAttack.gameObject.SetActive(false);

            //this.guage.fillAmount = 1;

            //eff_gauge_flame.gameObject.SetActive(false);
    

            //yield return new WaitForSeconds(0.1f);
            cbg_gauge.gameObject.SetActive(false);
            cbg_gaugeAttack.gameObject.SetActive(true);

            float guage = 1;

            //float rat = crtComboPower / (float)maxComboPower;

           // float frameTime = 0.05f;

            while (guage > 0)
            {
                guage -= Time.deltaTime * (duration-0.2f); // 1초 일때 

                this.guage_blue.fillAmount = guage ;

                yield return null;
            }

            guage_blue.fillAmount = 0;
            cbg_gauge.gameObject.SetActive(true);
            cbg_gaugeAttack.gameObject.SetActive(false);

            float g = 0;
            while (g+Time.deltaTime < preGuage)
            {
                g += Time.deltaTime;

                SetGaugeBar(g);
                yield return null;
            }
            SetGaugeBar(preGuage);

            //eff_gauge_side_light.gameObject.SetActive(false);
            //eff_gauge_flame.gameObject.SetActive(false);
            //combopower_text.AnimationState.SetAnimation(0, idle, true); // 콤보 게이지 노멀로 
        }

    }
}
