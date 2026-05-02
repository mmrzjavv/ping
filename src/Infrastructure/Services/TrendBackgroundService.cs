using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TwitterClone.Application.Common.Interfaces;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Services
{
    public class TrendBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TrendBackgroundService> _logger;
        private readonly IDateTime _dateTime;

        public TrendBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<TrendBackgroundService> logger,
            IDateTime dateTime)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _dateTime = dateTime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Trend Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateTrendsAsync(stoppingToken);
                    _logger.LogInformation("Trends updated successfully at: {time}", _dateTime.Now);
                }
                catch (Exception ex) when (!(ex is TaskCanceledException))
                {
                    _logger.LogError(ex, "An error occurred while updating trends.");
                }


                try
                {
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (TaskCanceledException)
                {

                }
            }
        }

        private async Task UpdateTrendsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var today = _dateTime.Now.Date;
            var windowStart = today.AddDays(-1);

            // واکشی پست‌های ۲۴ ساعت اخیر به همراه لایک‌ها
            var posts = await context.Posts
                .Include(p => p.Likes)
                .Where(p => p.Created >= windowStart)
                .AsNoTracking() // برای پرفورمنس بهتر چون فقط برای محاسبات استفاده می‌شود
                .ToListAsync(cancellationToken);

            // استخراج تگ‌ها و دسته‌بندی آن‌ها
            var taggedPosts = posts
                .Select(p => new { Post = p, Tags = ExtractTags(p.Content).ToList() })
                .Where(x => x.Tags.Any())
                .ToList();

            var trends = taggedPosts
                .SelectMany(x => x.Tags.Select(tag => new { x.Post, Tag = tag }))
                .GroupBy(x => x.Tag)
                .Select(group =>
                {
                    var distinctPosts = group
                        .GroupBy(x => x.Post.Id)
                        .Select(g => g.First().Post)
                        .ToList();

                    return new Trend
                    {
                        Tag = group.Key,
                        RecordedAt = today,
                        PostCount = distinctPosts.Count,
                        LikeCount = distinctPosts.Sum(p => p.Likes?.Count() ?? 0)
                    };
                })
                .ToList();



            var existingTrends = await context.Trends
                .Where(t => t.RecordedAt == today)
                .ToListAsync(cancellationToken);

            if (existingTrends.Any())
            {
                context.Trends.RemoveRange(existingTrends);
            }

            if (trends.Any())
            {
                context.Trends.AddRange(trends);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private IEnumerable<string> ExtractTags(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return Enumerable.Empty<string>();

            // استفاده از Regex برای دقت بیشتر در استخراج هشتگ‌ها (جلوگیری از جذب علائم نگارشی چسبیده به تگ)
            return Regex.Matches(content, @"#\w+")
                .Select(m => ((Match)m).Value.ToLowerInvariant())
                .Distinct();
        }
    }
}