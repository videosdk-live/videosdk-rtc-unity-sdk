using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace live.videosdk
{
    public interface IParticipant
    {
        string Id { get; }
        string Name { get; }
        bool IsLocal { get; }
        string ToString();
    }
}
