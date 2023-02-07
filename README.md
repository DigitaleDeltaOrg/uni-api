# uni-api

De UNI-API is een API voor waterbeheer in Nederland. Het is gebaseerd op een subset van Observations, Measurements and Sampling, met voldoende informatie om om te kunnen gaan met observaties, monsters, tijdsreeksen, grid, voorspellingen, etc.

De API werkt met een OData definitie, die vastlegt hoe de structuren van data eruit ziet. Hiervan mag *NIET* worden afgeweken. Dit zorgt ervoor dat de zoekervaring van gebruikers gelijk is over verschillende implementaties van de UNI-API heen. Er is een versienummering gekoppeld aan deze specificatie, onderverdeeld in een *major* en een *minor* versie.
Bij een *minor* revisie kunnen alleen niet-verplichte zaken worden toegevoegd. Alleen bij een *major* revisie kan nieuwe *verplichte functionaliteit* worden toegevoegd.

Daarnaast is er een Open API Specificatie die aangeeft welke OData verbs worden ondersteund. Hiervan *MAG* worden afgeweken, maar alleen om aan de specificatie toe te voegen. Er *MAG* niets worden verwijderd.
Ook hier geldt de voorgaande revisie regels.

Naast deze technische specificatie leggen we ook een semantische specificatie vast: het zogenaamde 'OMS profiel voor waterbeheer NL'.
Hierin wordt vastgelegd waar de verschillende entiteit types (zoals parameter, taxon, grootheid, levensstadium, lengteklasse) gedefinieerd zijn.
Eigen entiteit types kunnen worden vastgelegd, maar zodra deze tot een standaard worden verheven, moet aan de specificaties van de betreffende standaard-instelling worden voldaan.
Ook deze gebruikt weer dezelfde revisie regels.

Deze UNI-API implementatie (/DotNet) is open source en kan als volledige implementatie of als basis voor het implementeren worden gebruikt.

Er is een zeer eenvoudig voorbeeld datamodel aanwezig, echter deze hoeft niet ge&iuml;mplementeerd te worden. Het is waarschijnlijk dat de implementator een eigen datastorage laag wil gebruiken.

## TODO

- Specificeren en implementeren van API voor verwijderen/toevoegen van data (referentie en observaties).
- Specificeren van security (OOAUTH2 Client Credential Flow en OAUTH2 Resource Owner Flow) volgens Kennisplatform API.
- Specificeren van de semantische specificatie
