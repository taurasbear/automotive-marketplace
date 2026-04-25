export const defectKeys = {
  all: () => ["defects"] as const,
  categories: () => [...defectKeys.all(), "categories"] as const,
};
