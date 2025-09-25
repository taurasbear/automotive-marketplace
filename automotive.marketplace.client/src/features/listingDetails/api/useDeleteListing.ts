import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteListingCommand } from "../types/DeleteListingCommand";

const deleteListing = (query: DeleteListingCommand) =>
  axiosClient.delete<void>(ENDPOINTS.LISTING.DELETE, { params: query });

export const useDeleteListing = () =>
  useMutation({
    mutationFn: deleteListing,
    meta: {
      successMessage: "Successfully deleted listing!",
      errorMessage: "Sorry, we had trouble deleting your listing",
      invalidatesQuery: listingKeys.all(),
    },
  });
