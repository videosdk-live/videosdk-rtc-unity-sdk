using System;
namespace live.videosdk
{
    public class Participant : IParticipant
    {
        public string Id { get; }
        public string Name { get; }
        public bool IsLocal { get; }

        public Participant(string Id, string name, bool isLocal)
        {
            this.Id = Id;
            this.Name = name;
            this.IsLocal = isLocal;
        }

        public override string ToString()
        {
            return $"ParticipantId: {Id} Name: {Name} IsLocal: {IsLocal}";
        }

    }
}
