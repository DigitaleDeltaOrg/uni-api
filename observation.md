# Observation

An observation is a standard observation according to [Observations, Measurements and Sampling](...).
Due to the fact that not all systems will have sampling data and we wish to avoid specific contextual views on the data, specific sampling data (such as sampling code) is folded in the metadata section of an observation.
Timeseries, grids, coverages, etc are codes in TimeseriesML.

Observations in OMS pose a limitation: only one value (a timeseries is considered a value) can be stored in an observation, without having to model a specific observation type (result type. Do not confuse this with 'waarnemingssoort').

This is solved by using RelatedObservations.

Some usages:

- Provide an observed unit/value next to the specified unit/value
- Provide detailed coordinates for the location where the sample was taken (i.e. measurements on a boat).
