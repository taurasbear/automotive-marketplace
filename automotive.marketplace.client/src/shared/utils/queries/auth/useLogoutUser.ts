import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { useMutation } from "@tanstack/react-query";

const logoutUser = () => axiosClient.post<void>(ENDPOINTS.AUTH.LOGOUT);

export const useLogoutUser = () =>
  useMutation({
    mutationFn: logoutUser,
    meta: {
      successMessage: "Successfully logged out",
    },
  });
