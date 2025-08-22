import axiosClient from "@/api/axiosClient";
import { Mutation, Query } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { clearCredentials, setCredentials } from "../state/authSlice";
import { store } from "../state/store";
import { RefreshTokenResponse } from "../types/dto/auth/RefreshTokenResponse";

let isRedirecting = false;
let isRefreshing = false;
let failedQueue: {
  query?: Query;
  mutation?: Mutation<unknown, unknown, unknown, unknown>;
  variables?: unknown;
}[] = [];

const handleAxiosError = (
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
    if (mutation) refreshTokenAndRetry(undefined, mutation, variables);
    else refreshTokenAndRetry(query);
  } else console.error(axiosError.message);
};

export const handleQueryError = (
  error: Error,
  query: Query<unknown, unknown, unknown, readonly unknown[]>,
) => {
  handleAxiosError(error as AxiosError, query as Query);
};

export const handleMutationError = (
  error: Error,
  variables: unknown,
  _context: unknown,
  mutation: Mutation<unknown, unknown, unknown, unknown>,
) => {
  handleAxiosError(error as AxiosError, undefined, mutation, variables);
};

const processFailedQueue = () => {
  failedQueue.forEach(({ query, mutation, variables }) => {
    if (mutation) {
      const { options } = mutation;
      mutation.setOptions({ ...options });
      mutation.execute(variables);
    }
    if (query) query.fetch();
  });
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
      const { data } =
        await axiosClient.post<RefreshTokenResponse>("/auth/refresh");
      const accessToken = data.accessToken;

      store.dispatch(
        setCredentials({ accountId: "idk lol", role: "random", accessToken }),
      );
      processFailedQueue();
    } else {
      failedQueue.push({ query, mutation, variables });
    }
  } catch {
    store.dispatch(clearCredentials());
    await clearHttpOnlyRefreshToken();
    if (!isRedirecting) {
      isRedirecting = true;
      window.location.href = "/auth/session-expired";
    }
  }
};

const clearHttpOnlyRefreshToken = async () => {
  try {
    await axiosClient.post("auth/logout");
  } catch (error) {
    console.error("Failed to clear refresh token", error);
  }
};
