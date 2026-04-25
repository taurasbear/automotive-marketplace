import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteModelCommand } from "../types/DeleteModelCommand";

const deleteModel = (query: DeleteModelCommand) =>
  axiosClient.delete<void>(ENDPOINTS.MODEL.DELETE, { params: query });

export const useDeleteModel = () =>
  useMutation({
    mutationFn: deleteModel,
    meta: {
      successMessage: "toasts:model.deleteSuccess",
      errorMessage: "toasts:model.deleteError",
      invalidatesQuery: modelKeys.all(),
    },
  });
