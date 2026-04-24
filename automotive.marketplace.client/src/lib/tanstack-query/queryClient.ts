import { MutationCache, QueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

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
        toast.success(successMessage);
      }
    },
    onError: (_error, _variables, _context, mutation) => {
      const errorMessage = mutation.meta?.errorMessage;
      if (errorMessage) {
        toast.error(errorMessage);
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
