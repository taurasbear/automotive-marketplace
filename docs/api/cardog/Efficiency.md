Copy page
Vehicle Efficiency

The efficiency API provides access to comprehensive fuel economy and efficiency data aggregated from multiple international sources: EPA (US), Transport Canada, UK VCA, and EU EEA. All data is normalized to metric units for consistency.

Data Sources
Source Region Coverage Test Cycle
EPA United States 1984-2026 EPA 5-cycle
Transport Canada Canada 1995-2027 5-cycle
UK VCA United Kingdom Current WLTP
EU EEA European Union 2010-present WLTP

Note: EPA test cycle results typically differ from WLTP by 10-20% for ICE vehicles. BEV efficiency is more comparable across regions.

GET
/v1/efficiency/search
Search efficiency records

Search and filter efficiency records with comprehensive filtering options. Returns paginated results with all available efficiency metrics.

Optional parameters
Name
makes
Type
string
Description

Comma-separated list of vehicle makes (e.g., "Toyota,Honda"). Case-insensitive.

Name
models
Type
string
Description

Comma-separated list of models (e.g., "Camry,Accord"). Case-insensitive.

Name
yearMin
Type
number
Description

Minimum model year (e.g., 2020).

Name
yearMax
Type
number
Description

Maximum model year (e.g., 2024).

Name
powertrainTypes
Type
string
Description

Comma-separated powertrain types. See powertrain types.

Name
fuelTypes
Type
string
Description

Comma-separated fuel types (e.g., "Gasoline,Diesel,Electricity").

Name
sources
Type
string
Description

Comma-separated data sources: epa, tc, uk-vca, eu-eea.

Name
testCycles
Type
string
Description

Comma-separated test cycles: epa, wltp, mixed.

Name
sort
Type
string
Description

Sort field: make, model, year, combined_l_per_100km, combined_kwh_per_100km, co2_grams_per_km, electric_range_km.

Name
order
Type
string
Description

Sort order: asc or desc. Default: asc.

Name
limit
Type
number
Description

Results per page (1-250). Default: 50.

Name
offset
Type
number
Description

Number of results to skip for pagination.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/efficiency/search
curl "https://api.cardog.app/v1/efficiency/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "makes=Tesla" \
 -d "powertrainTypes=bev" \
 -d "yearMin=2022" \
 -d "sort=electric_range_km" \
 -d "order=desc" \
 -d "limit=10"

Copy
Copied!
Response
{
"success": true,
"data": [
{
"id": 12345,
"source": "epa",
"sourceId": "48765",
"year": 2024,
"make": "TESLA",
"model": "MODEL S",
"variant": "Long Range AWD",
"powertrainType": "bev",
"fuelType": "Electricity",
"transmission": "A1",
"driveType": "AWD",
"engineDisplacementCc": null,
"enginePowerKw": 375,
"cylinders": null,
"combinedLPer100km": null,
"cityLPer100km": null,
"highwayLPer100km": null,
"combinedKwhPer100km": 17.4,
"cityKwhPer100km": 16.8,
"highwayKwhPer100km": 18.0,
"co2GramsPerKm": 0,
"electricRangeKm": 652,
"totalRangeKm": 652,
"massKg": 2108,
"wheelbaseMm": null,
"testCycle": "epa"
}
],
"meta": {
"filter": {
"makes": ["TESLA"],
"powertrainTypes": ["bev"],
"year": { "min": 2022 }
},
"count": 10
}
}

Copy
Copied!
GET
/v1/efficiency/:make/:model/:year
Get efficiency by vehicle

Retrieve all efficiency records for a specific make, model, and year. Returns all available trims and variants.

Path parameters
Name
make
Type
string
Description

Vehicle make (e.g., "Toyota"). Case-insensitive.

Name
model
Type
string
Description

Vehicle model (e.g., "Camry"). Case-insensitive.

Name
year
Type
number
Description

Model year (e.g., 2024).

Request
cURL
@cardog/api
fetch
Python
GET
/v1/efficiency/:make/:model/:year
curl "https://api.cardog.app/v1/efficiency/toyota/camry/2024" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"success": true,
"data": [
{
"id": 54321,
"source": "epa",
"sourceId": "47892",
"year": 2024,
"make": "TOYOTA",
"model": "CAMRY",
"variant": "LE",
"powertrainType": "ice",
"fuelType": "Gasoline",
"transmission": "A8",
"driveType": "FWD",
"engineDisplacementCc": 2487,
"enginePowerKw": 155,
"cylinders": 4,
"combinedLPer100km": 7.1,
"cityLPer100km": 8.4,
"highwayLPer100km": 5.9,
"combinedKwhPer100km": null,
"cityKwhPer100km": null,
"highwayKwhPer100km": null,
"co2GramsPerKm": 166,
"electricRangeKm": null,
"totalRangeKm": null,
"massKg": 1590,
"wheelbaseMm": null,
"testCycle": "epa"
},
{
"id": 54322,
"source": "epa",
"sourceId": "47893",
"year": 2024,
"make": "TOYOTA",
"model": "CAMRY",
"variant": "SE Hybrid",
"powertrainType": "hev",
"fuelType": "Gasoline",
"transmission": "AV-S6",
"driveType": "FWD",
"engineDisplacementCc": 2487,
"enginePowerKw": 160,
"cylinders": 4,
"combinedLPer100km": 4.7,
"cityLPer100km": 4.4,
"highwayLPer100km": 5.0,
"combinedKwhPer100km": null,
"cityKwhPer100km": null,
"highwayKwhPer100km": null,
"co2GramsPerKm": 110,
"electricRangeKm": null,
"totalRangeKm": null,
"massKg": 1665,
"wheelbaseMm": null,
"testCycle": "epa"
}
],
"meta": {
"make": "TOYOTA",
"model": "CAMRY",
"year": 2024,
"count": 2
}
}

Copy
Copied!
GET
/v1/efficiency/:make/:model/:year/:trim
Get efficiency by vehicle with trim

Retrieve efficiency records for a specific make, model, year, and trim. Uses fuzzy matching on trim names. If no exact match is found, falls back to returning all trims for that vehicle.

Path parameters
Name
make
Type
string
Description

Vehicle make (e.g., "Ford"). Case-insensitive.

Name
model
Type
string
Description

Vehicle model (e.g., "Mustang"). Case-insensitive.

Name
year
Type
number
Description

Model year (e.g., 2024).

Name
trim
Type
string
Description

Trim level or variant (e.g., "GT"). Fuzzy matched.

Response metadata
Name
fallback
Type
boolean
Description

true if no trim match was found and all trims were returned instead.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/efficiency/:make/:model/:year/:trim
curl "https://api.cardog.app/v1/efficiency/ford/mustang/2024/GT" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"success": true,
"data": [
{
"id": 67890,
"source": "epa",
"sourceId": "48123",
"year": 2024,
"make": "FORD",
"model": "MUSTANG",
"variant": "GT Coupe",
"powertrainType": "ice",
"fuelType": "Gasoline",
"transmission": "M6",
"driveType": "RWD",
"engineDisplacementCc": 4951,
"enginePowerKw": 357,
"cylinders": 8,
"combinedLPer100km": 13.1,
"cityLPer100km": 15.7,
"highwayLPer100km": 10.2,
"combinedKwhPer100km": null,
"cityKwhPer100km": null,
"highwayKwhPer100km": null,
"co2GramsPerKm": 307,
"electricRangeKm": null,
"totalRangeKm": null,
"massKg": 1773,
"wheelbaseMm": null,
"testCycle": "epa"
}
],
"meta": {
"make": "FORD",
"model": "MUSTANG",
"year": 2024,
"trim": "GT",
"count": 1,
"fallback": false
}
}

Copy
Copied!
GET
/v1/efficiency/stats/overview
Get efficiency statistics

Retrieve aggregated statistics about the efficiency database, including record counts by source, powertrain type, test cycle, year range, and top makes.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/efficiency/stats/overview
curl "https://api.cardog.app/v1/efficiency/stats/overview" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"success": true,
"data": {
"totalRecords": 156234,
"bySource": [
{ "source": "epa", "count": 48721 },
{ "source": "tc", "count": 32456 },
{ "source": "uk-vca", "count": 42891 },
{ "source": "eu-eea", "count": 32166 }
],
"byPowertrain": [
{ "powertrainType": "ice", "count": 98234 },
{ "powertrainType": "hev", "count": 24567 },
{ "powertrainType": "phev", "count": 15234 },
{ "powertrainType": "bev", "count": 14532 },
{ "powertrainType": "mhev", "count": 3456 },
{ "powertrainType": "fcev", "count": 211 }
],
"byTestCycle": [
{ "testCycle": "epa", "count": 81177 },
{ "testCycle": "wltp", "count": 75057 }
],
"yearRange": {
"min": 1984,
"max": 2027
},
"topMakes": [
{ "make": "TOYOTA", "count": 12456 },
{ "make": "FORD", "count": 11234 },
{ "make": "CHEVROLET", "count": 10987 },
{ "make": "BMW", "count": 9876 },
{ "make": "MERCEDES-BENZ", "count": 9234 },
{ "make": "HONDA", "count": 8765 },
{ "make": "NISSAN", "count": 8234 },
{ "make": "VOLKSWAGEN", "count": 7654 },
{ "make": "AUDI", "count": 6543 },
{ "make": "HYUNDAI", "count": 5432 }
]
}
}

Copy
Copied!
Powertrain Types
Name
ice
Type
string
Description

Internal Combustion Engine - Traditional gasoline/diesel vehicles.

Name
bev
Type
string
Description

Battery Electric Vehicle - Fully electric, no combustion engine.

Name
phev
Type
string
Description

Plug-in Hybrid Electric Vehicle - Electric motor with combustion engine backup, can be charged externally.

Name
hev
Type
string
Description

Hybrid Electric Vehicle - Electric motor assists combustion engine, cannot be charged externally.

Name
mhev
Type
string
Description

Mild Hybrid Electric Vehicle - Small electric motor provides limited assist to combustion engine.

Name
fcev
Type
string
Description

Fuel Cell Electric Vehicle - Powered by hydrogen fuel cells.

The efficiency record model

The efficiency record contains comprehensive vehicle efficiency data normalized to metric units.

Identification
Name
id
Type
number
Description

Unique record identifier.

Name
source
Type
string
Description

Data source: epa, tc, uk-vca, or eu-eea.

Name
sourceId
Type
string
Description

Original identifier from the source database.

Name
year
Type
number
Description

Model year.

Name
make
Type
string
Description

Vehicle manufacturer (uppercase).

Name
model
Type
string
Description

Vehicle model name (uppercase).

Name
variant
Type
string | null
Description

Trim level or variant name.

Powertrain
Name
powertrainType
Type
string
Description

Powertrain type. See powertrain types.

Name
fuelType
Type
string
Description

Fuel type (e.g., "Gasoline", "Diesel", "Electricity").

Name
transmission
Type
string | null
Description

Transmission type (e.g., "A8", "M6", "AV-S6").

Name
driveType
Type
string | null
Description

Drive configuration (e.g., "FWD", "RWD", "AWD", "4WD").

Engine specifications
Name
engineDisplacementCc
Type
number | null
Description

Engine displacement in cubic centimeters (cc). Null for BEVs.

Name
enginePowerKw
Type
number | null
Description

Engine/motor power in kilowatts (kW).

Name
cylinders
Type
number | null
Description

Number of cylinders. Null for BEVs.

Fuel consumption
Name
combinedLPer100km
Type
number | null
Description

Combined fuel consumption in liters per 100 kilometers (L/100km).

Name
cityLPer100km
Type
number | null
Description

City fuel consumption in L/100km.

Name
highwayLPer100km
Type
number | null
Description

Highway fuel consumption in L/100km.

Electric consumption
Name
combinedKwhPer100km
Type
number | null
Description

Combined electric consumption in kilowatt-hours per 100 kilometers (kWh/100km).

Name
cityKwhPer100km
Type
number | null
Description

City electric consumption in kWh/100km.

Name
highwayKwhPer100km
Type
number | null
Description

Highway electric consumption in kWh/100km.

Emissions and range
Name
co2GramsPerKm
Type
number | null
Description

CO2 emissions in grams per kilometer (g/km). Zero for BEVs.

Name
electricRangeKm
Type
number | null
Description

Electric-only range in kilometers (km). Applicable to BEVs and PHEVs.

Name
totalRangeKm
Type
number | null
Description

Total range in kilometers (km).

Vehicle specifications
Name
massKg
Type
number | null
Description

Vehicle mass in kilograms (kg).

Name
wheelbaseMm
Type
number | null
Description

Wheelbase in millimeters (mm).

Name
testCycle
Type
string
Description

Test cycle used for measurements: epa, wltp, or mixed.

Was this page helpful?

Yes
No
