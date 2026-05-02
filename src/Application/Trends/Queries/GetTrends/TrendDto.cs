using AutoMapper;
using System;
using TwitterClone.Application.Common.Mappings;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Trends.Queries.GetTrends
{
    public class TrendDto : IMapFrom<Trend>
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public int PostCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime RecordedAt { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Trend, TrendDto>();
        }
    }
}