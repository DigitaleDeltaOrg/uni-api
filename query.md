# Query endpoints

Query endpoints are used to retrieve data using OData. Implementation of these endpoints is *mandatory*.

## /odata/reference

The /reference endpoint uses OData to query references. References are considered immutable.
References are described [here](reference.md).

## /odata/observation

The /observation endpoint uses OData to query observations, measurements, samples, timeseries, grids, etc.
Observations are described [here](observation.md).

For public data, no security is required.
For non-public data, an OAUTH2 Resource Owner flow is required for interactive sessions, or an OAUTH2 Authorization Code flow for non-interactive sessions, such as machine-to-machine scenarios.