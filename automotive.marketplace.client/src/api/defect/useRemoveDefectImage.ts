import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const removeDefectImage = (params: { id: string }) =>
  axiosClient.delete<void>(ENDPOINTS.DEFECT.REMOVE_IMAGE, { params });

export const useRemoveDefectImage = () =>
  useMutation({
    mutationFn: removeDefectImage,
    meta: {
      successMessage: "Photo removed!",
      errorMessage: "Failed to remove photo",
      invalidatesQuery: [...listingKeys.all(), ...myListingKeys.all()],
    },
  });