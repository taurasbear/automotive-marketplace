import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateModelCommand } from "../types/UpdateModelCommand";

const updateModel = (body: UpdateModelCommand) =>
  axiosClient.put<void>(ENDPOINTS.MODEL.UPDATE, body);

export const useUpdateModel = () =>
  useMutation({
    mutationFn: updateModel,
    meta: {
      successMessage: "Successfully updated model!",
      errorMessage: "Sorry, we couldn't update model",
      invalidatesQuery: modelKeys.all(),
    },
  });
