// <auto-generated/>
using System;

namespace Telegram.Api.TL
{
	public partial class TLMessageActionPinMessage : TLMessageActionBase 
	{
		public TLMessageActionPinMessage() { }
		public TLMessageActionPinMessage(TLBinaryReader from, TLType type = TLType.MessageActionPinMessage)
		{
			Read(from, type);
		}

		public override TLType TypeId { get { return TLType.MessageActionPinMessage; } }

		public override void Read(TLBinaryReader from, TLType type = TLType.MessageActionPinMessage)
		{
		}

		public override void Write(TLBinaryWriter to)
		{
			to.Write(0x94BD38ED);
		}
	}
}