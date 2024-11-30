using Microsoft.Extensions.DependencyInjection;

using StreamMaster.SchedulesDirect.Converters;
using StreamMaster.SchedulesDirect.Data;
using StreamMaster.SchedulesDirect.Images;

namespace StreamMaster.SchedulesDirect.Services;

public static class ConfigureServices
{
    public static IServiceCollection AddSchedulesDirectAPIServices(this IServiceCollection services)
    {

        _ = services.AddSingleton<ISchedulesDirectAPIService, SchedulesDirectAPIService>();
        _ = services.AddSingleton<ISchedulesDirectDataService, SchedulesDirectDataService>();

        _ = services.AddScoped<ISDXMLTVBuilder, SDXMLTVBuilder>();

        _ = services.AddSingleton<IScheduleService, ScheduleService>();

        //_ = services.AddSingleton<IKeywords, Keywords>();

        _ = services.AddScoped<IDescriptionService, DescriptionService>();
        _ = services.AddScoped<ILineupService, LineupService>();
        _ = services.AddScoped<IProgramService, ProgramService>();
        _ = services.AddScoped<ISchedulesDirect, SchedulesDirect>();
        _ = services.AddScoped<IMovieImages, MovieImages>();
        _ = services.AddScoped<ISeriesImages, SeriesImages>();
        _ = services.AddScoped<ISeasonImages, SeasonImages>();
        _ = services.AddScoped<ISportsImages, SportsImages>();

        _ = services.AddTransient<IXmltvChannelBuilder, XmltvChannelBuilder>();
        _ = services.AddTransient<IXmltvProgramBuilder, XmltvProgramBuilder>();
        _ = services.AddTransient<IDataPreparationService, DataPreparationService>();

        return services;
    }
}