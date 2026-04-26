Copy page
Vehicle Recalls

The vehicle recalls API provides access to comprehensive recall information from major regulatory bodies including Transport Canada and NHTSA. Search for recalls by vehicle make, model, year, and other criteria to identify potential safety issues affecting specific vehicles.

GET
/v1/recalls/:country/search
Search recalls

This endpoint allows you to search for vehicle recalls based on various filtering criteria. The search supports filtering by vehicle make, model, year range, and provides pagination and sorting options. Currently supports Transport Canada (CA) and NHTSA (US) data sources.

Required parameters
Name
country
Type
string
Description

Country code for recall data source. Must be either "ca" (Canada) or "us" (United States).

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

Field to sort by: "recall_date", "year", "make", or "model" (default: "recall_date").

Name
order
Type
string
Description

Sort order: "asc" or "desc" (default: "desc").

Transport Canada Example
cURL
@cardog/api
fetch
GET
/v1/recalls/ca/search
curl "https://api.cardog.app/v1/recalls/ca/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "makes=HONDA,TOYOTA" \
 -d "models=CIVIC,CAMRY" \
 -d "yearMin=2020" \
 -d "yearMax=2023" \
 -d "limit=5" \
 -d "sort=recall_date" \
 -d "order=desc"

Copy
Copied!
NHTSA Example
cURL
@cardog/api
fetch
GET
/v1/recalls/us/search
curl "https://api.cardog.app/v1/recalls/us/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "makes=FORD,CHEVROLET" \
 -d "models=F150,SILVERADO" \
 -d "yearMin=2020" \
 -d "yearMax=2023" \
 -d "limit=5" \
 -d "sort=recall_date" \
 -d "order=desc"

Copy
Copied!
Transport Canada Response
{
"success": true,
"provider": "Transport Canada",
"data": [
{
"id": "recall-2023-001",
"recallNumber": "2023-001",
"year": 2023,
"manufacturerRecallNumber": "HONDA-2023-001",
"category": "Safety",
"model": "CIVIC",
"make": "HONDA",
"unitsAffected": 15000,
"systemType": "Brake System",
"notificationType": "Owner Notification",
"comment": "Potential brake fluid leak in master cylinder",
"dateYear": 2023,
"recallDate": "2023-03-15",
"processedAt": "2023-03-20T10:30:00Z",
"provider": "Transport Canada"
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
"limit": 5,
"offset": 0,
"sort": "recall_date",
"order": "desc",
"country": "ca"
},
"count": 1
}
}

Copy
Copied!
NHTSA Response
{
"success": true,
"provider": "NHTSA",
"data": [
{
"manufacturer": "Ford Motor Company",
"nhtsaCampaignNumber": "23V123456",
"parkIt": false,
"parkOutSide": false,
"overTheAirUpdate": false,
"nhtsaActionNumber": "EA23001",
"reportReceivedDate": "2023-01-15",
"component": "Electrical System",
"summary": "Potential electrical system malfunction",
"consequence": "May cause vehicle to stall unexpectedly",
"remedy": "Dealer will inspect and replace affected components",
"notes": "Affects certain 2020-2023 F-150 vehicles",
"modelYear": "2023",
"make": "FORD",
"model": "F150",
"provider": "NHTSA"
}
],
"meta": {
"filter": {
"makes": ["FORD", "CHEVROLET"],
"models": ["F150", "SILVERADO"],
"year": {
"min": 2020,
"max": 2023
},
"limit": 5,
"offset": 0,
"sort": "recall_date",
"order": "desc",
"country": "us"
},
"count": 1
}
}

Copy
Copied!
GET
/v1/recalls/:country/:id
Get recall by ID

Retrieve detailed information about a specific recall by its unique identifier. This endpoint provides comprehensive recall details including affected units, system types, and manufacturer information. Works for both Transport Canada and NHTSA recalls.

Required parameters
Name
country
Type
string
Description

Country code for recall data source. Must be either "ca" (Canada) or "us" (United States).

Name
id
Type
string
Description

The unique identifier of the recall to retrieve.

Transport Canada Example
cURL
JavaScript
GET
/v1/recalls/ca/recall-2023-001
curl "https://api.cardog.app/v1/recalls/ca/recall-2023-001" \
 -H "x-api-key: your-api-key"

Copy
Copied!
NHTSA Example
cURL
JavaScript
GET
/v1/recalls/us/23V123456
curl "https://api.cardog.app/v1/recalls/us/23V123456" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Transport Canada Response
{
"id": "recall-2023-001",
"recallNumber": "2023-001",
"year": 2023,
"manufacturerRecallNumber": "HONDA-2023-001",
"category": "Safety",
"model": "CIVIC",
"make": "HONDA",
"unitsAffected": 15000,
"systemType": "Brake System",
"notificationType": "Owner Notification",
"comment": "Potential brake fluid leak in master cylinder that could result in reduced braking performance. This condition may lead to increased stopping distances and could increase the risk of a crash.",
"dateYear": 2023,
"recallDate": "2023-03-15",
"processedAt": "2023-03-20T10:30:00Z",
"provider": "Transport Canada"
}

Copy
Copied!
NHTSA Response
{
"manufacturer": "Ford Motor Company",
"nhtsaCampaignNumber": "23V123456",
"parkIt": false,
"parkOutSide": false,
"overTheAirUpdate": false,
"nhtsaActionNumber": "EA23001",
"reportReceivedDate": "2023-01-15",
"component": "Electrical System",
"summary": "Potential electrical system malfunction that could cause the vehicle to stall unexpectedly while driving, increasing the risk of a crash.",
"consequence": "May cause vehicle to stall unexpectedly",
"remedy": "Dealer will inspect and replace affected components at no cost to the owner.",
"notes": "Affects certain 2020-2023 F-150 vehicles manufactured between January 2020 and December 2022.",
"modelYear": "2023",
"make": "FORD",
"model": "F150",
"provider": "NHTSA"
}

Copy
Copied!
GET
/v1/recalls/transport-canada/:id
Legacy Transport Canada Endpoint

This legacy endpoint is maintained for backward compatibility. For new applications, use the unified country-based endpoint /v1/recalls/ca/:id.

Retrieve detailed information about a specific Transport Canada recall by its unique identifier.

Required parameters
Name
id
Type
string
Description

The unique identifier of the Transport Canada recall to retrieve.

Request
cURL
JavaScript
GET
/v1/recalls/transport-canada/recall-2023-001
curl "https://api.cardog.app/v1/recalls/transport-canada/recall-2023-001" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"id": "recall-2023-001",
"recallNumber": "2023-001",
"year": 2023,
"manufacturerRecallNumber": "HONDA-2023-001",
"category": "Safety",
"model": "CIVIC",
"make": "HONDA",
"unitsAffected": 15000,
"systemType": "Brake System",
"notificationType": "Owner Notification",
"comment": "Potential brake fluid leak in master cylinder that could result in reduced braking performance. This condition may lead to increased stopping distances and could increase the risk of a crash.",
"dateYear": 2023,
"recallDate": "2023-03-15",
"processedAt": "2023-03-20T10:30:00Z",
"provider": "Transport Canada"
}

Copy
Copied!
Data Sources

The recalls API aggregates data from multiple regulatory sources:

Name
Transport Canada
Type
source
Description

Canadian vehicle safety recalls and defect investigations. Provides comprehensive data on vehicles sold in Canada with detailed recall information including affected units, system types, and manufacturer details.

Name
NHTSA
Type
source
Description

National Highway Traffic Safety Administration (US) recall data. Covers vehicles sold in the United States with campaign numbers, component information, and detailed remedy instructions.

Data Format Differences

Transport Canada and NHTSA use different data formats and field names. The API normalizes responses to provide consistent access while preserving source-specific information.

Transport Canada Fields
Name
recallNumber
Type
string
Description

Transport Canada's unique recall identifier.

Name
manufacturerRecallNumber
Type
string
Description

Manufacturer's internal recall number.

Name
unitsAffected
Type
number
Description

Number of vehicles affected by the recall.

Name
systemType
Type
string
Description

Vehicle system affected by the recall.

NHTSA Fields
Name
nhtsaCampaignNumber
Type
string
Description

NHTSA's unique campaign identifier (e.g., "23V123456").

Name
nhtsaActionNumber
Type
string
Description

NHTSA's action number for the recall.

Name
component
Type
string
Description

Specific component affected by the recall.

Name
parkIt
Type
boolean
Description

Whether the vehicle should be parked until repaired.

Name
parkOutSide
Type
boolean
Description

Whether the vehicle should be parked outside until repaired.

Name
overTheAirUpdate
Type
boolean
Description

Whether the issue can be resolved via over-the-air software update.

Recall Categories

Recalls are categorized by severity and type:

Name
Safety
Type
category
Description

Critical safety issues that could affect vehicle operation or occupant safety.

Name
Compliance
Type
category
Description

Issues related to regulatory compliance or emissions standards.

Name
Service
Type
category
Description

Non-critical issues that may affect vehicle performance or convenience features.

System Types

Common vehicle systems that may be subject to recalls:

Name
Brake System
Type
system
Description

Brakes, brake fluid, brake lines, and related components.

Name
Air Bag System
Type
system
Description

Air bag deployment, sensors, and control modules.

Name
Engine System
Type
system
Description

Engine components, fuel system, and emissions controls.

Name
Electrical System
Type
system
Description

Wiring, lighting, and electronic control systems.

Name
Steering System
Type
system
Description

Steering components, power steering, and steering columns.

Name
Suspension System
Type
system
Description

Suspension components, shocks, and structural elements.

Error Responses

The API returns structured error responses when requests fail:

Invalid Country Error
{
"success": false,
"error": "Invalid country parameter"
}

Copy
Copied!
Recall Not Found Error
{
"success": false,
"error": "Recall not found"
}

Copy
Copied!
Use Cases
Name
Vehicle Safety Checks
Type
use-case
Description

Check if a specific vehicle has any active recalls before purchase or during ownership.

Name
Fleet Management
Type
use-case
Description

Monitor recalls across multiple vehicles in a fleet to ensure compliance and safety.

Name
Dealership Operations
Type
use-case
Description

Identify vehicles with recalls during inventory management and customer service.

Name
Consumer Applications
Type
use-case
Description

Provide recall information to vehicle owners through mobile apps or websites.

Name
Regulatory Compliance
Type
use-case
Description

Track and report recall data for regulatory and safety monitoring purposes.

Name
Cross-Border Operations
Type
use-case
Description

Monitor recalls for vehicles operating in both Canada and the United States.

Rate Limits

The recalls API is subject to rate limiting to ensure fair usage. Please implement appropriate retry logic with exponential backoff for production applications.

Next Steps
Explore VIN decoding to identify specific vehicles
Learn about vehicle research capabilities
Check out our market analysis tools
Review the complete API documentation

Was this page helpful?

Yes
No
