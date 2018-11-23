﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ImisRestApi.Chanels.Payment.Models;
using ImisRestApi.Chanels.Sms;
using ImisRestApi.Extensions;
using ImisRestApi.Logic;
using ImisRestApi.Models;
using ImisRestApi.Models.Payment;
using ImisRestApi.Models.Sms;
using ImisRestApi.Repo;
using ImisRestApi.Response;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ImisRestApi.Controllers
{
    public abstract class PaymentBaseController : Controller
    {
        public PaymentLogic _payment;
        public IConfiguration _configuration;
        public readonly IHostingEnvironment _hostingEnvironment;

        public PaymentBaseController(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
            _payment = new PaymentLogic(configuration, hostingEnvironment);
        }


        [HttpPost]
        [Route("api/MatchPayment")]
        public virtual async Task<IActionResult> MatchPayment([FromBody]MatchModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error_occured = true, error_message = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage });

            var response = await _payment.Match(model);           
           
            return Ok(response);
        }

        //Recieve Payment from Operator/
        [HttpPost]
        [Route("api/GetControlNumber")]
        public virtual async Task<IActionResult> GetControlNumber([FromBody]IntentOfPay intent)
        {
            if (!ModelState.IsValid)
            {           
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                var resp = await _payment.SaveIntent(intent, error.GetErrorNumber(), error.GetErrorMessage());
                return BadRequest(new { error_occured = true, error_message = error, control_number = resp.Data });
            }

            var response = await _payment.SaveIntent(intent);

            return Ok(new { code = response.Code, error_occured = response.ErrorOccured,error_message = response.MessageValue,control_number = response.Data});
        }

        [HttpPost]
        [Route("api/PostReqControlNumberAck")]
        public virtual IActionResult PostReqControlNumberAck([FromBody]Acknowledgement model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return BadRequest(new { error_occured = true, error_message = error });
            }

            var response = _payment.SaveAcknowledgement(model);

            return Ok(new { code = response.Code, error_occured = response.ErrorOccured, error_message = response.MessageValue, control_number = response.Data });
        }

        [HttpPost]
        [Route("api/GetReqControlNumber")]
        public virtual IActionResult GetReqControlNumber([FromBody]ControlNumberResp model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return BadRequest(new { error_occured = true, error_message = error });
            }

            var response = _payment.SaveControlNumber(model);
            return Ok(new { code = response.Code, error_occured = response.ErrorOccured, error_message = response.MessageValue });
        }

        [HttpPost]
        [Route("api/GetPaymentData")]
        public virtual IActionResult GetPaymentData([FromBody]PaymentData model)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;
                return BadRequest(new { error_occured = true, error_message = error });
            }

            var response = _payment.SavePayment(model);
            return Ok(new { code = response.Code, error_occured = response.ErrorOccured, error_message = response.MessageValue});
        }

       


    }
}
