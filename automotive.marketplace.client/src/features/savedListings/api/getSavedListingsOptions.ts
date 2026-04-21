import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { SavedListing } from "../types/SavedListing";

const getSavedListings = () =>
  axiosClient.get<SavedListing[]>(ENDPOINTS.SAVED_LISTING.GET_ALL);

export const getSavedListingsOptions = () =>
  queryOptions({
    queryKey: savedListingKeys.list(),
    queryFn: () => getSavedListings(),
  });
