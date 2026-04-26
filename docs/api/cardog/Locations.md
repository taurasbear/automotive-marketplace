Copy page
Vehicle Sellers & Locations

The locations endpoints provide access to a comprehensive database of vehicle sellers, including dealerships, individual sellers, and auction houses. Using these endpoints, you can search for sellers by geographic location, filter by seller type, and get detailed information about their inventory and contact details.

GET
/v1/locations/search
Search sellers by location

This endpoint allows you to search for vehicle sellers based on geographic location and various filtering criteria. The response includes detailed information about each seller, their address, dealership affiliation (if applicable), and active vehicle listings count.

Required parameters
Name
lat
Type
number
Description

Latitude coordinate for location-based search (-90 to 90).

Name
lng
Type
number
Description

Longitude coordinate for location-based search (-180 to 180).

Optional parameters
Name
radius
Type
number
Description

Search radius in kilometers (default: 50, max: 500).

Name
name
Type
string
Description

Filter by seller or dealership name (case-insensitive partial match).

Name
sellerType
Type
string
Description

Filter by seller type: individual, dealership, auction, or aggregator.

Name
websiteDomain
Type
string
Description

Filter by website domain (e.g., "autotrader.ca").

Name
hasActiveListings
Type
boolean
Description

Only include sellers with active vehicle listings (default: false).

Name
limit
Type
number
Description

Maximum number of results to return (default: 50, max: 100).

Name
offset
Type
number
Description

Number of results to skip for pagination (default: 0).

Request
cURL
@cardog/api
fetch
Python
GET
/v1/locations/search
curl "https://api.cardog.app/v1/locations/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "lat=43.6532" \
 -d "lng=-79.3832" \
 -d "radius=25" \
 -d "name=Toyota" \
 -d "sellerType=dealership" \
 -d "hasActiveListings=true" \
 -d "limit=10"

Copy
Copied!
Response
{
"results": [
{
"id": "d904fcbb-8859-4a60-a1b1-7f250e77fe62",
"name": "Downtown Toyota",
"type": "dealership",
"email": "sales@downtowntoyota.com",
"phone": "+1-416-555-0123",
"address": {
"id": 1234,
"street": "123 Queen St W",
"city": "Toronto",
"state": "ON",
"postalCode": "M5H 2M9",
"country": "CA",
"latitude": "43.6532",
"longitude": "-79.3832"
},
"dealership": {
"id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
"name": "Downtown Toyota",
"primaryDomain": "downtowntoyota.com"
},
"totalActiveListings": 127,
"distance": 2.3
},
{
"id": "f1e2d3c4-b5a6-9870-fedc-ba0987654321",
"name": "John Smith",
"type": "individual",
"email": "john.smith@email.com",
"phone": "+1-416-555-0456",
"address": {
"id": 5678,
"street": "456 Main St",
"city": "Toronto",
"state": "ON",
"postalCode": "M4E 1A1",
"country": "CA",
"latitude": "43.6700",
"longitude": "-79.3500"
},
"dealership": null,
"totalActiveListings": 3,
"distance": 5.7
}
],
"meta": {
"total": 2,
"params": {
"lat": 43.6532,
"lng": -79.3832,
"radius": 25,
"name": "Toyota",
"sellerType": "dealership",
"hasActiveListings": true,
"limit": 10,
"offset": 0
}
}
}

Copy
Copied!
GET
/v1/locations/:id
Get seller details

Retrieve detailed information about a specific seller by their unique identifier. This endpoint returns comprehensive seller information including contact details, address, dealership affiliation, and active listings count.

Path parameters
Name
id
Type
string
Description

The unique UUID identifier of the seller.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/locations/:id
curl "https://api.cardog.app/v1/locations/d904fcbb-8859-4a60-a1b1-7f250e77fe62" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"id": "d904fcbb-8859-4a60-a1b1-7f250e77fe62",
"name": "Downtown Toyota",
"type": "dealership",
"email": "sales@downtowntoyota.com",
"phone": "+1-416-555-0123",
"address": {
"id": 1234,
"street": "123 Queen St W",
"city": "Toronto",
"state": "ON",
"postalCode": "M5H 2M9",
"country": "CA",
"latitude": "43.6532",
"longitude": "-79.3832"
},
"dealership": {
"id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
"name": "Downtown Toyota",
"primaryDomain": "downtowntoyota.com"
},
"totalActiveListings": 127
}

Copy
Copied!
Response objects
Seller object
Name
id
Type
string
Description

Unique identifier for the seller.

Name
name
Type
string
Description

Display name of the seller or dealership.

Name
type
Type
string
Description

Type of seller: individual, dealership, auction, or aggregator.

Name
email
Type
string | null
Description

Contact email address (may be null for privacy).

Name
phone
Type
string | null
Description

Contact phone number (may be null for privacy).

Name
address
Type
object | null
Description

Physical address information (see Address object below).

Name
dealership
Type
object | null
Description

Dealership information if seller is affiliated with a dealership.

Name
totalActiveListings
Type
number
Description

Number of active vehicle listings from this seller.

Name
distance
Type
number
Description

Distance from search coordinates in kilometers (only in search results).

Address object
Name
id
Type
number | null
Description

Unique identifier for the address.

Name
street
Type
string | null
Description

Street address line.

Name
city
Type
string | null
Description

City name.

Name
state
Type
string | null
Description

State or province code.

Name
postalCode
Type
string | null
Description

ZIP or postal code.

Name
country
Type
string | null
Description

Country code (e.g., "US", "CA").

Name
latitude
Type
string | null
Description

Latitude coordinate as string.

Name
longitude
Type
string | null
Description

Longitude coordinate as string.

Dealership object
Name
id
Type
string
Description

Unique identifier for the dealership.

Name
name
Type
string
Description

Official dealership name.

Name
primaryDomain
Type
string | null
Description

Primary website domain for the dealership.

Error responses

The locations API uses standard HTTP status codes and returns detailed error information:

Name
400 Bad Request
Description

Invalid parameters (e.g., coordinates out of range, invalid seller type).

Name
404 Not Found
Description

Seller with the specified ID does not exist.

Name
500 Internal Server Error
Description

Server error occurred while processing the request.

Error Response Example
{
"error": "Validation Error",
"message": "Invalid request parameters",
"details": [
{
"field": "lat",
"message": "Latitude must be between -90 and 90"
}
]
}

Copy
Copied!
Usage examples
Find Toyota dealerships in Toronto
curl "https://api.cardog.app/v1/locations/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "lat=43.6532" \
 -d "lng=-79.3832" \
 -d "radius=50" \
 -d "name=Toyota" \
 -d "sellerType=dealership" \
 -d "hasActiveListings=true"

Copy
Copied!
Find all sellers with active listings nearby
curl "https://api.cardog.app/v1/locations/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "lat=40.7128" \
 -d "lng=-74.0060" \
 -d "radius=25" \
 -d "hasActiveListings=true" \
 -d "limit=20"

Copy
Copied!
Paginate through results

# First page

curl "https://api.cardog.app/v1/locations/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "lat=34.0522" \
 -d "lng=-118.2437" \
 -d "limit=50" \
 -d "offset=0"

# Second page

curl "https://api.cardog.app/v1/locations/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "lat=34.0522" \
 -d "lng=-118.2437" \
 -d "limit=50" \
 -d "offset=50"

Copy
Copied!

Was this page helpful?

Yes
No
