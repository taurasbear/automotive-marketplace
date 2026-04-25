import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import { GetListingEngagementsResponse } from "../types/GetListingEngagementsResponse";

const getListingEngagements = (listingId: string) =>
  axiosClient.get<GetListingEngagementsResponse>(ENDPOINTS.LISTING.GET_ENGAGEMENTS, {
    params: { id: listingId },
  });

export const getListingEngagementsOptions = (listingId: string) =>
  queryOptions({
    queryKey: myListingKeys.engagements(listingId),
    queryFn: () => getListingEngagements(listingId),
  });
