import { listingKeys } from "@/api/queryKeys/listingKeys";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type UpdateListingStatusCommand = {
  id: string;
  status: "Available" | "Sold" | "Bought" | "OnHold" | "Removed";
};

const updateListingStatus = (body: UpdateListingStatusCommand) =>
  axiosClient.put<void>("/Listing/status", body);

export const useUpdateListingStatus = () =>
  useMutation({
    mutationFn: updateListingStatus,
    meta: {
      successMessage: "toasts:listing.statusUpdateSuccess",
      errorMessage: "toasts:listing.statusUpdateError",
      invalidatesQuery: listingKeys.all(),
    },
  });