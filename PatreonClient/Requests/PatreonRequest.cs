﻿using PatreonClient.Models;

namespace PatreonClient.Requests
{
    public class PatreonRequest<TResponse, TAttribute, TRelationship> : IPatreonRequest<TResponse, TAttribute, TRelationship>
        where TResponse : IPatreonResponse<TAttribute, TRelationship>
        where TRelationship : IRelationship
    {
        internal PatreonRequest(string url) => Url = url;
        public string Url { get; }
    }
}