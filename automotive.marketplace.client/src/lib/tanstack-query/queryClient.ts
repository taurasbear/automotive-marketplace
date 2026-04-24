import { MutationCache, QueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import i18n from "@/lib/i18n/i18n";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 10000,
    },
  },

  mutationCache: new MutationCache({
    onSuccess: (_data, _variables, _context, mutation) => {
      const successMessage = mutation.meta?.successMessage;
      if (successMessage) {
        toast.success(i18n.t(successMessage));
      }
    },
    onError: (_error, _variables, _context, mutation) => {
      const errorMessage = mutation.meta?.errorMessage;
      if (errorMessage) {
        toast.error(i18n.t(errorMessage));
      }
    },
    onSettled: async (_data, _error, _variables, _context, mutation) => {
      const queryToInvalidate = mutation.meta?.invalidatesQuery;
      if (queryToInvalidate && queryToInvalidate.length > 0) {
        if (Array.isArray(queryToInvalidate[0])) {
          // Multiple query keys to invalidate
          for (const key of queryToInvalidate) {
            await queryClient.invalidateQueries({ queryKey: key as readonly unknown[] });
          }
        } else {
          // Single query key
          await queryClient.invalidateQueries({ queryKey: queryToInvalidate });
        }
      }
    },
  }),
});

export default queryClient;
