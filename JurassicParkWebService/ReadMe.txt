Notes:

The assignment was to create a REST service. Technically, REST should use HATEOAS, but I assumed this was out of scope.
Instead, this is an plain HTTP service that supports remote procedure calls (RPC), which is how most people use "REST"
I do have a C# library that supports HATEOAS, but decided that using it for this project might be too much: https://github.com/slyjeff/RestResource

Once consequence of not using HATEOAS is that the resources need to contain IDs so clients know how to interact with them.
In a HATEOAS implementation, the links would be provided so the client is unaware of resource ids. In either case, I do not consider ID
as part of the resource itself and am not including it in PUT operations since it is available in the URL.

I originally thought to consider "PowerStatus" a subresource of Cage and support PUT/DELETE to turn it off and on, but decided
if we are searching on it and returning the value as part of the Cage, that wasn't appropriate. With a team, I think this would at least
be a discussion to consider the pros and cons.

I do not have unit tests around the database logic. These would more properly be considered "integration tests" and my experience is that
if the database layer is well seperated, it is better not to use unit tests for automation, as they have extenral dependencies. Additionally,
Usually in C# I'd use NHibernate or another ORM rather than ADO.

On create, power status is not passed in- I am intentionally setting it to active. Without it being specified in the requeirements, I made
a judgement called, but I wanted to note that this intentional (that is, passing in a value will be ignored).

I did not allowed the editing of Species.SpeciesType once the record was created to avoid handling the complexity of the logic of Dinosaurs
in cages no longer meeting the Carnivore/Herbivore requirements. In real life, this would be a discussion, but I decided to keep the scope limited.

I did not allowed the editing of Dinosaure.SpeciesId if the dinosaur is in a cage to avoid handling the complexity of the logic of Dinosaurs
in cages no longer meeting the Carnivore/Herbivore requirements. In real life, this would be a discussion, but I decided to keep the scope limited.


Things to Add:
*Exception catching/error handling
*Versioning on database objects for concurrecy
*Authorization/Authentication
*Logging
*Versioning for the API

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
/{speciesId} DELETE: Remove a species

/dinosaur
GET: Search for Dinosaurs
  *Name
  *Species
POST: Add a Dinosaur
  *Name
  *Herbivore/Carnivore
/{dinosaurId} GET: Get a single Dinosaur
/{dinosaurId} PUT: Update a Dinosaure
  *Name
  *Species
/{dinosaurId} DELETE: Remove a dinosaur
