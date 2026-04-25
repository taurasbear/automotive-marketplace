import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { UpdateVariantCommand } from "../types/UpdateVariantCommand";

const updateVariant = ({ id, ...body }: UpdateVariantCommand) =>
  axiosClient.put<void>(`${ENDPOINTS.VARIANT.UPDATE}/${id}`, body);

export const useUpdateVariant = () =>
  useMutation({
    mutationFn: updateVariant,
    meta: {
      successMessage: "toasts:variant.updateSuccess",
      errorMessage: "toasts:variant.updateError",
      invalidatesQuery: variantKeys.all(),
    },
  });
