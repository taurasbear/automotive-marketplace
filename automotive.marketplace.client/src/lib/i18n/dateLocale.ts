import { useTranslation } from "react-i18next";
import { lt, enUS } from "date-fns/locale";
import type { Locale } from "date-fns/locale";

const localeMap: Record<string, Locale> = {
  lt: lt,
  en: enUS,
};

export const useDateLocale = (): Locale => {
  const { i18n } = useTranslation();
  return localeMap[i18n.language] ?? lt;
};
