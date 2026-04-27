import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingAiSummaryResponse } from "../types/GetListingAiSummaryResponse";

export const getListingAiSummaryOptions = (
  listingId: string,
  language: string = "lt",
  forceRegenerate: boolean = false,
) =>
  queryOptions({
    queryKey: listingKeys.aiSummary(listingId, language),
    queryFn: () =>
      axiosClient.get<GetListingAiSummaryResponse>(
        ENDPOINTS.LISTING.GET_AI_SUMMARY,
        {
          params: { listingId, language, forceRegenerate },
        },
      ),
    enabled: true,
  });
