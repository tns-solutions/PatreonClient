﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using PatreonClient.Models;
using PatreonClient.Models.Attributes;

namespace PatreonClient
{
    public class PatreonClient
    {
        private readonly HttpClient _client;

        public PatreonClient(HttpClient client)
        {
            _client = client;
        }

        public UserRequestBuilder Identity => new UserRequestBuilder(this);
        public CampaignRequestBuilder Campaigns => new CampaignRequestBuilder(this);
        public MemberRequestBuilder Members => new MemberRequestBuilder(this);
        public PostRequestBuilder Post => new PostRequestBuilder(this);

        private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private async Task<string> SendAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Bad Request {content}");
            }

            return content;
        }

        public async Task<PatreonResponse<T>> GetSingle<T>(string url)
        {
            var content = await SendAsync(url);

            var result = JsonSerializer.Deserialize<PatreonResponse<T>>(content, JsonSerializerOptions);

            ResolveRelationship(content,
                                (element, type, id) =>
                                    TryAddRelationship(element, type, id, result.Data.Relationships));

            return result;
        }

        public async Task<PatreonCollectionResponse<T>> GetCollection<T>(string url)
        {
            var content = await SendAsync(url);

            Console.WriteLine(content);

            var result = JsonSerializer.Deserialize<PatreonCollectionResponse<T>>(content, JsonSerializerOptions);

            ResolveRelationship(content,
                                (element, type, id) =>
                                {
                                    foreach (var d in result.Data)
                                        if (TryAddRelationship(element, type, id, d.Relationships))
                                            break;
                                });

            return result;
        }

        private delegate void AddToAttribute(string element, string type, string id);

        private static void ResolveRelationship(string content, AddToAttribute addToAttribute)
        {
            var doc = JsonDocument.Parse(Encoding.UTF8.GetBytes(content));

            var obj = (IEnumerable<JsonProperty>) doc.RootElement.EnumerateObject();
            var included = obj.FirstOrDefault(x => x.Name == "included");

            var array = included.Value.EnumerateArray();

            foreach (var el in array)
            {
                var type = el.EnumerateObject().FirstOrDefault(x => x.Name == "type").Value.ToString();
                var id = el.EnumerateObject().FirstOrDefault(x => x.Name == "id").Value.ToString();
                addToAttribute(el.ToString(), type, id);
            }
        }

        private static bool TryAddRelationship(
            string jsonElement,
            string type,
            string id,
            Relationships relationships)
        {
            if (relationships == null) return false;

            if (type.Equals(nameof(Relationships.Campaign), StringComparison.InvariantCultureIgnoreCase)
                && relationships.Campaign != null
                && relationships.Campaign.Data.Id.Equals(id))
            {
                relationships.Campaign.Data =
                    JsonSerializer.Deserialize<PatreonData<Campaign>>(jsonElement, JsonSerializerOptions);

                return true;
            }

            if (type.Equals(nameof(Relationships.User), StringComparison.InvariantCultureIgnoreCase)
                && relationships.User != null
                && relationships.User.Data.Id.Equals(id))
            {
                relationships.User.Data =
                    JsonSerializer.Deserialize<PatreonData<User>>(jsonElement, JsonSerializerOptions);

                return true;
            }

            if (type.Equals(nameof(Relationships.Memberships), StringComparison.InvariantCultureIgnoreCase)
                && relationships.Memberships != null)
            {
                relationships.Memberships.Data =
                    JsonSerializer.Deserialize<IEnumerable<PatreonData<Member>>>(jsonElement, JsonSerializerOptions);

                return true;
            }

            return false;
        }
    }
}