# uni-api

The uni-api is an API for water management in the Netherlands. It is based on a *strict* subset of Observations, Measurements and Sampling, with sufficient room for handling observations, samples, time series, grids, predictions, etc.

The API defines three pieces of information to make sure that the experience of the user is the same across all different implementations of the uni-api.

- [OData definition](#odata-definition)
- [OAS definition](#oas-definition)
- [Semantic definition](#semantic-definition)

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

The client can request a specific **MAJOR** version of **OData** by specifying the appropriate header:

- odata
  
the 'major-version' header with a specific version, i.e. 2023.

The responses will always return the specific versions for the data:

@sem=2023.01
@oas=2023.01
@odata=2023.01

This information is provided once per result page.

## OData definition

The OData definition is standard across all uni-api implementations. It's implementation is **mandatory** and deviations are **not** allowed.
This will be known as 'NL Profiel Waterbeheer'.

Compliance may be checked by comparing the store definition in this GitHub with the generated definition at the /odata/$metadata endpoint.

## OAS definition

The Open API Specification defines the minimal requirements that de uni-api **must** implement.

The [query endpoints](query.md) are **mandatory**.

The [data management](data-management.md) and [subscription endpoints](subscriptions.md) are *optional*, but if implemented, **must** adhere to the specification.

Compliance may be checked by comparing the store definition in this GitHub with the generated definition at the /odata/$openapi endpoint.

## Semantic definition

The semantic definition specifies where references are defined. The implementation is **mandatory**. For instance: parameters are defined in Aquo, quantities **must** exist in group 'quantity' within the Aquo Parameter table, biological taxa are defined in TWN, Length classes are defined as 'length class' in Aquo.
This ensured that data is the same across the border and has the same meaning.

Organisations can have there own classifications, next to the standard definition. However, if the semantic standard defines an entity, the use of that entity is **mandatory**. So: standard always outweighs an organisation-standard.
It is the responsibility of the implementor/data maintainer that the correct mapping is performed.

## Proof of Concept

In the /Source/DotNet part of this GitHub repository, a simple implementation of a proof of concept is available in .NET 7/C# 11.
It implements a simple yet functional and fast storage model using PostgreSQL 15. The implementor can, of course, implement their own storage model or extend their own platform to provide the functionality.

The management and subscription endpoints are not yet implemented in the Proof of Concept. That part is Work in Progress.

## TODO

- Specify the management endpoints
- Specify the subscription endpoint
- Specify the semantic definition
