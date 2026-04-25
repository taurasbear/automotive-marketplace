import { variantKeys } from "@/api/queryKeys/variantKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import { DeleteVariantCommand } from "../types/DeleteVariantCommand";

const deleteVariant = ({ id }: DeleteVariantCommand) =>
  axiosClient.delete<void>(`${ENDPOINTS.VARIANT.DELETE}/${id}`);

export const useDeleteVariant = () =>
  useMutation({
    mutationFn: deleteVariant,
    meta: {
      successMessage: "toasts:variant.deleteSuccess",
      errorMessage: "toasts:variant.deleteError",
      invalidatesQuery: variantKeys.all(),
    },
  });
