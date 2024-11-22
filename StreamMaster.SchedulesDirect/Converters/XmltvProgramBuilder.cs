﻿using System.Globalization;
using System.Net;

using StreamMaster.Domain.Enums;
using StreamMaster.Domain.Helpers;
using StreamMaster.Domain.Models;

namespace StreamMaster.SchedulesDirect.Converters
{
    public class XmltvProgramBuilder(IOptionsMonitor<Setting> settingsMonitor, IOptionsMonitor<SDSettings> sdSettingsMonitor, ILogoService logoService)
    {
        private static readonly string[] TvRatings =
        [
            "", "TV-Y", "TV-Y7", "TV-G", "TV-PG", "TV-14", "TV-MA",
            "", "Kinder bis 12 Jahren", "Freigabe ab 12 Jahren", "Freigabe ab 16 Jahren", "Keine Jugendfreigabe",
            "", "Déconseillé aux moins de 10 ans", "Déconseillé aux moins de 12 ans", "Déconseillé aux moins de 16 ans", "Déconseillé aux moins de 18 ans",
            "모든 연령 시청가", "7세 이상 시청가", "12세 이상 시청가", "15세 이상 시청가", "19세 이상 시청가",
            "SKY-UC", "SKY-U", "SKY-PG", "SKY-12", "SKY-15", "SKY-18", "SKY-R18"
        ];

        private static readonly string[] MpaaRatings =
        [
            "", "G", "PG", "PG-13", "R", "NC-17", "X", "NR", "AO"
        ];

        //public XmltvProgramme BuildXmltvProgram(MxfScheduleEntry scheduleEntry)
        //{
        //    MxfProgram mxfProgram = scheduleEntry.mxfProgram;
        //    SDSettings sdSettings = sdSettingsMonitor.CurrentValue;

        //    XmltvProgramme programme = new()
        //    {
        //        Start = FormatDateTime(scheduleEntry.StartTime),
        //        Stop = FormatDateTime(scheduleEntry.StartTime.AddSeconds(scheduleEntry.Duration)),
        //        //Channel = scheduleEntry.mxfProgram.Id,
        //        Titles = ConvertToXmltvTextList(mxfProgram.Title),
        //        SubTitles = ConvertToXmltvTextList(mxfProgram.EpisodeTitle),
        //        Descriptions = ConvertToXmltvTextList(GetFullDescription(mxfProgram, scheduleEntry, sdSettings)),
        //        Credits = BuildProgramCredits(mxfProgram),
        //        Date = BuildProgramDate(mxfProgram),
        //        Categories = BuildProgramCategories(mxfProgram),
        //        Language = ConvertToXmltvText(mxfProgram.Language),
        //        Icons = BuildProgramIcons(mxfProgram),
        //        EpisodeNums = BuildEpisodeNumbers(scheduleEntry),
        //        Video = BuildProgramVideo(scheduleEntry),
        //        Audio = BuildProgramAudio(scheduleEntry),
        //        PreviouslyShown = BuildProgramPreviouslyShown(scheduleEntry),
        //        Premiere = BuildProgramPremiere(scheduleEntry),
        //        Live = scheduleEntry.IsLive ? string.Empty : null,
        //        New = !scheduleEntry.IsRepeat ? string.Empty : null,
        //        SubTitles2 = BuildProgramSubtitles(scheduleEntry),
        //        Rating = BuildProgramRatings(scheduleEntry),
        //        StarRating = BuildProgramStarRatings(mxfProgram)
        //    };

        //    return programme;
        //}

        //public static XmltvProgramme BuildXmltvProgram(StationChannelName stationChannelName, DateTime StartDT, int Duration)
        //{
        //    XmltvProgramme programme = new()
        //    {
        //        Start = FormatDateTime(StartDT),
        //        Stop = FormatDateTime(StartDT.AddSeconds(Duration)),
        //        Channel = stationChannelName.Id,
        //        Titles = ConvertToXmltvTextList(stationChannelName.ChannelName)
        //    };

        //    return programme;
        //}

        public XmltvProgramme BuildXmltvProgram(MxfScheduleEntry scheduleEntry, string channelId, int timeShift, string baseUrl)
        {
            MxfProgram mxfProgram = scheduleEntry.mxfProgram;
            SDSettings sdSettings = sdSettingsMonitor.CurrentValue;

            DateTime startDT = scheduleEntry.StartTime.AddMinutes(timeShift);

            XmltvProgramme programme = new()
            {
                Start = FormatDateTime(startDT),
                Stop = FormatDateTime(startDT.AddSeconds(scheduleEntry.Duration)),
                Channel = channelId,
                Titles = ConvertToXmltvTextList(mxfProgram.Title),
                SubTitles = ConvertToXmltvTextList(mxfProgram.EpisodeTitle),
                Descriptions = ConvertToXmltvTextList(GetFullDescription(mxfProgram, scheduleEntry, sdSettings)),
                Credits = BuildProgramCredits(mxfProgram),
                Date = BuildProgramDate(mxfProgram),
                //Categories = BuildProgramCategories(mxfProgram),
                Language = ConvertToXmltvText(mxfProgram.Language),
                Icons = BuildProgramIcons(mxfProgram, baseUrl),
                EpisodeNums = BuildEpisodeNumbers(scheduleEntry),
                Video = BuildProgramVideo(scheduleEntry),
                Audio = BuildProgramAudio(scheduleEntry),
                PreviouslyShown = BuildProgramPreviouslyShown(scheduleEntry),
                Premiere = BuildProgramPremiere(scheduleEntry),
                Live = scheduleEntry.IsLive ? string.Empty : null,
                New = !scheduleEntry.IsRepeat ? string.Empty : null,
                SubTitles2 = BuildProgramSubtitles(scheduleEntry),
                Rating = BuildProgramRatings(scheduleEntry),
                StarRating = BuildProgramStarRatings(mxfProgram)
            };

            return programme;
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss +0000", CultureInfo.InvariantCulture);
        }

        private static List<XmltvText>? ConvertToXmltvTextList(string? text)
        {
            return string.IsNullOrEmpty(text) ? null : [new XmltvText { Text = text }];
        }

        private static XmltvText? ConvertToXmltvText(string? text)
        {
            return string.IsNullOrEmpty(text) ? null : new XmltvText { Text = text };
        }

        private static string GetFullDescription(MxfProgram mxfProgram, MxfScheduleEntry scheduleEntry, SDSettings sdSettings)
        {
            string extendedDescription = sdSettings.XmltvExtendedInfoInTitleDescriptions
                ? GetExtendedDescription(mxfProgram, scheduleEntry, sdSettings)
                : string.Empty;

            return (extendedDescription + mxfProgram.Description).Trim();
        }

        private static string GetExtendedDescription(MxfProgram mxfProgram, MxfScheduleEntry scheduleEntry, SDSettings sdSettings)
        {
            string descriptionExtended = string.Empty;

            if (mxfProgram.IsMovie && mxfProgram.Year > 0)
            {
                descriptionExtended = $"{mxfProgram.Year}";
            }
            else if (!mxfProgram.IsMovie)
            {
                if (scheduleEntry.IsLive)
                {
                    descriptionExtended = "[LIVE]";
                }
                else if (scheduleEntry.IsPremiere)
                {
                    descriptionExtended = "[PREMIERE]";
                }
                else if (scheduleEntry.IsFinale)
                {
                    descriptionExtended = "[FINALE]";
                }
                else if (!scheduleEntry.IsRepeat)
                {
                    descriptionExtended = "[NEW]";
                }
                else if (scheduleEntry.IsRepeat && !mxfProgram.IsGeneric)
                {
                    descriptionExtended = "[REPEAT]";
                }

                if (!sdSettings.PrefixEpisodeTitle && !sdSettings.PrefixEpisodeDescription && !sdSettings.AppendEpisodeDesc)
                {
                    if (mxfProgram.SeasonNumber > 0 && mxfProgram.EpisodeNumber > 0)
                    {
                        descriptionExtended += $" S{mxfProgram.SeasonNumber}:E{mxfProgram.EpisodeNumber}";
                    }
                    else if (mxfProgram.EpisodeNumber > 0)
                    {
                        descriptionExtended += $" #{mxfProgram.EpisodeNumber}";
                    }
                }
            }

            if (!string.IsNullOrEmpty(TvRatings[scheduleEntry.TvRating]))
            {
                descriptionExtended += $" {TvRatings[scheduleEntry.TvRating]}";
                if (mxfProgram.MpaaRating > 0)
                {
                    descriptionExtended += ",";
                }
            }
            if (mxfProgram.MpaaRating > 0)
            {
                descriptionExtended += $" {MpaaRatings[mxfProgram.MpaaRating]}";
            }

            string advisories = GetAdvisories(mxfProgram);

            if (!string.IsNullOrEmpty(advisories))
            {
                descriptionExtended += $" ({advisories.Trim().TrimEnd(',').Replace(",", ", ")})";
            }

            if (mxfProgram.IsMovie && mxfProgram.HalfStars > 0)
            {
                descriptionExtended += $" {mxfProgram.HalfStars * 0.5:N1}/4.0";
            }
            else if (!mxfProgram.IsMovie)
            {
                if (!mxfProgram.IsGeneric && !string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
                {
                    descriptionExtended += $" Original air date: {DateTime.Parse(mxfProgram.OriginalAirdate):d}";
                }
            }

            if (!string.IsNullOrEmpty(descriptionExtended))
            {
                descriptionExtended = descriptionExtended.Trim() + "\r\n";
            }

            return descriptionExtended;
        }

        private static string GetAdvisories(MxfProgram mxfProgram)
        {
            string advisories = string.Empty;
            if (mxfProgram.HasAdult)
            {
                advisories += "Adult Situations,";
            }

            if (mxfProgram.HasGraphicLanguage)
            {
                advisories += "Graphic Language,";
            }
            else if (mxfProgram.HasLanguage)
            {
                advisories += "Language,";
            }

            if (mxfProgram.HasStrongSexualContent)
            {
                advisories += "Strong Sexual Content,";
            }

            if (mxfProgram.HasGraphicViolence)
            {
                advisories += "Graphic Violence,";
            }
            else if (mxfProgram.HasMildViolence)
            {
                advisories += "Mild Violence,";
            }
            else if (mxfProgram.HasViolence)
            {
                advisories += "Violence,";
            }

            if (mxfProgram.HasNudity)
            {
                advisories += "Nudity,";
            }
            else if (mxfProgram.HasBriefNudity)
            {
                advisories += "Brief Nudity,";
            }

            if (mxfProgram.HasRape)
            {
                advisories += "Rape,";
            }

            return advisories;
        }

        private static XmltvCredit? BuildProgramCredits(MxfProgram mxfProgram)
        {
            bool hasCredits = mxfProgram.DirectorRole?.Count > 0 ||
                              mxfProgram.ActorRole?.Count > 0 ||
                              mxfProgram.WriterRole?.Count > 0 ||
                              mxfProgram.ProducerRole?.Count > 0 ||
                              mxfProgram.HostRole?.Count > 0 ||
                              mxfProgram.GuestActorRole?.Count > 0;

            return !hasCredits
                ? null
                : new XmltvCredit
                {
                    Directors = MxfPersonRankToXmltvCrew(mxfProgram.DirectorRole),
                    Actors = MxfPersonRankToXmltvActors(mxfProgram.ActorRole),
                    Writers = MxfPersonRankToXmltvCrew(mxfProgram.WriterRole),
                    Producers = MxfPersonRankToXmltvCrew(mxfProgram.ProducerRole),
                    Presenters = MxfPersonRankToXmltvCrew(mxfProgram.HostRole),
                    Guests = MxfPersonRankToXmltvCrew(mxfProgram.GuestActorRole)
                };
        }

        private static List<string>? MxfPersonRankToXmltvCrew(List<MxfPersonRank>? mxfPersons)
        {
            return mxfPersons?.Where(person => person.Name is not null).Select(person => person.Name!).ToList();
        }

        private static List<XmltvActor>? MxfPersonRankToXmltvActors(List<MxfPersonRank>? mxfPersons)
        {
            return mxfPersons?.Where(a => a.Name is not null).Select(person => new XmltvActor { Actor = person.Name!, Role = person.Character }).ToList();
        }

        private static string? BuildProgramDate(MxfProgram mxfProgram)
        {
            return mxfProgram.IsMovie && mxfProgram.Year > 0
                ? mxfProgram.Year.ToString()
                : !string.IsNullOrEmpty(mxfProgram.OriginalAirdate) ? DateTime.Parse(mxfProgram.OriginalAirdate).ToString("yyyyMMdd") : null;
        }

        //private List<XmltvText>? BuildProgramCategories(MxfProgram mxfProgram)
        //{
        //    if (string.IsNullOrEmpty(mxfProgram.Keywords))
        //    {
        //        return null;
        //    }

        //    List<string> categories = [];

        //    foreach (string keywordId in mxfProgram.Keywords.Split(','))
        //    {
        //        if (keywordDict.TryGetValue(keywordId, out string? word))
        //        {
        //            categories.Add(word);
        //        }
        //    }

        //    if (categories.Remove("Movies"))
        //    {
        //        if (!categories.Contains("Movie"))
        //        {
        //            categories.Add("Movie");
        //        }
        //    }

        //    // Remove "Kids" if "Children" is also present
        //    if (categories.Contains("Children"))
        //    {
        //        _ = categories.Remove("Kids");
        //    }

        //    return categories.Count == 0 ? null : categories.ConvertAll(category => new XmltvText { Text = category });
        //}

        private List<XmltvIcon>? BuildProgramIcons(MxfProgram mxfProgram, string baseUrl)
        {
            SDSettings sdSettings = sdSettingsMonitor.CurrentValue;

            if (sdSettings.XmltvSingleImage || !mxfProgram.Extras.ContainsKey("artwork"))
            {
                // Use the first available image URL
                string? url = mxfProgram.mxfGuideImage?.ImageUrl ??
                              mxfProgram.mxfSeason?.mxfGuideImage?.ImageUrl ??
                              mxfProgram.mxfSeriesInfo?.MxfGuideImage?.ImageUrl;

                return url != null ?
                [
                    new XmltvIcon
                    {
                        Src =GetIconUrl(EPGHelper.SchedulesDirectId, url, baseUrl),
                        // Height and Width can be set if available
                    }
                ] : null;
            }

            // Retrieve artwork from the program, season, or series info
            List<ProgramArtwork>? artwork = mxfProgram.Extras.GetValueOrDefault("artwork") as List<ProgramArtwork> ??
                                            mxfProgram.mxfSeason?.Extras.GetValueOrDefault("artwork") as List<ProgramArtwork> ??
                                            mxfProgram.mxfSeriesInfo?.Extras.GetValueOrDefault("artwork") as List<ProgramArtwork>;

            if (artwork == null)
            {
                return null;
            }

            // Convert artwork to XmltvIcon list
            return artwork.ConvertAll(image => new XmltvIcon
            {
                Src = GetIconUrl(EPGHelper.SchedulesDirectId, image.Uri, baseUrl),
                Height = image.Height,
                Width = image.Width
            });
        }

        public string GetIconUrl(int EPGNumber, string iconOriginalSource, string _baseUrl)//, SMFileTypes sMFileType)
        {

            if (EPGNumber == EPGHelper.CustomPlayListId)
            {
                return GetApiUrl(SMFileTypes.CustomPlayList, iconOriginalSource, _baseUrl);
            }

            if (EPGNumber == EPGHelper.DummyId)
            {
                return iconOriginalSource;
            }

            if (EPGNumber == EPGHelper.SchedulesDirectId)
            {
                ImagePath? imagePath = logoService.GetValidImagePath(iconOriginalSource, SMFileTypes.SDImage, false);

                if (imagePath != null)
                {
                    string retIcon = settingsMonitor.CurrentValue.LogoCache.EqualsIgnoreCase("cache") ? GetApiUrl(imagePath.SMFileType, iconOriginalSource, _baseUrl) : iconOriginalSource;
                    return retIcon;
                }

                return iconOriginalSource.StartsWith("http") ? iconOriginalSource : GetApiUrl(SMFileTypes.SDStationLogo, iconOriginalSource, _baseUrl);
            }


            if (string.IsNullOrEmpty(iconOriginalSource))
            {
                return $"{_baseUrl}{settingsMonitor.CurrentValue.DefaultLogo}";
            }

            string originalUrl = iconOriginalSource;

            if (iconOriginalSource.StartsWith('/'))
            {
                iconOriginalSource = iconOriginalSource[1..];
            }


            SMFileTypes smtype = SMFileTypes.Logo;
            //ImagePath? imagePath = logoService.GetValidImagePath(iconOriginalSource);

            //if (imagePath != null)
            //{
            //    smtype = imagePath.SMFileType;
            //}



            string icon = settingsMonitor.CurrentValue.LogoCache.Equals("cache", StringComparison.CurrentCultureIgnoreCase) ? GetApiUrl(smtype, originalUrl, _baseUrl) : iconOriginalSource;

            return icon;
        }

        private static string GetApiUrl(SMFileTypes path, string source, string? _baseUrl)
        {
            return $"{_baseUrl}/api/files/{(int)path}/{WebUtility.UrlEncode(source)}";
        }

        private List<XmltvEpisodeNum>? BuildEpisodeNumbers(MxfScheduleEntry scheduleEntry)
        {
            List<XmltvEpisodeNum> list = [];
            MxfProgram mxfProgram = scheduleEntry.mxfProgram;

            if (mxfProgram.EpisodeNumber != 0 || scheduleEntry.Part != 0)
            {
                string seasonPart = mxfProgram.SeasonNumber > 0 ? (mxfProgram.SeasonNumber - 1).ToString() : "";
                string episodePart = mxfProgram.EpisodeNumber > 0 ? (mxfProgram.EpisodeNumber - 1).ToString() : "";
                string part = scheduleEntry.Part > 0 ? $"{scheduleEntry.Part - 1}/" : "0/";
                string parts = scheduleEntry.Parts > 0 ? scheduleEntry.Parts.ToString() : "1";
                string text = $"{seasonPart}.{episodePart}.{part}{parts}";

                list.Add(new XmltvEpisodeNum { System = "xmltv_ns", Text = text });
            }
            else if (mxfProgram.EPGNumber is EPGHelper.DummyId or EPGHelper.CustomPlayListId)
            {
                list.Add(new XmltvEpisodeNum { System = "original-air-date", Text = $"{scheduleEntry.StartTime.ToLocalTime():yyyy-MM-dd}" });
            }
            else if (!mxfProgram.ProgramId.StartsWith("MV"))
            {
                if (string.IsNullOrEmpty(mxfProgram.OriginalAirdate))
                {
                    list.Add(new XmltvEpisodeNum { System = "original-air-date", Text = $"{scheduleEntry.StartTime.ToLocalTime():yyyy-MM-dd}" });
                }
                else
                {
                    string oad = !scheduleEntry.IsRepeat
                        ? $"{scheduleEntry.StartTime.ToLocalTime():yyyy-MM-dd}"
                        : $"{DateTime.Parse(mxfProgram.OriginalAirdate):yyyy-MM-dd}";

                    list.Add(new XmltvEpisodeNum
                    {
                        System = "original-air-date",
                        Text = oad
                    });
                }
            }

            //if (mxfProgram.Series != null)
            //{
            //    if (int.TryParse(mxfProgram.Series[2..], out int seriesIndex) &&
            //        seriesDict.TryGetValue(seriesIndex - 1, out SeriesInfo? mxfSeriesInfo) &&
            //        mxfSeriesInfo.Extras.TryGetValue("tvdb", out dynamic? value))
            //    {
            //        list.Add(new XmltvEpisodeNum { System = "thetvdb.com", Text = $"series/{value}" });
            //    }
            //}

            return list.Count > 0 ? list : null;
        }

        private static XmltvVideo? BuildProgramVideo(MxfScheduleEntry scheduleEntry)
        {
            return scheduleEntry.IsHdtv ? new XmltvVideo { Quality = "HDTV" } : null;
        }

        private static XmltvAudio? BuildProgramAudio(MxfScheduleEntry scheduleEntry)
        {
            if (scheduleEntry.AudioFormat <= 0)
            {
                return null;
            }

            string format = scheduleEntry.AudioFormat switch
            {
                1 => "mono",
                2 => "stereo",
                3 => "dolby",
                4 => "dolby digital",
                5 => "surround",
                _ => string.Empty
            };

            return !string.IsNullOrEmpty(format) ? new XmltvAudio { Stereo = format } : null;
        }

        private static XmltvPreviouslyShown? BuildProgramPreviouslyShown(MxfScheduleEntry scheduleEntry)
        {
            return scheduleEntry.IsRepeat && !scheduleEntry.mxfProgram.IsMovie ? new XmltvPreviouslyShown { Text = string.Empty } : null;
        }

        private static XmltvText? BuildProgramPremiere(MxfScheduleEntry scheduleEntry)
        {
            if (!scheduleEntry.IsPremiere)
            {
                return null;
            }

            MxfProgram mxfProgram = scheduleEntry.mxfProgram;
            string text = mxfProgram.IsMovie
                ? "Movie Premiere"
                : mxfProgram.IsSeriesPremiere ? "Series Premiere" : mxfProgram.IsSeasonPremiere ? "Season Premiere" : "Miniseries Premiere";
            return new XmltvText { Text = text };
        }

        private static List<XmltvSubtitles>? BuildProgramSubtitles(MxfScheduleEntry scheduleEntry)
        {
            List<XmltvSubtitles> list = [];

            if (scheduleEntry.IsCc)
            {
                list.Add(new XmltvSubtitles { Type = "teletext" });
            }
            if (scheduleEntry.IsSubtitled)
            {
                list.Add(new XmltvSubtitles { Type = "onscreen" });
            }
            if (scheduleEntry.IsSigned)
            {
                list.Add(new XmltvSubtitles { Type = "deaf-signed" });
            }

            return list.Count > 0 ? list : null;
        }

        private static List<XmltvRating> BuildProgramRatings(MxfScheduleEntry scheduleEntry)
        {
            List<XmltvRating> list = [];
            MxfProgram mxfProgram = scheduleEntry.mxfProgram;

            // Add advisories
            AddProgramRatingAdvisory(mxfProgram.HasAdult, list, "Adult Situations");
            AddProgramRatingAdvisory(mxfProgram.HasBriefNudity, list, "Brief Nudity");
            AddProgramRatingAdvisory(mxfProgram.HasGraphicLanguage, list, "Graphic Language");
            AddProgramRatingAdvisory(mxfProgram.HasGraphicViolence, list, "Graphic Violence");
            AddProgramRatingAdvisory(mxfProgram.HasLanguage, list, "Language");
            AddProgramRatingAdvisory(mxfProgram.HasMildViolence, list, "Mild Violence");
            AddProgramRatingAdvisory(mxfProgram.HasNudity, list, "Nudity");
            AddProgramRatingAdvisory(mxfProgram.HasRape, list, "Rape");
            AddProgramRatingAdvisory(mxfProgram.HasStrongSexualContent, list, "Strong Sexual Content");
            AddProgramRatingAdvisory(mxfProgram.HasViolence, list, "Violence");

            // Add ratings
            AddProgramRating(scheduleEntry, list);

            return list;
        }

        private static void AddProgramRatingAdvisory(bool hasAdvisory, List<XmltvRating> list, string advisory)
        {
            if (hasAdvisory)
            {
                list.Add(new XmltvRating { System = "advisory", Value = advisory });
            }
        }

        private static void AddProgramRating(MxfScheduleEntry scheduleEntry, List<XmltvRating> list)
        {
            HashSet<string> ratingsSet = [];

            if (scheduleEntry.extras.TryGetValue("ratings", out dynamic? value))
            {
                foreach (KeyValuePair<string, string> rating in (Dictionary<string, string>)value)
                {
                    if (ratingsSet.Add(rating.Key))
                    {
                        list.Add(new XmltvRating { System = rating.Key, Value = rating.Value });
                    }
                }
            }

            if (scheduleEntry.mxfProgram.Extras.TryGetValue("ratings", out dynamic? valueRatings))
            {
                foreach (KeyValuePair<string, string> rating in (Dictionary<string, string>)valueRatings)
                {
                    if (ratingsSet.Add(rating.Key))
                    {
                        list.Add(new XmltvRating { System = rating.Key, Value = rating.Value });
                    }
                }
            }

            if (scheduleEntry.TvRating != 0)
            {
                string rating = TvRatings[scheduleEntry.TvRating];
                if (!string.IsNullOrEmpty(rating))
                {
                    list.Add(new XmltvRating { System = "VCHIP", Value = rating });
                }
            }
        }

        private static List<XmltvRating>? BuildProgramStarRatings(MxfProgram mxfProgram)
        {
            return mxfProgram.HalfStars == 0
                ? null
                :
                [
                    new XmltvRating { Value = $"{mxfProgram.HalfStars * 0.5:N1}/4" }
                ];
        }
    }
}