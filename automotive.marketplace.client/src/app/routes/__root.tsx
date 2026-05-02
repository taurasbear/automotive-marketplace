import { ENDPOINTS } from "@/constants/endpoints";
import {
  applyAuthResponse,
  clearCredentials,
  RefreshTokenResponse,
} from "@/features/auth";
import Header from "@/components/layout/header/Header";
import Footer from "@/components/layout/footer/Footer";
import { useChatHub } from "@/features/chat";
import { authClient } from "@/lib/axios/authClient";
import { store } from "@/lib/redux/store";
import { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import { TanStackRouterDevtools } from "@tanstack/react-router-devtools";

type RouterContext = {
  queryClient: QueryClient;
};

// Child route loaders can await this to ensure auth is resolved before fetching
let _resolveAuthReady!: () => void;
export const authReady = new Promise<void>((resolve) => {
  _resolveAuthReady = resolve;
});

const RootLayout = () => {
  useChatHub();
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <div className="mx-8 flex-1 xl:mx-auto xl:max-w-6xl">
        <Outlet />
        <TanStackRouterDevtools />
      </div>
      <Footer />
    </div>
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
        applyAuthResponse(store.dispatch, data);
      } catch {
        store.dispatch(clearCredentials());
      }
    }
    _resolveAuthReady();
  },
});
