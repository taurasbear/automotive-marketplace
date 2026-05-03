import { ENDPOINTS } from "@/constants/endpoints";
import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type UpdateListingStatusCommand = {
  id: string;
  status: "Available" | "Sold" | "Bought" | "OnHold" | "Removed";
};

const updateListingStatus = (body: UpdateListingStatusCommand) =>
  axiosClient.put<void>(ENDPOINTS.LISTING.UPDATE_STATUS, body);

export const useUpdateListingStatus = () =>
  useMutation({
    mutationFn: updateListingStatus,
    meta: {
      successMessage: "toasts:listing.statusUpdateSuccess",
      errorMessage: "toasts:listing.statusUpdateError",
      invalidatesQuery: [myListingKeys.all(), listingKeys.all()],
    },
  });