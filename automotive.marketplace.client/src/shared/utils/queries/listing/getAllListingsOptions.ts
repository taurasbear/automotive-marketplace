import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { queryOptions } from "@tanstack/react-query";
import { listingKeys } from "./listingKeys";
import { GetListingsDetailsWithCarResponse } from "@/shared/types/dto/listing/GetListingDetailsWithCarResponse";
import { AxiosResponse } from "axios";

const getAllListings = (): Promise<
  AxiosResponse<GetListingsDetailsWithCarResponse>
> => axiosClient.get(ENDPOINTS.LISTING.GET_ALL);

export const getAllListingsOptions = queryOptions({
  queryKey: [listingKeys.all()],
  queryFn: getAllListings,
});
