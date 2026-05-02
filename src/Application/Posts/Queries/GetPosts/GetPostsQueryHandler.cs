using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Application.Common.Extensions;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Application.Posts.Queries.GetPosts
{
    public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, IEnumerable<PostDto>>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUser;

        public GetPostsQueryHandler(IApplicationDbContext context, IMapper mapper, ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<PostDto>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.DomainUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.ApplicationUserId == _currentUser.UserId, cancellationToken);

            var followingIds = user != null
                ? await _context.Follows
                    .Where(f => f.FollowerId == user.Id)
                    .Select(f => f.FollowedId)
                    .ToListAsync(cancellationToken)
                : new List<int>();

            var followerIds = user != null
                ? await _context.Follows
                    .Where(f => f.FollowedId == user.Id)
                    .Select(f => f.FollowerId)
                    .ToListAsync(cancellationToken)
                : new List<int>();

            var now = DateTime.UtcNow;
            var oneWeekAgo = now.AddDays(-7);

            // بارگذاری پست‌ها با تمام اطلاعات مورد نیاز
            var postsQuery = _context.Posts
                .AsNoTracking()
                .Where(p => !p.AnswerToId.HasValue && p.Created >= oneWeekAgo);

            if (request.BeforeId.HasValue)
                postsQuery = postsQuery.Where(p => p.Id < request.BeforeId.Value);

            var posts = await postsQuery
                .Include(p => p.CreatedBy) 
                .Include(p => p.Likes)
                .Include(p => p.RePosts)
                .Include(p => p.Answers)
                .ToListAsync(cancellationToken);

            // الگوریتم رتبه‌بندی پیشرفته
            var rankedPosts = posts
                .Select(p => new
                {
                    Post = p,
                    Score = CalculatePostScore(p, user?.Id, followingIds, followerIds, now)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Post.Created)
                .Take(request.Count ?? 20)
                .Select(x => x.Post)
                .ToList();

            return _mapper.Map<IEnumerable<PostDto>>(rankedPosts);
        }

       private double CalculatePostScore(Post post, int? currentUserId, List<int> followingIds, List<int> followerIds, DateTime now)
{
    double score = 0;

    // 1. امتیاز زمانی (تازگی محتوا) - Recency Score
    var ageInHours = (now - post.Created).TotalHours;
    var recencyScore = Math.Max(0, 1000 * Math.Exp(-ageInHours / 24));
    score += recencyScore;

    // 2. امتیاز تعامل (Engagement Score)
    var likesCount = post.Likes?.Count() ?? 0;
    var repostsCount = post.RePosts?.Count() ?? 0;
    var repliesCount = post.Answers?.Count() ?? 0;

    // وزن‌های متفاوت برای انواع تعامل
    var engagementScore = (likesCount * 1.0) + (repostsCount * 3.0) + (repliesCount * 2.0);
    
    // نرمال‌سازی لگاریتمی برای جلوگیری از تسلط پست‌های پرتعامل
    score += Math.Log(1 + engagementScore) * 500;

    // 3. نرخ تعامل (Engagement Rate) - تعامل نسبت به زمان
    if (ageInHours > 0)
    {
        var engagementRate = engagementScore / Math.Max(1, ageInHours);
        score += Math.Log(1 + engagementRate) * 300;
    }

    // 4. امتیاز شبکه اجتماعی (Social Network Score)
    if (currentUserId.HasValue)
    {
        // پست از کسی که فالو می‌کنیم
        if (followingIds.Contains(post.CreatedById))
        {
            score += 5000;
        }

        // پست از کسی که ما را فالو می‌کند
        if (followerIds.Contains(post.CreatedById))
        {
            score += 2000;
        }

        // پست خود کاربر
        if (post.CreatedById == currentUserId.Value)
        {
            score += 1000;
        }

        // لایک از افرادی که فالو می‌کنیم
        var likesFromFollowing = post.Likes?.Count(l => followingIds.Contains(l.CreatedById)) ?? 0;
        score += likesFromFollowing * 800;

        // ریپست از افرادی که فالو می‌کنیم
        var repostsFromFollowing = post.RePosts?.Count(r => followingIds.Contains(r.CreatedById)) ?? 0;
        score += repostsFromFollowing * 1200;

        // کامنت از افرادی که فالو می‌کنیم
        var repliesFromFollowing = post.Answers?.Count(a => followingIds.Contains(a.CreatedById)) ?? 0;
        score += repliesFromFollowing * 1000;
    }

    // 5. امتیاز محبوبیت سازنده (Creator Popularity)
    var creatorEngagement = (post.Likes?.Count() ?? 0) + (post.RePosts?.Count() ?? 0) * 2;
    score += Math.Log(1 + creatorEngagement) * 200;

    // 6. امتیاز تنوع (Diversity Boost)
    var diversityBoost = new Random(post.Id).NextDouble() * 100;
    score += diversityBoost;

    // 7. جریمه برای پست‌های قدیمی‌تر
    if (ageInHours > 48)
    {
        score *= 0.7;
    }
    else if (ageInHours > 24)
    {
        score *= 0.85;
    }

    // 8. بوست برای پست‌های ترند (Trending Boost)
    if (ageInHours < 6 && engagementScore > 50)
    {
        score *= 1.5;
    }

    return score;
}

    }
}
