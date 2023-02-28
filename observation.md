# Observation

An observation is a standard observation, according to [Observations, Measurements and Sampling](https://portal.ogc.org/files/?artifact_id=41510).
The definition in CSDL form for observation can be found [here](Definition/csdl/v2023.01/csdl.xml).

The supported observation types (result types) are:

- count
- measure
- timeseries
- truth
- categoryverb
- geometry

Timeseries are encoded in TimeseriesML/WaterML, capable of also handling grids, coverages, etc.

Observations in OMS have a limitation: 
only one value (a timeseries is considered a value) can be stored in an observation,
without having to model a specific new observation type.

This is solved by using RelatedObservations.

An observation in the Uni-API is a combination of the following properties:

- [Id](#Id)
- [Type](#Type)
- [ResultTime](#ResultTime)
- [PhenomenonTime](#PhenomenonTime)
- [ValidTime](#ValidTime)
- [Foi](#Foi)
- [ObservedProperty](#ObservedProperty)
- [ObservingProcedure](#ObservingProcedure)
- [Observer](#Observer)
- [Host](#Host)
- [Parameter](#Parameter)
- [Metadata](#Metadata)
- [Result](#Result)
- [RelatedObservation](#RelatedObservation)

// TODO:

Consider: observedProperty, observingProcedure, observer and host*could* be moved to parameter, to simplify filtering.
Since foi includes geometry, it should not be moved to parameter.
Parameter is used to keep the search interface clean.

## Id

The Id property is a unique string, within *at least* the context of the organisation to which the observation belongs. We **strongly** suggest the use of UUID v4.

## Type

The type defines the format of the [Result](#result). It can be one of the following:

- [count](#count) (an integer value)
- [measure](#measure) (a combination of a decimal value and a unit of measurement, where unit of measurement (uom) is a reference)
- [timeseries](#timeseries) (a timeseries, encoded in TimeseriesML)
- [truth](#truth) (a boolean value)
- [categoryverb](#categoryverb) (a combination of a category and a verb)
- [geometry](#geometry) (a geometry, encoded in GeoJSON)

## ResultTime

Analysis time of the observation.

## PhenomenonTime

Sample time of the observation.

## ValidTime

A set of date/time fields representing the period for which the observation is valid.

## Foi

Foi stands for Feature of Interest.
In the Uni-API context it is considered a measurement object representing the location where the sampling was performed.
The Foi property is a reference to a [reference](reference.md) **of type 'measurementobject'**.

## Parameter

The Parameter property (despite its singular name) consists of a dictionary of key/value pairs, 
where the key is a unique string within the observation 
and the value is an url of an existing reference within the implementation or an external link, 
provided it conveys meaning.
All keys in the dictionary form an additional piece of information observed for the observation that can be classified. 
The key represents the reference type.

The Parameter property is *optional*.

Note: data that can be used to identify **a person** are not allowed to be retrieved by the Uni-API.

## ParameterDetails

When the $expand parameter is used and its value is either * or contains ParameterDetails,
the result of the observation is expanded with the details of the parameter.
The ParameterDetails property is a dictionary of key/value pairs,
where the key is a unique string within the observation,
and the value is a JSON representation of the reference that the parameter points to.

Note: data that can be used to identify **a person** are not allowed to be retrieved by the Uni-API.

## Metadata

The Metadata property consists of a dictionary of key/value pairs,
where the key is a unique string within the observation
and the value is a string with data that are not referable. 
I.e. the sample number from which the observation was taken.
All keys in the dictionary form an additional piece of information observed for the observation that can be classified.
The Metadata property is optional.

Note: data that can be used to identify **a person** are not allowed to be retrieved by the Uni-API.

## RelatedObservation

RelatedObservation is a dictionary of key/value pairs,
where the key is a unique string within the observation
and the value is the id of an existing observation within the implementation that is related to the observation.
The key represents the type of relation (its role) with the source.

I.e. it can be used to provide an observed unit/value next to the specified unit/value
or to provide detailed coordinates for the location where the sample was taken (i.e. measurements on a boat).

The RelatedObservation property is *optional*.

## Result

The Result property is the value of the observation.
Its type defines the format of the result.
'Result' is a required property and must have one of the following types:

### Count

The count result type is an integer value.

### Measure

The measure result type is a combination of a decimal value and a unit of measurement, 
where unit of measurement (uom) is a reference.

### Timeseries

Timeseries are described [here](https://docs.ogc.org/is/15-043r3/15-043r3.html).
Timeseries are encoded in TimeseriesML. 
However, there is no official JSON encoding available yet.
A proper JSON encoding is needed for the Uni-API.
[Here](https://github.com/peterataylor/om-json) is a proposal for a JSON encoding in the files:

- Temporal.json
- TimeseriesMetadata.json
- TimeseriesTVP.json
- TimeseriesMonitoringFeature.json

### Truth

The truth result type is a boolean value.

### CategoryVerb

The categoryverb result type is a combination of a category and a verb. Both are [references](reference.md).

### Geometry

The geometry result type is a geometry, encoded in GeoJSON. Determine impact of JSON-FG instead of GeoJSON.



## TODO

- Examine impact of JSON-FG instead of GeoJSON.
- Examine CoverageJSON.
- As long as there is no official TimeseriesML or WaterML JSON encoding, define our own encoding. Decide between TimeseriesML and WaterML.

## History

- 20230226: ObservedProperty, observingProcedure, observer and host are moved to 'Parameter', to simplify filtering. Foi stays a separate entity, since it involves geometry. 'Parameter' is used to keep the search interface clean. OMS allows this, O&M had restrictions.