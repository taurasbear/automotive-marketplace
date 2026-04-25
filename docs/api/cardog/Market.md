Copy page
Market Analysis

The market analysis API provides comprehensive insights into vehicle pricing, market trends, and comparable listings. Use these endpoints to analyze market conditions, find similar vehicles, and get pricing guidance.

GET
/v1/market/overview
Get market overview

Get comprehensive market overview analytics across the entire vehicle database, including price distribution, make/model distribution, year distribution, and days on market analysis.

Optional parameters
Name
activeOnly
Type
boolean
Description

Filter to active listings only (default: true).

Name
daysCutoff
Type
number
Description

Maximum age of listings to include in days.

Name
maxTopMakes
Type
number
Description

Maximum number of top makes to return (default: 10).

Name
maxTopModels
Type
number
Description

Maximum number of top models per make to return (default: 5).

Name
priceRangeMin
Type
number
Description

Minimum price for distribution buckets.

Name
priceRangeMax
Type
number
Description

Maximum price for distribution buckets.

Name
priceRangeIncrement
Type
number
Description

Price increment for distribution buckets.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/overview
curl https://api.cardog.app/v1/market/overview \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"overview": {
"totalListings": 331078,
"avgPrice": 47329,
"medianPrice": 41590,
"minPrice": -5999,
"maxPrice": 3350000,
"stdDev": 31037
},
"priceDistribution": [
{
"range": "$0K-$25K",
"count": 60426
},
{
"range": "$25K-$50K",
"count": 147768
},
{
"range": "$50K-$75K",
"count": 77453
}
],
"makeDistribution": [
{
"make": "Ford",
"count": 51509
},
{
"make": "Chevrolet",
"count": 30241
},
{
"make": "Honda",
"count": 25860
}
],
"modelDistribution": {
"Ford": [
{
"model": "F-150",
"count": 15164
},
{
"model": "Escape",
"count": 8300
}
]
},
"yearDistribution": [
{
"range": "2023-2025 (New)",
"count": 210327
},
{
"range": "2020-2022 (Recent)",
"count": 63374
}
],
"daysOnMarketByPrice": [
{
"range": "$0K-$25K",
"avgDays": 128
},
{
"range": "$25K-$50K",
"avgDays": 117
}
]
}

Copy
Copied!
GET
/v1/market/analysis/vin/:vin
Get market analysis by VIN

Get comprehensive market analysis for a specific vehicle by VIN, including similar listings, price analysis, market trends, and pricing guidance.

Required parameters
Name
vin
Type
string
Description

17-character Vehicle Identification Number.

Optional parameters
Name
maxResults
Type
number
Description

Maximum number of similar listings to return (default: 100).

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/analysis/vin/1HGCM82633A123456
curl https://api.cardog.app/v1/market/analysis/vin/1HGCM82633A123456 \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"similarListings": [
{
"id": "f963c769-9a9a-436c-8edf-e478ecea3c6c",
"price": 2900,
"odometer": 102695,
"vin": "1HGCM82563A800231",
"year": 2003,
"make": "Honda",
"model": "Accord",
"trim": "EX",
"listingDate": "2025-03-18T20:56:10.807291",
"daysOnMarket": 164,
"location": "Whitby, ON"
}
],
"priceAnalysis": {
"count": 2,
"min": 2900,
"max": 4888,
"average": 3894,
"median": 3894,
"standardDev": 1406,
"priceDistribution": [
{
"range": "2900-2900",
"count": 1,
"percentage": 50
},
{
"range": "4888-4888",
"count": 1,
"percentage": 50
}
]
},
"marketTrends": {
"averageDaysOnMarket": 167,
"priceTrend": 0,
"listingVolume": 0
},
"pricingGuidance": {
"suggested": 3894,
"confidence": "low"
}
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/overview
Get cohort overview

Get comprehensive overview statistics for a specific make/model/year combination.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/overview
curl https://api.cardog.app/v1/market/Honda/Accord/2020/overview \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"vehicle": "2020 Honda Accord",
"totalListings": 80,
"avgPrice": 27768,
"medianPrice": 27996,
"minPrice": 17990,
"maxPrice": 34888,
"q1Price": 25882,
"q3Price": 30548,
"avgDaysOnMarket": 131,
"avgOdometer": 88045
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/pricing
Get price distribution

Get detailed price distribution analysis for a specific make/model/year combination.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/pricing
curl https://api.cardog.app/v1/market/Honda/Accord/2020/pricing \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"distribution": [
{
"bucketMin": 17990,
"bucketMax": 19680,
"count": 1,
"percentage": 1.3
},
{
"bucketMin": 19680,
"bucketMax": 21370,
"count": 4,
"percentage": 5
},
{
"bucketMin": 26439,
"bucketMax": 28129,
"count": 17,
"percentage": 21.3
},
{
"bucketMin": 29819,
"bucketMax": 31508,
"count": 19,
"percentage": 23.8
}
]
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/odometer
Get odometer analysis

Get odometer-based analysis showing price trends across different mileage segments.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/odometer
curl https://api.cardog.app/v1/market/Honda/Accord/2020/odometer \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"segments": [
{
"odometerMin": 10000,
"odometerMax": 20000,
"listings": 1,
"avgPrice": 32990,
"avgOdometer": 13351,
"minPrice": 32990,
"maxPrice": 32990
},
{
"odometerMin": 40000,
"odometerMax": 50000,
"listings": 6,
"avgPrice": 28941,
"avgOdometer": 45017,
"minPrice": 27200,
"maxPrice": 30900
},
{
"odometerMin": 80000,
"odometerMax": 90000,
"listings": 10,
"avgPrice": 28720,
"avgOdometer": 84297,
"minPrice": 23900,
"maxPrice": 32995
}
]
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/geography
Get geographic analysis

Get geographic market analysis showing price and listing distribution by region.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/geography
curl https://api.cardog.app/v1/market/Honda/Accord/2020/geography \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"markets": [
{
"state": "ON",
"listings": 53,
"avgPrice": 27144,
"medianPrice": 27500,
"avgOdometer": 95770,
"avgDaysOnMarket": 137,
"centerLat": 43.794,
"centerLng": -79.4763
},
{
"state": "QC",
"listings": 10,
"avgPrice": 26443,
"medianPrice": 27432,
"avgOdometer": 74359,
"avgDaysOnMarket": 111,
"centerLat": 45.5946,
"centerLng": -73.5234
},
{
"state": "BC",
"listings": 10,
"avgPrice": 31454,
"medianPrice": 30995,
"avgOdometer": 75166,
"avgDaysOnMarket": 124,
"centerLat": 49.1999,
"centerLng": -122.831
}
]
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/trends
Get market trends

Get time-series market trends showing price changes and listing volume over time.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Optional parameters
Name
period
Type
string
Description

Time period for trends: "week" or "month" (default: "week").

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/trends
curl "https://api.cardog.app/v1/market/Honda/Accord/2020/trends?period=week" \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"trends": [
{
"period": "2025-08-26T00:00:00.000Z",
"newListings": 2,
"avgPrice": 28966,
"avgOdometer": 72752,
"priceChangePct": 0
},
{
"period": "2025-08-18T00:00:00.000Z",
"newListings": 2,
"avgPrice": 27298,
"avgOdometer": 57746,
"priceChangePct": -5.8
},
{
"period": "2025-08-11T00:00:00.000Z",
"newListings": 1,
"avgPrice": 26998,
"avgOdometer": 103145,
"priceChangePct": -1.1
}
],
"period": "week"
}

Copy
Copied!
GET
/v1/market/listings/:listingId/position
Get listing market position

Get market position analysis for a specific listing, including price percentile and market comparison.

Required parameters
Name
listingId
Type
string
Description

Unique identifier for the listing.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/listings/f963c769-9a9a-436c-8edf-e478ecea3c6c/position
curl https://api.cardog.app/v1/market/listings/f963c769-9a9a-436c-8edf-e478ecea3c6c/position \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"id": "f963c769-9a9a-436c-8edf-e478ecea3c6c",
"price": 2900,
"odometer": 102695,
"trim": "EX",
"daysonmarket": "164",
"city": "Whitby",
"state": "ON",
"pricepercentile": "50.0",
"pricevsmedian": -994,
"pricecategory": "Great Deal",
"marketaverage": "3894"
}

Copy
Copied!
GET
/v1/market/:make/:model/:year/breakdown
Get trim breakdown

Get comprehensive trim-level breakdown for a specific make/model/year, including per-trim pricing statistics, odometer averages, days on market, and price comparison vs market average.

Required parameters
Name
make
Type
string
Description

Vehicle manufacturer.

Name
model
Type
string
Description

Vehicle model.

Name
year
Type
number
Description

Vehicle model year.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/Honda/Accord/2020/breakdown
curl https://api.cardog.app/v1/market/Honda/Accord/2020/breakdown \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"vehicle": "2020 Honda Accord",
"totalListings": 80,
"overview": {
"avgPrice": 27768,
"medianPrice": 27996,
"minPrice": 17990,
"maxPrice": 34888
},
"trims": [
{
"trim": "Sport",
"count": 22,
"avgPrice": 28450,
"medianPrice": 28500,
"minPrice": 23900,
"maxPrice": 32995,
"avgOdometer": 78500,
"avgDaysOnMarket": 125,
"priceVsMarketAvg": 682
},
{
"trim": "EX-L",
"count": 18,
"avgPrice": 29100,
"medianPrice": 29400,
"minPrice": 24500,
"maxPrice": 34888,
"avgOdometer": 72300,
"avgDaysOnMarket": 118,
"priceVsMarketAvg": 1332
},
{
"trim": "LX",
"count": 15,
"avgPrice": 25200,
"medianPrice": 25000,
"minPrice": 17990,
"maxPrice": 29500,
"avgOdometer": 95000,
"avgDaysOnMarket": 142,
"priceVsMarketAvg": -2568
}
]
}

Copy
Copied!
GET
/v1/market/listings/:listingId/similar
Get similar listings

Get similar vehicle listings based on the characteristics of a specific listing.

Required parameters
Name
listingId
Type
string
Description

Unique identifier for the listing.

Optional parameters
Name
limit
Type
number
Description

Maximum number of similar listings to return (default: 10, max: 50).

Request
cURL
@cardog/api
fetch
Python
GET
/v1/market/listings/f963c769-9a9a-436c-8edf-e478ecea3c6c/similar
curl "https://api.cardog.app/v1/market/listings/f963c769-9a9a-436c-8edf-e478ecea3c6c/similar?limit=5" \
 -H "x-api-key: YOUR_API_KEY"

Copy
Copied!
Response
{
"listings": [],
"count": 0,
"requestedLimit": 5
}

Copy
Copied!

Was this page helpful?

Yes
No
