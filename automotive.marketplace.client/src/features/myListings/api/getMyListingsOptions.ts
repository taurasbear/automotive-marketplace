import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetMyListingsResponse } from "../types/GetMyListingsResponse";

const getMyListings = () =>
  axiosClient.get<GetMyListingsResponse[]>(ENDPOINTS.LISTING.GET_MY);

export const getMyListingsOptions = queryOptions({
  queryKey: myListingKeys.all(),
  queryFn: getMyListings,
});
