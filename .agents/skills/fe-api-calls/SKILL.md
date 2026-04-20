---
name: fe-api-calls
description: Use when adding a new API call — query (fetching data) or mutation (creating, updating, deleting) — in the frontend
---

# Frontend API Calls

## Overview

Queries use `queryOptions()` from TanStack Query. Mutations use `useMutation()`. All HTTP goes through `axiosClient` (auth token injected automatically). Endpoints are defined in `src/constants/endpoints.ts`.

## Queries

```ts
// Feature-local query (used only by one feature) → feature/api/
// src/features/listingList/api/getAllListingsOptions.ts

import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import type { GetAllListingsResponse } from "../types/GetAllListingsResponse";

const getAllListings = (query: GetAllListingsQuery) =>
  axiosClient.get<GetAllListingsResponse[]>(ENDPOINTS.LISTING.GET_ALL, { params: query });

export const getAllListingsOptions = (query: GetAllListingsQuery) =>
  queryOptions({
    queryKey: listingKeys.bySearchParams(query),
    queryFn: () => getAllListings(query),
  });
```

Shared queries (used in 2+ features) go in `src/api/<resource>/getAllXxxOptions.ts` instead.

## Mutations

```ts
// Always inside feature/api/ as a custom hook
// src/features/createListing/api/useCreateListing.ts

import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import type { CreateListingCommand } from "../types/CreateListingCommand";

const createListing = (body: CreateListingCommand) =>
  axiosClient.post<void>(ENDPOINTS.LISTING.CREATE, body);

export const useCreateListing = () =>
  useMutation({
    mutationFn: createListing,
    meta: {
      successMessage: "Successfully created listing!",
      errorMessage: "Sorry, we had trouble creating your listing.",
    },
  });
```

Use `axiosClient.postForm` + `{ formSerializer: { indexes: null } }` when the payload includes `File`/`Blob` (e.g. image uploads).

## Adding a New Endpoint

1. Add to `src/constants/endpoints.ts`:
```ts
export const ENDPOINTS = {
  MY_RESOURCE: {
    GET_ALL: "/MyResource/GetAll",
    CREATE: "/MyResource/Create",
  },
  // ...
} as const;
```

2. Add query keys to `src/api/queryKeys/myResourceKeys.ts`:
```ts
export const myResourceKeys = {
  all: () => ["myResource"] as const,
  byId: (id: string) => ["myResource", id] as const,
};
```

## Quick Reference

| Scenario | Pattern |
|----------|---------|
| Fetch data (one feature) | `queryOptions()` in `feature/api/` |
| Fetch data (shared) | `queryOptions()` in `src/api/<resource>/` |
| Create / Update / Delete | `useMutation()` hook in `feature/api/` |
| File upload | `axiosClient.postForm()` with `formSerializer: { indexes: null }` |
| Array params | `axiosClient.defaults.paramsSerializer = { indexes: null }` (already configured globally) |
| Auth | Automatic — `axiosClient` injects `Authorization: Bearer <token>` from Redux store |
| 401 refresh | Automatic — `axiosErrorHandler` calls `/Auth/Refresh` and retries |
