import type { GetListingByIdResponse } from "@/features/listingDetails/types/GetListingByIdResponse";

export type GetListingComparisonResponse = {
  listingA: GetListingByIdResponse;
  listingB: GetListingByIdResponse;
};
