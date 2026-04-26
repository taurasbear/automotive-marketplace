import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingScoreResponse } from "../types/GetListingScoreResponse";

export const getListingScoreOptions = (listingId: string) =>
  queryOptions({
    queryKey: listingKeys.score(listingId),
    queryFn: () =>
      axiosClient.get<GetListingScoreResponse>(ENDPOINTS.LISTING.GET_SCORE, {
        params: { listingId },
      }),
  });
