Copy page
Vehicle Listings

Vehicle listings are a core part of the Cardog API — they represent vehicles being sold at specific locations with detailed information about the vehicle, seller, and media. On this page, we'll explore the different listing endpoints you can use to search and manage vehicle listings programmatically.

GET
/v1/listings/search
List all listings

This endpoint allows you to search and retrieve a paginated list of vehicle listings with comprehensive filtering options. By default, a maximum of 25 listings are shown per page.

Optional attributes
Name
makes
Type
array
Description

Filter by vehicle makes (e.g., ["Toyota", "Honda"]).

Name
models
Type
object
Description

Filter by make-specific models (e.g., {"Toyota": ["Camry", "Corolla"]}).

Name
year
Type
object
Description

Filter by year range (min and max).

Name
price
Type
object
Description

Filter by price range (min and max).

Name
odometer
Type
object
Description

Filter by odometer reading range (min and max).

Name
saleTypes
Type
array
Description

Filter by sale type (e.g., ["new", "used"]).

Name
bodyStyles
Type
array
Description

Filter by body styles (e.g., ["SUV", "Sedan"]).

Name
location
Type
object
Description

Filter by geographic location (lat, lng, radius in km).

Name
sort
Type
object
Description

Sort results by field and direction.

Name
pagination
Type
object
Description

Control page size and number.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/listings/search
curl "https://api.cardog.app/v1/listings/search" \
 -H "x-api-key: your-api-key" \
 -G \
 -d "makes=Toyota" \
 -d "models=Toyota|Camry,Corolla" \
 -d "yearMin=2018" \
 -d "yearMax=2023" \
 -d "priceMin=20000" \
 -d "priceMax=50000" \
 -d "sort=price" \
 -d "order=asc" \
 -d "limit=10"

Copy
Copied!
Response
{
"listings": [
{
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"vin": "1HGCM82633A123456",
"sellerId": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"price": 25000,
"currency": "CAD",
"odometer": 50000,
"make": "Toyota",
"model": "Camry",
"year": 2020,
"trim": "SE",
"bodyStyle": "Sedan",
"condition": "used",
"seller": {
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"name": "Toyota Dealership"
},
"media": [
{
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"url": "https://example.com/image.jpg",
"type": "image"
}
]
}
],
"pagination": {
"page": 1,
"limit": 10
},
"sort": {
"field": "price",
"direction": "asc"
},
"filters": {
"makes": ["Toyota"],
"models": {
"Toyota": ["Camry", "Corolla"]
},
"year": {
"min": 2018,
"max": 2023
},
"price": {
"min": 20000,
"max": 50000
}
}
}

Copy
Copied!
GET
/v1/listings/id/:id
Retrieve a listing

This endpoint allows you to retrieve a specific listing by its ID. The response includes detailed information about the vehicle, seller, dealership, and associated media.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/listings/id/:id
curl "https://api.cardog.app/v1/listings/id/01H4MXNVJ8TP5PQ8K3Y1VN4E9R" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"vin": "1HGCM82633A123456",
"sellerId": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"price": 25000,
"currency": "CAD",
"odometer": 50000,
"make": "Toyota",
"model": "Camry",
"year": 2020,
"trim": "SE",
"bodyStyle": "Sedan",
"condition": "used",
"seller": {
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"name": "Toyota Dealership"
},
"dealership": {
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"name": "Toyota Downtown"
},
"media": [
{
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"url": "https://example.com/image.jpg",
"type": "image"
}
]
}

Copy
Copied!
GET
/v1/listings/vin/:vin
Find listings by VIN

This endpoint allows you to find all listings for a specific vehicle by its VIN (Vehicle Identification Number). This is useful for tracking the same vehicle across different platforms or over time.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/listings/vin/:vin
curl "https://api.cardog.app/v1/listings/vin/1HGCM82633A123456" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
[
{
"listing": {
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"vin": "1HGCM82633A123456",
"price": 25000,
"currency": "CAD"
},
"vehicle": {
"vin": "1HGCM82633A123456",
"make": "Toyota",
"model": "Camry",
"year": 2020
},
"seller": {
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"name": "Toyota Dealership"
},
"media": [
{
"id": "01H4MXNVJ8TP5PQ8K3Y1VN4E9R",
"url": "https://example.com/image.jpg",
"type": "image"
}
]
}
]

Copy
Copied!

Was this page helpful?

Yes
No
