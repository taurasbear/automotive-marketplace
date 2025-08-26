import axiosClient from "@/lib/axios/axiosClient";
import { store } from "@/lib/redux/store";
import { router } from "@/lib/router";
import { Mutation, Query } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { toast } from "sonner";
import {
  clearCredentials,
  setCredentials,
} from "../../features/auth/state/authSlice";
import { RefreshTokenResponse } from "../../features/auth/types/RefreshTokenResponse";

let isRedirecting = false;
let isRefreshing = false;
let failedQueue: {
  query?: Query;
  mutation?: Mutation<unknown, unknown, unknown, unknown>;
  variables?: unknown;
}[] = [];

const handleAxiosError = async (
  error: AxiosError,
  query?: Query,
  mutation?: Mutation<unknown, unknown, unknown, unknown>,
  variables?: unknown,
) => {
  const axiosError = error;
  if (!axiosError.response) {
    console.error("Network error or no response:", error);
    return;
  }
  const { status } = axiosError.response;

  if (status === 401) {
    if (mutation) await refreshTokenAndRetry(undefined, mutation, variables);
    else await refreshTokenAndRetry(query);
  } else console.error(axiosError.message);
};

export const handleQueryError = async (
  error: Error,
  query: Query<unknown, unknown, unknown, readonly unknown[]>,
) => {
  await handleAxiosError(error as AxiosError, query as Query);
};

export const handleMutationError = async (
  error: Error,
  variables: unknown,
  _context: unknown,
  mutation: Mutation<unknown, unknown, unknown, unknown>,
) => {
  await handleAxiosError(error as AxiosError, undefined, mutation, variables);
};

const processFailedQueue = async () => {
  const promises = failedQueue.map(async ({ query, mutation, variables }) => {
    if (mutation) {
      const { options } = mutation;
      mutation.setOptions({ ...options });
      await mutation.execute(variables);
    }
    if (query) await query.fetch();
  });

  await Promise.all(promises);
  isRefreshing = false;
  failedQueue = [];
};

const refreshTokenAndRetry = async (
  query?: Query,
  mutation?: Mutation<unknown, unknown, unknown, unknown>,
  variables?: unknown,
) => {
  try {
    if (!isRefreshing) {
      isRefreshing = true;
      failedQueue.push({ query, mutation, variables });
      const { data: user } =
        await axiosClient.post<RefreshTokenResponse>("/auth/refresh");

      store.dispatch(
        setCredentials({
          accessToken: user.accessToken,
          userId: user.userId,
          permissions: user.permissions,
        }),
      );
      await processFailedQueue();
    } else {
      failedQueue.push({ query, mutation, variables });
    }
  } catch {
    store.dispatch(clearCredentials());
    await clearHttpOnlyRefreshToken();
    if (!isRedirecting) {
      isRedirecting = true;
      toast.error("Session has ended");
      await router.navigate({ to: "/login" });
    }
  }
};

const clearHttpOnlyRefreshToken = async () => {
  try {
    await axiosClient.post<void>("auth/logout");
  } catch (error) {
    console.error("Failed to clear refresh token", error);
  }
};
