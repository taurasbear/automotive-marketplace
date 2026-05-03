import { ENDPOINTS } from "@/constants/endpoints";
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type ReactivateListingCommand = {
  listingId: string;
};

const reactivateListing = (body: ReactivateListingCommand) =>
  axiosClient.put<void>(ENDPOINTS.LISTING.REACTIVATE, body);

export const useReactivateListing = () =>
  useMutation({
    mutationFn: reactivateListing,
    meta: {
      successMessage: "toasts:listing.reactivateSuccess",
      errorMessage: "toasts:listing.reactivateError",
      invalidatesQuery: [myListingKeys.all(), listingKeys.all()],
    },
  });
