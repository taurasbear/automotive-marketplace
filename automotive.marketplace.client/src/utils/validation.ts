export const validation = {
  required: ({ label }: { label: string }) => `${label} is required`,

  minLength: ({ label, length }: { label: string; length: number }) =>
    `${label} must be at least ${length} characters long`,

  maxLength: ({ label, length }: { label: string; length: number }) =>
    `${label} must be ${length} or fewer characters long`,
};
