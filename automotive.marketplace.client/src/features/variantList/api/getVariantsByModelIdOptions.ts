import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { Variant } from "@/features/variantList/types/Variant";
import { queryOptions, skipToken } from "@tanstack/react-query";
import { variantKeys } from "@/api/queryKeys/variantKeys";

export const getVariantsByModelId = (modelId: string) =>
  axiosClient.get<Variant[]>(`${ENDPOINTS.VARIANT.GET_BY_MODEL_ID}/${modelId}`);

export const getVariantsByModelIdOptions = (modelId: string | undefined) =>
  queryOptions({
    queryKey: variantKeys.byModelId(modelId ?? ""),
    queryFn: modelId ? () => getVariantsByModelId(modelId) : skipToken,
  });
