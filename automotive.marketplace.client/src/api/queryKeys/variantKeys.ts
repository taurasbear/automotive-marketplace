export const variantKeys = {
  all: () => ["variant"],
  byModelId: (modelId: string) => [...variantKeys.all(), modelId],
};
