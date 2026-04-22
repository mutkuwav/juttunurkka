using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Prototype.Services
{
    public class JuttunurkkaApiService
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl = "http://86.50.20.47:8080/";
        private readonly JsonSerializerOptions jsonOptions = new()
        
        {
            PropertyNameCaseInsensitive = true
        };

        public JuttunurkkaApiService()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl.TrimEnd('/') + "/")
            };
        }

        public async Task<CreateRoomResponse> CreateRoomAsync()
        {
            var response = await httpClient.PostAsync("rooms", null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<CreateRoomResponse>(jsonOptions);
            return result ?? throw new Exception("Room creation response was empty.");
        }

        public async Task PublishSurveyAsync(string roomId, Survey survey)
        {
            var selectedEmojis = survey.emojis.Where(e => e.IsChecked).ToList();
            if (selectedEmojis.Count == 0)
            {
                selectedEmojis = survey.emojis.ToList();
            }

            var request = new PublishSurveyRequest
            {
                IntroMessage = survey.introMessage,
                Emojis = selectedEmojis.Select(e => new EmojiLiteDto
                {
                    Id = e.ID,
                    Name = e.Name
                }).ToList()
            };

            var response = await httpClient.PutAsJsonAsync($"rooms/{roomId}/survey", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<JoinRoomResponse> JoinRoomAsync(string roomCode, string deviceId)
        {
            var response = await httpClient.PostAsJsonAsync("join", new
            {
                roomCode,
                deviceId
            });

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JoinRoomResponse>(jsonOptions);
            return result ?? throw new Exception("Join response was empty.");
        }

        public async Task SubmitEmojiVoteAsync(string roomId, string deviceId, int emojiId)
        {
            var response = await httpClient.PostAsJsonAsync($"rooms/{roomId}/emoji-vote", new
            {
                deviceId,
                emojiId
            });

            response.EnsureSuccessStatusCode();
        }

        public async Task<RoomStatusResponse> GetRoomStatusAsync(string roomId)
        {
            var response = await httpClient.GetAsync($"rooms/{roomId}/status");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<RoomStatusResponse>(jsonOptions);
            return result ?? throw new Exception("Status response was empty.");
        }

        public async Task<Dictionary<int, int>> GetEmojiResultsAsync(string roomId)
        {
            var response = await httpClient.GetAsync($"rooms/{roomId}/emoji-results");
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(jsonOptions);
            if (raw == null)
            {
                return new Dictionary<int, int>();
            }

            return raw.ToDictionary(kv => int.Parse(kv.Key), kv => kv.Value);
        }

        public async Task CloseSurveyAsync(string roomId)
        {
            var response = await httpClient.PostAsync($"rooms/{roomId}/close", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task StartActivityVoteAsync(string roomId, List<Activity> activities)
        {
            var request = new ActivityStartRequest
            {
                Activities = activities.Select(a => new ActivityDto
                {
                    Title = a.Title ?? string.Empty,
                    ImageSource = a.ImageSource ?? string.Empty
                }).ToList()
            };

            var response = await httpClient.PostAsJsonAsync($"rooms/{roomId}/activity-start", request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ActivityCandidatesResponse> GetActivityCandidatesAsync(string roomId)
        {
            var response = await httpClient.GetAsync($"rooms/{roomId}/activity-candidates");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ActivityCandidatesResponse>(jsonOptions);
            return result ?? new ActivityCandidatesResponse();
        }

        public async Task SubmitActivityVoteAsync(string roomId, string deviceId, string title)
        {
            var response = await httpClient.PostAsJsonAsync($"rooms/{roomId}/activity-vote", new
            {
                deviceId,
                title
            });

            response.EnsureSuccessStatusCode();
        }

        public async Task CloseActivityVoteAsync(string roomId)
        {
            var response = await httpClient.PostAsync($"rooms/{roomId}/activity-close", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Dictionary<string, int>> GetActivityResultsAsync(string roomId)
        {
            var response = await httpClient.GetAsync($"rooms/{roomId}/activity-results");
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>(jsonOptions);
            return raw ?? new Dictionary<string, int>();
        }

        public async Task<List<DrawnEmojiItem>> GetDrawnEmojiResultsAsync(string roomId)
        {
            using var client = new HttpClient();

            string url = $"{baseUrl}/rooms/{roomId}/drawn-emoji-results";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return new List<DrawnEmojiItem>();

            var json = await response.Content.ReadAsStringAsync();

            var doc = System.Text.Json.JsonDocument.Parse(json);

            var list = new List<DrawnEmojiItem>();

            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                list.Add(new DrawnEmojiItem
                {
                    DeviceId = item.GetProperty("deviceId").GetString(),
                    ImageUrl = item.GetProperty("imageUrl").GetString()
                });
            }

            return list;
        }

        public async Task<JoinRoomResponse> GetSurveyStateAsync(string roomId)
        {
            var response = await httpClient.GetAsync($"rooms/{roomId}/survey");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JoinRoomResponse>(jsonOptions);
            return result ?? throw new Exception("Survey state response was empty.");
        }

        public class CreateRoomResponse
        {
            [JsonPropertyName("roomId")]
            public string RoomId { get; set; }

            [JsonPropertyName("roomCode")]
            public string RoomCode { get; set; }
        }

        public class JoinRoomResponse
        {
            [JsonPropertyName("roomId")]
            public string RoomId { get; set; }

            [JsonPropertyName("surveyReady")]
            public bool SurveyReady { get; set; }

            [JsonPropertyName("introMessage")]
            public string IntroMessage { get; set; }

            [JsonPropertyName("emojis")]
            public List<EmojiLiteDto> Emojis { get; set; } = new();
        }

        public class RoomStatusResponse
        {
            [JsonPropertyName("joinedCount")]
            public int JoinedCount { get; set; }

            [JsonPropertyName("votedCount")]
            public int VotedCount { get; set; }

            [JsonPropertyName("surveyClosed")]
            public bool SurveyClosed { get; set; }
        }

        public class PublishSurveyRequest
        {
            [JsonPropertyName("introMessage")]
            public string IntroMessage { get; set; }

            [JsonPropertyName("emojis")]
            public List<EmojiLiteDto> Emojis { get; set; } = new();
        }

        public class EmojiLiteDto
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }
        }

        public class ActivityStartRequest
        {
            [JsonPropertyName("activities")]
            public List<ActivityDto> Activities { get; set; } = new();
        }

        public class ActivityDto
        {
            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("imageSource")]
            public string ImageSource { get; set; }
        }

        public class ActivityCandidatesResponse
        {
            [JsonPropertyName("voteOpen")]
            public bool VoteOpen { get; set; }

            [JsonPropertyName("activities")]
            public List<ActivityDto> Activities { get; set; } = new();
        }
        public class DrawnEmojiItem
        {
            public string DeviceId { get; set; }
            public string ImageUrl { get; set; }
        }
    }


}