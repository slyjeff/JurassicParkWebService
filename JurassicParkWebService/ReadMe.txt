Note: the assignment was to create a REST service. Technically, REST should use HATEOAS, but I assumed this was out of scope.
Instead, this is an plain HTTP service that supports remote procedure calls (RPC), which is how most people use "REST"

I do have a C# library that supports HATEOAS, but decided that using it for this project might be too much: https://github.com/slyjeff/RestResource

Note: I originally thought to consider "Power Status" a subresource of Cage and support PUT/DELETE to turn it off and on, but decided
if we are searching on it and returning the value as part of the Cage, that wasn't appropriate. With a team, I think this would at least
be a discussion to consider the pros and cons.

Things to Add:
*Versioning on database objects for concurrecy
*Authorization/Authentication
*Logging

--------------------

Endpoints:

/cage
GET: Search for cages
  *Name
  *Power Status
POST: Add a cage
  *Name
  *Max Capacity
/{cageId} PUT: update cage
  *Name
  *Max Capacity

/cage/dinosaur
GET: List of all dinosaures in a cage
/{dinosaurId} PUT: Add dinosaure to cage
/{dinosaurId} DELETE: Remove dinosaure from cage

/species
GET: List all Species
POST: Add a Species
  *Name
  *Herbivore/Carnivore
/{speciesId} GET: Get a single species
/{speciesId} PUT: Update a Species
  *Name
  *Herbivore/Carnivore
/{speciesId} DELETE: Remove a species

/dinosaur
GET: Search for Dinosaures
  *Name
  *Species
POST: Add a Species
  *Name
  *Herbivore/Carnivore
/{dinosaurId} GGET: Get a single species
/{dinosaurId} PUT: Update a Species
  *Name
  *Herbivore/Carnivore
/{dinosaurId} DELETE: Remove a dinosaur
