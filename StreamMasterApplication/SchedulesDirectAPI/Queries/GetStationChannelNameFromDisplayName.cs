﻿using StreamMaster.SchedulesDirectAPI.Domain.Models;

namespace StreamMasterApplication.SchedulesDirectAPI.Queries;

public record GetStationChannelNameFromDisplayName(string value) : IRequest<StationChannelName?>;

internal class GetStationChannelNameFromDisplayNameHandler(ILogger<GetStationChannelNameFromDisplayName> logger, ISchedulesDirect schedulesDirect, IRepositoryWrapper repository, IMapper mapper, ISettingsService settingsService, IPublisher publisher, ISender sender, IHubContext<StreamMasterHub, IStreamMasterHub> hubContext, IMemoryCache memoryCache) : BaseMediatorRequestHandler(logger, repository, mapper, settingsService, publisher, sender, hubContext, memoryCache), IRequestHandler<GetStationChannelNameFromDisplayName, StationChannelName?>
{
    public Task<StationChannelName?> Handle(GetStationChannelNameFromDisplayName request, CancellationToken cancellationToken)
    {
        List<StationChannelName> stationChannelNames = schedulesDirect.GetStationChannelNames();

        StationChannelName? check = stationChannelNames.FirstOrDefault(a => a.Channel == request.value);
        return Task.FromResult(check);
    }
}
