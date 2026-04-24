import { savedListingKeys } from "@/api/queryKeys/savedListingKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";

type DeleteListingNoteCommand = {
  listingId: string;
};

const deleteListingNote = (command: DeleteListingNoteCommand) =>
  axiosClient.delete<void>(ENDPOINTS.SAVED_LISTING.DELETE_NOTE, {
    params: command,
  });

export const useDeleteListingNote = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deleteListingNote,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: savedListingKeys.list() });
    },
    meta: {
      errorMessage: "toasts:saved.noteDeleteError",
    },
  });
};
