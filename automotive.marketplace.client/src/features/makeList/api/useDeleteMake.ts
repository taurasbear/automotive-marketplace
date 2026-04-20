import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteMakeCommand } from "../types/DeleteMakeCommand";

const deleteMake = (query: DeleteMakeCommand) =>
  axiosClient.delete<void>(ENDPOINTS.MAKE.DELETE, { params: query });

export const useDeleteMake = () =>
  useMutation({
    mutationFn: deleteMake,
    meta: {
      successMessage: "Successfully deleted make!",
      errorMessage: "Sorry, we had trouble deleting your make",
      invalidatesQuery: makeKeys.all(),
    },
  });
