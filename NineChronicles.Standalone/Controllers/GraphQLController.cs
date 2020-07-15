using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Bencodex.Types;
using Libplanet;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.KeyStore;
using Microsoft.AspNetCore.Mvc;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.State;
using NineChronicles.Standalone.GraphTypes;
using Libplanet.Crypto;
using Microsoft.Extensions.Hosting;
using NineChronicles.Standalone.Properties;
using Serilog;


namespace NineChronicles.Standalone.Controllers
{
    [ApiController]
    public class GraphQLController : ControllerBase
    {
        private ConcurrentDictionary<Address, long> NotificationRecords { get; }
            = new ConcurrentDictionary<Address, long>();
        private StandaloneContext StandaloneContext { get; }

        public const string RunStandaloneEndpoint = "/run-standalone";

        public const string SetMiningEndpoint = "/set-mining";

        public GraphQLController(StandaloneContext standaloneContext)
        {
            StandaloneContext = standaloneContext;
        }

        [HttpPost(RunStandaloneEndpoint)]
        public IActionResult RunStandalone()
        {
            try
            {
                IHostBuilder nineChroniclesNodeHostBuilder = Host.CreateDefaultBuilder();
                nineChroniclesNodeHostBuilder =
                    StandaloneContext.NineChroniclesNodeService.Configure(
                        nineChroniclesNodeHostBuilder);
                nineChroniclesNodeHostBuilder.RunConsoleAsync();
            }
            catch (Exception e)
            {
                return BadRequest($"Failed to launch node service. {e.Message}");
            }

            return Ok("Node service started.");
        }

        [HttpPost(SetMiningEndpoint)]
        public IActionResult SetMining([FromBody] SetMiningRequest request)
        {
            if (request.Mine)
            {
                var privateKey = new PrivateKey(ByteUtil.ParseHex(request.PrivateKeyString));
                StandaloneContext.NineChroniclesNodeService.StartMining(privateKey);
            }
            else
            {
                StandaloneContext.NineChroniclesNodeService.StopMining();
            }

            return Ok($"Set mining status to {request.Mine}.");
        }

        private void NotifyRefillActionPoint(
            object sender, BlockChain<PolymorphicAction<ActionBase>>.TipChangedEventArgs args)
        {
            List<Tuple<Guid, ProtectedPrivateKey>> tuples =
                StandaloneContext.KeyStore.List().ToList();
            if (!tuples.Any())
            {
                return;
            }

            IEnumerable<Address> playerAddresses = tuples.Select(tuple => tuple.Item2.Address);
            var chain = StandaloneContext.BlockChain;
            List<IValue> states = playerAddresses
                .Select(addr => chain.GetState(addr))
                .Where(value => !(value is null))
                .ToList();

            if (!states.Any())
            {
                return;
            }

            var agentStates =
                states.Select(state => new AgentState((Bencodex.Types.Dictionary) state));
            var avatarStates = agentStates.SelectMany(agentState =>
                agentState.avatarAddresses.Values.Select(address =>
                    new AvatarState((Bencodex.Types.Dictionary) chain.GetState(address))));

            bool IsDailyRewardRefilled(long dailyRewardReceivedIndex)
            {
                return args.Index >= dailyRewardReceivedIndex + GameConfig.DailyRewardInterval;
            }

            bool NeedsRefillNotification(AvatarState avatarState)
            {
                if (NotificationRecords.TryGetValue(avatarState.address, out long record))
                {
                    return avatarState.dailyRewardReceivedIndex != record
                           && IsDailyRewardRefilled(avatarState.dailyRewardReceivedIndex);
                }

                return IsDailyRewardRefilled(avatarState.dailyRewardReceivedIndex);
            }

            var avatarStatesToSendNotification = avatarStates
                .Where(NeedsRefillNotification)
                .ToList();

            if (avatarStatesToSendNotification.Any())
            {
                var notification = new Notification(NotificationEnum.Refill);
                StandaloneContext.NotificationSubject.OnNext(notification);
            }

            foreach (var avatarState in avatarStatesToSendNotification)
            {
                Log.Debug(
                    "Record notification for {AvatarAddress}",
                    avatarState.address.ToHex());
                NotificationRecords[avatarState.address] = avatarState.dailyRewardReceivedIndex;
            }
        }
    }
}
