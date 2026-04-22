import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { SearchListingsResponse } from "../types/SearchListingsResponse";

const searchListings = (q: string) =>
  axiosClient.get<SearchListingsResponse[]>(ENDPOINTS.LISTING.SEARCH, {
    params: { q },
  });

export const searchListingsOptions = (q: string) =>
  queryOptions({
    queryKey: listingKeys.search(q),
    queryFn: () => searchListings(q),
    enabled: q.trim().length > 0,
  });
