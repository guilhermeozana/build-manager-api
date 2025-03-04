using Marelli.Business.Exceptions;
using Marelli.Business.Services;
using Marelli.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marelli.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class BuildTableController : ControllerBase
    {
        private readonly BuildTableRowsServices _buildTableRowsServices;


        public BuildTableController(BuildTableRowsServices buildTableRowsServices)
        {
            _buildTableRowsServices = buildTableRowsServices;
        }

        [HttpGet("BancoGetBuildTable")]
        public async Task<IActionResult> BancoGetBuildTable()
        {
            try
            {
                var retorno = await _buildTableRowsServices.ObtemBuildTable();
                return Ok(retorno);
            }
            catch (BusinessException bex)
            {
                return BadRequest(bex.BrokenRules?[0]);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }




        //[HttpGet]
        //public IActionResult GetBuildTable()
        //{
        //    var buildTableData = new List<BuildTableRow>
        //    {
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A",
        //            Disabled = true
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        },
        //        new BuildTableRow
        //        {
        //            Developer = "Carlos A. Santos",
        //            Date = "19 May, 2023 - 10:12 AM",
        //            Status = "Finished",
        //            Tag = "376_H_21442A"
        //        }
        //    };

        //    return Ok(buildTableData);
        //}
    }

    public class BuildTableRow
    {
        public string Developer { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string Tag { get; set; }
        public bool? Disabled { get; set; }
    }
}