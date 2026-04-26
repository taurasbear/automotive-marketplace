Copy page
VIN Decoding

The VIN decoding endpoints provide comprehensive vehicle information by analyzing Vehicle Identification Numbers (VINs). These endpoints can decode VINs to extract detailed vehicle specifications, variants, process VIN images using AI, and batch decode up to 1000 VINs in a single request.

GET
/v1/vin/:vin
Decode VIN (Full)

This endpoint provides the most comprehensive VIN decoding, combining both VIN pattern matching and vehicle variant data. It first decodes the VIN using our Corgi decoder, then enriches the data with detailed variant information from our vehicle database.

Required parameters
Name
vin
Type
string
Description

A valid 17-character Vehicle Identification Number.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/vin/1HGBH41JXMN109186
curl "https://api.cardog.app/v1/vin/1HGBH41JXMN109186" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"variants": [
{
"id": "honda-civic-2021-lx",
"make": "Honda",
"model": "Civic",
"year": 2021,
"trim": "LX",
"msrp": 22350,
"bodyStyle": "Sedan",
"styleName": "Civic LX 4dr Sedan",
"region": "US",
"spec": {
"engine": "1.5L 4-Cylinder",
"transmission": "CVT",
"drivetrain": "FWD"
}
}
],
"pass": {
"vin": "1HGBH41JXMN109186",
"valid": true,
"components": {
"vehicle": {
"make": "Honda",
"model": "Civic",
"year": 2021,
"bodyStyle": "Sedan",
"driveType": "FWD",
"fuelType": "Gasoline"
},
"wmi": {
"manufacturer": "Honda Motor Co., Ltd.",
"make": "Honda",
"country": "United States",
"region": "North America"
},
"checkDigit": {
"isValid": true,
"expected": "X",
"actual": "X"
}
}
},
"vin": "1HGBH41JXMN109186"
}

Copy
Copied!
POST
/v1/vin/batch
Batch Decode VINs

Decode multiple VINs in a single request for high-throughput applications. This endpoint efficiently processes up to 1000 VINs in parallel, returning detailed decode results for each VIN. Perfect for fleet management, inventory systems, and bulk vehicle data enrichment.

Required parameters
Name
vins
Type
string[]
Description

An array of 17-character VINs to decode. Minimum 1, maximum 1000 VINs per request.

Performance
Throughput: ~50 VINs/second (D1-based)
Success Rate: 97%+ for valid VINs
Max Batch Size: 1000 VINs

For higher throughput, use /v1/vin/batch/corgi which achieves ~6000 VINs/second using binary indexes.

Request
cURL
@cardog/api
fetch
Python
POST
/v1/vin/batch
curl "https://api.cardog.app/v1/vin/batch" \
 -H "x-api-key: your-api-key" \
 -H "Content-Type: application/json" \
 -d '{
"vins": [
"1HGBH41JXMN109186",
"2HGFC2F77GH036115",
"3GCUKDED0SG211499"
]
}'

Copy
Copied!
Response
{
"success": true,
"count": 3,
"successful": 3,
"failed": 0,
"processingTime": "156.42ms",
"avgTimePerVin": "52.14ms",
"results": [
{
"vin": "1HGBH41JXMN109186",
"result": {
"vin": "1HGBH41JXMN109186",
"valid": true,
"components": {
"vehicle": {
"make": "Honda",
"model": "Civic",
"year": 2021,
"bodyStyle": "Sedan",
"fuelType": "Gasoline"
},
"wmi": {
"manufacturer": "Honda Motor Co., Ltd.",
"make": "Honda",
"country": "United States"
}
},
"metadata": {
"processingTime": 45.2,
"confidence": 0.96
}
}
},
{
"vin": "2HGFC2F77GH036115",
"result": {
"vin": "2HGFC2F77GH036115",
"valid": true,
"components": {
"vehicle": {
"make": "Honda",
"model": "Civic",
"year": 2016,
"bodyStyle": "Sedan",
"fuelType": "Gasoline"
}
}
}
},
{
"vin": "3GCUKDED0SG211499",
"result": {
"vin": "3GCUKDED0SG211499",
"valid": true,
"components": {
"vehicle": {
"make": "Chevrolet",
"model": "Silverado",
"year": 2025,
"bodyStyle": "Pickup",
"fuelType": "Gasoline"
}
}
}
}
]
}

Copy
Copied!
GET
/v1/vin/corgi/v3/:vin
Decode VIN (Corgi v3)

High-performance VIN decoding using pre-built binary indexes. This endpoint is ~3x faster than the standard Corgi decoder and requires no database calls. Perfect for high-throughput applications.

Required parameters
Name
vin
Type
string
Description

A valid 17-character Vehicle Identification Number.

Performance
Latency: ~12ms average
Throughput: ~80 VINs/second (single requests)
No database calls: Fully offline decoding
Request
cURL
@cardog/api
GET
/v1/vin/corgi/v3/5YJ3E1EA1PF123456
curl "https://api.cardog.app/v1/vin/corgi/v3/5YJ3E1EA1PF123456" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"vin": "5YJ3E1EA1PF123456",
"valid": true,
"components": {
"wmi": "5YJ",
"vds": "3E1EA1",
"vis": "PF123456",
"modelYear": 2023,
"checkDigit": "1",
"serialNumber": "123456"
},
"vehicle": {
"make": "Tesla",
"model": "Model 3",
"year": 2023,
"bodyType": "Sedan",
"fuelType": "Electric"
},
"confidence": 0.95,
"warnings": [],
"errors": []
}

Copy
Copied!
POST
/v1/vin/batch/corgi
Batch Decode (Corgi v3)

Ultra high-throughput batch VIN decoding using Corgi v3 binary indexes. Achieves ~6000 VINs/second with synchronous processing and no database rate limits. Ideal for fleet management, inventory systems, and bulk data processing.

Required parameters
Name
vins
Type
string[]
Description

An array of 17-character VINs to decode. Minimum 1, maximum 1000 VINs per request.

Performance
Throughput: ~6000 VINs/second
Batch 100 VINs: ~27ms
Batch 500 VINs: ~88ms
No rate limits: Synchronous processing
Request
cURL
@cardog/api
POST
/v1/vin/batch/corgi
curl "https://api.cardog.app/v1/vin/batch/corgi" \
 -H "x-api-key: your-api-key" \
 -H "Content-Type: application/json" \
 -d '{
"vins": [
"5YJ3E1EA1PF123456",
"JN8BT3DD3RW250328",
"1HGBH41JXMN109186"
]
}'

Copy
Copied!
Response
{
"success": true,
"count": 3,
"successful": 3,
"failed": 0,
"processingTime": "4.52ms",
"avgTimePerVin": "1.51ms",
"results": [
{
"vin": "5YJ3E1EA1PF123456",
"result": {
"vin": "5YJ3E1EA1PF123456",
"valid": true,
"vehicle": {
"make": "Tesla",
"model": "Model 3",
"year": 2023
},
"confidence": 0.95
}
},
{
"vin": "JN8BT3DD3RW250328",
"result": {
"vin": "JN8BT3DD3RW250328",
"valid": true,
"vehicle": {
"make": "Nissan",
"model": "Rogue",
"year": 2024
},
"confidence": 0.92
}
},
{
"vin": "1HGBH41JXMN109186",
"result": {
"vin": "1HGBH41JXMN109186",
"valid": true,
"vehicle": {
"make": "Honda",
"model": "Civic",
"year": 2021
},
"confidence": 0.96
}
}
]
}

Copy
Copied!
POST
/v1/vin/batch/nano
Batch Decode (Nano)

Batch VIN lookup using the nano database. Returns variant information for multiple VINs. Best for applications that need detailed trim and MSRP data.

Required parameters
Name
vins
Type
string[]
Description

An array of VINs to look up. Minimum 1, maximum 1000 VINs per request.

Request
cURL
@cardog/api
POST
/v1/vin/batch/nano
curl "https://api.cardog.app/v1/vin/batch/nano" \
 -H "x-api-key: your-api-key" \
 -H "Content-Type: application/json" \
 -d '{
"vins": [
"1HGBH41JXMN109186",
"JN8BT3DD3RW250328"
]
}'

Copy
Copied!
Response
{
"success": true,
"count": 2,
"successful": 2,
"failed": 0,
"processingTime": "45.23ms",
"avgTimePerVin": "22.62ms",
"results": [
{
"vin": "1HGBH41JXMN109186",
"result": {
"make": "Honda",
"model": "Civic",
"year": 2021,
"trim": "LX",
"msrp": 22350
}
},
{
"vin": "JN8BT3DD3RW250328",
"result": {
"make": "Nissan",
"model": "Rogue",
"year": 2024,
"trim": "SV",
"msrp": 31840
}
}
]
}

Copy
Copied!
GET
/v1/vin/nano/:vin
Decode VIN (Nano)

This endpoint provides a lightweight VIN lookup using our nano VIN database. It's optimized for speed and returns basic variant information without the full decoding process. Nano VINs are condensed 11-character identifiers that exclude the check digit.

Required parameters
Name
vin
Type
string
Description

A valid VIN (17, 11, or 10 characters). Will be automatically converted to nano format.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/vin/nano/1HGBH41JXMN109186
curl "https://api.cardog.app/v1/vin/nano/1HGBH41JXMN109186" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"id": "honda-civic-2021-lx",
"make": "Honda",
"model": "Civic",
"year": 2021,
"trim": "LX",
"msrp": 22350,
"bodyStyle": "Sedan",
"styleName": "Civic LX 4dr Sedan",
"region": "US"
}

Copy
Copied!
GET
/v1/vin/corgi/:vin
Decode VIN (Corgi)

This endpoint provides raw VIN decoding using our Corgi decoder engine. It returns detailed pattern matching results, component analysis, and validation information without additional variant enrichment. Perfect for applications that need low-level VIN analysis.

Required parameters
Name
vin
Type
string
Description

A valid 17-character Vehicle Identification Number.

Request
cURL
@cardog/api
fetch
Python
GET
/v1/vin/corgi/1HGBH41JXMN109186
curl "https://api.cardog.app/v1/vin/corgi/1HGBH41JXMN109186" \
 -H "x-api-key: your-api-key"

Copy
Copied!
Response
{
"vin": "1HGBH41JXMN109186",
"valid": true,
"components": {
"checkDigit": {
"position": 8,
"actual": "X",
"expected": "X",
"isValid": true
},
"modelYear": {
"year": 2021,
"source": "VIS_POSITION_10",
"confidence": 1.0
},
"wmi": {
"code": "1HG",
"manufacturer": "Honda Motor Co., Ltd.",
"make": "Honda",
"country": "United States",
"vehicleType": "Passenger Car",
"region": "North America"
},
"vds": {
"raw": "BH41JX",
"patterns": [
{
"element": "Body Style",
"code": "BH4",
"attributeId": "body_style",
"value": "Sedan",
"confidence": 0.95,
"positions": [3, 4, 5],
"schema": "honda_passenger",
"metadata": {
"lookupTable": "honda_body_styles",
"groupName": "Vehicle Description",
"elementWeight": 0.8,
"patternType": "direct_lookup",
"rawPattern": "BH4"
}
}
]
},
"vis": {
"raw": "MN109186",
"patterns": [
{
"element": "Plant Code",
"code": "M",
"attributeId": "plant_code",
"value": "Marysville, OH",
"confidence": 1.0,
"positions": [10],
"schema": "honda_plant_codes",
"metadata": {
"lookupTable": "honda_plants",
"groupName": "Manufacturing",
"elementWeight": 1.0,
"patternType": "plant_lookup",
"rawPattern": "M"
}
}
]
},
"vehicle": {
"make": "Honda",
"model": "Civic",
"year": 2021,
"bodyStyle": "Sedan",
"driveType": "FWD",
"fuelType": "Gasoline",
"doors": "4"
}
},
"errors": [],
"metadata": {
"processingTime": 45.2,
"confidence": 0.96,
"schemaVersion": "1.0"
}
}

Copy
Copied!
POST
/v1/vin/image
Extract VIN from Image

This endpoint uses AI to extract VIN numbers from images of vehicle dashboards or door jambs. It accepts base64-encoded images and returns the detected VIN using Google's Gemini vision model.

Required parameters
Name
image
Type
string
Description

Base64-encoded image data (without data URL prefix).

Request
cURL
@cardog/api
fetch
Python
POST
/v1/vin/image
curl "https://api.cardog.app/v1/vin/image" \
 -H "x-api-key: your-api-key" \
 -H "Content-Type: application/json" \
 -d '{
"image": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg=="
}'

Copy
Copied!
Response
{
"success": true,
"result": "1HGBH41JXMN109186"
}

Copy
Copied!
Error Responses

All VIN endpoints return structured error responses when validation fails or processing errors occur:

Invalid VIN Error
{
"error": "Invalid VIN"
}

Copy
Copied!
No Variant Found Error
{
"error": "No variant found"
}

Copy
Copied!
Image Processing Error
{
"error": "Image data is required as base64 string"
}

Copy
Copied!
Batch Validation Error
{
"success": false,
"error": {
"issues": [
{
"code": "too_small",
"minimum": 17,
"path": ["vins", 0],
"message": "String must contain exactly 17 character(s)"
}
]
}
}

Copy
Copied!

For batch requests, individual VIN decode failures are returned inline with the results rather than failing the entire request:

Batch with Partial Failures
{
"success": true,
"count": 3,
"successful": 2,
"failed": 1,
"results": [
{ "vin": "1HGBH41JXMN109186", "result": { "valid": true, "..." } },
{ "vin": "INVALID12345678", "error": "No matching patterns found" },
{ "vin": "2HGFC2F77GH036115", "result": { "valid": true, "..." } }
]
}

Copy
Copied!
VIN Format Support

Our VIN decoding endpoints support various VIN formats:

Name
17-character VIN
Type
string
Description

Full standard VIN format (e.g., "1HGBH41JXMN109186")

Name
11-character VIN
Type
string
Description

VIN with check digit included (e.g., "1HGBH41JXMN")

Name
10-character VIN
Type
string
Description

Nano VIN without check digit (e.g., "1HGBH41JMN")

Use Cases
Name
Full Decode
Type
endpoint
Description

Use /v1/vin/:vin when you need comprehensive vehicle information including variants, specifications, and detailed decoding results.

Name
High-Performance Decode
Type
endpoint
Description

Use /v1/vin/corgi/v3/:vin for fast, offline VIN decoding (~3x faster than standard). Best for real-time applications.

Name
Ultra High-Throughput Batch
Type
endpoint
Description

Use /v1/vin/batch/corgi for maximum throughput (~6000 VINs/sec). Ideal for fleet management and bulk processing.

Name
Standard Batch Processing
Type
endpoint
Description

Use /v1/vin/batch for database-backed batch decoding. Supports up to 1000 VINs per request.

Name
Quick Lookup
Type
endpoint
Description

Use /v1/vin/nano/:vin for fast variant lookups when you only need basic vehicle information.

Name
Raw Analysis
Type
endpoint
Description

Use /v1/vin/corgi/:vin when you need detailed VIN pattern analysis and component breakdown.

Name
Image Processing
Type
endpoint
Description

Use /v1/vin/image to extract VINs from photos of vehicle dashboards or door jambs.

Was this page helpful?

Yes
No
