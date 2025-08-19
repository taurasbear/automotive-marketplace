import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { queryOptions } from "@tanstack/react-query";
import { listingKeys } from "./listingKeys";
import { GetAllListingsResponse } from "@/shared/types/dto/listing/GetAllListingsResponse";
import { AxiosResponse } from "axios";

const getAllListings = (): Promise<AxiosResponse<GetAllListingsResponse[]>> =>
  axiosClient.get(ENDPOINTS.LISTING.GET_ALL);

export const getAllListingsOptions = queryOptions({
  queryKey: [listingKeys.all()],
  queryFn: getAllListings,
});
