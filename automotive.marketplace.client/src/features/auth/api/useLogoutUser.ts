import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const logoutUser = () => axiosClient.post<void>(ENDPOINTS.AUTH.LOGOUT);

export const useLogoutUser = () =>
  useMutation({
    mutationFn: logoutUser,
    meta: {
      successMessage: "Successfully logged out",
      errorMessage: "Sorry, had trouble logging you out. Please try again.",
    },
  });
