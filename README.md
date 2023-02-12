# uni-api

The uni-api is an API for water management in the Netherlands. It is based on a *strict* subset of Observations, Measurements and Sampling, with sufficient room for handling observations, samples, time series, grids, predictions, etc.

The API defined three pieces of information to make sure that the experience of the user is the same across all different implementations of the uni-api.

- OData definition
- OAS definition
- Semantic definition

## Definitions

All the definition specifications have a semantic version system according to [semver.org](https://semver.org/).
Patches are not used, only **MAJOR** and *MINOR* versions.

The **MAJOR** version is always the 4-digit year of the specification.

*MINOR* versions are not allowed to break anything for implementations that use the same **MAJOR** version.
*MINOR* versions are only allowed to **ADD** to the specification and cannot modify behavior.
The implementor **MUST** implement a new **MAJOR** version within three months of the publication here on GitHub.
The implementor is *encouraged* to support a new *MINOR* version as soon as possible.
Older **MAJOR** version **MUST** be supported at least for ONE YEAR after a *MAJOR** version is released.
This means that two **MAJOR** versions can co-exist.

The client can request a specific **MAJOR** version by specifying the appropriate header:

- sem=
- oas=
- odata=
  
the 'major-version' header with a specific version, i.e. 2023.

The responses will always return the specific versions for the data:

@sem=2023.01
@oas=2023.01
@odata=2023.01

## OData definition

The OData definition is standard across all uni-api implementations. It's implementation is **mandatory** and deviations are not allowed.
This will be known as 'NL Profiel Waterbeheer'.

Compliance may be checked by comparing the store definition in this GitHub with the generated definition at the /odata/$metadata endpoint.

## OAS definition

The Open API Specification defines the minimal requirements that de uni-api **must** implement.

The /bulk endpoint and the /subscribe endpoints are *optional*, but if implemented, **must** adhere to the specification.

Compliance may be checked by comparing the store definition in this GitHub with the generated definition at the /odata/$openapi endpoint.

### Query endpoints

Query endpoints are used to retrieve data using OData. Implementation of these endpoints is *mandatory*.

#### /odata/reference

The /reference endpoint uses OData to query references. References are considered immutable.
References are described [here](reference.md).

#### /odata/observation

The /observation endpoint uses OData to query observations, measurements, samples, timeseries, grids, etc.
Observations are described [here](observation.md).

### Management endpoints (work in progress)

The purpose of management endpoints is to allow to add, modify or remove data.

Notes:

- Observations are considered immutable. Incorrect observations will have to be removed and re-added.
- References are *not* considered immutable. For instance: the name of a taxon may change, but the taxon itself remains the same.
- Removal of a reference is *only allowed* if no observation in the system uses the reference.

#### /reference/add

The *optional* /reference/add endpoint is used to add a single reference.

#### /reference/modify

The *optional* /reference/modify endpoint is used to modify a single reference.

#### /reference/remove

The *optional* /reference/remove endpoint is used to remove a single reference.

#### /observation/add

The *optional* /observation/add endpoint is used to add a single observation.

#### /observation/modify

The *optional* /observation/modify endpoint is used to modify a single observation.

#### /observation/bulk-insert

The *optional* /observation/bulk-insert endpoint is used to add observations in bulk, meant to process payloads with many entities.
The rationale to implement a bulk operation is that it can be used in optimized database features to copy data much faster than individual inserts or transactions allow. The number of items that can be inserted in one bulk operation depends on the implementation, but a suggested size of the maximum number of items accepted is 250000.
This allows laboratories to add a checked bulk of measurements in a single operation.

#### /observation/bulk-remove

The *optional* /observation/bulk-remove endpoint is used to remove observations in bulk, meant to process payloads with many entities.
The rationale to implement a bulk operation is that it can be used in optimized database features to copy data much faster than individual inserts or transactions allow. The number of items that can be inserted in one bulk operation depends on the implementation, but a suggested size of the maximum number of items accepted is 250000.
This allows laboratories to remove incorrect data quickly.

### Subscription endpoints (work in progress)

Subscription endpoints can be used to subscribe some form of data changes. For instance: data is added to a specific location with a specific quantity. The MQTT protocol is used to communicate.

#### /subscribe

The *optional* /subscribe endpoint is used to subscribe to changes on a specific changes or additions to observations. It uses the MQTT protocol.

## Semantic definition

The semantic definition specifies where references are defined. The implementation is **mandatory**. For instance: parameters are defined in Aquo, quantities **must** exist in group 'quantity' within the Aquo Parameter table, biological taxa are defined in TWN, Length classes are defined as 'length class' in Aquo.
This ensured that data is the same across the border and has the same meaning.

Organisations can have there own classifications, next to the standard definition. However, if the semantic standard defines an entity, the use of that entity is **mandatory**.
It is the responsibility of the implementor/data maintainer that the correct mapping is performed.

## Proof of Concept

In the /source/DotNet part of this GitHub repository, a simple implementation of a proof of concept is available in .NET 7/C# 11.
It implements a simple yet functional and fast storage model using PostgreSQL 15. The implementor can, of course, implement their own storage model or extend their own platform to provide the functionality.

The management and subscription endpoints are not yet implemented in the Proof of Concept. That part is Work in Progress.

## TODO

- Specify the management endpoints
- Specify the /subscribe endpoint
- Specify the semantic definition
