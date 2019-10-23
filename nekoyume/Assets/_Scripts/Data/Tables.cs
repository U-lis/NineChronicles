using Nekoyume.Data.Table;
using Nekoyume.Pattern;
using UnityEngine;

namespace Nekoyume.Data
{
    public class Tables : MonoSingleton<Tables>
    {
        public Table<StageDialog> StageDialogs { get; private set; }

        public void Initialize()
        {
            StageDialogs = new Table<StageDialog>();
            Load(StageDialogs, "DataTable/stage_dialog");
        }

        private void Load(ITable table, string filename)
        {
            var file = Resources.Load<TextAsset>(filename);
            if (file != null)
            {
                table.Load(file.text);
            }
        }
    }
}
