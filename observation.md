# Observation

An observation is a standard observation according to [Observations, Measurements and Sampling](...).
The definition in CSDL form for observation can be found [here](Definition/csdl/v2023.01/csdl.xml).

The supported observation types (result types) are:

- count
- measure
- timeseries
- truth
- categoryverb
- geometry

Timeseries is encoded in TimeseriesML, capable of also handling grids, coverages, etc.

Observations in OMS pose a limitation: only one value (a timeseries is considered a value) can be stored in an observation, without having to model a specific nre observation type.

This is solved by using RelatedObservations.

Some usages:

- Provide an observed unit/value next to the specified unit/value.
- Provide detailed coordinates for the location where the sample was taken (i.e. measurements on a boat).
