using Nekoyume.Game.Item;
using Nekoyume.UI.Module;
using UniRx;

namespace Nekoyume.UI.Model
{
    public class InventoryItem : CountableItem
    {
        public readonly ReactiveProperty<bool> Covered = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> Equipped = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<bool> Glowed = new ReactiveProperty<bool>(false);

        public InventoryItemView View;
        
        public InventoryItem(ItemBase item, int count) : base(item, count)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Covered.Dispose();
            Equipped.Dispose();
            Glowed.Dispose();
        }
    }
}
