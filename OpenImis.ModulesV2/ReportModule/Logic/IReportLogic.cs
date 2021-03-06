﻿using OpenImis.ModulesV2.ReportModule.Models;
using System;

namespace OpenImis.ModulesV2.ReportModule.Logic
{
    public interface IReportLogic
    {
        FeedbackReportModel GetFeedbackStats(ReportRequestModel feedbackRequestModel, string officerCode);
        RenewalReportModel GetRenewalStats(ReportRequestModel renewalRequestModel, string officerCode);
        EnrolmentReportModel GetEnrolmentStats(ReportRequestModel enrolmentRequestModel, string officerCode);
        SnapshotResponseModel GetSnapshotIndicators(SnapshotRequestModel snapshotRequestModel, string officerCode);
        CumulativeIndicatorsResponseModel GetCumulativeIndicators(IndicatorRequestModel cumulativeIndicatorsRequestModel, string officerCode);
    }
}
