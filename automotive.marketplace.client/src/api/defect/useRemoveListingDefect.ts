import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const removeListingDefect = (params: { id: string }) =>
  axiosClient.delete<void>(ENDPOINTS.DEFECT.REMOVE, { params });

export const useRemoveListingDefect = () =>
  useMutation({
    mutationFn: removeListingDefect,
    meta: {
      successMessage: "Defect removed!",
      errorMessage: "Failed to remove defect",
      invalidatesQuery: [...listingKeys.all(), ...myListingKeys.all()],
    },
  });