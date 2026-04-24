import { listingKeys } from "@/api/queryKeys/listingKeys";
import { myListingKeys } from "@/api/queryKeys/myListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type AddDefectImageParams = {
  listingDefectId: string;
  image: File;
};

const addDefectImage = ({ listingDefectId, image }: AddDefectImageParams) => {
  const formData = new FormData();
  formData.append("listingDefectId", listingDefectId);
  formData.append("image", image);
  return axiosClient.post<string>(ENDPOINTS.DEFECT.ADD_IMAGE, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

export const useAddDefectImage = () =>
  useMutation({
    mutationFn: addDefectImage,
    meta: {
      successMessage: "Photo added!",
      errorMessage: "Failed to upload photo",
      invalidatesQuery: [listingKeys.all(), myListingKeys.all()],
    },
  });
