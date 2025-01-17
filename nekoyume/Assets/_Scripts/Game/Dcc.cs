﻿using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Libplanet;
using Nekoyume.EnumType;
using Nekoyume.Pattern;
using Nekoyume.UI;
using Nekoyume.UI.Model;
using UniRx;
using UnityEngine;

namespace Nekoyume.Game
{
    public class Dcc : MonoSingleton<Dcc>
    {
        public static string DccVisible = "dcc_visible";

        public Dictionary<string, int> Avatars { get; private set; }
        public bool? IsConnected { get; set; } = null;
        private readonly Dictionary<int, Dictionary<DccPartsType, int>> _parts = new();

        public void Init(Dictionary<string, int> avatars)
        {
            Avatars = avatars;
        }

        public bool IsVisible(Address address, out int id, out bool isVisible)
        {
            var addr = address.ToString();
            if (Avatars is null || addr == "0x0000000000000000000000000000000000000000")
            {
                id = 0;
                isVisible = false;
                return false;
            }

            var isExistDcc = Avatars.ContainsKey(addr);
            var key = $"{DccVisible}_{addr}";
            if (!PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.SetInt(key, 1);
            }

            id = Avatars.ContainsKey(addr) ? Avatars[addr] : 0;
            isVisible = PlayerPrefs.GetInt(key, 0) > 0;
            return isExistDcc;
        }

        public void SetVisible(int value)
        {
            var avatarState = Game.instance.States.CurrentAvatarState;
            PlayerPrefs.SetInt($"{DccVisible}_{avatarState.address.ToString()}", value);
        }

        public async Task<Dictionary<DccPartsType, int>> GetParts(int dccId)
        {
            if (_parts.ContainsKey(dccId))
            {
                return _parts[dccId];
            }

            await StartCoroutine(RequestParts(dccId));
            return _parts[dccId];
        }

        IEnumerator RequestParts(int dccId)
        {
            var dccParts = new Dictionary<DccPartsType, int>();
            var url = $"{Game.instance.URL.DccMetadata}{dccId}.json";
            var headerName = Game.instance.URL.DccEthChainHeaderName;
            var headerValue = Game.instance.URL.DccEthChainHeaderValue;
            yield return StartCoroutine(RequestManager.instance.GetJson(
                url,
                headerName,
                headerValue,
                (json) =>
            {
                var result = JsonSerializer.Deserialize<DccMetadata>(json);
                dccParts.Add(DccPartsType.background, result.traits[0]);
                dccParts.Add(DccPartsType.skin, result.traits[1]);
                dccParts.Add(DccPartsType.face, result.traits[2]);
                dccParts.Add(DccPartsType.ear_tail, result.traits[3]);
                dccParts.Add(DccPartsType.ac_face, result.traits[4]);
                dccParts.Add(DccPartsType.hair, result.traits[5]);
                dccParts.Add(DccPartsType.ac_eye, result.traits[6]);
                dccParts.Add(DccPartsType.ac_head, result.traits[7]);
                if (!_parts.ContainsKey(dccId))
                {
                    _parts.Add(dccId, dccParts);
                }
            }));
        }
    }
}
