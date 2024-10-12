using System.IO;
using JetBrains.Annotations;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Content.Shared.Speech;
using Robust.Shared.Prototypes;
using Content.Shared.Inventory;

namespace Content.Shared.Chat
{
    [Serializable, NetSerializable]
    public sealed class ChatMessage
    {
        public ChatChannel Channel;
        public string Message;
        public string WrappedMessage;
        public NetEntity SenderEntity;

        /// <summary>
        ///     Identifier sent when <see cref="SenderEntity"/> is <see cref="NetEntity.Invalid"/>
        ///     if this was sent by a player to assign a key to the sender of this message.
        ///     This is unique per sender.
        /// </summary>
        public int? SenderKey;

        public bool HideChat;
        public Color? MessageColorOverride;
        public string? AudioPath;
        public float AudioVolume;

        [NonSerialized]
        public bool Read;

        public ChatMessage(ChatChannel channel, string message, string wrappedMessage, NetEntity source, int? senderKey, bool hideChat = false, Color? colorOverride = null, string? audioPath = null, float audioVolume = 0)
        {
            Channel = channel;
            Message = message;
            WrappedMessage = wrappedMessage;
            SenderEntity = source;
            SenderKey = senderKey;
            HideChat = hideChat;
            MessageColorOverride = colorOverride;
            AudioPath = audioPath;
            AudioVolume = audioVolume;
        }
    }

    /// <summary>
    ///     Sent from server to client to notify the client about a new chat message.
    /// </summary>
    [UsedImplicitly]
    public sealed class MsgChatMessage : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public ChatMessage Message = default!;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            var length = buffer.ReadVariableInt32();
            using var stream = new MemoryStream(length);
            buffer.ReadAlignedMemory(stream, length);
            serializer.DeserializeDirect(stream, out Message);
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            var stream = new MemoryStream();
            serializer.SerializeDirect(stream, Message);
            buffer.WriteVariableInt32((int) stream.Length);
            buffer.Write(stream.AsSpan());
        }
    }
    /// <summary>
    ///     This event should be sent everytime an entity talks (Radio, local chat, etc...).
    ///     The event is sent to both the entity itself, and all clothing (For stuff like voice masks).
    /// </summary>
    public sealed class TransformSpeakerNameEvent : EntityEventArgs, IInventoryRelayEvent
    {
        public SlotFlags TargetSlots { get; } = SlotFlags.WITHOUT_POCKET;
        public EntityUid Sender;
        public string VoiceName;
        public ProtoId<SpeechVerbPrototype>? SpeechVerb;

        public TransformSpeakerNameEvent(EntityUid sender, string name)
        {
            Sender = sender;
            VoiceName = name;
            SpeechVerb = null;
        }
    }
}
