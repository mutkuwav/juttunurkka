using System;
using System.Collections.Generic;
using Microsoft.Maui.Storage;

namespace Prototype
{
    public class OnlineSession
    {
        private static OnlineSession instance;

        public static OnlineSession Current
        {
            get
            {
                instance ??= new OnlineSession();
                return instance;
            }
        }

        public string RoomId { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;

        public string IntroMessage { get; set; } = string.Empty;
        public List<Emoji> Emojis { get; set; } = new();
        public Dictionary<int, int> EmojiResults { get; set; } = new();

        public List<Activity> ActivityCandidates { get; set; } = new();
        public Dictionary<string, int> ActivityResults { get; set; } = new();
        public bool ActivityVoteOpen { get; set; }

        public int JoinedCount { get; set; }
        public int VotedCount { get; set; }
        public bool SurveyClosed { get; set; }

        public string DeviceId { get; private set; }

        private OnlineSession()
        {
            var existing = Preferences.Default.Get("juttunurkka_device_id", string.Empty);
            if (string.IsNullOrWhiteSpace(existing))
            {
                existing = Guid.NewGuid().ToString();
                Preferences.Default.Set("juttunurkka_device_id", existing);
            }

            DeviceId = existing;
        }

        public void ResetSessionData()
        {
            RoomId = string.Empty;
            RoomCode = string.Empty;
            IntroMessage = string.Empty;
            Emojis = new List<Emoji>();
            EmojiResults = new Dictionary<int, int>();

            ActivityCandidates = new List<Activity>();
            ActivityResults = new Dictionary<string, int>();
            ActivityVoteOpen = false;

            JoinedCount = 0;
            VotedCount = 0;
            SurveyClosed = false;
        }
    }
}