﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using PatreonClient.Models.Attributes;
using PatreonClient.Responses;

namespace PatreonClient.Models.Relationships
{
    public class AddressRelationships : IRelationship
    {
        [JsonPropertyName("user")] public PatreonResponse<User, UserRelationships> User { get; set; }
        [JsonPropertyName("campaigns")] public PatreonCollectionResponse<Campaign, CampaignRelationships> Campaigns { get; set; }

        public void AssignRelationship(IReadOnlyCollection<PatreonData> includes)
        {
            this.AssignDataAndRelationship(includes, User)
	            .AssignCollectionAttributesAndRelationships(includes, Campaigns);
      }
    }
}
