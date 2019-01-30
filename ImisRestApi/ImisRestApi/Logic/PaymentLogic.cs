﻿using ImisRestApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImisRestApi.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using ImisRestApi.Models.Payment;
using ImisRestApi.Responses;
using ImisRestApi.Models.Sms;
using ImisRestApi.Chanels.Sms;
using Newtonsoft.Json;
using ImisRestApi.Models.Payment.Response;

namespace ImisRestApi.Logic
{
    public class PaymentLogic
    {

        private IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PaymentLogic(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
           
        }
        public async Task<DataMessage> SaveIntent(IntentOfPay intent,int? errorNumber = 0,string errorMessage = null)
        {

            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment);
            var intentResponse = payment.SaveIntent(intent,errorNumber,errorMessage);

            DataMessage return_message = new DataMessage();
            return_message.Code = intentResponse.Code;
            return_message.MessageValue = intentResponse.MessageValue;

            if (intentResponse.Code == 0)
            {
                var objstring = intentResponse.Data.ToString();
                List<AssignedControlNumber> data = JsonConvert.DeserializeObject<List<AssignedControlNumber>>(objstring);
                var ret_data = data.FirstOrDefault();

                
                var response = payment.PostReqControlNumber(intent.enrolment_officer_code, payment.PaymentId, intent.phone_number, payment.ExpectedAmount, intent.policies);             
                if (response.ControlNumber != null)
                {
                    var controlNumberExists = payment.CheckControlNumber(payment.PaymentId,response.ControlNumber);
                    return_message = payment.SaveControlNumber(response.ControlNumber, controlNumberExists);
                    if (payment.PaymentId != null)
                    {
                        if (!return_message.ErrorOccured && !controlNumberExists)
                        {
                            ret_data.control_number = response.ControlNumber;
                            ControlNumberAssignedSms(payment);
                        }
                        else
                        {
                            ControlNumberNotassignedSms(payment, return_message.MessageValue);
                        }
                    }                    
                }
                else if (response.Posted == true)
                {
                    return_message = payment.SaveControlNumberAkn(!response.ErrorOccured,response.ErrorMessage);
                }
                else if (response.ErrorOccured == true)
                {
                    return_message = payment.SaveControlNumberAkn(!response.ErrorOccured, response.ErrorMessage);
                    ControlNumberNotassignedSms(payment,response.ErrorMessage);
                   
                }

                return_message.Data = ret_data;

            }
            else
            {
                return_message = intentResponse;
                return_message.Data = new AssignedControlNumber();
            }
            
            return return_message;
        }

        public async Task<DataMessage> MatchPayment(MatchModel model)
        {
            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment);
            var response = payment.MatchPayment(model);

            if (model.internal_identifier == null) {
                List<MatchSms> PaymentIds = payment.GetPaymentIdsForSms();

                if(PaymentIds != null)
                    SendMatchSms(PaymentIds);
            }         

            return response;
        }

        

        public DataMessage SaveAcknowledgement(Acknowledgement model)
        {
            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment) { PaymentId = model.internal_identifier};
            var response = payment.SaveControlNumberAkn(model.error_occured, model.error_message);

            return response;
        }

        public DataMessage SavePayment(PaymentData model)
        {
            
            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment);
            //var controlNumberExists = payment.CheckControlNumber(model.internal_identifier, model.control_number);
            var response = payment.SavePayment(model);

            if(payment.PaymentId != null && model.enrolment_officer_code == null && !response.ErrorOccured)
            {
                var ackResponse = payment.GetPaymentDataAck(payment.PaymentId,payment.ControlNum);

                MatchModel matchModel = new MatchModel() { internal_identifier = payment.PaymentId, audit_user_id = -3 };
                var matchresponse = MatchPayment(matchModel);
                SendPaymentSms(payment);
            }

            return response;
        }      

        public DataMessage SaveControlNumber(ControlNumberResp model)
        {
            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment);
            var controlNumberExists = payment.CheckControlNumber(model.internal_identifier, model.control_number);
            var response = payment.SaveControlNumber(model,controlNumberExists);

            if (payment.PaymentId != null)
            {
                var ackResponse = payment.GetReqControlNumberAck(payment.PaymentId);

                if (!response.ErrorOccured && !controlNumberExists)
                {
                    ControlNumberAssignedSms(payment);
                }
                else
                {
                    ControlNumberNotassignedSms(payment, response.MessageValue);
                }
            }
           
            return response;
        }

        public async void ControlNumberAssignedSms(ImisPayment payment)
        {

            ImisSms sms = new ImisSms(_configuration, _hostingEnvironment);
            var txtmsgTemplate = string.Empty;
            string othersCount = string.Empty;

            if (payment.InsureeProducts.Count > 1)
            {
                txtmsgTemplate = sms.GetMessage("ControlNumberAssignedV2");
                othersCount = Convert.ToString(payment.InsureeProducts.Count - 1);
            }
            else
            {
                txtmsgTemplate = sms.GetMessage("ControlNumberAssigned");
            }
            var txtmsg = string.Format(txtmsgTemplate,
                payment.ControlNum,
                DateTime.UtcNow.ToLongDateString(),
                DateTime.UtcNow.ToLongTimeString(),
                payment.InsureeProducts.FirstOrDefault().InsureeNumber,
                payment.InsureeProducts.FirstOrDefault().InsureeName,
                payment.InsureeProducts.FirstOrDefault().ProductCode,
                payment.InsureeProducts.FirstOrDefault().ProductName,
                payment.ExpectedAmount,
                othersCount);

            List<SmsContainer> message = new List<SmsContainer>();
            message.Add(new SmsContainer() { Message = txtmsg, Recipient = payment.PhoneNumber });

            var fileName = "CnAssigned_" + payment.PhoneNumber;

            string test = await sms.SendSMS(message,fileName);
        }

        public async Task<DataMessage> GetControlNumbers(PaymentRequest requests)
        {
            ImisPayment payment = new ImisPayment(_configuration, _hostingEnvironment);
            var PaymentIds = requests.requests.Select(x => x.internal_identifier).ToArray();

            var PaymentIds_string = string.Join(",", PaymentIds);

            var response = await payment.GetControlNumbers(PaymentIds_string);

            return response;
        }

        public async void ControlNumberNotassignedSms(ImisPayment payment,string error)
        {
            ImisSms sms = new ImisSms(_configuration, _hostingEnvironment);
            var txtmsgTemplate = string.Empty;
            string othersCount = string.Empty;

            if (payment.InsureeProducts.Count > 1)
            {
                txtmsgTemplate = sms.GetMessage("ControlNumberErrorV2");
                othersCount = Convert.ToString(payment.InsureeProducts.Count - 1);
            }
            else
            {
                txtmsgTemplate = sms.GetMessage("ControlNumberError");
            }

            var txtmsg = string.Format(txtmsgTemplate,
                payment.ControlNum,
                DateTime.UtcNow.ToLongDateString(),
                DateTime.UtcNow.ToLongTimeString(),
                payment.InsureeProducts.FirstOrDefault().InsureeNumber,
                payment.InsureeProducts.FirstOrDefault().ProductCode,
                error,
                othersCount);

            List<SmsContainer> message = new List<SmsContainer>();
            message.Add(new SmsContainer() { Message = txtmsg, Recipient = payment.PhoneNumber });

            var fileName = "CnError_" + payment.PhoneNumber;

            string test = await sms.SendSMS(message,fileName);
        }

        public async void SendPaymentSms(ImisPayment payment)
        {
            ImisSms sms = new ImisSms(_configuration, _hostingEnvironment);
            List<SmsContainer> message = new List<SmsContainer>();
            var familyproduct = payment.InsureeProducts.FirstOrDefault();

            if (familyproduct.PolicyActivated)
            {
                
                var txtmsg = string.Format(sms.GetMessage("PaidAndActivated"),
                    payment.PaidAmount,
                    DateTime.UtcNow.ToLongDateString(),
                    payment.ControlNum,
                    familyproduct.InsureeNumber,
                    familyproduct.InsureeName,
                    familyproduct.ProductCode,
                    familyproduct.ProductName,
                    familyproduct.EffectiveDate.Value.ToShortDateString(),
                    familyproduct.ExpiryDate.Value.ToShortDateString(),
                    payment.PaidAmount);

                
                message.Add(new SmsContainer() { Message = txtmsg, Recipient = payment.PhoneNumber });

            }
            else
            {

                var txtmsg = string.Format(sms.GetMessage("PaidAndNotActivated"),
                    payment.PaidAmount,
                    DateTime.UtcNow.ToLongDateString(),
                    payment.ControlNum,
                    familyproduct.InsureeNumber,
                    familyproduct.InsureeName,
                    familyproduct.ProductCode,
                    familyproduct.ProductName,
                    payment.ExpectedAmount,
                    payment.OutStAmount);

                message.Add(new SmsContainer() { Message = txtmsg, Recipient = payment.PhoneNumber });

            }
            var fileName = "PayStatSms_" + payment.PhoneNumber;

            string test = await sms.SendSMS(message,fileName);
            payment.MatchedSmsSent();
        }

        public async void SendMatchSms(ImisPayment payment)
        {
            ImisSms sms = new ImisSms(_configuration, _hostingEnvironment);
            List<SmsContainer> message = new List<SmsContainer>();

            var txtmsgTemplate = string.Empty;
            string othersCount = string.Empty;

            if (payment.InsureeProducts.Count > 1)
            {
                txtmsgTemplate = sms.GetMessage("PaidAndNotMatchedV2");
                othersCount = Convert.ToString(payment.InsureeProducts.Count - 1);
            }
            else
            {
                txtmsgTemplate = sms.GetMessage("PaidAndNotMatched");
            }
            var familyproduct = payment.InsureeProducts.FirstOrDefault();
            var txtmsg = string.Format(txtmsgTemplate,
                     payment.PaidAmount,
                     DateTime.UtcNow.ToLongDateString(),
                     payment.ControlNum,
                     familyproduct.InsureeNumber,
                     familyproduct.InsureeName,
                     familyproduct.ProductCode,
                     familyproduct.ProductName,
                     othersCount);


            message.Add(new SmsContainer() { Message = txtmsg, Recipient = payment.PhoneNumber });

            var fileName = "PayNotMatched_" + payment.PhoneNumber;

            string test = await sms.SendSMS(message, fileName);
        }

        public async void SendMatchSms(List<MatchSms> Ids)
        {
            ImisSms sms = new ImisSms(_configuration, _hostingEnvironment);
            List<SmsContainer> message = new List<SmsContainer>();

            foreach (var m in Ids)
            {
                bool shoulSendSms = LocalDefault.ShouldSendSms(_configuration, m.DateLastSms, m.MatchedDate);

                if (shoulSendSms)
                {
                    var txtmsgTemplate = string.Empty;
                    string othersCount = string.Empty;

                    ImisPayment _pay = new ImisPayment(_configuration, _hostingEnvironment);
                    _pay.GetPaymentInfo(m.PaymentId.ToString());

                    if (_pay.PaymentId != null)
                    {
                        if (_pay.InsureeProducts.Count > 1)
                        {
                            txtmsgTemplate = sms.GetMessage("PaidAndNotMatchedV2");
                            othersCount = Convert.ToString(_pay.InsureeProducts.Count - 1);
                        }
                        else
                        {
                            txtmsgTemplate = sms.GetMessage("PaidAndNotMatched");
                        }
                        var familyproduct = _pay.InsureeProducts.FirstOrDefault();
                        var txtmsg = string.Format(txtmsgTemplate,
                                 _pay.PaidAmount,
                                 DateTime.UtcNow.ToLongDateString(),
                                 _pay.ControlNum,
                                 familyproduct.InsureeNumber,
                                 familyproduct.InsureeName,
                                 familyproduct.ProductCode,
                                 familyproduct.ProductName,
                                 othersCount);


                        message.Add(new SmsContainer() { Message = txtmsg, Recipient = _pay.PhoneNumber });
                        _pay.UnMatchedSmsSent(m.PaymentId);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                
            }
            var fileName = "PayNotMatched_";

            string test = await sms.SendSMS(message, fileName);
        }
    }
}
