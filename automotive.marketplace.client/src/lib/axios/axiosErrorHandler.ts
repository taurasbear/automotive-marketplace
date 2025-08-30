import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { store } from "@/lib/redux/store";
import { router } from "@/lib/router";
import { AxiosError, AxiosResponse, InternalAxiosRequestConfig } from "axios";
import { toast } from "sonner";
import {
  clearCredentials,
  setCredentials,
} from "../../features/auth/state/authSlice";
import { RefreshTokenResponse } from "../../features/auth/types/RefreshTokenResponse";
import { authClient } from "./authClient";

let isRedirecting = false;
let isRefreshing = false;
let failedQueue: {
  originalRequest: InternalAxiosRequestConfig;
  resolve: (value: AxiosResponse) => void;
  reject: (value: AxiosError) => void;
}[] = [];

export const handleAxiosError = async (error: AxiosError) => {
  if (!error.response || !error.config) {
    console.error("Network error or no response:", error);
    toast.error("Network error. Please check your connection.");
    return Promise.reject(error);
  }
  const { status } = error.response;

  if (status === 401) {
    return refreshTokenAndRetry(error.config);
  }

  return Promise.reject(error);
};

const refreshTokenAndRetry = async (
  originalRequest: InternalAxiosRequestConfig,
) => {
  if (isRefreshing) {
    return createRetryRequest(originalRequest);
  }

  isRefreshing = true;
  try {
    const { data: user } = await authClient.post<RefreshTokenResponse>(
      ENDPOINTS.AUTH.REFRESH,
    );

    store.dispatch(
      setCredentials({
        accessToken: user.accessToken,
        userId: user.userId,
        permissions: user.permissions,
      }),
    );

    const retryRequest = createRetryRequest(originalRequest);
    setTimeout(() => void processFailedQueue(), 0);

    return retryRequest;
  } catch (error) {
    await clearSession(error as AxiosError);
    if (!isRedirecting) {
      isRedirecting = true;
      toast.error("Session has ended");
      await router.navigate({ to: "/login" });
      isRedirecting = false;
    }

    return Promise.reject(error as AxiosError);
  }
};

const createRetryRequest = (originalRequest: InternalAxiosRequestConfig) =>
  new Promise((resolve, reject) =>
    failedQueue.push({ originalRequest, resolve, reject }),
  );

const processFailedQueue = async () => {
  const promises = failedQueue.map(
    async ({ originalRequest, resolve, reject }) =>
      await axiosClient(originalRequest)
        .then((response) => resolve(response))
        .catch((error) => reject(error as AxiosError)),
  );

  await Promise.allSettled(promises);

  failedQueue = [];
  isRefreshing = false;
};

const clearSession = async (error: AxiosError) => {
  failedQueue.forEach(({ reject }) => reject(error));
  failedQueue = [];
  isRefreshing = false;
  store.dispatch(clearCredentials());
  await clearHttpOnlyRefreshToken();
};

const clearHttpOnlyRefreshToken = async () =>
  await authClient
    .post<void>(ENDPOINTS.AUTH.LOGOUT)
    .catch((error) => console.error("Failed to clear refresh token", error));
