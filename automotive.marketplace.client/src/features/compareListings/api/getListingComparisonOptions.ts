import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetListingComparisonResponse } from "../types/GetListingComparisonResponse";

const getListingComparison = (a: string, b: string) =>
  axiosClient.get<GetListingComparisonResponse>(ENDPOINTS.LISTING.COMPARE, {
    params: { a, b },
  });

export const getListingComparisonOptions = (a: string, b: string) =>
  queryOptions({
    queryKey: listingKeys.comparison(a, b),
    queryFn: () => getListingComparison(a, b),
  });
