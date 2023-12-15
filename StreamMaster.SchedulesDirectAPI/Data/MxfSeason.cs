﻿using System.Xml.Serialization;

namespace StreamMaster.SchedulesDirectAPI.Data;

public partial class SchedulesDirectData
{
    [XmlIgnore] public List<MxfSeason> SeasonsToProcess { get; set; } = [];

    private Dictionary<string, MxfSeason> _seasons = [];
    public MxfSeason FindOrCreateSeason(string seriesId, int seasonNumber, string protoTypicalProgram)
    {
        if (_seasons.TryGetValue($"{seriesId}_{seasonNumber}", out MxfSeason? season))
        {
            season.ProtoTypicalProgram ??= protoTypicalProgram;
            return season;
        }
        Seasons.Add(season = new MxfSeason(Seasons.Count + 1, FindOrCreateSeriesInfo(seriesId), seasonNumber, protoTypicalProgram));
        _seasons.Add($"{seriesId}_{seasonNumber}", season);
        SeasonsToProcess.Add(season);
        return season;
    }
}
