import { queryOptions, skipToken } from "@tanstack/react-query";
import { getVariantsByModelId } from "./getVariantsByModelIdOptions";
import { variantKeys } from "@/api/queryKeys/variantKeys";

type GetVariantsByModelParams = {
  modelId: string | undefined;
  year?: number;
};

export const getVariantsByModelOptions = ({ modelId, year }: GetVariantsByModelParams) =>
  queryOptions({
    queryKey: [...variantKeys.byModelId(modelId ?? ""), year ?? null],
    queryFn: modelId ? () => getVariantsByModelId(modelId) : skipToken,
  });
