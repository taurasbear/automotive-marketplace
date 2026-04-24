import type { QueryKey } from "@tanstack/react-query";

declare module "@tanstack/react-query" {
  interface Register {
    mutationMeta: {
      invalidatesQuery?: QueryKey | readonly QueryKey[];
      successMessage?: string;
      errorMessage?: string;
    };
  }
}
