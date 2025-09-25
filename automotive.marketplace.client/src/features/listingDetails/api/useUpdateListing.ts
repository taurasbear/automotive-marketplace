import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateListingCommand } from "../types/UpdateListingCommand";

const updateListing = (body: UpdateListingCommand) =>
  axiosClient.put<void>(ENDPOINTS.LISTING.UPDATE, body);

export const useUpdateListing = () =>
  useMutation({
    mutationFn: updateListing,
    meta: {
      successMessage: "Successfully updated listing!",
      errorMessage: "Sorry, we couldn't update listing",
      invalidatesQuery: listingKeys.all(),
    },
  });
