import { ENDPOINTS } from "@/constants/endpoints";
import {
  applyAuthResponse,
  clearCredentials,
  RefreshTokenResponse,
} from "@/features/auth";
import Header from "@/components/layout/header/Header";
import { useChatHub } from "@/features/chat";
import { authClient } from "@/lib/axios/authClient";
import { store } from "@/lib/redux/store";
import { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

type RouterContext = {
  queryClient: QueryClient;
};

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
  staleTime: Infinity,
  loader: async () => {
    const { auth } = store.getState();
    if (auth.userId && !auth.accessToken) {
      try {
        const { data } = await authClient.post<RefreshTokenResponse>(
          ENDPOINTS.AUTH.REFRESH,
        );
        applyAuthResponse(data);
      } catch {
        store.dispatch(clearCredentials());
      }
    }
  },
});
