import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import { GetAllListingsQuery } from "../types/GetAllListingsQuery";
import { GetAllListingsResponse } from "../types/GetAllListingsResponse";
import { listingKeys } from "./listingKeys";

const getAllListings = (
  query: GetAllListingsQuery,
): Promise<AxiosResponse<GetAllListingsResponse[]>> =>
  axiosClient.get(ENDPOINTS.LISTING.GET_ALL, { params: query });

export const getAllListingsOptions = (query: GetAllListingsQuery) =>
  queryOptions({
    queryKey: listingKeys.bySearchParams(query),
    queryFn: () => getAllListings(query),
  });
