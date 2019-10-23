using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Data;
using Nekoyume.Data.Table;

namespace Nekoyume.TableData
{
    [Serializable]
    public class StageSheet : Sheet<int, StageSheet.Row>
    {
        [Serializable]
        public class WaveData
        {
            public int Number { get; }
            public List<MonsterData> Monsters { get; }
            public bool IsBoss { get; }
            public int RewardId { get; }
            public long Exp { get; }

            public WaveData(int number, List<MonsterData> monsters, bool isBoss, int rewardId, long exp)
            {
                Number = number;
                Monsters = monsters;
                IsBoss = isBoss;
                RewardId = rewardId;
                Exp = exp;
            }
        }

        [Serializable]
        public class MonsterData
        {
            public int CharacterId { get; }
            public int Level { get; }
            public int Count { get; }

            public MonsterData(int characterId, int level, int count)
            {
                CharacterId = characterId;
                Level = level;
                Count = count;
            }
        }

        [Serializable]
        public class Row : SheetRow<int>
        {
            public override int Key => Id;
            public int Id { get; private set; }
            public List<WaveData> Waves { get; private set; }
            public bool HasBoss { get; private set; }
            public List<int> TotalMonsterIds { get; private set; }
            public List<int> TotalRewardIds { get; private set; }
            public long TotalExp { get; private set; }

            public override void Set(IReadOnlyList<string> fields)
            {
                Id = int.TryParse(fields[0], out var id) ? id : 0;
                Waves = new List<WaveData>();
                if (!int.TryParse(fields[1], out var wave))
                    return;

                var monsters = new List<MonsterData>();
                for (var i = 0; i < 4; i++)
                {
                    var offset = i * 3;
                    var characterId = int.TryParse(fields[2 + offset], out var outCharacterId) ? outCharacterId : 0;
                    if (characterId == 0)
                        break;

                    monsters.Add(new MonsterData(
                        characterId,
                        int.TryParse(fields[3 + offset], out var level) ? level : 0,
                        int.TryParse(fields[4 + offset], out var count) ? count : 0
                    ));
                }

                var isBoss = fields[14].Equals("1");
                var rewardId = int.TryParse(fields[15], out var outRewardId) ? outRewardId : 0;
                var exp = int.TryParse(fields[16], out var outExp) ? outExp : 0;
                Waves.Add(new WaveData(wave, monsters, isBoss, rewardId, exp));
            }

            public override void EndOfSheetInitialize()
            {
                Waves.Sort((left, right) =>
                {
                    if (left.Number > right.Number) return 1;
                    if (left.Number < right.Number) return -1;
                    return 0;
                });

                HasBoss = Waves.Any(wave => wave.IsBoss);
                TotalMonsterIds = new List<int>();
                TotalMonsterIds.AddRange(Waves.SelectMany(wave => wave.Monsters)
                    .Select(monster => monster.CharacterId)
                    .Distinct());
                TotalRewardIds = new List<int>();
                TotalRewardIds.AddRange(Waves.Select(wave => wave.RewardId)
                    .Where(rewardId => rewardId != 0)
                    .Distinct());
                TotalExp = Waves.Sum(wave => wave.Exp);
            }
        }
        
        public StageSheet() : base(nameof(StageSheet))
        {
        }

        protected override void AddRow(int key, Row value)
        {
            if (!TryGetValue(key, out var row))
            {
                Add(key, value);

                return;
            }

            if (value.Waves.Count == 0)
                return;

            row.Waves.Add(value.Waves[0]);
        }
    }

    public static class StageSheetExtension
    {
        public static string GetLocalizedDescription(this StageSheet.Row stageRow)
        {
            // todo: return LocalizationManager.Localize($"{stageRow.Key}");
            return $"{stageRow.Key}: Description";
        }

        public static List<MaterialItemSheet.Row> GetRewardItemRows(this StageSheet.Row stageRow)
        {
            var tableSheets = Game.Game.instance.TableSheets;
            var itemRows = new List<MaterialItemSheet.Row>();
            foreach (var rewardId in stageRow.TotalRewardIds)
            {
                if (!tableSheets.StageRewardSheet.TryGetValue(rewardId, out var rewardRow))
                {
                    continue;
                }

                foreach (var rewardData in rewardRow.Rewards)
                {
                    var itemId = rewardData.ItemId;
                    if (!tableSheets.MaterialItemSheet.TryGetValue(itemId, out var item))
                    {
                        continue;
                    }

                    itemRows.Add(item);
                }
            }

            return itemRows;
        }
    }
}
