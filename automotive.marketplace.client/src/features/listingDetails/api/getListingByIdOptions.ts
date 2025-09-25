import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetListingByIdQuery } from "../types/GetListingByIdQuery";
import { GetListingByIdResponse } from "../types/GetListingByIdResponse";

const getListingById = (query: GetListingByIdQuery) =>
  axiosClient.get<GetListingByIdResponse>(ENDPOINTS.LISTING.GET_BY_ID, {
    params: query,
  });

export const getListingByIdOptions = (query: GetListingByIdQuery) =>
  queryOptions({
    queryKey: listingKeys.byId(query.id),
    queryFn: () => getListingById(query),
  });
