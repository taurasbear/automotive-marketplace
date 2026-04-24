import type { Translation } from "@/types/shared/Translation";

export const getTranslatedName = (
  translations: Translation[],
  language: string,
): string => {
  const match = translations.find((t) => t.languageCode === language);
  if (match) return match.name;

  const fallback = translations.find((t) => t.languageCode === "en");
  if (fallback) return fallback.name;

  return translations[0]?.name ?? "";
};
