import { listingKeys } from "@/api/queryKeys/listingKeys";
import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import type { ToggleLikeResponse } from "../types/ToggleLikeResponse";

type ToggleLikeCommand = {
  listingId: string;
};

const toggleLike = (command: ToggleLikeCommand) =>
  axiosClient.post<ToggleLikeResponse>(
    ENDPOINTS.SAVED_LISTING.TOGGLE_LIKE,
    command,
  );

export const useToggleLike = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: toggleLike,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: savedListingKeys.all() });
      void queryClient.invalidateQueries({ queryKey: listingKeys.all() });
    },
    meta: {
      errorMessage: "Could not update like. Please try again.",
    },
  });
};
