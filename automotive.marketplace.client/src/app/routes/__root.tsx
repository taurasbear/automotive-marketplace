import Header from "@/components/layout/header/Header";
import { useChatHub } from "@/features/chat";
import { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

interface RouterContext {
  queryClient: QueryClient;
}

const RootLayout = () => {
  useChatHub();
  return (
    <>
      <Header />
      <div className="mx-8 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
    </>
  );
};

export const Route = createRootRouteWithContext<RouterContext>()({
  component: RootLayout,
});
