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
      successMessage: "Successfully deleted model!",
      errorMessage: "Sorry, we had trouble deleting your model",
      invalidatesQuery: modelKeys.all(),
    },
  });
