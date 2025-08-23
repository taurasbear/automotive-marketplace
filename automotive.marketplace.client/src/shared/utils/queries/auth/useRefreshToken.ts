import axiosClient from "@/api/axiosClient";
import { ENDPOINTS } from "@/shared/constants/endpoints";
import { RefreshTokenResponse } from "@/shared/types/dto/auth/RefreshTokenResponse";
import { useMutation } from "@tanstack/react-query";

const refreshToken = () =>
  axiosClient.post<RefreshTokenResponse>(ENDPOINTS.AUTH.REFRESH);

export const useRefreshToken = () =>
  useMutation({
    mutationFn: refreshToken,
  });
