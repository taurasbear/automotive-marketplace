export const validation = {
  required: ({ label }: { label: string }) => `${label} is required`,

  min: ({ label, length }: { label: string; length: number }) =>
    `${label} must be at least ${length} characters long`,

  max: ({ label, length }: { label: string; length: number }) =>
    `${label} must be ${length} or fewer characters long`,
};
