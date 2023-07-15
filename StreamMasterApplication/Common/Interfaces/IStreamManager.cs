﻿using StreamMasterApplication.Common.Models;

using StreamMasterDomain.Dto;

namespace StreamMasterApplication.Common.Interfaces;

public interface IStreamManager
{
    void Dispose();

    Task<IStreamInformation?> GetOrCreateBuffer(ChildVideoStreamDto childVideoStreamDto, int rank);

    SingleStreamStatisticsResult GetSingleStreamStatisticsResult(string streamUrl);
    IStreamInformation? GetStreamInformationFromStreamUrl(string streamUrl);
    ICollection<IStreamInformation> GetStreamInformations();

    IStreamInformation? Stop(string streamUrl);
}
