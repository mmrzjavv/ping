using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Application.Trends.Queries.GetTrends;

namespace TwitterClone.WebUI.Controllers
{
    public class TrendsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<TrendDto>> Get([FromQuery] GetTrendsQuery query)
        {
            return await Mediator.Send(query);
        }
    }
}