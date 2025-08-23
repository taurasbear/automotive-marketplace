import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { useMutation } from "@tanstack/react-query";

const refreshToken = (): Promise<void> =>
  axiosClient.post(ENDPOINTS.AUTH.REFRESH);

export const useRefreshToken = () =>
  useMutation({
    mutationFn: refreshToken,
  });
