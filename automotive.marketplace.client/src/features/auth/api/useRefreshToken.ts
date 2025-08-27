import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { RefreshTokenResponse } from "../types/RefreshTokenResponse";

const refreshToken = () =>
  axiosClient.post<RefreshTokenResponse>(ENDPOINTS.AUTH.REFRESH);

export const useRefreshToken = () =>
  useMutation({
    mutationFn: refreshToken,
  });
