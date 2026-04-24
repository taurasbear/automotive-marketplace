import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type AddListingDefectCommand = {
  listingId: string;
  defectCategoryId?: string;
  customName?: string;
};

const addListingDefect = (body: AddListingDefectCommand) =>
  axiosClient.post<string>(ENDPOINTS.DEFECT.ADD, body);

export const useAddListingDefect = () =>
  useMutation({
    mutationFn: addListingDefect,
    meta: {
      successMessage: "Defect added!",
      errorMessage: "Failed to add defect",
      invalidatesQuery: [...listingKeys.all(), ...myListingKeys.all()],
    },
  });
