﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ImisRestApi.Responses
{
    public class GetCoverageResponse : ImisApiResponse
    {
        public GetCoverageResponse(Exception error):base(error)
        {

        }
        public GetCoverageResponse(int value,bool error):base(value,error)
        {
            SetMessage(value);
        }
        public GetCoverageResponse(int value,bool error,DataTable data) : base(value,error,data)
        {
            var firstRow = data.Rows[0];
            var jsonString = JsonConvert.SerializeObject(data);
            var coverage_products = JsonConvert.DeserializeObject<List<CoverageProduct>>(jsonString);
            var _data = new { OtherNames = firstRow["CHFID"], LastNames = firstRow["InsureeName"],BirthDate = firstRow["DOB"],CoverageProducts = data};
            msg.Data = _data;
            SetMessage(value);
        }

        private void SetMessage(int value)
        {
            switch (value)
            {
                case 0:
                    msg.Code = value;
                    msg.MessageValue = "Success.";
                    Message = msg;
                    break;
                case 1:

                    msg.Code = value;
                    msg.MessageValue = "Wrong Format or Missing Insurance Number of insuree";
                    Message = msg;
                    break;
                case 2:
                    msg.Code = value;
                    msg.MessageValue = "Insurance number of Insuree not found";
                    Message = msg;
                    break;
            }
        }

        private class CoverageProduct
        {
            public string ProductCode { get; set; }
            public string ValuePolicy { get; set; }
            public string EffectiveDate { get; set; }
            public string ExpiryDate { get; set; }
        }
    }
}