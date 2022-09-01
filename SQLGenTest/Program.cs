using SQLGenTest;

Config.InitDB("test.db");

Countries.SetDebugModeOn();
Regions.SetDebugModeOn();
Cities.SetDebugModeOn();

Countries.Create();
Regions.Create();
Cities.Create();

var country = new Country();
country.Name = "Россия";
country.Save();
Console.WriteLine(country.Id);

var region = new Region();
region.Country = country;
region.Name = "Москва";
region.Number = 77;
region.Save();
Console.WriteLine(region.Id);

var city = new City();
city.Country = country;
city.Region = region;
city.Name = "Москва";
city.Save();
Console.WriteLine(city.Id);

var countryNew = new Country();
countryNew.Name = "Франция";
countryNew.Save();
Console.WriteLine(countryNew.Id);

region.Country = countryNew;
region.Name = "Париж";
region.Number = 101;
region.Save();

var countries = Countries.Get();
Console.WriteLine(countries.Count);
var regions = Regions.Get();
Console.WriteLine(regions.Count);
var cities = Cities.Get();
Console.WriteLine(cities.Count);

Countries.Delete();
Regions.Delete(region);
Cities.Delete(city);