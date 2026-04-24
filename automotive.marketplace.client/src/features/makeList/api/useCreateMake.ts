import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateMakeCommand } from "../types/CreateMakeCommand";

const createMake = (body: CreateMakeCommand) =>
  axiosClient.post<void>(ENDPOINTS.MAKE.CREATE, body);

export const useCreateMake = () =>
  useMutation({
    mutationFn: createMake,
    meta: {
      successMessage: "toasts:make.createSuccess",
      errorMessage: "toasts:make.createError",
      invalidatesQuery: makeKeys.all(),
    },
  });
