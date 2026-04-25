import i18n from "./i18n";

const localeMap: Record<string, string> = {
  lt: "lt-LT",
  en: "en-US",
};

const getLocale = (): string => localeMap[i18n.language] ?? "lt-LT";

export const formatNumber = (value: number): string => {
  return new Intl.NumberFormat(getLocale()).format(value);
};

export const formatCurrency = (value: number): string => {
  return new Intl.NumberFormat(getLocale(), {
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
};
