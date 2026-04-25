---
name: cardog-api-integration
description: Use when making requests to Cardog API — VIN decoding, vehicle listings search, or any endpoint at https://api.cardog.io/v1
---

# Cardog API Integration

## Overview

Cardog API provides vehicle data including VIN decoding, listings search, market analysis, and recall information. All requests require the `x-api-key` header with the API key from `CARDOG_APIKEY` environment variable.

## Quick Reference

| Task | Endpoint | Method | Key Details |
|------|----------|--------|------------|
| Decode single VIN | `/v1/vin/:vin` | GET | Returns variants, component analysis, confidence |
| Batch VIN decode (fast) | `/v1/vin/batch/corgi` | POST | ~6000 VINs/sec, no rate limits |
| Batch VIN decode (full) | `/v1/vin/batch` | POST | Up to 1000 VINs, includes variants |
| Extract VIN from image | `/v1/vin/image` | POST | Base64-encoded image, returns detected VIN |
| Search listings | `/v1/listings/search` | GET | Query params for filtering (makes, models, price, year, etc.) |
| Get single listing | `/v1/listings/id/:id` | GET | Returns full listing with seller and dealership info |
| Listings by VIN | `/v1/listings/vin/:vin` | GET | Find all listings for specific vehicle |

## Authentication

Always set the `x-api-key` header. Get API key from environment variable:

```bash
# Load from shell
source ~/.profile  # Sets CARDOG_APIKEY

# In code (Node.js example)
const apiKey = process.env.CARDOG_APIKEY;
const headers = {
  'x-api-key': apiKey,
  'Content-Type': 'application/json'
};
```

## Base URL

**Always use:** `https://api.cardog.io/v1`

Do NOT use `api.cardog.app` — that's incorrect.

## Implementation

### Single VIN Decode (Full Information)

**Endpoint:** `GET /v1/vin/:vin`

```bash
curl "https://api.cardog.io/v1/vin/1HGBH41JXMN109186" \
  -H "x-api-key: $CARDOG_APIKEY"
```

Returns variants array, component analysis, and full vehicle details.

### High-Performance VIN Batch (Corgi v3)

**Endpoint:** `POST /v1/vin/batch/corgi`

```bash
curl "https://api.cardog.io/v1/vin/batch/corgi" \
  -H "x-api-key: $CARDOG_APIKEY" \
  -H "Content-Type: application/json" \
  -d '{
    "vins": [
      "5YJ3E1EA1PF123456",
      "1HGBH41JXMN109186"
    ]
  }'
```

**Performance:** ~6000 VINs/second, no rate limits, max 1000 per request.

### Search Vehicle Listings

**Endpoint:** `GET /v1/listings/search`

Query parameters:
- `makes`: array of makes to filter (e.g., `"Toyota"`)
- `models`: object mapping make to models (e.g., `"Toyota|Camry,Corolla"`)
- `yearMin`, `yearMax`: year range
- `priceMin`, `priceMax`: price range in cents
- `odometerMin`, `odometerMax`: mileage range
- `saleTypes`: `"new"` or `"used"`
- `bodyStyles`: `"SUV"`, `"Sedan"`, etc.
- `sort`: field to sort by (e.g., `"price"`)
- `order`: `"asc"` or `"desc"`
- `limit`: results per page (default 25, max 100)
- `location`: object with `lat`, `lng`, `radius`

```bash
curl "https://api.cardog.io/v1/listings/search" \
  -H "x-api-key: $CARDOG_APIKEY" \
  -G \
  -d "makes=Toyota" \
  -d "yearMin=2018" \
  -d "yearMax=2023" \
  -d "priceMin=20000" \
  -d "priceMax=50000" \
  -d "sort=price" \
  -d "order=asc" \
  -d "limit=10"
```

## Common Mistakes

| Mistake | Fix |
|--------|-----|
| Using `https://api.cardog.app` | Use `https://api.cardog.io/v1` as base URL |
| Forgetting `x-api-key` header | Always include: `-H "x-api-key: $CARDOG_APIKEY"` |
| Sending API key in body/query | Send ONLY in `x-api-key` header |
| Using wrong VIN format | Accepts 17-char VIN; `/v1/vin/nano` accepts 10-11 char |
| Batch endpoint confusion | Use `/v1/vin/batch/corgi` for max throughput; `/v1/vin/batch` for variants |

## Response Format

Responses are JSON. Check for `"error"` field (strings) or `"success": false` in batch responses.

**Successful single request:** Returns object with relevant fields (variants, pass, components, etc.)

**Successful batch request:**
```json
{
  "success": true,
  "count": 2,
  "successful": 2,
  "failed": 0,
  "results": [
    {
      "vin": "1HGBH41JXMN109186",
      "result": { "valid": true, "vehicle": { ... } }
    }
  ]
}
```

**Error:** `{ "error": "Invalid VIN" }` or similar

## Red Flags - STOP and Verify

- Using `api.cardog.app` instead of `api.cardog.io/v1`
- Forgetting to `source ~/.profile` to load environment variables
- Sending API key as query parameter or body field instead of header
- Not checking for `"error"` field in responses
- Assuming batch requests succeed entirely (must check `failed` count)

## Documentation

See `@docs/api/cardog` for comprehensive endpoint reference including all optional parameters and example responses.
