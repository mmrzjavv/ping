using MediatR;
using System.Collections.Generic;

namespace TwitterClone.Application.Trends.Queries.GetTrends
{
    public class GetTrendsQuery : IRequest<IEnumerable<TrendDto>>
    {
        public int? Count { get; set; }
    }
}