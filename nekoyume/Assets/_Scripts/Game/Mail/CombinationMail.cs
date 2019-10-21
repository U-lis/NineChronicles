using System;
using Nekoyume.Action;

namespace Nekoyume.Game.Mail
{
    [Serializable]
    public class CombinationMail : AttachmentMail
    {
        protected override string TypeId => "combinationMail";
        public override MailType MailType { get => MailType.Forge; }

        public CombinationMail(Combination.Result attachmentActionResult, long blockIndex) : base(attachmentActionResult, blockIndex)
        {
            
        }

        public CombinationMail(Bencodex.Types.Dictionary serialized)
            : base(serialized)
        {
        }

        public override string ToInfo()
        {
            return "조합 완료";
        }

        public override void Read(IMail mail)
        {
            mail.Read(this);
        }
    }
}
