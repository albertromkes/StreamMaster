﻿using System.Text.Json.Serialization;
using System.Xml;

namespace StreamMaster.SchedulesDirectAPI.Domain.Models;

public class EPGJsonCache
{
    [JsonPropertyName("jsonEntry")]
    public string? JsonEntry { get; set; }

    [JsonPropertyName("images")]
    public string? Images { get; set; }

    [JsonIgnore]
    public bool Current { get; set; }
}

