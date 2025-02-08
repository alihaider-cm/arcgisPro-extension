using ArcGIS.Desktop.Framework;
using ArcGisPlannerToolbox.Core.Contexts;
using ArcGisPlannerToolbox.Core.Contracts;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.Persistence.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using ArcGisPlannerToolbox.WPF.Services;
using ArcGisPlannerToolbox.WPF.ViewModels;
using ArcGisPlannerToolbox.WPF.Views;
using ArcGisPlannerToolbox.WPF.Views.Wizard;
using Autofac;
using System;
using System.Windows;

namespace ArcGisPlannerToolbox.WPF.Startup;

public static class App
{
    public static string Theme { get => FrameworkApplication.ApplicationTheme.ToString(); }
    public static IContainer Container { get; private set; }

    public static void RegisterDependencies()
    {
            Container = Bootstrap();
    }
    private static IContainer Bootstrap()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MediaDigitizerWindow>().AsSelf();
        builder.RegisterType<MediaDigitizerWindowViewModel>().AsSelf();

        builder.RegisterType<AreaDigitizerWindow>().AsSelf();
        builder.RegisterType<AreaDigitizerWindowViewModel>().AsSelf();

        builder.RegisterType<TourSplitterWindow>().AsSelf();
        builder.RegisterType<TourSplitterWindowViewModel>().AsSelf();

        builder.RegisterType<SubTourWindow>().AsSelf();
        builder.RegisterType<SubTourWindowViewModel>().AsSelf();

        builder.RegisterType<MultiBranchPlanAdvertisementAreaWizard>().AsSelf();
        builder.RegisterType<MultiBranchPlanAdvertisementAreaWizardViewModel>().AsSelf();

        builder.RegisterType<PlanAdvertisementAreaWizard>().AsSelf();
        builder.RegisterType<PlanAdvertisementAreaWizardViewModel>().AsSelf();

        builder.RegisterType<CustomerDataView>().AsSelf();
        builder.RegisterType<CustomerDataViewModel>().AsSelf();

        builder.RegisterType<AddBranchWindow>().AsSelf();
        builder.RegisterType<AddBranchWindowViewModel>().AsSelf();

        builder.RegisterType<PlanningLayerSelectionWindow>().AsSelf();
        builder.RegisterType<PlanningLayerSelectionWindowViewModel>().AsSelf();

        builder.RegisterType<DigitizedMediaStatusWindow>().AsSelf();
        builder.RegisterType<DigitizedMediaStatusWindowViewModel>().AsSelf();

        builder.RegisterType<ParticipatingBranchesViewModel>().AsSelf();
        builder.RegisterType<ParticipatingBranchesView1ViewModel>().AsSelf();
        builder.RegisterType<ParticipatingBranchesView2ViewModel>().AsSelf();
        builder.RegisterType<PreliminaryBranchAreasViewModel>().AsSelf();
        builder.RegisterType<DetailinformationenViewModel>().AsSelf();
        builder.RegisterType<PlanningStatisticsViewModel>().AsSelf();
        builder.RegisterType<DetailedPlanningViewModel>().AsSelf();
        builder.RegisterType<OptimizationViewModel>().AsSelf();
        builder.RegisterType<ExportViewModel>().AsSelf();
        builder.RegisterType<WizardControlViewModel>().AsSelf();
        builder.RegisterType<SecuredPlanningsViewModel>().AsSelf();
        builder.RegisterType<CustomerViewModel>().AsSelf();
        builder.RegisterType<BranchViewModel>().AsSelf();
        builder.RegisterType<AdvertiseNeighborBarnchesViewModel>().AsSelf();
        builder.RegisterType<MediaChoiceViewModel>().AsSelf();
        builder.RegisterType<MapSelectionViewModel>().AsSelf();
        builder.RegisterType<ConceptDataViewModel>().AsSelf();

        builder.RegisterType<WindowService>().As<IWindowService>().SingleInstance();
        builder.RegisterType<CursorService>().As<ICursorService>().SingleInstance();
        builder.RegisterType<MapManager>().As<IMapManager>().SingleInstance();
        builder.RegisterType<LayerOperator>().As<ILayerOperator>().SingleInstance();
        builder.RegisterType<MapOperator>().As<IMapOperator>().SingleInstance();

        builder.RegisterType<ToolboxDbContext>().As<IDbContext>().InstancePerLifetimeScope();
        //builder.RegisterType<ToolboxDbContext>().As<IDbContext>().InstancePerRequest();
        builder.RegisterType<MediaRepository>().As<IMediaRepository>().InstancePerDependency();
        builder.RegisterType<Repository<Media>>().As<IRepository<Media>>().InstancePerDependency();
        builder.RegisterType<AppearanceRhythmRepository>().As<IAppearanceRhythmRepository>().InstancePerDependency();
        builder.RegisterType<GebietsassistentRepository>().As<IGebietsassistentRepository>().InstancePerDependency();
        builder.RegisterType<MapViewMediaRepository>().As<IMapViewMediaRepository>().InstancePerDependency();
        builder.RegisterType<GeometryRepository>().As<IGeometryRepository>().InstancePerDependency();
        builder.RegisterType<GeometryRepositoryGebietsassistent>().As<IGeometryRepositoryGebietsassistent>().InstancePerDependency();
        builder.RegisterType<CustomerRepository>().As<ICustomerRepository>().InstancePerDependency();
        builder.RegisterType<PlanningRepository>().As<IPlanningRepository>().InstancePerDependency();
        builder.RegisterType<AnalysisRepository>().As<IAnalysisRepository>().InstancePerDependency();
        builder.RegisterType<AdvertisementAreaStatisticsRepository>().As<IAdvertisementAreaStatisticsRepository>().InstancePerDependency();
        builder.RegisterType<AdvertisementAreaRepository>().As<IAdvertisementAreaRepository>().InstancePerDependency();
        builder.RegisterType<AdvertisementAreaGeometryRepository>().As<IAdvertisementAreaGeometryRepository>().InstancePerDependency();

        return builder.Build();
    }
}
