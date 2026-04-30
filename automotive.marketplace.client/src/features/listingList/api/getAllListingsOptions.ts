import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import { GetAllListingsResponse } from "../types/GetAllListingsResponse";

export type PagedResult<T> = {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
};

const getAllListings = (query: GetAllListingsQuery) =>
  axiosClient.get<PagedResult<GetAllListingsResponse>>(ENDPOINTS.LISTING.GET_ALL, {
    params: query,
  });

export const getAllListingsOptions = (query: GetAllListingsQuery) =>
  queryOptions({
    queryKey: listingKeys.bySearchParams(query),
    queryFn: () => getAllListings(query),
  });
