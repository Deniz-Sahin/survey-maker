using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SurveyMaker.Api.Application.Services;
using SurveyMaker.Api.Application.Dtos;
using System.Threading.Tasks;
using System;

namespace SurveyMaker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _svc;

    public QuestionsController(IQuestionService svc)
    {
        _svc = svc;
    }

    // GET: api/questions
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List()
    {
        var items = await _svc.ListAsync();
        return Ok(items);
    }

    // GET: api/questions/{id}
    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _svc.GetAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    // POST: api/questions
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        try
        {
            var id = await _svc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("options", ex.Message);
            return BadRequest(ModelState);
        }
    }

    // PUT: api/questions/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionDto dto)
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
            ModelState.AddModelError("options", ex.Message);
            return BadRequest(ModelState);
        }
    }

    // DELETE: api/questions/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _svc.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}