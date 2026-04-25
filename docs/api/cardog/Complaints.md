Copy page
Vehicle Complaints

The vehicle complaints API provides access to comprehensive complaint information from the National Highway Traffic Safety Administration (NHTSA). Search for complaints by vehicle make, model, year, component, and other criteria to identify potential safety issues and patterns affecting specific vehicles.

GET
/v1/complaints/search
Search complaints

This endpoint allows you to search for NHTSA vehicle complaints based on various filtering criteria. The search supports filtering by vehicle make, model, year range, components, states, and provides pagination and sorting options.

Optional parameters
Name
makes
Type
string
Description

Comma-separated list of vehicle makes to filter by (e.g., "HONDA,TOYOTA,FORD").

Name
models
Type
string
Description

Comma-separated list of vehicle models to filter by (e.g., "CIVIC,CAMRY,F150").

Name
yearMin
Type
number
Description

Minimum model year for filtering (default: 1900).

Name
yearMax
Type
number
Description

Maximum model year for filtering (default: current year + 2).

Name
components
Type
string
Description

Comma-separated list of components to filter by (e.g., "Brake System,Air Bag System").

Name
states
Type
string
Description

Comma-separated list of states to filter by (e.g., "CA,TX,NY").

Name
productTypes
Type
string
Description

Comma-separated list of product types: "V" (Vehicle), "T" (Tires), "E" (Equipment), "C" (Child Restraint).

Name
crashInvolved
Type
boolean
Description

Filter for complaints involving crashes (true/false).

Name
fireInvolved
Type
boolean
Description

Filter for complaints involving fires (true/false).

Name
hasInjuries
Type
boolean
Description

Filter for complaints with injuries (true/false).

Name
hasDeaths
Type
boolean
Description

Filter for complaints with deaths (true/false).

Name
dateStart
Type
string
Description

Start date for incident date range (YYYY-MM-DD format).

Name
dateEnd
Type
string
Description

End date for incident date range (YYYY-MM-DD format).

Name
limit
Type
number
Description

Maximum number of results to return (default: 10).

Name
offset
Type
number
Description

Number of results to skip for pagination (default: 0).

Name
sort
Type
string
Description

Field to sort by: "incident_date", "date_received", "year", "make", "model", "component_description", "injured_count", "deaths_count" (default: "date_received").

Name
order
Type
string
Description

Sort order: "asc" or "desc" (default: "desc").

Basic Search
cURL
@cardog/api
fetch
GET
/v1/complaints/search
curl "https://api.cardog.app/v1/complaints/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "makes=HONDA,TOYOTA" \
 -d "models=CIVIC,CAMRY" \
 -d "yearMin=2020" \
 -d "yearMax=2023" \
 -d "limit=5" \
 -d "sort=date_received" \
 -d "order=desc"

Copy
Copied!
Advanced Search
cURL
@cardog/api
fetch
GET
/v1/complaints/search
curl "https://api.cardog.app/v1/complaints/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "components=Brake System,Air Bag System" \
 -d "crashInvolved=true" \
 -d "hasInjuries=true" \
 -d "states=CA,TX,NY" \
 -d "dateStart=2023-01-01" \
 -d "dateEnd=2023-12-31" \
 -d "limit=10"

Copy
Copied!
Response
{
"success": true,
"provider": "NHTSA",
"data": [
{
"complaintId": 1234567,
"odiNumber": "ODI-2023-001234",
"manufacturerName": "Honda Motor Co., Ltd.",
"make": "HONDA",
"model": "CIVIC",
"year": 2023,
"vin": "1HGCV1F34PA123456",
"crashInvolved": true,
"incidentDate": "2023-06-15",
"fireInvolved": false,
"injuredCount": 1,
"deathsCount": 0,
"componentDescription": "Brake System",
"consumerCity": "Los Angeles",
"consumerState": "CA",
"dateAdded": "2023-07-01",
"dateReceived": "2023-06-20",
"mileage": 15000,
"occurrences": 1,
"complaintDescription": "Brake pedal went to floor while driving, causing extended stopping distance",
"complaintType": "Safety",
"policeReportFiled": true,
"purchaseDate": "2023-01-15",
"originalOwner": true,
"antiLockBrakes": true,
"cruiseControl": true,
"numCylinders": 4,
"driveTrain": "FWD",
"fuelSystem": "FI",
"fuelType": "GS",
"transmissionType": "AUTO",
"vehicleSpeed": 45,
"dotTireId": null,
"tireSize": null,
"tireLocation": null,
"tireFailureType": null,
"originalEquipment": true,
"manufactureDate": "2022-12-01",
"seatType": null,
"restraintType": null,
"dealerName": "Honda of Downtown LA",
"dealerTelephone": "213-555-0123",
"dealerCity": "Los Angeles",
"dealerState": "CA",
"dealerZip": "90012",
"productType": "V",
"repaired": false,
"medicalAttention": true,
"vehicleTowed": true,
"processedAt": "2023-07-01T10:30:00Z",
"provider": "NHTSA"
}
],
"meta": {
"filter": {
"makes": ["HONDA", "TOYOTA"],
"models": ["CIVIC", "CAMRY"],
"year": {
"min": 2020,
"max": 2023
},
"components": [],
"states": [],
"productTypes": [],
"crashInvolved": null,
"fireInvolved": null,
"hasInjuries": null,
"hasDeaths": null,
"dateRange": {
"start": null,
"end": null
},
"limit": 5,
"offset": 0,
"sort": "date_received",
"order": "desc"
},
"count": 1
}
}

Copy
Copied!
GET
/v1/complaints/:id
Get complaint by ID

Retrieve detailed information about a specific complaint by its unique identifier. This endpoint provides comprehensive complaint details including incident information, vehicle details, and consumer information.

Required parameters
Name
id
Type
number
Description

The unique identifier of the complaint to retrieve.

Request
cURL
@cardog/api
fetch
GET
/v1/complaints/1234567
curl "https://api.cardog.app/v1/complaints/1234567" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"success": true,
"data": {
"complaintId": 1234567,
"odiNumber": "ODI-2023-001234",
"manufacturerName": "Honda Motor Co., Ltd.",
"make": "HONDA",
"model": "CIVIC",
"year": 2023,
"vin": "1HGCV1F34PA123456",
"crashInvolved": true,
"incidentDate": "2023-06-15",
"fireInvolved": false,
"injuredCount": 1,
"deathsCount": 0,
"componentDescription": "Brake System",
"consumerCity": "Los Angeles",
"consumerState": "CA",
"dateAdded": "2023-07-01",
"dateReceived": "2023-06-20",
"mileage": 15000,
"occurrences": 1,
"complaintDescription": "Brake pedal went to floor while driving, causing extended stopping distance. Vehicle was traveling at approximately 45 mph when the brake pedal suddenly went to the floor. Driver was able to stop the vehicle using the emergency brake, but the incident resulted in a minor collision with another vehicle. Driver sustained minor injuries and was treated at the scene. Police report was filed and vehicle was towed to a repair facility.",
"complaintType": "Safety",
"policeReportFiled": true,
"purchaseDate": "2023-01-15",
"originalOwner": true,
"antiLockBrakes": true,
"cruiseControl": true,
"numCylinders": 4,
"driveTrain": "FWD",
"fuelSystem": "FI",
"fuelType": "GS",
"transmissionType": "AUTO",
"vehicleSpeed": 45,
"dotTireId": null,
"tireSize": null,
"tireLocation": null,
"tireFailureType": null,
"originalEquipment": true,
"manufactureDate": "2022-12-01",
"seatType": null,
"restraintType": null,
"dealerName": "Honda of Downtown LA",
"dealerTelephone": "213-555-0123",
"dealerCity": "Los Angeles",
"dealerState": "CA",
"dealerZip": "90012",
"productType": "V",
"repaired": false,
"medicalAttention": true,
"vehicleTowed": true,
"processedAt": "2023-07-01T10:30:00Z",
"provider": "NHTSA"
}
}

Copy
Copied!
GET
/v1/complaints/stats/overview
Get complaint statistics

Retrieve comprehensive statistics about NHTSA complaints including totals, top makes, components, and states. This endpoint provides aggregated data for analysis and reporting.

Request
cURL
@cardog/api
fetch
GET
/v1/complaints/stats/overview
curl "https://api.cardog.app/v1/complaints/stats/overview" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"success": true,
"data": {
"totalComplaints": 1542034,
"complaintsWithInjuries": 234567,
"complaintsWithDeaths": 12345,
"complaintsWithCrashes": 456789,
"complaintsWithFires": 67890,
"topMakes": [
{ "make": "FORD", "count": 156789 },
{ "make": "TOYOTA", "count": 145678 },
{ "make": "HONDA", "count": 134567 },
{ "make": "CHEVROLET", "count": 123456 },
{ "make": "NISSAN", "count": 112345 }
],
"topComponents": [
{ "component": "Brake System", "count": 234567 },
{ "component": "Air Bag System", "count": 198765 },
{ "component": "Engine System", "count": 187654 },
{ "component": "Electrical System", "count": 176543 },
{ "component": "Steering System", "count": 165432 }
],
"topStates": [
{ "state": "CA", "count": 234567 },
{ "state": "TX", "count": 198765 },
{ "state": "FL", "count": 187654 },
{ "state": "NY", "count": 176543 },
{ "state": "PA", "count": 165432 }
]
}
}

Copy
Copied!
Additional statistics endpoints
Get top makes

Retrieve the top vehicle makes by complaint count.

Get top components

Retrieve the top components by complaint count.

Get top states

Retrieve the top states by complaint count.

Data Fields

The complaints API provides comprehensive data fields for each complaint:

Name
complaintId
Type
number
Description

Unique NHTSA complaint identifier.

Name
odiNumber
Type
string
Description

NHTSA ODI (Office of Defects Investigation) number.

Name
make
Type
string
Description

Vehicle make (e.g., "HONDA", "TOYOTA").

Name
model
Type
string
Description

Vehicle model (e.g., "CIVIC", "CAMRY").

Name
year
Type
number
Description

Vehicle model year.

Name
vin
Type
string
Description

Vehicle Identification Number.

Name
crashInvolved
Type
boolean
Description

Whether the complaint involved a crash.

Name
fireInvolved
Type
boolean
Description

Whether the complaint involved a fire.

Name
injuredCount
Type
number
Description

Number of people injured in the incident.

Name
deathsCount
Type
number
Description

Number of people killed in the incident.

Name
componentDescription
Type
string
Description

Description of the component involved in the complaint.

Name
complaintDescription
Type
string
Description

Detailed description of the complaint.

Name
consumerCity
Type
string
Description

City where the complaint originated.

Name
consumerState
Type
string
Description

State where the complaint originated.

Name
incidentDate
Type
string
Description

Date when the incident occurred.

Name
dateReceived
Type
string
Description

Date when NHTSA received the complaint.

Name
mileage
Type
number
Description

Vehicle mileage at the time of the incident.

Name
policeReportFiled
Type
boolean
Description

Whether a police report was filed.

Name
medicalAttention
Type
boolean
Description

Whether medical attention was required.

Name
vehicleTowed
Type
boolean
Description

Whether the vehicle was towed.

Product Types

Complaints are categorized by product type:

Name
V
Type
product-type
Description

Vehicle complaints.

Name
T
Type
product-type
Description

Tire complaints.

Name
E
Type
product-type
Description

Equipment complaints.

Name
C
Type
product-type
Description

Child restraint complaints.

Was this page helpful?

Yes
No
