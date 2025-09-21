export const validation = {
  required: ({ label }: { label: string }) => `${label} is required`,

  minLength: ({ label, length }: { label: string; length: number }) =>
    `${label} must be at least ${length} characters long`,

  maxLength: ({ label, length }: { label: string; length: number }) =>
    `${label} must be ${length} or fewer characters long`,

  minSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) => `${label} must be at least ${size}${unit ? ` ${unit}` : ""}`,

  maxSize: ({
    label,
    size,
    unit,
  }: {
    label: string;
    size: number;
    unit?: string;
  }) => `${label} must be ${size}${unit ? ` ${unit}` : ""} or less`,
};
