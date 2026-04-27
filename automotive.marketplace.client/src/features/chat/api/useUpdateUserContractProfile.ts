import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

type UpdateProfilePayload = {
  phoneNumber?: string | null;
  personalIdCode?: string | null;
  address?: string | null;
};

const updateUserContractProfile = (payload: UpdateProfilePayload) =>
  axiosClient.put(ENDPOINTS.CHAT.UPDATE_USER_CONTRACT_PROFILE, payload);

export const useUpdateUserContractProfile = () =>
  useMutation({ mutationFn: updateUserContractProfile });
