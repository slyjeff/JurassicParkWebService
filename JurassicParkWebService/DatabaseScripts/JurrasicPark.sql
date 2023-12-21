use JurassicPark

IF (NOT EXISTS (SELECT 0 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Species'))
BEGIN
	CREATE TABLE Species (
		Id          int          IDENTITY(1,1) PRIMARY KEY,
		Name        varchar(MAX) NOT NULL,
		SpeciesType varchar(9)   NOT NULL,
	);

	insert into Species(Name, SpeciesType) values ('Tyrannosaurus', 'Carnivore');
	insert into Species(Name, SpeciesType) values ('Velociraptor', 'Carnivore');
	insert into Species(Name, SpeciesType) values ('Spinosaurus', 'Carnivore');
	insert into Species(Name, SpeciesType) values ('Megalosaurus', 'Carnivore');

	insert into Species(Name, SpeciesType) values ('Brachiosaurus', 'Herbivore');
	insert into Species(Name, SpeciesType) values ('Stegosaurus', 'Herbivore');
	insert into Species(Name, SpeciesType) values ('Ankylosaurus', 'Herbivore');
	insert into Species(Name, SpeciesType) values ('Triceratops', 'Herbivore');
END