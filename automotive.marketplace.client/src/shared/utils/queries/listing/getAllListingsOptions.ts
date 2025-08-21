import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { queryOptions } from "@tanstack/react-query";
import { listingKeys } from "./listingKeys";
import { GetAllListingsResponse } from "@/shared/types/dto/listing/GetAllListingsResponse";
import { AxiosResponse } from "axios";
import { GetAllListingsQuery } from "@/shared/types/dto/listing/GetAllListingsQuery";

const getAllListings = (
  query: GetAllListingsQuery,
): Promise<AxiosResponse<GetAllListingsResponse[]>> =>
  axiosClient.get(ENDPOINTS.LISTING.GET_ALL, { params: query });

export const getAllListingsOptions = (query: GetAllListingsQuery) =>
  queryOptions({
    queryKey: listingKeys.bySearchParams(query),
    queryFn: () => getAllListings(query),
  });
