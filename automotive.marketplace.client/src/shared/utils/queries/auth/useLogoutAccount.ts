import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { useMutation } from "@tanstack/react-query";

const logoutAccount = (): Promise<void> =>
  axiosClient.post(ENDPOINTS.AUTH.LOGOUT);

export const useLogoutAccount = () =>
  useMutation({
    mutationFn: logoutAccount,
    meta: {
      successMessage: "Successfully logged out",
    },
  });
