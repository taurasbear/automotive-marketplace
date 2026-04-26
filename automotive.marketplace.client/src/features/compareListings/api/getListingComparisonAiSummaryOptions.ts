import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonAiSummaryResponse } from "../types/GetListingComparisonAiSummaryResponse";

export const getListingComparisonAiSummaryOptions = (listingAId: string, listingBId: string) =>
  queryOptions({
    queryKey: listingKeys.comparisonAiSummary(listingAId, listingBId),
    queryFn: () =>
      axiosClient.get<GetListingComparisonAiSummaryResponse>(ENDPOINTS.LISTING.GET_COMPARISON_AI_SUMMARY, {
        params: { listingAId, listingBId },
      }),
    enabled: false,
  });
