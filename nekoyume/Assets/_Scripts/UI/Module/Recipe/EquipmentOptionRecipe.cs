using Nekoyume.TableData;
using Nekoyume.UI.Scroller;
using System;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    public class EquipmentOptionRecipe : MonoBehaviour
    {
        [SerializeField]
        private EquipmentRecipeCellView equipmentRecipeCellView = null;

        [SerializeField]
        private EquipmentOptionRecipeView[] equipmentOptionRecipeViews = null;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Show(int recipe)
        {
            Show();
        }

        public void SetData(EquipmentRecipeCellView view, Action<EquipmentRecipeCellView, int> onSelectOption)
        {
            equipmentRecipeCellView.Set(view.model);

            for (int i = 0; i < equipmentOptionRecipeViews.Length; ++i)
            {
                if (i >= view.model.SubRecipeIds.Count)
                {
                    equipmentOptionRecipeViews[i].Hide();
                    continue;
                }

                var subRecipeId = view.model.SubRecipeIds[i];

                var equipmentSheet = Game.Game.instance.TableSheets.EquipmentItemSheet;
                if (!equipmentSheet.TryGetValue(view.model.ResultEquipmentId, out var row))
                {
                    Hide();
                    return;
                }

                equipmentOptionRecipeViews[i].Show(
                    row.GetLocalizedName(),
                    new EquipmentItemSubRecipeSheet.MaterialInfo(view.model.MaterialId, view.model.MaterialCount),
                    subRecipeId,
                    () => onSelectOption(view, subRecipeId));
            }

            Show();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
