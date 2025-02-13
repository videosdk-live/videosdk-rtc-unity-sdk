using System;
namespace live.videosdk
{
    public class Participant : IParticipant
    {
        public string ParticipantId { get; }
        public string Name { get; }
        public bool IsLocal { get; }

        public Participant(string Id, string name, bool isLocal)
        {
            this.ParticipantId = Id;
            this.Name = name;
            this.IsLocal = isLocal;
        }

        public override string ToString()
        {
            return $"ParticipantId: {ParticipantId} Name: {Name} IsLocal: {IsLocal}";
        }

    }
}
