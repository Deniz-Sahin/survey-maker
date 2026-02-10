using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyMaker.Api.Application.Dtos;
using SurveyMaker.Api.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace SurveyMaker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveysController : ControllerBase
    {
        private readonly ISurveyService _svc;

        public SurveysController(ISurveyService svc)
        {
            _svc = svc;
        }


        // GET: api/surveys
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var items = await _svc.ListAsync();
            return Ok(items);
        }

        // GET: api/surveys/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var survey = await _svc.GetAsync(id);
            if (survey == null) return NotFound();
            return Ok(survey);
        }

        // POST: api/surveys
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSurveyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var id = await _svc.CreateAsync(dto);
                return CreatedAtAction(nameof(Get), new { id }, new { id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PUT: api/surveys/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSurveyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _svc.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // DELETE: api/surveys/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }

        // POST: api/surveys/{id}/responses
        [HttpPost("{id:int}/responses")]
        [Authorize]
        public async Task<IActionResult> Fill(int id, [FromBody] FillSurveyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var responseId = await _svc.FillAsync(id, userId, dto);
                return Ok(new FillResultDto { ResponseId = responseId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/surveys/{id}/submissions
        [HttpGet("{id:int}/submissions")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Submissions(int id)
        {
            var dto = await _svc.GetSubmissionsAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        // GET: api/surveys/assigned
        // Returns surveys assigned to the current user that the user has NOT submitted yet
        [HttpGet("assigned")]
        [Authorize]
        public async Task<IActionResult> Assigned()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var items = await _svc.ListAssignedPendingAsync(userId);
            return Ok(items);
        }
    }
}
