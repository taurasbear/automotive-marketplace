import { ENDPOINTS } from "@/constants/endpoints";
import { RefreshTokenResponse } from "@/features/auth/types/RefreshTokenResponse";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

const refreshToken = () =>
  axiosClient.post<RefreshTokenResponse>(ENDPOINTS.AUTH.REFRESH);

export const useRefreshToken = () =>
  useMutation({
    mutationFn: refreshToken,
  });
