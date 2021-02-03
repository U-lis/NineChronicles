﻿using UnityEngine;

namespace Nekoyume.UI
{
    public class GuideBackgroundData : ITutorialData
    {
        public TutorialItemType Type { get; } = TutorialItemType.Background;
        public bool IsExistFadeIn { get; }
        public bool IsEnableMask { get; }
        public RectTransform Target { get; }

        public GuideBackgroundData(bool isExistFadeIn, bool isEnableMask, RectTransform target)
        {
            IsExistFadeIn = isExistFadeIn;
            IsEnableMask = isEnableMask;
            Target = target;
        }
    }
}
