import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonAiSummaryResponse } from "../types/GetListingComparisonAiSummaryResponse";

export const getListingComparisonAiSummaryOptions = (
  listingAId: string,
  listingBId: string,
  language: string = "lt",
  forceRegenerate: boolean = false,
) =>
  queryOptions({
    queryKey: listingKeys.comparisonAiSummary(listingAId, listingBId, language),
    queryFn: () =>
      axiosClient.get<GetListingComparisonAiSummaryResponse>(
        ENDPOINTS.LISTING.GET_COMPARISON_AI_SUMMARY,
        {
          params: { listingAId, listingBId, language, forceRegenerate },
        },
      ),
    enabled: true,
  });
