﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libplanet.Action;
using Libplanet.Assets;
using Nekoyume.Game;
using Nekoyume.Game.Controller;
using Nekoyume.Game.VFX;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.State;
using Nekoyume.UI.Tween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class WorldBossRewardPopup : PopupWidget
    {
        private const float ContinueTime = 3f;

        [Serializable]
        private class Items
        {
            public GameObject Object;
            public Image Icon;
            public TextMeshProUGUI Count;
        }

        [SerializeField]
        private GraphicAlphaTweener graphicAlphaTweener;

        [SerializeField]
        private TextMeshProUGUI continueText;

        [SerializeField]
        private TextMeshProUGUI crystalCountText;

        [SerializeField]
        private List<Items> runes;

        private RaiderState _cachedRaiderState;
        private PraiseVFX _praiseVFX;
        private Coroutine _coCloseCoroutine;
        private int _cachedBossId;
        private int timer;

        protected override void Awake()
        {
            base.Awake();

            CloseWidget = () =>
            {
                Close(true);
                Game.Event.OnRoomEnter.Invoke(true);
            };
        }

        public void CachingInformation(RaiderState raiderState, int bossId)
        {
            _cachedRaiderState = raiderState;
            _cachedBossId = bossId;
        }

        public void Show(IRandom random)
        {
            base.Show();
            var rewards = GetRewards(random);
            UpdateRewardItems(rewards);
            Find<WorldBossDetail>().UpdateReward();
            graphicAlphaTweener.Play();
            PlayEffects();
            _coCloseCoroutine = StartCoroutine(CoClose());
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            StopEffects();
            if (_coCloseCoroutine != null)
            {
                StopCoroutine(_coCloseCoroutine);
            }

            base.Close(ignoreCloseAnimation);
        }

        private IEnumerator CoClose()
        {
            timer = 3;
            while (timer >= 0)
            {
                continueText.text = L10nManager.Localize("UI_PRESS_TO_CONTINUE_FORMAT", timer);
                yield return new WaitForSeconds(1f);
                timer--;
            }

            Close();
        }

        private List<FungibleAssetValue> GetRewards(IRandom random)
        {
            var runeWeightSheet = Game.Game.instance.TableSheets.RuneWeightSheet;
            var rewardSheet = Game.Game.instance.TableSheets.WorldBossRankRewardSheet;
            var rank = WorldBossHelper.CalculateRank(_cachedRaiderState.HighScore);
            var totalRewards = new List<FungibleAssetValue>();
            for (var i = _cachedRaiderState.LatestRewardRank; i < rank; i++)
            {
                var rewards = RuneHelper.CalculateReward(
                    i + 1,
                    _cachedBossId,
                    runeWeightSheet,
                    rewardSheet,
                    random
                );
                totalRewards.AddRange(rewards);
            }

            return totalRewards;
        }

        private void UpdateRewardItems(IReadOnlyList<FungibleAssetValue> rewards)
        {
            var crystalReward = rewards.FirstOrDefault(x => x.Currency.Ticker == "CRYSTAL");
            var crystal = Convert.ToInt32(crystalReward.GetQuantityString());
            crystalCountText.text = $"{crystal:#,0}";

            foreach (var rune in runes)
            {
                rune.Object.SetActive(false);
            }

            var runeRewards = rewards.Where(x => x.Currency.Ticker != "CRYSTAL").ToList();
            for (var i = 0; i < runeRewards.Count; i++)
            {
                runes[i].Object.SetActive(true);
                var currency = runeRewards[i].Currency;
                var count = Convert.ToInt32(runeRewards[i].GetQuantityString());
                runes[i].Count.text = $"{count:#,0}";
                if (WorldBossFrontHelper.TryGetRuneIcon(currency, out var icon))
                {
                    runes[i].Icon.sprite = icon;
                }
            }
        }

        private void PlayEffects()
        {
            AudioController.instance.PlaySfx(AudioController.SfxCode.RewardItem);

            if (_praiseVFX)
            {
                _praiseVFX.Stop();
            }

            var position = ActionCamera.instance.transform.position;
            _praiseVFX = VFXController.instance.CreateAndChaseCam<PraiseVFX>(position);
            _praiseVFX.Play();
        }

        private void StopEffects()
        {
            AudioController.instance.StopSfx(AudioController.SfxCode.RewardItem);

            if (_praiseVFX)
            {
                _praiseVFX.Stop();
                _praiseVFX = null;
            }
        }
    }
}
