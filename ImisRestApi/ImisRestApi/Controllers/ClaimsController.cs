﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImisRestApi.Data;
using ImisRestApi.Models;
using ImisRestApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ImisRestApi.Controllers
{
    [Produces("application/json")]
    public class ClaimsController : Controller
    {
        private ImisClaims imisClaims;

        public ClaimsController(IConfiguration configuration)
        {
            imisClaims = new ImisClaims(configuration);
        }

        [HttpPost]
        [Route("api/GetDiagnosesServicesItems")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public IActionResult GetDiagnosesServicesItems([FromBody]DsiInputModel model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return BadRequest(new { error_occured = true, error_message = error });
            }
            var response = new DiagnosesServicesItems();
           // var response = imisClaims.GetDsi(model);
            return Json(response);
        }

        [HttpPost]
        [Route("api/GetPaymentLists")]
        [ProducesResponseType(typeof(void), 200)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        public IActionResult GetPaymentLists([FromBody]PaymentListsInputModel model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return BadRequest(new { error_occured = true, error_message = error });
            }

            var response = new PaymentLists();
           // var response = imisClaims.GetPaymentLists(model);
            return Json(response);
        }
    }
}