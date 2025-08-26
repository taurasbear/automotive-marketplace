import {
  handleMutationError,
  handleQueryError,
} from "@/shared/utils/errorHandler";
import { MutationCache, QueryCache, QueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 10000,
    },
  },

  queryCache: new QueryCache({
    onError: void handleQueryError,
  }),

  mutationCache: new MutationCache({
    onSuccess: (_data, _variables, _context, mutation) => {
      const successMessage = mutation.meta?.successMessage;
      if (successMessage) {
        toast.success(successMessage);
      }
    },
    onError: handleMutationError,
    onSettled: async (_data, _error, _variables, _context, mutation) => {
      const queryToInvalidate = mutation.meta?.invalidatesQuery;
      if (queryToInvalidate) {
        await queryClient.invalidateQueries({ queryKey: queryToInvalidate });
      }
    },
  }),
});

export default queryClient;
