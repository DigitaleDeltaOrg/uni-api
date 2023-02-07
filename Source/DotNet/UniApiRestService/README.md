# Introduction

The UniAPI is a universal [API](https://nl.wikipedia.org/wiki/Application_programming_interface) for requesting all kinds measurement-related data for the Dutch aquatic domain.
The goal is to uniform data and a simple method to request data for different kinds of audiences.
The requested data can be exported in several formats.
A further goal is to query across several data sources, sharing the same API.

## Why

Currently, the [Digitale Delta]() comprises of a several [API's](https://nl.wikipedia.org/wiki/Application_programming_interface) serving specific goals. Furthermore, not all implementations of those API's are implemented correctly. Also the data sources they are serving, are not all implemented according to the Dutch [Aquo](https://aquo.nl) standard.
This leads to ambiguous data, which makes combining data unreliable.
This [API](https://nl.wikipedia.org/wiki/Application_programming_interface) tries to solve these issues.

## How

We combine [OData](https://www.odata.org) (a search standard) with OMS (exchange standard) and standard definition sources (i.e. [Aquo](https://aquo.nl)).

## OData

[OData](https://www.odata.org) is een ISO-standaard voor het opvragen van data via een REST API.
Het vereist, om goed te werken, een datamodel in de vorm van een [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/). Dit is een [XML](https://www.w3.org/XML/)- of [JSON](https://www.json.org/json-en.html)-bestand dat de entiteiten en relaties beschrijft.
Normaliter wordt dit [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/)-bestand gegenereerd uit een database. 
In dit geval is het [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/)-bestand handmatig gemaakt, omdat de UniAPI niet gaat voorschrijven hoe het datamodel in elkaar moet zitten.
Door de [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) in de standaard op te nemen, wordt een soort virtueel datamodel gecreëerd. Dat is mogelijk, omdat we een Nederlands profiel voor OMS gaan maken.

## OMS

OMS (Observations, Measurements & Sampling, or O&M:2022) is a [OGC](https://www.ogc.org)/[ISO](https://www.iso.org/home.html)-standard for exchanging measurement data.
It is the 2022-edition of [Observations & Measurements](https://www.ogc.org/standards/om).
We'll try to change as little as possible to the standard OMS-format.
OMS supports all measurement types we work with in the aquatic domain:

- quantity (timeseries)
- grids
- coverages
- quality (field- and laboratory measurements)

### OMS profile

OMS is big and flexible, this multi-interpretable. The UniAPI is designed, however, to share the same definition and meaning. Therefore, we'll create a Dutch aqua-profile for OMS. This will result in a clear, concised definition for measurements and related data for the Dutch aquatic domain.
This will make it possible to combine data from different sources, without ambiguity.
And in turn, this will make it easier to exchange data with, for example, INSPIRE.

## Reference system

De UniAPI maakt gebruik van een intern referentiesysteem. 
Alle entiteiten binnen het systeem (of binnen een import) moeten gevonden kunnen worden in het referentiesysteem.

### Why a reference system

Doel van [JSON](https://www.json.org/json-en.html) als dataformaat is om het leesbaar en begrijpelijk te houden voor mensen. Wanneer entiteiten alleen in de data staan als een niets-zeggend id (zoals bij [Aquo](https://aquo.nl) het geval is), dan is dat niet meer het geval.
Het referentiesysteem is een brug tussen leesbare entiteiten en mogelijk onleesbare id's buiten het systeem.

## How it works

De UniAPI is een [REST API](https://en.wikipedia.org/wiki/Representational_state_transfer). Het bestaat uit verschillende lagen, waarvan een aantal implementatie-specifiek.

### EDMX/Open API

De [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) en de [Open API-specificatie](https://www.openapis.org/) zijn de basis van de [API](https://nl.wikipedia.org/wiki/Application_programming_interface). 
Een [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) wordt geschreven in CSDL: [Common Schema Definition Language](https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html).

### OData Parsers

Deze zijn generiek over alle implementaties heen.
Via tools kunnen vanuit de [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/) en de [Open API-spec](https://www.openapis.org/) een client of een server gegenereerd worden.
Deze zijn specifiek voor programmeertalen en frameworks.
Voor specifieke frameworks zijn ook bibliotheken beschikbaar die een [OData](https://www.odata.org) verzoek, gecombineerd met de [EDMX](https://docs.oasis-open.org/odata/odata/v4.0/errata02/os/complete/schemas/), kunnen omzetten naar een [AST](https://en.wikipedia.org/wiki/Abstract_syntax_tree), een [Abstract Syntax Tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree).

### Query/Data layer

Een implementatie-afhankelijke laag kan de [AST](https://en.wikipedia.org/wiki/Abstract_syntax_tree) omzetten naar opslag-specifieke queries.
Deze laag is ook verantwoordelijk voor het converteren van de resultaten naar het juiste (aangevraagde en gedefinieerde) dataformaat.

## Parameters

A parameter can be (almost) any lookup value (observed property).
The quantity is used as an observable property.
Exceptions are made for:

- quantity (observable property)
- project (deployment)
- observer (organisation that did the analysis)
- method (observingprocedure)
- foi (location)
- ProximateFeatureOfInterest: Location
- Units (UOM)

## Metadata

Metadata will be any non-observed value that holds a non-lookup value. Examples:
- Magnification factor
- Shape of the organism (such as shape of a colony)



##### Polymorphic (de)serialization: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0
##### Required: [JsonRequired]: https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties

## GeoJSON encoding
https://github.com/opengeospatial/omsf-profile/blob/master/omsf-json/examples/measure-observation_geojson_feature-collection.json


/references
    id: string (uuid)
    type: string
    organisation: /references/type eq 'organisation'
    code: string
    parametertype: string
    cas: string

/observations
    type: string (oneof)
    phenomenontime: datetime
    validtime: datetime
    resulttime: datetime
    observedproperty/code: /reference/type eq 'quantity'
    observedproperty/id: /reference/type eq 'quantity'
    observedproperty/name: /reference/type eq 'quantity'
    observingprocedure/code: /references/type eq 'analysismethod'
    observingprocedure/id: /references/type eq 'analysismethod'
    observingprocedure/name: /references/type eq 'analysismethod'
    host/code: string: valid: /references/type eq 'organisation'
    host/id: string: valid: /references/type eq 'organisation'
    host/name: string: valid: /references/type eq 'organisation'
    observer/code: string: valid: /references/type eq 'organisation'
    observer/id: string: valid: /references/type eq 'organisation'
    observer/name: string: valid: /references/type eq 'organisation'
    foi/code: /references/type eq 'measurementobject'
    foi/id: /references/type eq 'measurementobject'
    foi/geometry: /references/type eq 'measurementobject'
    truth: bool
    uom/code: /references/type eq 'uom'
    uom/id: /references/type eq 'uom'
    uom/name: /references/type eq 'uom'
    measure: double
    count: int
    parameter/ANY(d:d/type eq ''): string (reference type)
    parameter/ANY(d:d/code eq ''): string
    parameter/ANY(d:d/taxontype eq ''): string
    parameter/ANY(d:d/taxongroup eq ''): string
    parameter/ANY(d:d/cas eq ''): string
    parameter/ANY(d:d/organisation eq ''): string

https://devblogs.microsoft.com/odata/customizing-filter-for-spatial-data-in-asp-net-core-odata-8/
