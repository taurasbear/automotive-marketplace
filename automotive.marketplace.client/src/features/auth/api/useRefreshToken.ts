import axiosClient from "@/api/axiosClient";
import { RefreshTokenResponse } from "@/features/auth/types/RefreshTokenResponse";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { useMutation } from "@tanstack/react-query";

const refreshToken = () =>
  axiosClient.post<RefreshTokenResponse>(ENDPOINTS.AUTH.REFRESH);

export const useRefreshToken = () =>
  useMutation({
    mutationFn: refreshToken,
  });
