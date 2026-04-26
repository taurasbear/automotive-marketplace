Copy page
EV Charging Stations

The EV charging station endpoints provide access to a comprehensive database of electric vehicle charging locations. Using these endpoints, you can search for charging stations by location, filter by connector types, and get detailed station information including real-time availability.

GET
/v1/charging
Search charging stations

This endpoint allows you to search for EV charging stations based on location and various filtering criteria. The response includes detailed information about each station, including available connectors and operational status.

Required parameters
Name
country
Type
string
Description

Two-letter country code (e.g., "US", "CA").

Optional parameters
Name
latitude
Type
number
Description

Latitude coordinate for location-based search.

Name
longitude
Type
number
Description

Longitude coordinate for location-based search.

Name
radius
Type
number
Description

Search radius in kilometers (default: 10).

Name
region
Type
string
Description

State/province code for regional search.

Name
connector
Type
string
Description

Filter by connector type (type1, type2, ccs1, ccs2, chademo, tesla, nacs).

Name
minPower
Type
number
Description

Minimum power output in kilowatts.

Name
limit
Type
number
Description

Maximum number of results to return (default: 20).

Name
includeInactive
Type
boolean
Description

Include non-operational stations.

Name
includePhotos
Type
boolean
Description

Include station photos in response.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/charging
curl "https://api.cardog.app/v1/charging" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "country=US" \
 -d "latitude=37.7749" \
 -d "longitude=-122.4194" \
 -d "radius=10" \
 -d "connector=ccs1" \
 -d "minPower=50" \
 -d "limit=2"

Copy
Copied!
Response
{
"stations": [
{
"id": "460571",
"name": "Exxon Utica ",
"address": {
"addressLine1": "11335 Hall Rd",
"addressLine2": null,
"town": "Utica",
"stateOrProvince": "MI",
"postcode": "48317",
"countryCode": "US"
},
"location": {
"latitude": 42.62658474046012,
"longitude": -83.0174563876152
},
"connections": [
{
"connectionTypeId": 32,
"connectionType": "CCS Type 2",
"powerKW": 240,
"currentType": "DC",
"level": 3,
"voltage": 1000,
"amperage": 400
}
],
"status": "Operational",
"network": null,
"operator": null,
"dateCreated": "2025-08-25T20:29:00Z",
"dateLastVerified": "2025-08-25T20:29:00Z",
"usage": "Public",
"supportPhone": "3132626722",
"supportUrl": null,
"isMembershipRequired": false,
"isPayAtLocation": false
},
],
"totalCount": 20,
"location": {
"name": "Utica",
"countryCode": "US",
"regionCode": null
}
}

Copy
Copied!

Was this page helpful?

Yes
No
