namespace DatabaseModel.Models;

using NetTopologySuite.Geometries;

public class Observation
{
	public                  long                          Count                { get; set; }
	public                  Guid                          Id                   { get; set; }
	public                  string                        ObservationType      { get; set; } = null!;
	public                  DateTime                      PhenomenonStart      { get; set; }
	public                  DateTime?                     PhenomenonEnd        { get; set; }
	public                  DateTime                      ResultTime           { get; set; }
	public                  long?                         FeatureOfInterestId  { get; set; }
	public                  long?                         ResultUomId          { get; set; }
	public                  double?                       ResultMeasure        { get; set; }
	public                  bool?                         ResultTruth          { get; set; }
	public                  string?                       ResultTerm           { get; set; }
	public                  string?                       ResultVocab          { get; set; }
	public                  string?                       ResultTimeseries     { get; set; }
	public                  Geometry?                     ResultGeometry       { get; set; }
	public                  long?                         ResultCount          { get; set; }
	public                  DateTime                      ValidTimeStart       { get; set; }
	public                  DateTime?                     ValidTimeEnd         { get; set; }
	public                  string?                       ResultComplex        { get; set; }
	public                  string?                       ResultText           { get; set; }
	public                  Dictionary<string, long>?     Parameter            { get; set; }
	public                  Dictionary<string, string>?   Metadata             { get; set; }
	public                  Guid?                         RelatedObservationId { get; set; }
	public                  string?                       RelationRole         { get; set; }
	public                  double?                       BaseMeasure          { get; set; }
	public                  long?                         BaseUomId            { get; set; }
	private static readonly NetTopologySuite.IO.WKTReader WktReader = new();

	public Observation()
	{
	}

	public Observation(Observation observation)
	{
		Id                   = observation.Id;
		ObservationType      = observation.ObservationType;
		PhenomenonStart      = observation.PhenomenonStart;
		PhenomenonEnd        = observation.PhenomenonEnd;
		ResultTime           = observation.ResultTime;
		FeatureOfInterestId  = observation.FeatureOfInterestId;
		ValidTimeStart       = observation.ValidTimeStart;
		ValidTimeEnd         = observation.ValidTimeEnd;
		BaseMeasure          = observation.BaseMeasure;
		BaseUomId            = observation.BaseUomId;
	}

	public static Observation FromDynamic(dynamic item)
	{
		var dynamic = new Observation();
		dynamic.Count                = item.count;
		dynamic.Id                   = item.id;
		dynamic.ObservationType      = item.observation_type;
		dynamic.PhenomenonStart      = item.phenomenon_time_start;
		dynamic.PhenomenonEnd        = item.phenomenon_time_end;
		dynamic.ResultTime           = item.result_time;
		dynamic.FeatureOfInterestId  = item.foi_id;
		dynamic.ResultUomId          = item.result_uom_id;
		dynamic.ResultMeasure        = item.result_measure;
		dynamic.ResultTruth          = item.result_truth;
		dynamic.ResultTerm           = item.result_term;
		dynamic.ResultVocab          = item.result_vocab;
		dynamic.ResultTimeseries     = item.result_timeseries;
		dynamic.ResultGeometry       = item.result_geometry == null ? null : WktReader.Read(item.result_geometry);
		dynamic.ResultCount          = item.result_count;
		dynamic.ValidTimeStart       = item.valid_time_start;
		dynamic.ValidTimeEnd         = item.valid_time_end;
		dynamic.ResultComplex        = item.result_complex;
		dynamic.ResultText           = item.result_text;
		dynamic.Parameter            = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, long>>(item.parameter);
		dynamic.Metadata             = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(item.metadata);
		dynamic.BaseMeasure          = item.base_measure;
		dynamic.BaseUomId            = item.base_uom_id;
		return dynamic;
	}
}
