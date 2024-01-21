using Kalbe.App.InternshipLogbookLogbook.Api.Models.Commons;
using Kalbe.App.InternshipLogbookLogbook.Api.Services;
using Kalbe.Library.Common.EntityFramework.Controllers;
using Kalbe.Library.Common.EntityFramework.Data;
using Kalbe.Library.Data.EntityFrameworkCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kalbe.App.InternshipLogbookLogbook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class InternshipLogbookLogbookController : SimpleBaseCrudController<Models.InternshipLogbookLogbook>
    {
        private readonly IInternshipLogbookLogbookService _internshipLogbookLogbookService;
        private readonly IDatabaseExceptionHandler _databaseExceptionHandler;
        private readonly IPDFGenerator _pdfGenerator;
        public InternshipLogbookLogbookController(IInternshipLogbookLogbookService internshipLogbookLogbookService, IDatabaseExceptionHandler databaseExceptionHandler, IPDFGenerator pdfGenerator)
            : base(internshipLogbookLogbookService, databaseExceptionHandler)
        {
            _internshipLogbookLogbookService = internshipLogbookLogbookService;
            _databaseExceptionHandler = databaseExceptionHandler;
            _pdfGenerator = pdfGenerator;
        }

        [HttpGet("GetLogbookData")]
        public async Task<IActionResult> GetLogbookData()
        {
            try
            {
                PagedOptions pagedOptions = PagedOptions.GetPagedOptions(base.Request);
                if (pagedOptions == null)
                {
                    return BadRequest("This request require pagination header parameter");
                }
                var result = await _internshipLogbookLogbookService.GetLogbookData(pagedOptions);
                return Ok(result);
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpPut("CalculateWorkType")]
        public async Task<IActionResult> CalculateWorkTypeAsync([FromBody] Models.InternshipLogbookLogbook data)
        {
            try
            {
                await _internshipLogbookLogbookService.CalculatedWorkTypeandAllowance(data);
                return Ok("Success");
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpPost("GeneratePDF")]
        public async Task<IActionResult> GeneratePDF([FromBody] Models.InternshipLogbookLogbook data)
        {
            try
            {
                var result = await _internshipLogbookLogbookService.GeneratePDF(data);
                return File(result, "application/pdf", "generated.pdf");

            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpGet("GetFilterByMonth")]
        public async Task<IActionResult> GetFilterMonth([FromBody] DateTime start, DateTime end)
        {
            try
            {
                var result = await _internshipLogbookLogbookService.GetFilterMonth(start, end);
                return Ok(result);
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpPost("SaveLogbook")]
        public async Task<IActionResult> SaveLogbook([FromBody] Models.InternshipLogbookLogbook logbook)
        {
            try
            {
                var result = await _internshipLogbookLogbookService.Save(logbook);
                return Ok(result);
            }
            catch (DbUpdateException ex2)
            {
                DbUpdateException ex = ex2;
                DatabaseExceptionType exceptionType = _databaseExceptionHandler.HandleException(ex);
                if (exceptionType == DatabaseExceptionType.UniqueDuplicate)
                {
                    return Conflict();
                }

                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] Models.InternshipLogbookLogbook data)
        {
            try
            {
                var result = await _internshipLogbookLogbookService.Submit(data);
                return Ok(result);
            }
            catch (DbUpdateException ex2)
            {
                DbUpdateException ex = ex2;
                DatabaseExceptionType exceptionType = _databaseExceptionHandler.HandleException(ex);
                if (exceptionType == DatabaseExceptionType.UniqueDuplicate)
                {
                    return Conflict();
                }

                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPut("Revise/{id}")]
        public async Task<IActionResult> Revise([FromRoute] long id, [FromBody] Comment comment)
        {
            try
            {
                await _internshipLogbookLogbookService.Revise(id, comment.Alasan);
                return Ok("Success");
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpPut("Approve/{id}")]
        public async Task<IActionResult> Approve([FromRoute] long id)
        {
            try
            {
                await _internshipLogbookLogbookService.Approve(id);
                return Ok("Success");
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }
        [HttpPost("UploadSign")]
        public async Task<IActionResult> UploadSign([FromForm] IFormFile file)
        {
            try
            {
                var result = await _internshipLogbookLogbookService.UploadSign(file);
                return Ok(result);
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }
        [HttpGet("PreviewSign")]
        public async Task<IActionResult> PreviewSign()
        {
            try
            {
                var result = await _internshipLogbookLogbookService.PreviewSign();
                return Ok(result);
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }

        [HttpGet("GetMentorTask")]
        public async Task<IActionResult> GetMentorTask()
        {
            try
            {
                PagedOptions pagedOptions = PagedOptions.GetPagedOptions(base.Request);
                if (pagedOptions == null)
                {
                    return BadRequest("This request require pagination header parameter");
                }
                var result = await _internshipLogbookLogbookService.GetMentorTask(pagedOptions);
                return Ok(result);
            }
            catch (Exception x)
            {
                return BadRequest(x.Message.ToString());
            }
        }
    }
}
