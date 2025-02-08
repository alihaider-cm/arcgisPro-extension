using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using ArcGisPlannerToolbox.Core.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace ArcGisPlannerToolbox.WPF.Repositories.Contracts
{
    public interface IMapManager
    {
        List<FeatureLayer> GetDistributionAreas();
        Task LoadDistributionArea(List<Media> mediaList, bool issuesAsFeatureLayer = false);
        List<string> GetCoords();
        void SetVisiblityParameter(bool visibility);
        void SetZoomParameter(string zoomParameter);
        void SetCollapseParameter(bool collabsed);
        void SetTransparency(double transparency);
        bool DisributiontLayerIsSet();
        void SetAreaReloadability(bool reloadable);
        Task CreateNecessaryGroupLayer();
        Task ZoomToDesiredExtend();
        Task ZoomToBestFit();
        bool CheckIfMicroZipCodeLayerExists();
        bool CheckIfZipCodeLayerExists();
        Task SetMircoZipCodeLayer();
        Task<bool> CheckIfMicroZipCodeLayerHasDBConnection();
        Task<bool> CheckIfZipCodeLayerHasDBConnection();
        Task SetMicroZipCodeLayerVisiblity(bool isVisible);
        Task SetZipCodeLayerVisibility(bool isVisible);
        Task ConnectMicroZipCodeToDataBase();
        Task ConnectZipCodeToDataBase();
        Task ClearCurrentMicroZipCodeLayerSelection();
        Task SelectFeatureOnMicroZipCodeLayer(string microZipCode);
        Task DeselectFeatureOnMicroZipCodeLayer(string microZipCode);
        Task<List<string>> GetSelectedMicroZipCodeFromLayer(List<long> selectedFIDList);
        Task<List<string>> GetSelectedZipCodeFromLayer(List<long> selectedFIDList);
        Task SelectManyFeaturesOnMicroZipCodeLayer(List<string> microZipCodes);
        Task DeselectManyFeaturesOnMicroZipCodeLayer(List<string> microZipCodes);
        Task ClearCurrentZipCodeLayerSelection();
        Task SetZipCodeLayer();
        Task CreateMultiBranchPlanningLayer(List<PlanningDataArea> plannedAreas, string customer, string planningNumber, string branchNumber);
        Task SelectFeatureOnZipCodeLayer(string zipCode);
        Task DeselectFeatureOnZipCodeLayer(string zipCode);
        Task SelectManyFeaturesOnZipCodeLayer(string fieldName, List<string> zipCodes);
        Task DeselectManyFeaturesOnZipCodeLayer(string fieldName, List<string> zipCodes);
        Task DeSelectFeaturesOnPlanningLayer(string query);
        Task DeSelectFeaturesOnPlanningLayer();
        Task ZoomToZipCodeLayer();
        Task ZoomToMultiBranchLayer();
        Task ClearMapTreeView();
        Task CreateAdvertisementAreaLayer(List<AdvertisementAreaGeometry> advertisementAreaGeometries, string customerName, string customerBranchName);
        Task<List<string>> GetAllMediaNamesFromArcMapTreeView();
        Task<List<string>> GetMediaIdsFromCustomerAdvertisementAreas();
        Task CreatePlanningLayer(string conceptName, List<Core.Models.Geometry> polygons);
        Task SelectManyFeaturesOnPlanningLayer(string fieldName, List<string> occupancyUnitIds);
        Task SelectManyFeaturesOnPlanningLayer(string fieldName1, string fieldName2, List<Tuple<string, string>> occupancyUnitIds);
        Task<List<ExpandoObject>> GetSelectionDataFromConceptLayer(string query);
        Task<Selection> GetSelectionFromPlanningLayer();
    }
}
