Copy page
Fuel Prices

The fuel prices API provides real-time access to fuel prices across different locations. It supports querying prices for different fuel types (e.g., regular gas, diesel) and includes detailed station information.

GET
/v1/fuel/:fuelType
Get fuel prices

Retrieve current fuel prices for a specific location and fuel type.

Required parameters
Name
fuelType
Type
FuelType
Description

Type of fuel to query (e.g., "regular", "diesel"). See fuel types for more details.

Name
country
Type
string
Description

Two-letter country code (e.g., "US").

Optional parameters
Name
country
Type
string
Description

Two-letter country code (e.g., "US", "CA").

Name
region
Type
string
Description

Region/state code (e.g., "CA").

Name
city
Type
string
Description

City name (e.g., "San Francisco").

OR

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

Request
cURL
@cardog/api
fetch
Python
GET
/v1/fuel/gas?country=US&region=CA&city=San%20Francisco
curl https://api.cardog.app/v1/fuel/regular \
-H "x-api-key: your-api-key" \
-G \
-d "country=US" \
-d "region=CA" \
-d "city=San%20Francisco"

Copy
Copied!
Response
{
"current": 4.23,
"average": 4.138,
"postedTime": "2025-03-17T03:41:47.204Z",
"unit": "USD/gal",
"isMetric": false,
"fuelType": 1,
"fuelName": "Regular Gasoline",
"location": {
"name": "San Francisco",
"countryCode": "US",
"regionCode": "CA",
"locationType": "metro",
"nearbyLocalities": ["San Jose", "Oakland", "Fremont"]
},
"history": [
{
"date": "2025-03-16T18:05:31.746Z",
"price": 3.89,
"formattedPrice": "3.89 USD/gal"
}
],
"stations": [
{
"id": "77402",
"name": "DoubleTime",
"address": {
"line1": "3400 Mission St",
"locality": "San Francisco",
"region": "CA",
"postalCode": "94110-5420",
"country": "US"
},
"location": {
"latitude": 37.742135270779,
"longitude": -122.422392368317
},
"price": {
"credit": 4.09,
"cash": 3.99,
"postedTime": "2025-03-17T01:24:11.212Z",
"formattedPrice": "4.09 USD/gal"
},
"amenities": ["Offers Cash Discount", "C-Store", "Pay At Pump", "Restrooms", "Air Pump"],
"brands": [
{
"id": "365",
"name": "DoubleTime",
"type": "fuel"
}
],
"rating": 3.8,
"ratingsCount": 95,
"fuels": ["regular_gas", "midgrade_gas", "premium_gas"]
}
]
}

Copy
Copied!
The Fuel Types

The following fuel types are supported, simply replace the fuel type in the URL with the fuel type you want to query.

Name
regular
Type
string
Description

Regular gasoline - /v1/fuel/regular

Name
midgrade
Type
string
Description

Midgrade gasoline - /v1/fuel/midgrade

Name
premium
Type
string
Description

Premium gasoline - /v1/fuel/premium

Name
diesel
Type
string
Description

Diesel fuel - /v1/fuel/diesel

Name
e85
Type
string
Description

E85 fuel - /v1/fuel/e85

Name
e15
Type
string
Description

E15 fuel - /v1/fuel/e15

Name
propane
Type
string
Description

Propane fuel - /v1/fuel/propane

Name
biodiesel
Type
string
Description

Biodiesel fuel - /v1/fuel/biodiesel

Name
cng
Type
string
Description

CNG fuel - /v1/fuel/cng

Name
lng
Type
string
Description

LNG fuel - /v1/fuel/lng

Was this page helpful?

Yes
No
