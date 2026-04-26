Copy page
Vehicle Research

The Vehicle Research API provides comprehensive access to detailed vehicle specifications, variants, images, and color information. Use these endpoints to retrieve accurate and up-to-date vehicle data for any make, model, and year combination.

Supported Regions

The Research API provides vehicle data from three market regions:

Region Code Coverage
United States US 1990-present
Canada CA 1990-present
Europe EU 1950-present

Use the region query parameter to filter results by market. If omitted, results from all regions are returned.

The Vehicle Variant Model

The vehicle variant model contains detailed specifications and information about a specific vehicle configuration.

Properties
Name
id
Type
string
Description

Unique identifier for the vehicle variant.

Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Name
year
Type
integer
Description

Model year of the vehicle.

Name
trim
Type
string
Description

Specific trim level of the vehicle.

Name
msrp
Type
number
Description

Manufacturer's suggested retail price.

Name
styleName
Type
string
Description

Full style name including trim and transmission.

Name
region
Type
string
Description

Market region code: US, CA, or EU.

Name
bodyStyle
Type
string
Description

Body style description.

Name
spec
Type
object
Description

Detailed vehicle specifications. Structure varies by region - see below.

Spec Structure by Region

The spec object structure differs by region:

North American Spec (US/CA)

Specs are grouped by category with detailed feature flags:

{
"Fuel": {
"Fuel": "Gasoline",
"Fuel Capacity": "56.0 L",
"Fuel Consumption: City": "8.1 L/100 km",
"Fuel Consumption: Highway": "6.4 L/100 km"
},
"Safety": {
"Brake": "4 Wheel Disc",
"Driver Air Bag": true,
"Brake ABS System": true,
"Stability Control": true
},
"Mechanical": {
"Engine": "Intercooled Turbo Regular Unleaded I-4",
"Torque": "192 ft-lb @ 1700 rpm",
"Horsepower": "192 hp @ 6000 rpm",
"Drivetrain": "Front-Wheel Drive",
"Transmission": "Continuously Variable"
},
"Exterior": {
"Wheelbase": "2,830 mm",
"Height, Overall": "1,448 mm",
"Length, Overall": "4,971 mm",
"Base Curb Weight": "1,493 kg"
},
"Interior": {
"Trunk Volume": "473 L",
"Passenger Capacity": "5",
"Front Leg Room": "1,074 mm"
},
"Comfort": { ... },
"Lighting": { ... },
"Security": { ... },
"Convenience": { ... },
"Connectivity": { ... },
"Infotainment": { ... },
"DriverAssistance": { ... }
}

Copy
Copied!
European Spec (EU)

Specs use a flat structure with numeric values in metric units:

{
"horsepower": 170,
"torque": 250,
"displacement": 1597,
"cylinders": 4,
"fuelType": "gasoline",
"driveType": "RWD",
"transmission": "automatic",
"acceleration0to100": 7.7,
"topSpeed": 232,
"weight": 1450,
"length": 4709,
"width": 1827,
"height": 1435,
"wheelbase": 2851,
"fuelTank": 59,
"seats": 5,
"doors": 4,
"fuelUrban": 7.7,
"fuelHighway": 5.6,
"fuelCombined": 6.4,
"co2": 147
}

Copy
Copied!
Field Unit Description
horsepower Hp Engine power
torque Nm Engine torque
displacement cc Engine displacement
acceleration0to100 sec 0-100 km/h time
topSpeed km/h Maximum speed
weight kg Curb weight
length, width, height, wheelbase mm Dimensions
fuelTank L Fuel tank capacity
fuelUrban, fuelHighway, fuelCombined L/100km Fuel consumption
co2 g/km CO2 emissions
EU Extra Metadata

EU variants include additional metadata in the extra field:

Name
generation
Type
string
Description

Model generation name (e.g., "3 Series Sedan (G20)").

Name
yearStart
Type
number
Description

Production start year.

Name
yearEnd
Type
number
Description

Production end year (if discontinued).

Name
rawSpecs
Type
object
Description

Original detailed specifications with 50+ fields.

The Vehicle Image Model
Name
id
Type
integer
Description

Unique identifier for the image.

Name
url
Type
string
Description

URL path to the image resource.

Name
width
Type
integer
Description

Image width in pixels.

Name
height
Type
integer
Description

Image height in pixels.

Name
type
Type
string
Description

Type of image asset.

Name
metadata
Type
object
Description

Additional image metadata including category, provider, and shot type.

The Vehicle Colors Model
Name
EXTERIOR
Type
array
Description

Array of available exterior colors.

Name
name
Type
string
Description

Color name/description.

Name
rgb
Type
string
Description

RGB color values.

Name
hex
Type
string
Description

Hexadecimal color code.

Name
INTERIOR
Type
array
Description

Array of available interior colors with the same structure as exterior colors.

GET
/v1/research/make/:make
Get variants by make

Retrieve all vehicle variants for a specific manufacturer.

Required parameters
Name
make
Type
string
Description

The vehicle manufacturer name.

Optional query parameters
Name
region
Type
string
Description

Filter by market region: US, CA, or EU.

Request
cURL
JavaScript
Python
GET
/v1/research/make/:make
curl "https://api.cardog.app/v1/research/make/bmw?region=EU" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"make": "BMW",
"model": "i3",
"year": 2020,
"trim": "42.2 kWh (170 Hp)",
"msrp": null,
"styleName": "42.2 kWh (170 Hp)",
"region": "EU",
"bodyStyle": "Hatchback"
}
]

Copy
Copied!
GET
/v1/research/lineup/:make
Get vehicle lineup

Retrieve the complete vehicle lineup for a specific manufacturer, including models, years, MSRPs, and associated images.

Required parameters
Name
make
Type
string
Description

The vehicle manufacturer name.

Optional query parameters
Name
region
Type
string
Description

Filter by market region: US, CA, or EU.

Name
page
Type
integer
Description

Page number for pagination (default: 1).

Name
limit
Type
integer
Description

Results per page, max 500 (default: 200).

Name
bodyStyle
Type
string
Description

Filter by body style (e.g., "Sedan", "SUV").

Request
cURL
@cardog/api
fetch
Python
GET
/v1/research/lineup/:make
curl "https://api.cardog.app/v1/research/lineup/bmw?region=EU&limit=10" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"data": [
{
"make": "Honda",
"model": "Accord",
"year": 2025,
"msrp": 28295,
"images": [
{
"url": "https://images.cardog.io/research/honda/accord/2025/2025-honda-accord-touring-hybrid-sedan-exterior-shown.jpg",
"make": "Honda",
"model": "Accord",
"year": 2025,
"shotType": "FQ",
"category": "EXTERIOR",
"title": "2025 Honda Accord Touring Hybrid Sedan Exterior Shown",
"color": "Canyon River Blue Metallic"
}
]
}
],
"total": 50,
"page": 1,
"limit": 50
}

Copy
Copied!
GET
/v1/research/vin/:vin
Get variant by VIN

Retrieve vehicle variant information using a VIN (Vehicle Identification Number).

Required parameters
Name
vin
Type
string
Description

17-character Vehicle Identification Number.

Request
cURL
JavaScript
Python
GET
/v1/research/vin/:vin
curl "https://api.cardog.app/v1/research/vin/1HGCM82633A123456" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"error": "Variant not found"
}

Copy
Copied!
GET
/v1/research/make/:make/model/:model
Get variants by make and model

Retrieve all variants for a specific make and model combination.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Optional query parameters
Name
region
Type
string
Description

Filter by market region: US, CA, or EU.

Request
cURL
JavaScript
Python
GET
/v1/research/make/:make/model/:model
curl "https://api.cardog.app/v1/research/make/porsche/model/911?region=EU" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"id": "Pors-20EU-a1b2c3d4e5f6",
"make": "Porsche",
"model": "911",
"year": 2020,
"trim": "Carrera (385 Hp)",
"msrp": null,
"styleName": "Carrera (385 Hp)",
"region": "EU",
"bodyStyle": "Coupe"
}
]

Copy
Copied!
GET
/v1/research/make/:make/year/:year
Get variants by make and year

Retrieve all variants for a specific make and year combination.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
year
Type
integer
Description

Model year.

Optional query parameters
Name
region
Type
string
Description

Filter by market region: US, CA, or EU.

Request
cURL
JavaScript
Python
GET
/v1/research/make/:make/year/:year
curl "https://api.cardog.app/v1/research/make/bmw/year/2020?region=EU" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"id": "BMW-20EU-5a258a444187",
"make": "BMW",
"model": "iX3",
"year": 2020,
"trim": "80 kWh (286 Hp)",
"msrp": null,
"styleName": "80 kWh (286 Hp)",
"region": "EU",
"bodyStyle": "SUV"
}
]

Copy
Copied!
GET
/v1/research/make/:make/model/:model/year/:year
Get variants by make, model and year

Retrieve all variants for a specific make, model, and year combination. Returns full spec details.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Name
year
Type
integer
Description

Model year.

Optional query parameters
Name
region
Type
string
Description

Filter by market region: US, CA, or EU.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/research/make/:make/model/:model/year/:year
curl "https://api.cardog.app/v1/research/make/bmw/model/i3/year/2020?region=EU" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response (EU)
[
{
"id": "BMW-20EU-afb9e7cb02a1",
"make": "BMW",
"model": "i3",
"year": 2020,
"trim": "42.2 kWh (170 Hp)",
"msrp": null,
"styleName": "42.2 kWh (170 Hp)",
"region": "EU",
"bodyStyle": "Hatchback",
"spec": {
"doors": 5,
"seats": 4,
"width": 1775,
"height": 1598,
"length": 4011,
"weight": 1270,
"fuelType": "electric",
"topSpeed": 150,
"driveType": "RWD",
"wheelbase": 2570,
"transmission": "automatic",
"acceleration0to100": 7.3
}
}
]

Copy
Copied!
GET
/v1/research/images/profiles/:make/:model/:year
Get profile images

Retrieve profile images for a specific vehicle make, model, and year combination.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Name
year
Type
integer
Description

Model year.

Request
cURL
JavaScript
Python
GET
/v1/research/images/profiles/:make/:model/:year
curl "https://api.cardog.app/v1/research/images/profiles/honda/accord/2020" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"id": 4065483,
"listingId": null,
"type": "image",
"url": "https://images.cardog.io/profiles/Honda/Accord Hybrid/2018/Honda Accord Hybrid Touring Sedan Profile Shown.png",
"make": null,
"model": null,
"year": null,
"width": 1280,
"height": 854,
"sizeBytes": null,
"mimeType": "image/png",
"sourceUri": "/Honda/Accord Hybrid/2018/Honda Accord Hybrid Touring Sedan Profile Shown.png",
"sourceType": "bg-removed",
"metadata": {
"make": "Honda",
"tags": "honda-accord-hybrid,honda,2021-honda-accord-hybrid",
"year": "2021",
"model": "Accord Hybrid",
"category": "EXTERIOR",
"provider": "OEM",
"shot_type": "S",
"submodels": "Accord Hybrid Sedan",
"model_year_id": 401874145
},
"createdAt": "2025-03-08T20:29:14.069Z",
"updatedAt": "2025-03-08T20:29:14.069Z",
"removedAt": null
}
]

Copy
Copied!
GET
/v1/research/images
Get vehicle images

Retrieve images for a specific vehicle make, model, and year combination.

Required query parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Name
year
Type
integer
Description

Model year.

Optional query parameters
Name
limit
Type
integer
Description

Maximum number of images to return (default: 10).

Request
cURL
@cardog/api
fetch
Python
GET
/v1/research/images?make=Honda&model=Accord&year=2020&limit=5
curl "https://api.cardog.app/v1/research/images?make=Honda&model=Accord&year=2020&limit=5" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"id": 4065483,
"type": "image",
"url": "https://images.cardog.io/profiles/Honda/Accord Hybrid/2018/Honda Accord Hybrid Touring Sedan Profile Shown.png",
"width": 1280,
"height": 854,
"metadata": {
"make": "Honda",
"model": "Accord Hybrid",
"year": "2021",
"category": "EXTERIOR",
"provider": "OEM",
"shot_type": "S"
}
}
]

Copy
Copied!
GET
/v1/research/colors
Get vehicle colors

Retrieve available exterior and interior colors for a specific vehicle.

Required query parameters
Name
make
Type
string
Description

Vehicle manufacturer name.

Name
model
Type
string
Description

Vehicle model name.

Name
year
Type
integer
Description

Model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/research/colors
curl "https://api.cardog.app/v1/research/colors?make=Honda&model=Accord&year=2020" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"EXTERIOR": [
{
"name": "Platinum White Pearl",
"rgb": "231,233,232",
"hex": "#E7E9E8"
},
{
"name": "Crystal Black Pearl",
"rgb": "29,29,29",
"hex": "#1D1D1D"
},
{
"name": "Modern Steel Metallic",
"rgb": "51,59,72",
"hex": "#333B48"
},
{
"name": "Radiant Red Metallic",
"rgb": "115,18,33",
"hex": "#731221"
}
],
"INTERIOR": [
{
"name": "Black, cloth",
"rgb": "25,25,25",
"hex": "#191919"
},
{
"name": "Gray, cloth",
"rgb": "134,132,131",
"hex": "#868483"
},
{
"name": "Ivory, cloth",
"rgb": "222,214,195",
"hex": "#DED6C3"
},
{
"name": "Black, leather",
"rgb": "25,25,25",
"hex": "#191919"
}
]
}

Copy
Copied!

Was this page helpful?

Yes
No
