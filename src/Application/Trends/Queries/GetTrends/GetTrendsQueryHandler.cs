using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Trends.Queries.GetTrends
{
    public class GetTrendsQueryHandler : IRequestHandler<GetTrendsQuery, IEnumerable<TrendDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetTrendsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TrendDto>> Handle(GetTrendsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Trends.AsNoTracking().OrderByDescending(t => t.RecordedAt).ThenByDescending(t => t.PostCount);
            var limitedQuery = request.Count.HasValue ? query.Take(request.Count.Value) : query;

            return await limitedQuery
                .ProjectTo<TrendDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}