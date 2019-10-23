using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nekoyume.EnumType;
using Nekoyume.Game.Character;
using Nekoyume.Game.Controller;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nekoyume.UI.Module
{
    public class ItemView<TViewModel> : MonoBehaviour
        where TViewModel : Model.Item
    {
        protected static readonly Color DefaultColor = Color.white;
        protected static readonly Color DimColor = new Color(1f, 1f, 1f, 0.3f);

        public TouchHandler touchHandler;
        public Button itemButton;
        public Image backgroundImage;
        public Image iconImage;
        public Image gradeImage;
        public Image selectionImage;

        private readonly List<IDisposable> _disposablesAtSetData = new List<IDisposable>();

        public RectTransform RectTransform { get; private set; }

        public Vector3 CenterOffsetAsPosition
        {
            get
            {
                var pivotPosition = RectTransform.GetPivotPositionFromAnchor(PivotPresetType.MiddleCenter);
                var position = new Vector3(pivotPosition.x, pivotPosition.y);
                return RectTransform.localToWorldMatrix * position;
            }
        }

        [CanBeNull] public TViewModel Model { get; private set; }
        public bool IsEmpty => Model?.ItemBase.Value is null;

        public readonly Subject<ItemView<TViewModel>> OnClick = new Subject<ItemView<TViewModel>>();
        public readonly Subject<ItemView<TViewModel>> OnRightClick = new Subject<ItemView<TViewModel>>();

        #region Mono

        protected virtual void Awake()
        {
            RectTransform = GetComponent<RectTransform>();

            touchHandler.OnRightClick.Subscribe(_ =>
            {
                OnRightClick.OnNext(this);
                Model?.OnRightClick.OnNext(Model);
            }).AddTo(gameObject);

            itemButton.OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (Model is null)
                        return;

                    AudioController.PlayClick();
                    OnClick.OnNext(this);
                    Model.OnClick.OnNext(Model);
                })
                .AddTo(gameObject);
        }

        protected virtual void OnDestroy()
        {
            Model?.Dispose();
            OnClick.Dispose();
            OnRightClick.Dispose();
            Clear();
        }

        #endregion

        public virtual void SetData(TViewModel model)
        {
            if (model is null)
            {
                Clear();
                return;
            }

            _disposablesAtSetData.DisposeAllAndClear();
            Model = model;
            Model.GradeEnabled.SubscribeTo(gradeImage).AddTo(_disposablesAtSetData);
            Model.Selected.SubscribeTo(selectionImage).AddTo(_disposablesAtSetData);

            UpdateView();
        }

        public virtual void Clear()
        {
            Model = null;
            _disposablesAtSetData.DisposeAllAndClear();

            UpdateView();
        }

        protected virtual void SetDim(bool isDim)
        {
            iconImage.color = isDim ? DimColor : DefaultColor;
            gradeImage.color = isDim ? DimColor : DefaultColor;
            selectionImage.color = isDim ? DimColor : DefaultColor;
        }

        private void UpdateView()
        {
            if (Model is null ||
                Model.ItemBase.Value is null)
            {
                iconImage.enabled = false;
                gradeImage.enabled = false;
                selectionImage.enabled = false;
                return;
            }

            var item = Model.ItemBase.Value;

            var itemSprite = item.GetIconSprite();
            if (itemSprite is null)
            {
                throw new FailedToLoadResourceException<Sprite>(item.Data.Id.ToString());
            }

            iconImage.enabled = true;
            iconImage.overrideSprite = itemSprite;
            iconImage.SetNativeSize();

            var gradeSprite = item.GetBackgroundSprite();
            if (gradeSprite is null)
                throw new FailedToLoadResourceException<Sprite>(item.Data.Grade.ToString());

            gradeImage.overrideSprite = gradeSprite;
        }
    }
}
