import { makeKeys } from "@/api/queryKeys/makeKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateMakeCommand } from "../types/UpdateMakeCommand";

const updateMake = (body: UpdateMakeCommand) =>
  axiosClient.put<void>(ENDPOINTS.MAKE.UPDATE, body);

export const useUpdateMake = () =>
  useMutation({
    mutationFn: updateMake,
    meta: {
      successMessage: "toasts:make.updateSuccess",
      errorMessage: "toasts:make.updateError",
      invalidatesQuery: makeKeys.all(),
    },
  });
