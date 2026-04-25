import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const deleteMyListing = (params: { id: string }) =>
  axiosClient.delete<void>(ENDPOINTS.LISTING.DELETE, { params });

export const useDeleteMyListing = () =>
  useMutation({
    mutationFn: deleteMyListing,
    meta: {
      successMessage: "toasts:myListing.deleteSuccess",
      errorMessage: "toasts:myListing.deleteError",
      invalidatesQuery: [myListingKeys.all(), listingKeys.all()],
    },
  });
