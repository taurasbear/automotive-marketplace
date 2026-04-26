import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { userPreferencesKeys } from "@/api/queryKeys/userPreferencesKeys";
import { listingKeys } from "@/api/queryKeys/listingKeys";
import type { UpsertUserPreferencesCommand } from "../types/UpsertUserPreferencesCommand";

const upsertUserPreferences = (body: UpsertUserPreferencesCommand) =>
  axiosClient.put(ENDPOINTS.USER_PREFERENCES.UPSERT, body);

export const useUpsertUserPreferences = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: upsertUserPreferences,
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: userPreferencesKeys.current(),
      });
      void queryClient.invalidateQueries({ queryKey: listingKeys.all() });
    },
    meta: {
      successMessage: "toasts:preferences.saved",
      errorMessage: "toasts:preferences.saveError",
    },
  });
};
