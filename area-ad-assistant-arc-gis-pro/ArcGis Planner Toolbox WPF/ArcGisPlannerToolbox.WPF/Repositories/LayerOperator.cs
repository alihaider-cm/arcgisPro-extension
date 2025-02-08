using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.DDL;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGisPlannerToolbox.Core.Models;
using ArcGisPlannerToolbox.WPF.Constants;
using ArcGisPlannerToolbox.WPF.Helpers;
using ArcGisPlannerToolbox.WPF.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Geometry = ArcGisPlannerToolbox.Core.Models.Geometry;

namespace ArcGisPlannerToolbox.WPF.Repositories;

public class LayerOperator : ILayerOperator
{
    #region Fields

    private readonly IMapOperator _mapOperator;

    #endregion

    #region Initialization

    public LayerOperator(IMapOperator mapOperator)
    {
        _mapOperator = mapOperator;
    }

    #endregion

    #region public methods

    public List<FeatureLayer> GetAllFeatureLayersOfSubGroupLayer(CompositeLayer groupLayer)
    {
        var featureLayers = new List<FeatureLayer>();
        if (groupLayer is not null)
        {
            var subGroupLayerList = GetAllSubGroupLayersOfGroupLayer(groupLayer).Result;
            foreach (CompositeLayer layer in subGroupLayerList)
                foreach (FeatureLayer layerLayer in layer.Layers)
                    featureLayers.Add(layerLayer);
        }
        return featureLayers;
    }
    public async Task<List<CompositeLayer>> GetAllSubGroupLayersOfGroupLayer(CompositeLayer groupLayer)
    {
        return await Task.Run<List<CompositeLayer>>(() =>
        {
            var compositLayers = new List<CompositeLayer>();
            if (groupLayer is not null)
                foreach (CompositeLayer layer in groupLayer.Layers)
                    compositLayers.Add(layer);

            return compositLayers;
        });
    }
    public async Task<FeatureClass> GetFeatureClassFromSQLPolygons(List<Geometry> geometries, string fileName, string mediaName)
    {
        var featureClass = await CreateFeatureClassFromMemoryDatabase(fileName, MapParams.FeatureClass.Columns);
        await QueuedTask.Run(() =>
        {
            int rowIndex = 0;
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                var cursor = featureClass.CreateInsertCursor();
                foreach (var geometry in geometries)
                {
                    var row = featureClass.CreateRowBuffer();
                    row["ID"] = geometry.Id;
                    row["Medium_ID"] = geometry.MediaId;
                    row["BBE_ID"] = geometry.OccupancyUnitId;
                    row["Tour_ID"] = geometry.TourId;
                    row["Tour_Nr"] = geometry.TourNumber;
                    row["Tour"] = geometry.TourName;
                    row["Auflage_Info"] = geometry.NumberOfCopiesInfo;
                    row["Auflage"] = geometry.NumberOfCopies;
                    row["HH_Brutto"] = geometry.GrossHouseHolds;
                    row["Medium"] = geometry.MediaName;
                    row["Datenquelle"] = geometry.DataSource;
                    row["Ausgabe"] = geometry.Issue;
                    row["Ausgabe_Nr"] = geometry.IssueNumber;
                    row["Datenstand"] = geometry.DataStatus;
                    row["generiert_am"] = geometry.CreationDate;
                    row["Anzahl_Geometrien"] = geometry.NumberOfGeometries;
                    row["Fehlerhafte_Geometrien"] = geometry.NumberOfFaultyGeometries;
                    row["Geometrie"] = geometry.GeographyString;
                    row["Belegungseinheit"] = geometry.OccupancyUnit;
                    row["Bereinigt_am"] = geometry.CleaningDate;
                    row["Erscheintage"] = geometry.Appearance;
                    cursor.Insert(row);
                }
                cursor.Flush();
                var rows = featureClass.Search(null, false);
                while (rows.MoveNext())
                {
                    var feature = rows.Current as Feature;
                    if (rowIndex >= geometries.Count)
                        continue;

                    var geometry = GeometryEngine.Instance.ImportFromWKB(WkbImportFlags.WkbImportNonTrusted, geometries[rowIndex].Geom, SpatialReferences.WGS84);
                    feature.SetShape(geometry);
                    feature.Store();
                    context.Invalidate(feature);
                    rowIndex++;
                }
            }, featureClass);
            editOperation.Execute();
        });
        return featureClass;
    }
    public async Task<FeatureLayer> CreateFeatureLayer(GroupLayer groupLayer, FeatureClass featureClass)
    {
        return await QueuedTask.Run<FeatureLayer>(() =>
        {
            var color = GenerateRandomColor();
            var featureLayerCreationParam = new FeatureLayerCreationParams(featureClass);
            featureLayerCreationParam.RendererDefinition = new SimpleRendererDefinition()
            {
                SymbolTemplate = SymbolFactory.Instance.ConstructPolygonSymbol(color).MakeSymbolReference()
            };
            return LayerFactory.Instance.CreateLayer<FeatureLayer>(featureLayerCreationParam, groupLayer);
        });
    }
    public async Task<FeatureClass> GetFeatureClassFromSQLPolygons(Geometry geometry, string fileName, string mediaName)
    {
        var columns = new Dictionary<string, string>() { { "FID", "int" }, { "Shape", "string" }, { "Id", "int" }, { "Auflage", "int" } };
        var featureClass = await CreateFeatureClassFromMemoryDatabase(fileName, columns);

        await QueuedTask.Run(() =>
        {
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                var cursor = featureClass.CreateInsertCursor();
                var row = featureClass.CreateRowBuffer();
                row["FID"] = 0;
                row["Shape"] = "Polygon";
                row["Id"] = 0;
                row["Auflage"] = 0;
                cursor.Insert(row);
                cursor.Flush();
                var rows = featureClass.Search(null, false);
                if (rows.MoveNext())
                {
                    var feature = rows.Current as Feature;
                    var shape = GeometryEngine.Instance.ImportFromWKB(WkbImportFlags.WkbImportNonTrusted, geometry.Geom, SpatialReferences.WGS84); ;
                    feature.SetShape(shape);
                    feature.Store();
                    context.Invalidate(feature);
                }
            }, featureClass);
            editOperation.Execute();
        });
        return featureClass;
    }
    public async Task<FeatureClass> GetFeatureClassFromPlanning(List<PlanningDataArea> planningDataAreas, string fileName)
    {
        var featureClass = await CreateFeatureClassFromMemoryDatabase(fileName, MapParams.ShapeFile.Columns);
        await QueuedTask.Run(() =>
        {
            int rowIndex = 0;
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                var cursor = featureClass.CreateInsertCursor();
                foreach (var planningData in planningDataAreas)
                {
                    var row = featureClass.CreateRowBuffer();
                    row["Planungs_ID"] = planningData.Planungs_ID;
                    row["Planungs_Nr"] = planningData.Planungs_Nr;
                    row["Filial_Nr"] = planningData.Filial_Nr;
                    row["Geokey"] = planningData.Geokey;
                    row["Geokey_Name"] = planningData.Geokey_Name;
                    row["HH_Brutto"] = planningData.HH_Brutto;
                    row["HH_Netto"] = planningData.HH_Netto;
                    //row["Geom"] = planningData.Geom;
                    cursor.Insert(row);
                }
                cursor.Flush();
                var rows = featureClass.Search(null, false);
                while (rows.MoveNext())
                {
                    var feature = rows.Current as Feature;
                    if (rowIndex >= planningDataAreas.Count)
                        continue;

                    var geometry = GeometryEngine.Instance.ImportFromWKB(WkbImportFlags.WkbImportNonTrusted, planningDataAreas[rowIndex].Geom, SpatialReferences.WGS84);
                    feature.SetShape(geometry);
                    feature.Store();
                    context.Invalidate(feature);
                    rowIndex++;
                }
            }, featureClass);
            editOperation.Execute();
        });
        return featureClass;
    }
    public async Task<FeatureClass> GetFeatureClassFromAdvertisementAreaGeometry(List<AdvertisementAreaGeometry> advertisementAreaGeometries, string fileName)
    {
        var featureClass = await CreateFeatureClassFromMemoryDatabase(fileName, MapParams.FeatureClass.AdvertisementArea);
        await QueuedTask.Run(() =>
        {
            int rowIndex = 0;
            EditOperation editOperation = new EditOperation();
            editOperation.Callback(context =>
            {
                var cursor = featureClass.CreateInsertCursor();
                foreach (var planningData in advertisementAreaGeometries)
                {
                    var row = featureClass.CreateRowBuffer();
                    row["Werbegebiets_nr"] = planningData.Werbegebiets_nr;
                    row["Medium_id"] = planningData.Medium_id;
                    row["Stand"] = planningData.Stand;
                    row["Datenstand"] = planningData.Datenstand;
                    row["Anzahl_geoschluessel"] = planningData.Anzahl_geoschluessel;
                    row["Generiert_am"] = planningData.Generiert_am;
                    row["Generiert_in_ms"] = planningData.Generiert_in_ms;
                    row["Aktives_gebiet_kunden_id"] = planningData.Aktives_gebiet_kunden_id;
                    row["Aktives_gebiet_reihenfolge"] = planningData.Generiert_in_ms;
                    row["Aktives_gebiet_style_id"] = planningData.Aktives_gebiet_style_id;
                    row["HH_brutto"] = planningData.HH_brutto;
                    row["Ew"] = planningData.Ew;
                    row["KK_id"] = planningData.KK_idx;
                    row["Berechnungsmethode"] = planningData.Berechnungsmethode;
                    row["Geom_is_valid"] = planningData.Geom_is_valid;
                    cursor.Insert(row);
                }
                cursor.Flush();
                var rows = featureClass.Search(null, false);
                while (rows.MoveNext())
                {
                    var feature = rows.Current as Feature;
                    if (rowIndex >= advertisementAreaGeometries.Count)
                        continue;

                    var geometry = GeometryEngine.Instance.ImportFromWKB(WkbImportFlags.WkbImportNonTrusted, advertisementAreaGeometries[rowIndex].Geom, SpatialReferences.WGS84);
                    feature.SetShape(geometry);
                    feature.Store();
                    context.Invalidate(feature);
                    rowIndex++;
                }
            }, featureClass);
            editOperation.Execute();
        });
        return featureClass;
    }
    public async Task<bool> CheckIfLayerHasActiveDataBaseConnection(FeatureLayer featureLayer)
    {
        if (featureLayer is null)
            return false;

        return await QueuedTask.Run<bool>(() =>
        {
            var table = featureLayer.GetTable();
            return table.IsJoinedTable();
        });
    }
    public async Task ClearLayerSelection(FeatureLayer featureLayer)
    {
        await QueuedTask.Run(() =>
        {
            try
            {
                featureLayer?.ClearSelection();
            }
            catch (Exception)
            {
            }
        });
    }
    public async Task SelectMicroZipCode(FeatureLayer featureLayer, string microZipCode)
    {
        if (featureLayer is not null && microZipCode is not null)
        {
            await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = $"mPLZ='{microZipCode}'" };
                featureLayer.Select(filter, SelectionCombinationMethod.Add);
            });
        }
    }
    public async Task SelectMicroZipCode(FeatureLayer featureLayer, List<string> microZipCodes)
    {
        if (featureLayer is not null)
        {
            await QueuedTask.Run(() =>
            {
                var query = string.Empty;
                query = string.Join("', '", microZipCodes);
                query = query.Insert(0, "'");
                query += "'";
                query = $"mPLZ in ({query})";
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Add);
            });
        }
    }
    public async Task DeselectMicroZipCode(FeatureLayer featureLayer, string microZipCode)
    {
        if (featureLayer is not null && microZipCode is not null)
        {
            await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = $"mPLZ='{microZipCode}'" };
                featureLayer.Select(filter, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public async Task DeselectMicroZipCode(FeatureLayer featureLayer, List<string> microZipCodes)
    {
        if (featureLayer is not null)
        {
            await QueuedTask.Run(() =>
            {
                var query = string.Empty;
                query = string.Join("', '", microZipCodes);
                query = query.Insert(0, "'");
                query += "'";
                query = $"mPLZ in ({query})";
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public async Task<FeatureLayer> ConnectLayerToDataBase(FeatureLayer featureLayer, string tableName, string targetFieldName, string joinFieldName, string featureLayerName)
    {
        return await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run<FeatureLayer>(async () =>
        {
            var featureLayerTable = featureLayer.GetTable();

            // Opening a Non-Versioned SQL Server instance.
            DatabaseConnectionProperties connectionProperties = new DatabaseConnectionProperties(EnterpriseDatabaseType.SQLServer)
            {
                AuthenticationMode = AuthenticationMode.OSA, // Nach Ablöse des sgdb1 muss das Join Problem über sessions gelöst werden
                //User = @"Gebietsassistent",
                //Password = MapParams.Database.Password,
                Instance = @"sgdb1",
                Database = "GEBIETSASSISTENT",
                Version = "dbo.DEFAULT"
            };
            using (Geodatabase geodatabase = new Geodatabase(connectionProperties))
            {
                // Use the geodatabase
                var tableDefinition = geodatabase.GetDefinition<TableDefinition>(tableName);
                var table = geodatabase.OpenDataset<Table>(tableName);

                Field originPrimaryKey = featureLayerTable.GetDefinition().GetFields().FirstOrDefault(field => field.Name.Equals(targetFieldName));
                Field destinationForeignKey = tableDefinition.GetFields().FirstOrDefault(field => field.Name.Equals(joinFieldName));

                var description = new VirtualRelationshipClassDescription(
                  originPrimaryKey, destinationForeignKey, RelationshipCardinality.OneToMany);

                RelationshipClass relationshipClass = featureLayerTable.RelateTo(table, description);
                JoinDescription joinDescription = new JoinDescription(relationshipClass)
                {
                    JoinDirection = JoinDirection.Forward,
                    JoinType = JoinType.InnerJoin,
                    TargetFields = featureLayerTable.GetDefinition().GetFields(),
                };
                Join join = new Join(joinDescription);
                using (Table joinedTable = join.GetJoinedTable())
                {
                    var LayerName = joinedTable.GetName();
                    if (joinedTable is FeatureClass featureClass)
                    {
                        var compositeLayer = (GroupLayer)_mapOperator.GetGroupLayerByName(MapView.Active.Map, MapParams.GroupLayers.Datenebenen);
                        await _mapOperator.RemoveFeatureLayerFromGroup(compositeLayer, featureLayerName);
                        try
                        {
                            var featureLayerCreationParams = new FeatureLayerCreationParams(featureClass) { Name = featureLayerName }; 
                            
                            return LayerFactory.Instance.CreateLayer<FeatureLayer>(featureLayerCreationParams, compositeLayer);
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                }
            }
            return null;
        });
    }
    public async Task SetRenderer(FeatureLayer featureLayer, CIMColor color,
        FeatureRendererTarget featureRendererTarget = FeatureRendererTarget.Default,
        SimpleFillStyle fillStyle = SimpleFillStyle.Solid,
        bool drawOutline = false,
        double outlineStroke = 2.0,
        SimpleLineStyle outlineStyle = SimpleLineStyle.Solid)
    {
        await QueuedTask.Run(() =>
        {
            var renderer = featureLayer.GetRenderer() as CIMSimpleRenderer;
            if (drawOutline)
            {
                CIMStroke outline = SymbolFactory.Instance.ConstructStroke(color, outlineStroke, outlineStyle);
                renderer.Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(color, fillStyle, outline).MakeSymbolReference();
            }
            else
                renderer.Symbol = SymbolFactory.Instance.ConstructPolygonSymbol(color, fillStyle).MakeSymbolReference();

            featureLayer.SetRenderer(renderer, featureRendererTarget);
        });
    }

    public async Task<List<string>> GetSelectedFeatureValuesByFieldName(FeatureLayer featureLayer, string fieldName, List<long> selectedFIDList)
    {
        if (featureLayer is not null)
        {
            return await QueuedTask.Run<List<string>>(() =>
            {
                var values = new List<string>();
                foreach (var fID in selectedFIDList)
                {
                    var rowCursor = featureLayer.Search(new QueryFilter() { WhereClause = $"{featureLayer.Name}.FID={fID}" });
                    if (rowCursor.MoveNext())
                    {
                        var row = rowCursor.Current;
                        var fieldIndex = row.FindField(fieldName);
                        var fieldValue = row[fieldIndex];
                        values.Add(fieldValue.ToString());
                    }
                }
                return values;
            });
        }
        return Enumerable.Empty<string>().ToList();
    }

    public async Task<List<ExpandoObject>> GetSelectedFeatureValuesByFieldName(FeatureLayer featureLayer, string query)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(query))
        {
            return await QueuedTask.Run<List<ExpandoObject>>(() =>
            {
                var values = new List<ExpandoObject>();
                var rowCursor = featureLayer.Search(new QueryFilter() { WhereClause = query });
                while (rowCursor.MoveNext())
                {
                    var expando = new ExpandoObject();
                    var row = rowCursor.Current;
                    var fields = row.GetFields();
                    for (int i = 0; i < fields.Count; i++)
                    {
                        var field = fields[i];
                        var fieldIndex = row.FindField(field.AliasName);
                        var fieldValue = row[fieldIndex];
                        expando.TryAdd(field.AliasName, fieldValue);
                    }
                    values.Add(expando);
                }
                return values;
            });
        }
        return Enumerable.Empty<ExpandoObject>().ToList();
    }
    public async Task<Selection> GetSelectionFromLayer(FeatureLayer featureLayer)
    {
        return await QueuedTask.Run<Selection>(() => featureLayer.GetSelection());
    }
    public async Task<CIMUniqueValueRenderer> CreateUniqueValueRenderer(FeatureLayer featureLayer, List<string> fields)
    {
        return await QueuedTask.Run<CIMUniqueValueRenderer>(() =>
        {
            //var fields = new List<string> { field }; //field to be used to retrieve unique values
            var symbol = SymbolFactory.Instance.ConstructPolygonSymbol(GenerateRandomColor());
            CIMSymbolReference symbolPointTemplate = symbol.MakeSymbolReference();
            UniqueValueRendererDefinition uniqueValueRendererDef = new UniqueValueRendererDefinition(fields, symbolPointTemplate);
            CIMUniqueValueRenderer uniqueValueRenderer = featureLayer.CreateRenderer(uniqueValueRendererDef) as CIMUniqueValueRenderer;
            return uniqueValueRenderer;
        });
    }
    public async Task SetRenderer(FeatureLayer featureLayer, CIMUniqueValueRenderer uniqueValueRenderer)
    {
        await QueuedTask.Run(() => featureLayer.SetRenderer(uniqueValueRenderer));
    }
    public async Task SelectFeature(FeatureLayer featureLayer, string fieldName, string value)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(value))
        {
            await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = $"{fieldName}='{value}'" };
                featureLayer.Select(filter, SelectionCombinationMethod.Add);
            });
        }
    }
    public async Task SelectFeatures(FeatureLayer featureLayer, string fieldName, List<string> features)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(fieldName) && features.Count > 0)
        {
            await QueuedTask.Run(() =>
            {
                var query = string.Empty;
                query = string.Join("', '", features);
                query = query.Insert(0, "'");
                query += "'";
                query = $"{fieldName} in ({query})";
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Add);
            });
        }
    }
    public async Task SelectFeatures(FeatureLayer featureLayer, string fieldName1, string fieldName2, List<Tuple<string, string>> features)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(fieldName1) && !string.IsNullOrWhiteSpace(fieldName2) && features.Count > 0)
        {
            await QueuedTask.Run(() =>
            {
                var query = string.Empty;
                query = string.Join("', '", features);
                query = query.Insert(0, "'");
                query += "'";
                query = $"{fieldName1} in ({query})";
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Add);
            });
        }
    }
    public async Task DeselectFeature(FeatureLayer featureLayer, string fieldName, string value)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(fieldName) && !string.IsNullOrWhiteSpace(value))
        {
            await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = $"{fieldName}='{value}'" };
                featureLayer.Select(filter, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public async Task DeselectFeatures(FeatureLayer featureLayer, string fieldName, List<string> features)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(fieldName) && features.Count > 0)
        {
            await QueuedTask.Run(() =>
            {
                var query = string.Empty;
                query = string.Join("', '", features);
                query = query.Insert(0, "'");
                query += "'";
                query = $"{fieldName} in ({query})";
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public async Task DeselectFeatures(FeatureLayer featureLayer, string query)
    {
        if (featureLayer is not null && !string.IsNullOrWhiteSpace(query))
        {
            await QueuedTask.Run(() =>
            {
                ArcGIS.Core.Data.QueryFilter filter = new ArcGIS.Core.Data.QueryFilter() { WhereClause = query };
                featureLayer.Select(filter, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public async Task DeselectFeatures(FeatureLayer featureLayer)
    {
        if (featureLayer is not null)
        {
            await QueuedTask.Run(() =>
            {
                featureLayer.Select(null, SelectionCombinationMethod.Subtract);
            });
        }
    }
    public List<FeatureLayer> GetAllFeatureLayers(CompositeLayer groupLayer)
    {
        var featureLayers = new List<FeatureLayer>();
        if (groupLayer is not null)
        {
            if (groupLayer.Layers.Count > 0)
            {
                foreach (FeatureLayer layer in groupLayer.Layers)
                    featureLayers.Add(layer);
            }
        }

        return featureLayers;
    }
    public async Task<List<string>> GetFieldValuesByName(FeatureLayer featureLayer, string fieldName)
    {
        return await QueuedTask.Run<List<string>>(() =>
        {
            List<string> values = new();
            var rowCursor = featureLayer.Search();
            while (rowCursor.MoveNext())
            {
                var row = rowCursor.Current;
                var fieldIndex = row.FindField(fieldName);
                var fieldValue = row[fieldIndex];
                values.Add(fieldValue.ToString());
            }
            return values;
        });
    }
    public async Task<FeatureClass> AddFeaturesToFeatureClass(FeatureClass featureClass, List<Geometry> geometries)
    {
        await QueuedTask.Run(() =>
        {
            int rowIndex = 0;
            EditOperation editOperation = new EditOperation();
            featureClass.DeleteRows(new QueryFilter());
            editOperation.Callback(context =>
            {

                var cursor = featureClass.CreateInsertCursor();
                foreach (var geometry in geometries)
                {
                    var row = featureClass.CreateRowBuffer();
                    row["ID"] = geometry.Id;
                    row["Medium_ID"] = geometry.MediaId;
                    row["BBE_ID"] = geometry.OccupancyUnitId;
                    row["Tour_ID"] = geometry.TourId;
                    row["Tour_Nr"] = geometry.TourNumber;
                    row["Tour"] = geometry.TourName;
                    row["Auflage_Info"] = geometry.NumberOfCopiesInfo;
                    row["Auflage"] = geometry.NumberOfCopies;
                    row["HH_Brutto"] = geometry.GrossHouseHolds;
                    row["Medium"] = geometry.MediaName;
                    row["Datenquelle"] = geometry.DataSource;
                    row["Ausgabe"] = geometry.Issue;
                    row["Ausgabe_Nr"] = geometry.IssueNumber;
                    row["Datenstand"] = geometry.DataStatus;
                    row["generiert_am"] = geometry.CreationDate;
                    row["Anzahl_Geometrien"] = geometry.NumberOfGeometries;
                    row["Fehlerhafte_Geometrien"] = geometry.NumberOfFaultyGeometries;
                    row["Geometrie"] = geometry.GeographyString;
                    row["Belegungseinheit"] = geometry.OccupancyUnit;
                    row["Bereinigt_am"] = geometry.CleaningDate;
                    row["Erscheintage"] = geometry.Appearance;
                    cursor.Insert(row);
                }
                cursor.Flush();
                var rows = featureClass.Search(null, false);
                while (rows.MoveNext())
                {
                    var feature = rows.Current as Feature;
                    if (rowIndex >= geometries.Count)
                        continue;

                    var geometry = GeometryEngine.Instance.ImportFromWKB(WkbImportFlags.WkbImportNonTrusted, geometries[rowIndex].Geom, SpatialReferences.WGS84);
                    feature.SetShape(geometry);
                    feature.Store();
                    context.Invalidate(feature);
                    rowIndex++;
                }
            }, featureClass);
            editOperation.Execute();
        });
        return featureClass;
    }
    public async Task<FeatureClass> CreateFeatureClassFromMemoryDatabase(string fileName, Dictionary<string, string> fields)
    {
        try
        {
            var fieldDescriptionList = TypeHelper.DictionaryToFieldDescriptionList(fields);
            FeatureClassDescription featureClassDescription = new FeatureClassDescription(fileName, fieldDescriptionList,
            new ShapeDescription(GeometryType.Polygon, SpatialReferences.WGS84));
            var memoryConnectionProperties = new MemoryConnectionProperties();

            return await QueuedTask.Run<FeatureClass>(() =>
            {
                var geoDatabase = new Geodatabase(memoryConnectionProperties);
                SchemaBuilder schemaBuilder = new SchemaBuilder(geoDatabase);
                var featureClassToken = schemaBuilder.Create(featureClassDescription);
                if (!schemaBuilder.Build())
                {
                    var errors = schemaBuilder.ErrorMessages;
                    foreach (var error in errors)
                        foreach (var errorContext in error.Split("\n"))
                            if (errorContext.Contains("Error:"))
                                return geoDatabase.OpenDataset<FeatureClass>(fileName);
                }
                return geoDatabase.OpenDataset<FeatureClass>(fileName);
            });
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    #endregion

    #region private methods

    private CIMColor GenerateRandomColor()
    {
        var red = Random.Shared.Next(0, 255);
        var green = Random.Shared.Next(0, 255);
        var blue = Random.Shared.Next(0, 255);
        return CIMColor.CreateRGBColor(red, green, blue);
    }

    #endregion

}