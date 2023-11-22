using Kalbe.App.InternshipLogbookLogbook.Api.Services;
using Kalbe.Library.Common.EntityFramework.Controllers;
using Kalbe.Library.Data.EntityFrameworkCore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var result = await _internshipLogbookLogbookService.GeneratePDF(data);
            return File(result, "application/pdf", "generated.pdf");
        }
    }
}
