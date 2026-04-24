import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { CreateVariantCommand } from "../types/CreateVariantCommand";

const createVariant = (body: CreateVariantCommand) =>
  axiosClient.post<void>(ENDPOINTS.VARIANT.CREATE, body);

export const useCreateVariant = () =>
  useMutation({
    mutationFn: createVariant,
    meta: {
      successMessage: "toasts:variant.createSuccess",
      errorMessage: "toasts:variant.createError",
      invalidatesQuery: variantKeys.all(),
    },
  });
