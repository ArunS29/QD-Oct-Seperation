using QD.ERP.Web.Models.DAL;
using QD.ERP.Web.Models.DALCommon;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QD.ERP.Web.Areas.Help.Controllers
{
    [Area("Help")] // Specify the area name
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HelpVideosController : ControllerBase
    {
        private readonly ERPCommonContext _dbContext;
        public HelpVideosController(ERPCommonContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public async Task<IActionResult> GetVideos(string formName="")
        {
            var videos = await _dbContext.TblHelpVideos
                .Where(v => v.FormName == formName)
                .ToListAsync();

            if (!videos.Any())
            {
                return NotFound("No videos found for the given FormId.");
            }

            return Ok(videos);
        }
    }
}
