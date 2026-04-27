import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";

export type UserContractProfile = {
  phoneNumber: string | null;
  personalIdCode: string | null;
  address: string | null;
};

const getUserContractProfile = () =>
  axiosClient.get<UserContractProfile>(ENDPOINTS.CHAT.GET_USER_CONTRACT_PROFILE);

export const getUserContractProfileOptions = () =>
  queryOptions({
    queryKey: chatKeys.userContractProfile(),
    queryFn: getUserContractProfile,
  });
