import i18n from "@/lib/i18n/i18n";

export const validation = {
  required: ({ label }: { label: string }) =>
    i18n.t("required", { label, ns: "validation" }),

  minLength: ({ label, length }: { label: string; length: number }) =>
    i18n.t("minLength", { label, length, ns: "validation" }),

  maxLength: ({ label, length }: { label: string; length: number }) =>
    i18n.t("maxLength", { label, length, ns: "validation" }),

  minSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) =>
    i18n.t("minSize", {
      label,
      size,
      unit: unit ? ` ${unit}` : "",
      ns: "validation",
    }),

  maxSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) =>
    i18n.t("maxSize", {
      label,
      size,
      unit: unit ? ` ${unit}` : "",
      ns: "validation",
    }),
};
