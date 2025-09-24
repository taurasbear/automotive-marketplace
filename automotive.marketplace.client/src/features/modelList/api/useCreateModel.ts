import { modelKeys } from "@/api/queryKeys/modelKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateModelCommand } from "../types/CreateModelCommand";

const createModel = (body: CreateModelCommand) =>
  axiosClient.post<void>(ENDPOINTS.MODEL.CREATE, body);

export const useCreateModel = () =>
  useMutation({
    mutationFn: createModel,
    meta: {
      successMessage: "Successfully created model!",
      errorMessage: "Sorry, we couldn't create your model",
      invalidatesQuery: modelKeys.all(),
    },
  });
