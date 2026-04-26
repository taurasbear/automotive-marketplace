import { userPreferencesKeys } from "@/api/queryKeys/userPreferencesKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { UserPreferencesResponse } from "../types/UserPreferencesResponse";

export const getUserPreferencesOptions = queryOptions({
  queryKey: userPreferencesKeys.current(),
  queryFn: () =>
    axiosClient.get<UserPreferencesResponse>(ENDPOINTS.USER_PREFERENCES.GET),
});
