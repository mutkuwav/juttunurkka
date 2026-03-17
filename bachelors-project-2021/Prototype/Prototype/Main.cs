using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prototype.Services;

namespace Prototype
{
    public class Main
    {
        private static Main instance = null;

        public enum MainState
        {
            Default = 0,
            Browsing = 1,
            Editing = 2,
            Hosting = 3,
            Participating = 4,
            CreatingNew = 5
        }

        public MainState state;
        public SurveyClient client = null;   // legacy
        public SurveyHost host = null;       // legacy

        public JuttunurkkaApiService Api { get; }

        private Main()
        {
            state = MainState.Default;
            Api = new JuttunurkkaApiService();
        }

        public void BrowseSurveys()
        {
            state = MainState.Browsing;
        }

        public void EditSurvey()
        {
            state = MainState.Editing;
        }

        public void CreateNewSurvey()
        {
            state = MainState.CreatingNew;
            SurveyManager.GetInstance().ResetSurvey();
        }

        public async Task<bool> JoinSurvey(string roomCode)
        {
            try
            {
                var result = await Api.JoinRoomAsync(roomCode, OnlineSession.Current.DeviceId);

                OnlineSession.Current.ResetSessionData();
                OnlineSession.Current.RoomId = result.RoomId;
                OnlineSession.Current.RoomCode = roomCode;
                OnlineSession.Current.IntroMessage = result.IntroMessage;
                OnlineSession.Current.Emojis = MapServerEmojis(result.Emojis);

                state = MainState.Participating;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JoinSurvey failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> HostSurvey()
        {
            try
            {
                var survey = SurveyManager.GetInstance().GetSurvey();

                var room = await Api.CreateRoomAsync();
                await Api.PublishSurveyAsync(room.RoomId, survey);

                OnlineSession.Current.ResetSessionData();
                OnlineSession.Current.RoomId = room.RoomId;
                OnlineSession.Current.RoomCode = room.RoomCode;
                OnlineSession.Current.IntroMessage = survey.introMessage;
                OnlineSession.Current.Emojis = GetSelectedSurveyEmojis(survey);

                survey.RoomCode = room.RoomCode;

                state = MainState.Hosting;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HostSurvey failed: {ex.Message}");
                return false;
            }
        }

        private List<Emoji> GetSelectedSurveyEmojis(Survey survey)
        {
            var selected = survey.emojis.Where(e => e.IsChecked).ToList();
            if (selected.Count == 0)
            {
                selected = survey.emojis.ToList();
            }

            return selected.Select(CloneEmoji).ToList();
        }

        private List<Emoji> MapServerEmojis(List<JuttunurkkaApiService.EmojiLiteDto> serverEmojis)
        {
            var defaults = new Survey().emojis.ToDictionary(e => e.ID, e => e);

            var result = new List<Emoji>();
            foreach (var item in serverEmojis)
            {
                if (defaults.TryGetValue(item.Id, out var template))
                {
                    var clone = CloneEmoji(template);
                    clone.Name = item.Name;
                    result.Add(clone);
                }
                else
                {
                    result.Add(new Emoji
                    {
                        ID = item.Id,
                        Name = item.Name,
                        ImageSource = $"emoji{item.Id}lowres.png"
                    });
                }
            }

            return result;
        }

        private Emoji CloneEmoji(Emoji source)
        {
            return new Emoji
            {
                ID = source.ID,
                Name = source.Name,
                Impact = source.Impact,
                IsChecked = source.IsChecked,
                Activities = source.Activities?.ToList() ?? new List<Activity>(),
                ImageSource = source.ImageSource
            };
        }

        public MainState GetMainState()
        {
            return state;
        }

        public static Main GetInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            instance = new Main();
            return instance;
        }
    }
}