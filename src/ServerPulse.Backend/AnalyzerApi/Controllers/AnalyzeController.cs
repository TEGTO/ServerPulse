using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AnalyzerApi.Controllers
{
    [Route("analyze")]
    [ApiController]
    public class AnalyzeController : ControllerBase
    {
        private readonly IMapper mapper;

        public AnalyzeController(IMapper mapper)
        {
            this.mapper = mapper;
        }

    }
}