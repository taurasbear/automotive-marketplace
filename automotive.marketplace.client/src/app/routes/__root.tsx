import Header from "@/components/layout/header/Header";
import { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

interface RouterContext {
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<RouterContext>()({
  component: () => (
    <>
      <Header />
      <div className="mx-8 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
    </>
  ),
});
