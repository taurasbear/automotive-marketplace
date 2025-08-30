import { Spinner } from "@/components/ui/shadcn-io/spinner";
import { routeTree } from "@/routeTree.gen";
import { createRouter } from "@tanstack/react-router";
import queryClient from "./tanstack-query/queryClient";

export const router = createRouter({
  routeTree,
  context: { queryClient },
  defaultPendingComponent: () => (
    <div className="flex h-screen items-center justify-center">
      <Spinner variant="bars" />
    </div>
  ),
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
