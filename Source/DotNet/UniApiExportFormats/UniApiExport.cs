namespace UniApiExportFormats;

using DigitaleDelta;
using Microsoft.Spatial;
using Observation = DatabaseModel.Models.Observation;
using Reference = DatabaseModel.Models.Reference;

public class UniApiExport : IUniApiExport<DigitaleDelta.Observation>
{
	public async Task<List<DigitaleDelta.Observation>> GenerateExportDataAsync(List<Observation>? observations, Dictionary<long, Reference> references)
	{
		if (observations == null || observations.Count == 0)
		{
			return new List<DigitaleDelta.Observation>();
		}

		var result                  = new List<DigitaleDelta.Observation>(observations.Count);
		var referencesByExternalKey = references.Values.ToDictionary(r => r.ExternalKey, r => r);
		result.AddRange(observations.Select(observation => DatabaseObservationToExportObservation(observation, references, referencesByExternalKey)));

		return await Task.FromResult(result);
	}

	private static GeometryPoint? WktToGeometryPoint(string? wkt)
	{
		GeometryPoint? point = null;
		if (string.IsNullOrEmpty(wkt))
		{
			return point;
		}

		var geoPoint = new NetTopologySuite.IO.WKTReader().Read(wkt);
		point = GeometryPoint.Create(CoordinateSystem.Geometry(4258), geoPoint.Coordinate.X, geoPoint.Coordinate.Y, null, null);
		return point;
	}
	
	private static DigitaleDelta.Observation DatabaseObservationToExportObservation(Observation databaseObservation, Dictionary<long, Reference> references, Dictionary<Guid, Reference> referencesByExternalKey)
	{
		var generatedRelatedObservations = new List<DigitaleDelta.Observation>();
		if (databaseObservation.ResultGeometry != null)
		{
			var geoObservation = new DigitaleDelta.Observation
			{
				Id             = $"{databaseObservation.Id}-geo",
				Type           = "geometry",
				ResultTime     = databaseObservation.ResultTime,
				ValidTime      = databaseObservation.ValidTimeStart,
				PhenomenonTime = databaseObservation.PhenomenonStart,
				Result = new Result
				{
					Id       = $"{databaseObservation.Id}-geo",
					Geometry = NtsGeometryToGeometryPoint(databaseObservation.ResultGeometry)
				}
			};
			generatedRelatedObservations.Add(geoObservation);
		}
		
		if (databaseObservation.BaseUomId != databaseObservation.ResultUomId)
		{
			var baseMeasureObservation = new DigitaleDelta.Observation
			{
				Id             = $"{databaseObservation.Id}-base",
				Type           = "measure",
				ResultTime     = databaseObservation.ResultTime,
				ValidTime      = databaseObservation.ValidTimeStart,
				PhenomenonTime = databaseObservation.PhenomenonStart,
				Result = new Result
				{
					Id      = $"{databaseObservation.Id}-base",
					Measure = new Measure { Value = databaseObservation.BaseMeasure!.Value, Uom = references[databaseObservation.BaseUomId!.Value].Code }
				}
			};
			generatedRelatedObservations.Add(baseMeasureObservation);
		}
		
		var resultObservation = new DigitaleDelta.Observation
		{
			Id                 = databaseObservation.Id.ToString(),
			Type               = databaseObservation.ObservationType,
			ResultTime         = databaseObservation.ResultTime,
			PhenomenonTime     = databaseObservation.PhenomenonStart,
			ValidTime          = databaseObservation.ValidTimeStart,
			Foi                = GetParameterReferenceById(databaseObservation.FeatureOfInterestId, references, referencesByExternalKey, null),
			ObservedProperty   = GetParameterReferenceById(databaseObservation.ObservedPropertyId, references, referencesByExternalKey, null),
			ObservingProcedure = GetParameterReferenceById(databaseObservation.ObservingProcedureId, references, referencesByExternalKey, null),
			Parameter          = GetExportReferencesForParameter(databaseObservation.Parameter, references, referencesByExternalKey),
			Observer           = GetParameterReferenceById(databaseObservation.ObserverId, references, referencesByExternalKey, null),
			Host               = GetParameterReferenceById(databaseObservation.HostId, references, referencesByExternalKey, null),
			Result             = GetExportForResult(databaseObservation, references),
			Metadata           = GetExportReferencesForMetadata(databaseObservation.Metadata)
		};
		
		if (resultObservation.RelatedObservations == null)
		{
			resultObservation.RelatedObservations = generatedRelatedObservations;
		}
		else
		{
			resultObservation.RelatedObservations.AddRange(generatedRelatedObservations);
		}
		
		return resultObservation;
	}

	private static Result GetExportForResult(Observation observation, Dictionary<long, Reference> references)
	{
		var result = new Result();

		switch (observation.ObservationType)
		{
			case "measure":
				result.Measure = new Measure
				{
					Uom = references[observation.ResultUomId!.Value].Code,
					Value = observation.ResultMeasure!.Value
				};
				break;
			case "geography":
				var point = NtsGeometryToGeometryPoint(observation.ResultGeometry);
				result.Geometry = point;
				break;
			case "count":
				result.Count = observation.ResultCount!.Value;
				break;
			case "truth":
				result.Truth = observation.ResultTruth!.Value;
				break;
			case "term":
				result.Vocab = new CategoryVerb { Term = observation.ResultTerm!, Vocabulary = observation.ResultVocab! };
				break;
			case "timeseries":
				result.Timeseries = System.Text.Json.JsonSerializer.Deserialize<TimeseriesResult>(observation.ResultTimeseries!);
				break;
			case "complex":
				result.Complex = System.Text.Json.JsonSerializer.Deserialize<object>(observation.ResultComplex!);
				break;
		}

		return result;
	}
	
	private static ODataNamedValueDictionary<string>? GetExportReferencesForMetadata(Dictionary<string, string>? databaseMetadata)
	{
		if (databaseMetadata == null)
		{
			return null;
		}

		var result = new ODataNamedValueDictionary<string>();
		foreach (var (key, value) in databaseMetadata)
		{
			result.Add(key, value);
		}

		return result;
	}
	
	private static ODataNamedValueDictionary<string>? GetExportReferencesForParameter(Dictionary<string, long>? databaseObservationParameter, Dictionary<long, Reference> references, Dictionary<Guid, Reference> referencesByExternalKey)
	{
		if (databaseObservationParameter == null || databaseObservationParameter.Count == 0)
		{
			return null;
		}
		var dictionary = new ODataNamedValueDictionary<string>();
		foreach (var parameter in databaseObservationParameter)
		{
			var reference = GetExportReference(parameter.Value, references, referencesByExternalKey);
			if (reference == null)
			{
				continue;
			}

			var item = GetParameterReferenceById(parameter.Value, references, referencesByExternalKey, parameter.Key);
			if (item != null)
			{
				dictionary.Add(parameter.Key.ToLower().Replace(" ", string.Empty), item.Code ?? string.Empty);
			}
			
			if (reference.TaxonType != null)
			{
				dictionary.Add("taxontype", reference.TaxonType);
			}
			
			
			if (reference.TaxonGroup != null)
			{
				dictionary.Add("taxongroup", reference.TaxonGroup);
			}
		}

		return dictionary;
	}

	
	private static ParameterReference? GetExportReference(long? id, Dictionary<long, Reference> references, Dictionary<Guid, Reference> referencesByExternalKey)
	{
		return id == null ? null : GetParameterReferenceById(id.Value, references, referencesByExternalKey, null);
	}
	
	private static string? GetCodeFromReferenceByExternalKey(Guid? id, Dictionary<Guid, Reference> references)
	{
		return id == null ? null : ReferenceToExportReferenceByExternalKey(id, references)?.Code;
	}

	private static DigitaleDelta.Reference? ReferenceToExportReferenceByExternalKey(Guid? externalKey, Dictionary<Guid, Reference> referencesByExternalKey)
	{
		if (externalKey == null)
		{
			return null;
		}
		
		var exportReference = new DigitaleDelta.Reference
		{
			Id            = referencesByExternalKey[externalKey.Value].ExternalKey.ToString(),
			Type          = referencesByExternalKey[externalKey.Value].ReferenceType,
			Organisation  = referencesByExternalKey[externalKey.Value].Organisation ?? string.Empty,
			Code          = referencesByExternalKey[externalKey.Value].Code,
			Href          = referencesByExternalKey[externalKey.Value].Uri ?? string.Empty,
			Description   = referencesByExternalKey[externalKey.Value].Description,
			ExternalKey   = referencesByExternalKey[externalKey.Value].ExternalKey.ToString() ,
			TaxonRank     = referencesByExternalKey[externalKey.Value].TaxonRank,
			TaxonAuthors  = referencesByExternalKey[externalKey.Value].TaxonAuthor,
			//TaxonParent   = GetExportReferenceByExternalKey(referencesByExternalKey[externalKey.Value].TaxonParentExternalKey, referencesByExternalKey),
			TaxonNameNl   = referencesByExternalKey[externalKey.Value].TaxonNameNL,
			//TaxonType     = GetExportReferenceByExternalKey(referencesByExternalKey[externalKey.Value].TaxonTypeExternalKey, referencesByExternalKey),
			//TaxonGroup    = GetExportReferenceByExternalKey(referencesByExternalKey[externalKey.Value].TaxonGroupExternalKey, referencesByExternalKey),
			ParameterType = referencesByExternalKey[externalKey.Value].ParameterType,
			Geometry      = WktToGeometryPoint(referencesByExternalKey[externalKey.Value].Geometry)
		};
		
		return exportReference;
	}
	
	private static GeometryPoint? NtsGeometryToGeometryPoint(NetTopologySuite.Geometries.Geometry? geometry)
	{
		return geometry == null ? null : GeometryPoint.Create(CoordinateSystem.Geometry(4258), geometry.Centroid.Coordinate.X, geometry.Centroid.Coordinate.Y, null, null);
	}
	
	private static ParameterReference? GetParameterReferenceById(long? id, Dictionary<long, Reference> references, Dictionary<Guid, Reference> referencesByExternalKey, string? role)
	{
		
		if (id == null)
		{
			return null;
		}
		
		var reference = references[id.Value];
		return new ParameterReference
		{
			Id              = reference.ExternalKey.ToString(),
			Type            = reference.ReferenceType,
			Organisation    = reference.Organisation,
			Description     = reference.Description,
			Code            = reference.Code,
			Role            = role,
			TaxonRank       = reference.TaxonRank,
			TaxonAuthors    = reference.TaxonAuthor,
			TaxonNameNl     = reference.TaxonNameNL,
			ParameterType   = reference.ParameterType,
			TaxonStatusCode = reference.TaxonStatusCode,
			CasNumber       = reference.CasNumber,
			TaxonType       = GetCodeFromReferenceByExternalKey(reference.TaxonTypeExternalKey, referencesByExternalKey),
			TaxonGroup      = GetCodeFromReferenceByExternalKey(reference.TaxonGroupExternalKey, referencesByExternalKey),
			TaxonParent     = GetCodeFromReferenceByExternalKey(reference.TaxonParentExternalKey, referencesByExternalKey),
			TaxonTypeId     = reference.TaxonTypeExternalKey?.ToString(),
			TaxonGroupId    = reference.TaxonGroupExternalKey?.ToString(),
			TaxonParentId   = reference.TaxonParentExternalKey?.ToString(),
			Geometry        = WktToGeometryPoint(reference.Geometry)
		};
	}

}