using Nekoyume.UI.Scroller;
using UnityEngine;

namespace Nekoyume.UI
{
    public class InventoryRenew : Widget
    {
        [SerializeField] public InventoryScrollerController scrollerController;

        #region Mono

        private void Awake()
        {
            if (ReferenceEquals(scrollerController, null))
            {
                throw new SerializeFieldNullException();
            }
        }
        
        #endregion
        
        public void SetData(Model.Inventory data)
        {
            scrollerController.SetData(data.Items);
        }
    }
}
