﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImisRestApi.Chanels.Payment.Models
{
    [XmlRoot("Gepg")]
    public class GepgReconcMessage
    {
        public gepgSpReconcResp gepgSpReconcResp { get; set; }
        public string gepgSignature { get; set; }
    }

    [XmlRoot("Gepg")]
    public class GepgReconcAck
    {
        public gepgSpReconcReqAck gepgSpReconcReqAck { get; set; }
        public string gepgSignature { get; set; }
    }

    public class gepgSpReconcResp
    {
        public ReconcBatchInfo ReconcBatchInfo { get; set; }
        [XmlArray("ReconcTrans")]
        public List<ReconcTrxInf> ReconcTrxInf { get; set; }
    }

    public class ReconcTrxInf
    {
        public string SpBillId { get; set; }
        public string PayRefId { get; set; }
        public int BillCtrNum { get; set; }
        public double PaidAmt { get; set; }
        public string Ccy { get; set; }
        public DateTime TrxDtTm { get; set; }
        public string CtrAccNum { get; set; }
        public string UsdPayChnl { get; set; }
        public string DptCellNum { get; set; }
        public string DptName { get; set; }
        public string DptNameEmailAddr { get; set; }
        public string Remarks { get; set; }
        public string ReconcRsv01 { get; set; }
        public string ReconcRsv02 { get; set; }
        public string ReconcRsv03 { get; set; }
    }

    public class ReconcBatchInfo
    {
        public int SpReconcReqId { get; set; }
        public string SpCode { get; set; }
        public string SpName { get; set; }
        public string ReconcStsCode { get; set; }
    }

    [XmlRoot("Gepg")]
    public class ReconcRequest {
        public gepgSpReconcReq gepgSpReconcReq { get; set; }
        public string gepgSignature { get; set; }
    }

    public class gepgSpReconcReq
    {
        public int SpReconcReqId { get; set; }
        public string SpCode { get; set; }
        public string SpSysId { get; set; }
        public DateTime TnxDt { get; set; }
        public int ReconcOpt { get; set; }
    }

    public class gepgSpReconcReqAck
    {
        public int ReconcStsCode { get; set; }
    }
}
