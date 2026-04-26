import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetSellerListingInsightsResponse } from "../types/GetSellerListingInsightsResponse";

export const getSellerListingInsightsOptions = (listingId: string) =>
  queryOptions({
    queryKey: listingKeys.sellerInsights(listingId),
    queryFn: () =>
      axiosClient.get<GetSellerListingInsightsResponse>(ENDPOINTS.LISTING.GET_SELLER_INSIGHTS, {
        params: { listingId },
      }),
  });
