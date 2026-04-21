import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";

type UpsertListingNoteCommand = {
  listingId: string;
  content: string;
};

const upsertListingNote = (command: UpsertListingNoteCommand) =>
  axiosClient.put<void>(ENDPOINTS.SAVED_LISTING.UPSERT_NOTE, command);

export const useUpsertListingNote = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: upsertListingNote,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: savedListingKeys.list() });
    },
    meta: {
      errorMessage: "Could not save note. Please try again.",
    },
  });
};
