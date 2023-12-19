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
/{cageId} Update: update cage
  *Name
  *Max Capacity

/cage/powerstatus
GET: return power status
PUT: power up
DELETE: power down

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
